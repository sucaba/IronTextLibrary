using System.Collections.Generic;
using IronText.Framework;

namespace IronText.Runtime
{
    sealed class GssReducePath<T>
    {
        public readonly GssNode<T> LeftNode;
        public readonly GssLink<T>[] Links;    // Left-to-right reduction path labels
        public int                 Size;
        public BnfRule             Rule;

        public GssReducePath(GssNode<T> leftNode, GssLink<T>[] links)
        {
            this.LeftNode = leftNode;
            this.Links = links;
        }

        public static GssReducePath<T>[] GetAll(
            GssNode<T> rightNode,
            int        size,
            int        tail,
            BnfRule    rule,
            GssLink<T> rightLink)
        {
            var result = GetAll(rightNode, size, tail);
            int i = result.Length;
            while (i-- != 0)
            {
                result[i].Rule = rule;
                result[i].Size = size + tail;

                if (tail != 0)
                {
                    result[i].Links[size] = rightLink;
                }
            }

            return result;
        }

        private static GssReducePath<T>[] GetAll(GssNode<T> rightNode, int size, int tail)
        {
            GssReducePath<T>[] result;

            if (size == 0)
            {
                result = new [] { new GssReducePath<T>(rightNode, new GssLink<T>[tail]) };
            }
            else if (size <= rightNode.DeterministicDepth)
            {
                var links = new GssLink<T>[size + tail];

                GssNode<T> node = rightNode;
                int i = size;
                while (i-- != 0)
                {
                    var link = node.PrevLink;
                    links[i] = link;

                    node = link.LeftNode;
                }

                result = new [] { new GssReducePath<T>(node, links) };
            }
            else
            {
                // TODO: Get rid of front. 'front link' is frontPaths[j][k]
                var front      = new List<GssLink<T>>();
                var frontPaths = new List<GssLink<T>[]>(rightNode.LinkCount);

                foreach (var frontLink in rightNode.Links)
                {
                    front.Add(frontLink);
                    frontPaths.Add(new GssLink<T>[size + tail]);
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
                result = new GssReducePath<T>[count];
                for (int i = 0; i != count; ++i)
                {
                    result[i] = new GssReducePath<T>(front[i].LeftNode, frontPaths[i]);
                }
            }

            return result;
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
