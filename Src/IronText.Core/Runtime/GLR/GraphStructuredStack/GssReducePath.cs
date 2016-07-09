using System;
using System.Collections.Generic;

namespace IronText.Runtime
{
    sealed class GssReducePath<T> : IStackLookback<T>
    {
        public readonly RuntimeProduction Production;
        public readonly GssNode<T>        LeftNode;
    
        /// <summary>
        /// Left-to-right reduction path labels
        /// </summary>
        public readonly GssBackLink<T>[]      Links;

        public int Size => Production.InputLength;

        public GssReducePath(
            GssNode<T>        leftNode,
            GssBackLink<T>[]      links,
            RuntimeProduction production)
        {
            this.LeftNode   = leftNode;
            this.Links      = links;
            this.Production = production;
        }

        public static void ForEach(
            RuntimeProduction production,
            GssNode<T>        rightNode,
            GssBackLink<T>        rightLink,
            Action<GssReducePath<T>> action)
        {
            int fullSize = production.InputLength;
            int tail = (fullSize != 0 && rightLink != null) ? 1 : 0;
            int size = fullSize - tail;

            Action<GssReducePath<T>> action1;
            if (tail == 0)
            {
                action1 = action;
            }
            else 
            {
                action1 = path =>
                    {
                        path.Links[size] = rightLink;
                        action(path);
                    };
            }

            if (size == 0)
            {
                action1(
                    new GssReducePath<T>(
                        rightNode,
                        new GssBackLink<T>[tail],
                        production));
            }
            else if (size <= rightNode.DeterministicDepth)
            {
                var links = new GssBackLink<T>[fullSize];

                GssNode<T> node = rightNode;
                int i = size;
                while (i-- != 0)
                {
                    var link = node.BackLink;
                    links[i] = link;

                    node = link.PriorNode;
                }

                action1(
                    new GssReducePath<T>(
                        node,
                        links,
                        production));
            }
            else
            {
                // TODO: Get rid of front. 'front link' is frontPaths[j][k]
                var front      = new List<GssBackLink<T>>(2);
                var frontPaths = new List<GssBackLink<T>[]>(rightNode.LinkCount);

                var frontLink = rightNode.BackLink;
                while (frontLink != null)
                {
                    front.Add(frontLink);
                    frontPaths.Add(new GssBackLink<T>[fullSize]);

                    frontLink = frontLink.Alternative;
                }

                int k = size;
                while (0 != k--)
                {
                    int frontCount = front.Count;
                    for (int j = 0; j != frontCount; ++j)
                    {
                        var currLink = front[j];
                        frontPaths[j][k] = currLink;

                        if (k != 0)
                        {
                            front[j] = currLink.PriorNode.BackLink;
                            var link = front[j].Alternative;

                            while (link != null)
                            {
                                front.Add(link);
                                var newPathLinks = (GssBackLink<T>[])frontPaths[j].Clone();
                                frontPaths.Add(newPathLinks);

                                link = link.Alternative;
                            }
                        }
                    }
                }

                int count = front.Count;
                for (int i = 0; i != count; ++i)
                {
                    action1( new GssReducePath<T>(front[i].PriorNode, frontPaths[i], production));
                }
            }
        }

        int IStackLookback<T>.GetParentState()
        {
            return ((IStackLookback<T>)this).GetState(Links.Length);
        }

        int IStackLookback<T>.GetState(int backOffset)
        {
            int result;

            int index = Links.Length - backOffset;
            if (index >= 0)
            {
                result = Links[index].PriorNode.State;
            }
            else
            {
                result = ((IStackLookback<T>)LeftNode).GetState(-index);
            }

            return result;
        }

        T IStackLookback<T>.GetNodeAt(int backOffset)
        {
            T result;
            int index = Links.Length - backOffset;
            if (index >= 0)
            {
                result = Links[index].Label;
            }
            else
            {
                result = ((IStackLookback<T>)LeftNode).GetNodeAt(-index);
            }

            return result;
        }
    }
}
