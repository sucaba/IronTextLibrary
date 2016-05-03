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
        public readonly GssLink<T>[]      Links;

        public int Size => Production.InputLength;

        public GssReducePath(
            GssNode<T>        leftNode,
            GssLink<T>[]      links,
            RuntimeProduction production)
        {
            this.LeftNode   = leftNode;
            this.Links      = links;
            this.Production = production;
        }

        public static void ForEach(
            RuntimeProduction production,
            GssNode<T>        rightNode,
            GssLink<T>        rightLink,
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
                        new GssLink<T>[tail],
                        production));
            }
            else if (size <= rightNode.DeterministicDepth)
            {
                var links = new GssLink<T>[fullSize];

                GssNode<T> node = rightNode;
                int i = size;
                while (i-- != 0)
                {
                    var link = node.FirstLink;
                    links[i] = link;

                    node = link.LeftNode;
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
                var front      = new List<GssLink<T>>(2);
                var frontPaths = new List<GssLink<T>[]>(rightNode.LinkCount);

                var frontLink = rightNode.FirstLink;
                while (frontLink != null)
                {
                    front.Add(frontLink);
                    frontPaths.Add(new GssLink<T>[fullSize]);

                    frontLink = frontLink.NextLink;
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
                            front[j] = currLink.LeftNode.FirstLink;
                            var link = front[j].NextLink;

                            while (link != null)
                            {
                                front.Add(link);
                                var newPathLinks = (GssLink<T>[])frontPaths[j].Clone();
                                frontPaths.Add(newPathLinks);

                                link = link.NextLink;
                            }
                        }
                    }
                }

                int count = front.Count;
                for (int i = 0; i != count; ++i)
                {
                    action1( new GssReducePath<T>(front[i].LeftNode, frontPaths[i], production));
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
                result = Links[index].LeftNode.State;
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
