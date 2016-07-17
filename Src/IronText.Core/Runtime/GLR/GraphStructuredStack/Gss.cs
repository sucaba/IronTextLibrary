using IronText.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Collections;
using static IronText.Misc.ObjectUtils;

namespace IronText.Runtime
{
    using State = System.Int32;

    sealed class Gss<T>
        : IGraphStructuredStack<T>
        , IUndoable
    {
        private int currentLayer = 0;
        private MutableArray<GssNode<T>> next;
        private MutableArray<GssNode<T>> front;
        private MutableArray<GssNode<T>> prior;
        private readonly CircularStack<GssNode<T>> history;

        public Gss(int stateCount)
        {
            next  = new MutableArray<GssNode<T>>(stateCount);
            front = new MutableArray<GssNode<T>>(stateCount);
            prior = new MutableArray<GssNode<T>>(stateCount);
            history = new CircularStack<GssNode<T>>(2 * stateCount + 2);
            AddTopmost(0, -1);
        }

        private Gss(int currentLayer, MutableArray<GssNode<T>> front)
        {
            this.currentLayer = currentLayer;
            this.front = front;
            this.prior = new MutableArray<GssNode<T>>(front.Capacity);
            this.next  = new MutableArray<GssNode<T>>(front.Capacity);
        }

        public bool HasLayers => currentLayer != 0;

        public ImmutableArray<GssNode<T>> Front => front;

        public ImmutableArray<GssNode<T>> Prior => prior;

        public void PushLayer()
        {
            RotateLeft(ref prior, ref front, ref next);
            next.Clear();

            ++currentLayer;
        }

        public void PopLayer()
        {
            RotateRight(ref prior, ref front, ref next);
            prior.Clear();

            --currentLayer;

            AddDirectPriorLayerNodes(front, prior);
            AddDirectPriorLayerNodes(prior, prior);

            // TODO: Decide if needed: outcome.RemoveAll(node => node.Stage != GssStage.FinalShift);
        }

        private void AddDirectPriorLayerNodes(
            ImmutableArray<GssNode<T>> followingNodes,
            MutableArray<GssNode<T>>   outcome)
        {
            foreach (var followingNode in followingNodes)
                foreach (var backLink in followingNode.BackLink.Alternatives())
                {
                    var priorNode = backLink.PriorNode;
                    if (priorNode.Layer == (currentLayer - 1))
                    {
                        outcome.AddDistinct(priorNode);
                    }
                }
        }

        public GssNode<T> GetFrontNode(State state, int lookahead)
        {
            foreach (var node in front)
            {
                if (node.State == state && (lookahead < 0 || node.Lookahead < 0 || lookahead == node.Lookahead))
                {
                    return node;
                }
            }

            return default(GssNode<T>);
        }

        private GssNode<T> AddTopmost(State state, int lookahead)
        {
            var result = new GssNode<T>(state, currentLayer, GssStage.InitialReduce, lookahead);
            front.Add(result);
            return result;
        }

        private GssNode<T> AddToNextLayer(int state)
        {
            var result = new GssNode<T>(state, currentLayer + 1, GssStage.FinalShift);
            next.Add(result);
            return result;
        }

        public GssBackLink<T> PushShift(
            GssNode<T> priorNode,
            int        toState,
            T          label)
        {
            GssNode<T> toNode = AddToNextLayer(toState);

            var result = toNode.PushLinkAlternative(priorNode, label);

            DeterministicDepthUpdater.OnLinkAdded(front, toNode);

            return result;
        }

        public GssBackLink<T> PushReduced(
            GssNode<T>  priorNode,
            int         toState,
            T           label,
            int         lookahead,
            Func<T,T,T> merge)
        {
            GssNode<T> toNode = GetFrontNode(toState, lookahead)
                                ?? AddTopmost(toState, lookahead);

            var link = toNode.ResolveBackLink(priorNode);
            if (link != null)
            {
                var value = merge(link.Label, label);
                link.AssignLabel(value);
                return null;
            }

            var result = toNode.PushLinkAlternative(priorNode, label);

            DeterministicDepthUpdater.OnLinkAdded(front, toNode);

            return result;
        }

        private static string StateName(GssNode<T> node) =>
            $"{node.State}:{node.Stage}";

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
                    var link = from.BackLink;
                    while (link != null)
                    {
                        var to = link.PriorNode;

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

                        link = link.Alternative;
                    }
                }
            }

            view.EndDigraph();
        }

        // Note: peformance non-critical. Used for diagnostics.
        private List<GssNode<T>> GetAllNodes()
        {
            var result = new List<GssNode<T>>(front);

            for (int i = 0; i != result.Count; ++i)
            {
                var item = result[i];
                var f = item
                    .BackLink
                    .Alternatives()
                    .Select(l => l.PriorNode)
                    .Except(result);
                result.AddRange(f);
            }

            return result;
        }

        public void BeginEdit()
        {
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

            history.Push(null);

            DeterministicDepthUpdater.Update(front);
        }

        public Gss<T> CloneWithoutData()
        {
            return new Gss<T>(currentLayer, front.Clone());
        }
    }
}
