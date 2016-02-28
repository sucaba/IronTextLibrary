using System;
using System.Collections.Generic;

namespace IronText.Runtime
{
    sealed class GssReducePath<T>
    {
        public readonly GssNode<T>   LeftNode;
        public readonly GssLink<T>[] Links;    // Left-to-right reduction path labels
        public readonly int          Size;
        public readonly RuntimeProduction   Production;

        public GssReducePath(GssNode<T> leftNode, GssLink<T>[] links, RuntimeProduction prod, int size)
        {
            this.LeftNode   = leftNode;
            this.Links      = links;
            this.Production = prod;
            this.Size       = size;
        }

        public IStackLookback<T> GetStackLookback()
        {
            return LeftNode;
        } 

        public static void GetAll(
            GssNode<T> rightNode,
            int        size,
            int        tail,
            RuntimeProduction prod,
            GssLink<T> rightLink,
            Action<GssReducePath<T>> action0)
        {
            Action<GssReducePath<T>> action;
            if (tail == 0)
            {
                action = action0;
            }
            else 
            {
                action = path =>
                    {
                        path.Links[size] = rightLink;
                        action0(path);
                    };
            }

            int fullSize = size + tail;

            if (size == 0)
            {
                action( new GssReducePath<T>(rightNode, new GssLink<T>[tail], prod, fullSize) );
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

                action( new GssReducePath<T>(node, links, prod, fullSize) );
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
                    action( new GssReducePath<T>(front[i].LeftNode, frontPaths[i], prod, fullSize) );
                }
            }
        }

        public void CopyDataTo(T[] buffer)
        {
            int count = 0;

            foreach (var link in this.Links)
            {
                buffer[count++] = link.Label;
            }
        }
    }
}
