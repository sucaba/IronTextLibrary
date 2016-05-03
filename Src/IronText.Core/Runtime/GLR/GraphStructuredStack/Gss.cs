using IronText.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IronText.Runtime
{
    using State = System.Int32;

    sealed class Gss<T>
        : IGss<T>
        , IUndoable
    {
        private int currentLayer = 0;
        private byte currentStage = 0;
        private GssNode<T>[] front;
        private readonly CircularStack<GssNode<T>> history;

        public Gss(int stateCount)
        {
            front = new GssNode<T>[stateCount];
            Count = 0;
            history = new CircularStack<GssNode<T>>(2 * stateCount + 2);
            AddTopmost(0);
        }

        private Gss(int currentLayer, GssNode<T>[] front, int frontCount)
        {
            this.currentLayer = currentLayer;
            this.front = front;
            this.Count = frontCount;
        }

        public int Count { get; private set; }

        public int CurrentLayer { get { return currentLayer; } }

        public GssNode<T>[] FrontArray { get { return front; } }

        public bool IsEmpty { get { return Count == 0; } }

        public void PushLayer()
        {
            currentStage = 0;
            Count = 0;
            ++currentLayer;
        }

        public void PopLayer()
        {
            var visited = front;
            int visitedCount = Count;
            this.front = new GssNode<T>[front.Length];
            int frontCount = 0;

            --currentLayer;

            for (int i = 0; i != visitedCount; ++i)
            {
                var node = visited[i];
                if (node.Layer == currentLayer)
                {
                    front[frontCount++] = node;
                }

                if (node.Layer >= currentLayer)
                {
                    var link = node.FirstLink;
                    while (link != null)
                    {
                        if (!visited.Contains(link.LeftNode))
                        {
                            visited[visitedCount++] = link.LeftNode;
                        }

                        link = link.NextLink;
                    }
                }
            }

            this.Count = frontCount;

            // Following line removes nodes created by the lookahead triggered 
            // reductions.
            //
            // This line optional since PopLayer() is used in the panic-mode 
            // error recovery only and such nodes should not cause incorrect
            // parsing recovery sequences (TODO: prove!). However removing 
            // these nodes should simplify debugging and can slightly improve
            // performance.
            ArrayRemoveAll(front, frontCount, node => node.Stage != 0);
        }

        public static void ArrayRemoveAll<U>(U[] array, int count, Predicate<U> shouldRemove)
        {
            int srcIndex = 0, destIndex = 0;
            while (srcIndex != count)
            {
                if (!shouldRemove(array[srcIndex]))
                {
                    array[destIndex++] = array[srcIndex];
                }

                ++srcIndex;
            }
        }

        public GssNode<T> GetFrontNode(State state, int lookahead = -1)
        {
            for (int i = 0; i != Count; ++i)
            {
                var node = front[i];
                if (node.State == state && (lookahead < 0 || node.Lookahead < 0 || lookahead == node.Lookahead))
                {
                    return node;
                }
            }

            return default(GssNode<T>);
        }

        private GssNode<T> AddTopmost(State state, int lookahead = -1)
        {
            var result = new GssNode<T>(state, currentLayer, currentStage, lookahead);
            front[Count++] = result;
            return result;
        }

        public GssLink<T> Push(GssNode<T> leftNode, int rightState, T label, int lookahead = -1)
        {
            GssNode<T> rightmostNode = GetFrontNode(rightState, lookahead)
                                     ?? AddTopmost(rightState, lookahead);

            var link = GetLink(rightmostNode, leftNode);
            if (link != null)
            {
                // Side-effect! How to undo it before the error recovery?
                link.AssignLabel(label);
                return null;
            }

            var result = rightmostNode.AddLink(leftNode, label);
            if (result.NextLink != null)
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
                int count = this.Count;
                for (int i = 0; i != count; ++i)
                {
                    var topNode = front[i];

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
            var link = fromNode.FirstLink;
            while (link != null)
            {
                if (link.LeftNode == toNode)
                {
                    return link;
                }

                link = link.NextLink;
            }

            return default(GssLink<T>);
        }

        
        private static string StateName(GssNode<T> node)
        {
            return string.Format("{0}:{1}", node.State, node.Stage);
        }

        public void WriteGraph(IGraphView view, RuntimeGrammar grammar, int[] stateToSymbol)
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
                    if (from.State < 0)
                    {
                        continue;
                    }

                    int token = stateToSymbol[from.State];
                    var link = from.FirstLink;
                    while (link != null)
                    {
                        var to = link.LeftNode;

                        view.AddNode(Tuple.Create("t", linkIndex), grammar.SymbolName(token));

                        view.AddEdge(
                            Tuple.Create("s", layerIndex, nodeIndex),
                            Tuple.Create("t", linkIndex)
                            );

                        view.AddEdge(
                            Tuple.Create("t", linkIndex),
                            Tuple.Create("s", to.Layer, layers[to.Layer].IndexOf(to))
                            );

                        ++linkIndex;

                        link = link.NextLink;
                    }
                }
            }

            view.EndDigraph();
        }

        // Note: peformance non-critical
        private List<GssNode<T>> GetAllNodes()
        {
            var result = new List<GssNode<T>>(front.Take(Count));

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
                Debug.Assert(Count != 0);

                history.Push(null); // sentinel

                int count = this.Count;
                for (int i = 0; i != count; ++i)
                {
                    history.Push(front[i]);
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

            Count = 0;
            while ((node = history.Pop()) != null)
            {
                front[Count++] = node;
            }

            this.currentLayer -= inputCount;
            this.currentStage = 0;

            history.Push(null);

            UpdateDeterministicDepths();
        }

        public Gss<T> CloneWithoutData()
        {
            var newFront = new GssNode<T>[front.Length];
            front.CopyTo(newFront, 0);
            return new Gss<T>(currentLayer, newFront, Count);
        }
    }
}
