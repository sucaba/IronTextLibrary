using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Algorithm;
using IronText.Diagnostics;

namespace IronText.Framework
{
    using State = System.Int32;
    using Token = System.Int32;

    sealed class Gss<T>
        : IGss<T>
        , IUndoable
    {
        private int currentLayer = 0;
        private byte currentStage = 0;
        private List<GssNode<T>> front;
        private readonly CircularStack<GssNode<T>> history;

        public Gss(int stateCount)
        {
            front = new List<GssNode<T>>(stateCount);
            history = new CircularStack<GssNode<T>>(2 * stateCount);
            AddTopmost(0);
        }

        private Gss(int currentLayer, List<GssNode<T>> front)
        {
            this.currentLayer = currentLayer;
            this.front = front;
        }

        public int CurrentLayer { get { return currentLayer; } }

        public IEnumerable<GssNode<T>> Front { get { return front; } }

        public bool IsEmpty { get { return front.Count == 0; } }

        public void PushLayer()
        {
            currentStage = 0;
            front.Clear();
            ++currentLayer;
        }

        public void PopLayer()
        {
            var visited = front;
            front = new List<GssNode<T>>(front.Capacity);

            --currentLayer;

            for (int i = 0; i != visited.Count; ++i)
            {
                var node = visited[i];
                if (node.Layer == currentLayer)
                {
                    front.Add(node);
                }

                if (node.Layer >= currentLayer)
                {
                    foreach (var link in node.Links)
                    {
                        if (!visited.Contains(link.LeftNode))
                        {
                            visited.Add(link.LeftNode);
                        }
                    }
                }
            }

            // Following line removes nodes created by the lookahead triggered 
            // reductions.
            //
            // This is line optional since PopLayer() is used in panic-mode 
            // error recovery only and such nodes should not cause incorrect
            // parsing recovery sequences (TODO: prove!). However removing 
            // these nodes should simplify debugging and can slightly improve
            // performance.
            front.RemoveAll(node => node.Stage != 0);
        }

        public GssNode<T> GetFrontNode(State state)
        {
            foreach (var node in front)
            {
                if (node.State == state)
                {
                    return node;
                }
            }

            return default(GssNode<T>);
        }

        private GssNode<T> AddTopmost(State state)
        {
            var result = new GssNode<T>(state, currentLayer, currentStage);
            front.Add(result);
            return result;
        }

        public GssLink<T> Push(GssNode<T> leftNode, int rightState, T label)
        {
            GssNode<T> rightmostNode = GetFrontNode(rightState) ?? AddTopmost(rightState);

            var link = GetLink(rightmostNode, leftNode);
            if (link != null)
            {
                // Side-effect! How to undo it before the error recovery?
                link.AssignLabel(label);
                return null;
            }

            var result = rightmostNode.AddLink(leftNode, label);
            if (result.NextSibling != null)
            {
                rightmostNode.DeterministicDepth = 0;
                UpdateDeterministicDepths();
            }
            else
            {
                rightmostNode.DeterministicDepth = leftNode.DeterministicDepth + 1;
            }

            return result;
        }

        private void UpdateDeterministicDepths()
        {
            int changes;
            do
            {
                changes = 0;
                // Note: Just added link can affect deterministic 
                //       depth only of the topmost state nodes.
                foreach (var topNode in front)
                {
                    int newDepth = topNode.ComputeDeterministicDepth();
                    if (newDepth != topNode.DeterministicDepth)
                    {
                        topNode.DeterministicDepth = newDepth;
                        ++changes;
                    }
                }
            }
            while (changes != 0);
        }

        private static GssLink<T> GetLink(GssNode<T> fromNode, GssNode<T> toNode)
        {
            foreach (var link in fromNode.Links)
            {
                if (link.LeftNode == toNode)
                {
                    return link;
                }
            }

            return default(GssLink<T>);
        }

        
        private static string StateName(GssNode<T> node)
        {
            return string.Format("{0}:{1}", node.State, node.Stage);
        }

        public void WriteGraph(IGraphView view, BnfGrammar grammar, int[] stateToSymbol)
        {
            var allAccessibleByLayer = GetAllNodes().GroupBy(state => state.Layer);

            var layers = Enumerable
                            .Range(0, currentLayer + 1)
                            .Select(i => new List<GssNode<T>>(2))
                            .ToList();

            foreach (var group in allAccessibleByLayer)
            {
                layers[group.Key].AddRange(group);
            }

            view.BeginDigraph("Gss");

            view.SetGraphProperties(rankDir: RankDir.RightToLeft);

            for (int layerIndex = 0; layerIndex != layers.Count; ++layerIndex)
            {
                var layer = layers[layerIndex];
                view.BeginCluster(layerIndex.ToString());

                view.SetGraphProperties(style: Style.Dotted, label: "U" + layerIndex);
                view.SetNodeProperties(shape: Shape.Circle);

                for (int nodeIndex = 0; nodeIndex != layer.Count; ++nodeIndex)
                {
                    view.AddNode(
                        Tuple.Create("s", layerIndex, nodeIndex),
                        label: StateName(layer[nodeIndex]));
                }

                view.EndCluster();
            }

            view.SetNodeProperties(shape: Shape.Rect);

            int linkIndex = 0;
            for (int layerIndex = 0; layerIndex != layers.Count; ++layerIndex)
            {
                var layer = layers[layerIndex];
                for (int nodeIndex = 0; nodeIndex != layer.Count; ++nodeIndex)
                {
                    var from = layer[nodeIndex];
                    Token token = stateToSymbol[from.State];
                    foreach (var link in from.Links)
                    {
                        var to = link.LeftNode;

                        view.AddNode(Tuple.Create("t", linkIndex), grammar.TokenName(token));

                        view.AddEdge(
                            Tuple.Create("s", layerIndex, nodeIndex),
                            Tuple.Create("t", linkIndex)
                            );

                        view.AddEdge(
                            Tuple.Create("t", linkIndex),
                            Tuple.Create("s", to.Layer, layers[to.Layer].IndexOf(to))
                            );

                        ++linkIndex;
                    }
                }
            }

            view.EndDigraph();
        }

        private List<GssNode<T>> GetAllNodes()
        {
            var result = new List<GssNode<T>>(front);

            for (int i = 0; i != result.Count; ++i)
            {
                var item = result[i];
                var f = item.Links.Select(l => l.LeftNode).Except(result);
                result.AddRange(f);
            }

            return result;
        }

        public void BeginEdit()
        {
            currentStage = 1;

            // Save front to history before editing
            if (history != null)
            {
                Debug.Assert(front.Count != 0);

                history.Push(null); // sentinel
                foreach (var node in front)
                {
                    history.Push(node);
                }
            }
        }

        public void EndEdit()
        {
        }

        public void Undo(int inputCount)
        {
            Debug.Assert(history != null);

            GssNode<T> node;
            int i = inputCount;
            while (i-- != 0)
            {
                while ((node = history.Pop()) != null)
                {
                }
            }

            front.Clear();
            while ((node = history.Pop()) != null)
            {
                front.Add(node);
            }

            this.currentLayer -= inputCount;
            this.currentStage = 0;

            history.Clear();
            history.Push(null);

            UpdateDeterministicDepths();
        }

        public Gss<T> CloneWithoutData()
        {
            return new Gss<T>(currentLayer, front.ToList());
        }
    }
}
