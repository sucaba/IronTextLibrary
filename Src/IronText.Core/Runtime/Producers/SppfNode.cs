using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IronText.Diagnostics;
using IronText.Logging;
using IronText.Reflection;

namespace IronText.Runtime
{
    public sealed class SppfNode
    {
        /// <summary>
        /// Positive value for token and negative value for production index
        /// </summary>
        public int        Id       { get; private set; }
        public object     Value    { get; private set; }
        public Loc        Location { get; private set; }
        public SppfNode[] Children { get; private set; }

        public SppfNode   NextAlternative { get; set; }

#if DEBUG
        private static int timestampGen = 0;
        internal readonly int timestamp;
        internal bool isChild; // to ensure that child in not alternative
        internal bool isAlternative; // to ensure that alternative in not child
#endif

        // Leaf
        public SppfNode(int token, object value, Loc location, HLoc hLocation)
        {
            this.Id       = token;
            this.Value    = value;
            this.Location = location;

#if DEBUG
            timestamp = timestampGen++;
#endif
        }

        // Branch
        public SppfNode(int productionIndex, Loc location, SppfNode[] children)
        {
            this.Id       = -productionIndex;
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

        public bool IsTerminal { get { return Id >= 0; } }

        public int GetTokenId(Grammar grammar)
        {
            if (Id < 0)
            {
                return grammar.Productions[-Id].OutcomeToken;
            }

            return Id;
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
            else if (Id > 0)
            {
                visitor.VisitLeaf(Id, Value, Location);
            }
            else
            {
                visitor.VisitBranch(-Id, Children, Location);
            }
        }

        public T Accept<T>(ISppfNodeVisitor<T> visitor, bool ignoreAlternatives)
        {
            if (!ignoreAlternatives && NextAlternative != null)
            {
                return visitor.VisitAlternatives(this);
            }

            if (Id > 0)
            {
                return visitor.VisitLeaf(Id, Value, Location);
            }

            return visitor.VisitBranch(-Id, Children, Location);
        }

        public override string ToString()
        {
            using (var output = new StringWriter())
            {
                WriteIndented(null, output, 0);
                return output.ToString();
            }
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

        public void WriteIndented(Grammar grammar, TextWriter output, int indentLevel)
        {
            const int IndentStep = 2;

            string indent = new string(' ', indentLevel);
            output.WriteLine("{0}{1} = {2}", indent, "ID", Id);
            if (grammar != null)
            {
                if (Id > 0)
                {
                    output.WriteLine("{0}{1} = {2}", indent, "Token", grammar.Symbols[Id].Name);
                }
                else
                {
                    var prod = grammar.Productions[-Id];
                    output.Write("{0}Rule: {1} -> ", indent, prod.Outcome.Name);
                    output.WriteLine(string.Join(" ", from s in prod.Pattern select s.Name));
                }
            }

            output.WriteLine("{0}{1} = {2}", indent, "Loc", Location);
            if (Children != null && Children.Length != 0)
            {
                output.WriteLine(indent + "Children (" + Children.Length + ")");

                int i = 0;
                foreach (var child in Children)
                {
                    output.WriteLine(indent + "#" + i++ + ":");
                    child.WriteIndented(grammar, output, indentLevel + 2 * IndentStep);
                }
            }
        }

        public void WriteGraph(IGraphView graph, Grammar grammar, bool showRules = false)
        {
            var graphWriter = new SppfGraphWriter(grammar, graph, showRules: showRules);
            graphWriter.WriteGraph(this);
        }

        public bool EquivalentTo(SppfNode alt)
        {
            if (this.Id != alt.Id)
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
