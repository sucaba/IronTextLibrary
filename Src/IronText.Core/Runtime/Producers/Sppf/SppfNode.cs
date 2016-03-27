using IronText.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace IronText.Runtime
{
    public sealed class SppfNode
    {
        internal int id;

        /// <summary>
        /// Positive value for token and negative value for production index
        /// </summary>
        public string     Text     { get; private set; }
        public HLoc       Location { get; private set; }
        public SppfNode[] Children { get; private set; }

        public SppfNode   NextAlternative { get; set; }

#if DEBUG
        private static int timestampGen = 0;
        internal readonly int timestamp;
        internal bool isChild; // to ensure that child in not alternative
        internal bool isAlternative; // to ensure that alternative in not child
#endif

        // Leaf
        public SppfNode(int matcherIndex, string text, HLoc location)
        {
            this.id       = matcherIndex;
            this.Text     = text;
            this.Location = location;

#if DEBUG
            timestamp = timestampGen++;
#endif
        }

        // Branch
        public SppfNode(int productionIndex, HLoc location, SppfNode[] children)
        {
            this.id       = -productionIndex;
            this.Location = location;
            this.Children = children;

#if DEBUG
            timestamp = timestampGen++;
            foreach (var child in children)
            {
                Debug.Assert(!child.isAlternative);
                child.isChild = true;
            }
#endif
        }

        public bool IsTerminal { get { return id >= 0; } }

        public int ProductionIndex
        {
            get
            {
                if (IsTerminal)
                {
                    throw new InvalidOperationException();
                }

                return -id;
            }
        }

        public int MatcherIndex
        {
            get
            {
                if (!IsTerminal)
                {
                    throw new InvalidOperationException();
                }

                return id;
            }
        }

        internal SppfNode AddAlternative(SppfNode other)
        {
#if DEBUG
            Debug.Assert(!other.isChild);
#endif

            if (other == (object)this)
            {
                return this;
            }

#if DEBUG
            other.isAlternative = true;
#endif
            other.NextAlternative = NextAlternative;
            NextAlternative = other;
            return this;
        }

        public void Accept(ISppfNodeVisitor visitor, bool ignoreAlternatives)
        {
            if (!ignoreAlternatives && NextAlternative != null)
            {
                visitor.VisitAlternatives(this);
            }
            else if (IsTerminal)
            {
                visitor.VisitLeaf(MatcherIndex, Text, Location);
            }
            else
            {
                visitor.VisitBranch(ProductionIndex, Children, Location);
            }
        }

        public T Accept<T>(ISppfNodeVisitor<T> visitor, bool ignoreAlternatives)
        {
            if (!ignoreAlternatives && NextAlternative != null)
            {
                return visitor.VisitAlternatives(this);
            }

            if (IsTerminal)
            {
                return visitor.VisitLeaf(MatcherIndex, Text, Location);
            }

            return visitor.VisitBranch(ProductionIndex, Children, Location);
        }

        public IEnumerable<SppfNode> Flatten()
        {
            var result = new List<SppfNode>();
            Flatten(result);
            return result;
        }

        private void Flatten(List<SppfNode> all)
        {
            if (all.Contains(this))
            {
                return;
            }

            all.Add(this);
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    child.Flatten(all);
                }
            }

            if (NextAlternative != null)
            {
                NextAlternative.Flatten(all);
            }
        }

        public bool EquivalentTo(SppfNode alt)
        {
            if (this.id != alt.id)
            {
                return false;
            }

            if (this == (object)alt)
            {
                return true;
            }
            
            if (Children == null)
            {
                return alt.Children == null;
            }

            int len = Children.Length;

            if (alt.Children == null || len != alt.Children.Length)
            {
                return false;
            }

            for (int i = 0; i != len; ++i)
            {
                if (Children[i] != (object)alt.Children[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
