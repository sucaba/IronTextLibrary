using System;
using System.Collections.Generic;
using IronText.Algorithm;

namespace IronText.Framework
{
    sealed class GssReducePath<T>
    {
        public readonly GssNode<T> LeftNode;
        public readonly GssLink<T>[] Links;    // Left-to-right reduction path labels
        public readonly int          Size;
        public readonly BnfRule      Rule;

        public GssReducePath(GssNode<T> leftNode, GssLink<T>[] links, BnfRule rule, int size)
        {
            this.LeftNode = leftNode;
            this.Links = links;
            this.Rule = rule;
            this.Size = size;
        }

        public static void GetAll(
            GssNode<T> rightNode,
            int        size,
            int        tail,
            BnfRule    rule,
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
                action( new GssReducePath<T>(rightNode, new GssLink<T>[tail], rule, fullSize) );
            }
            else if (size <= rightNode.DeterministicDepth)
            {
                var links = new GssLink<T>[fullSize];

                GssNode<T> node = rightNode;
                int i = size;
                while (i-- != 0)
                {
                    var link = node.PrevLink;
                    links[i] = link;

                    node = link.LeftNode;
                }

                action( new GssReducePath<T>(node, links, rule, fullSize) );
            }
            else
            {
                // TODO: Get rid of front. 'front link' is frontPaths[j][k]
                var front      = new Buffer<GssLink<T>>(2);
                var frontPaths = new Buffer<GssLink<T>[]>(rightNode.LinkCount);

                foreach (var frontLink in rightNode.Links)
                {
                    front.Add(frontLink);
                    frontPaths.Add(new GssLink<T>[fullSize]);
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
                            front[j] = currLink.LeftNode.PrevLink;
                            var link = front[j].NextSibling;

                            while (link != null)
                            {
                                front.Add(link);
                                var newPathLinks = (GssLink<T>[])frontPaths[j].Clone();
                                frontPaths.Add(newPathLinks);

                                link = link.NextSibling;
                            }
                        }
                    }
                }

                int count = front.Count;
                for (int i = 0; i != count; ++i)
                {
                    action( new GssReducePath<T>(front[i].LeftNode, frontPaths[i], rule, fullSize) );
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
