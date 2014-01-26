using System.Collections.Generic;
using System.Linq;
using IronText.Diagnostics;
using IronText.Reflection;

namespace IronText.Framework
{
    sealed class SppfGraphWriter : ISppfNodeVisitor
    {
        private readonly EbnfGrammar grammar;
        private readonly IGraphView graph;
        private readonly bool showRules;
        private HashSet<SppfNode> visited;
        private Stack<SppfNode> front;
        private object currentNodeIdentity;
        private SppfNode currentNode;

        public SppfGraphWriter(EbnfGrammar grammar, IGraphView graph, bool showRules)
        {
            this.grammar   = grammar;
            this.graph     = graph;
            this.visited   = new HashSet<SppfNode>();
            this.front     = new Stack<SppfNode>();
            this.showRules = showRules;
        }

        public void WriteGraph(SppfNode rootNode)
        {
            front.Push(rootNode);

            graph.BeginDigraph("SPPF");
            graph.SetGraphProperties(ordering: Ordering.Out);

            while (front.Count != 0)
            {
                var node = front.Pop();
                WriteNode(node);
            }

            graph.EndDigraph();
        }

        private bool EnqueueNode(SppfNode node)
        {
            bool result = !visited.Contains(node);
            if (result)
            {
                visited.Add(node);
                front.Push(node);
            }

            return result;
        }

        private void WriteNode(SppfNode node)
        {
            this.currentNodeIdentity = node;
            this.currentNode = node;
            node.Accept(this, false);
        }

        void ISppfNodeVisitor.VisitLeaf(int token, object value, Loc location)
        {
            string label = grammar.Symbols[token].Name + " pos: " + location.Position 
#if DEBUG
                + " t: " + currentNode.timestamp
#endif
                ;
            graph.AddNode(currentNodeIdentity, label: label, shape: Shape.Circle);
        }

        void ISppfNodeVisitor.VisitBranch(int ruleIndex, SppfNode[] children, Loc location)
        {
            var rule = grammar.Productions[ruleIndex];

            var tokenName = grammar.Symbols[rule.OutcomeToken].Name;

            string label;
            if (showRules)
            {
                label = string.Format(
                        "{0} -> {1}" 
#if DEBUG
                        + " t: " + currentNode.timestamp
#endif
                        ,
                        tokenName,
                        string.Join(" ", from s in rule.Pattern select s.Name));
            }
            else
            {
                label = tokenName 
#if DEBUG
                    + " t: " + currentNode.timestamp
#endif
                    ;
            }

            graph.AddNode(currentNodeIdentity, label: label, shape: Shape.Ellipse);

            if (children != null && children.Length != 0)
            {
                int i = 0;
                foreach (var child in children)
                {
                    graph.AddEdge(currentNodeIdentity, child, label: "child" + ++i);
                    EnqueueNode(child);
                }
            }
        }

        void ISppfNodeVisitor.VisitAlternatives(SppfNode alternatives)
        {
            var token = alternatives.GetTokenId(grammar);
            var symbol = grammar.Symbols[token];
            string label = symbol.Name + " pos: " + alternatives.Location.Position 
#if DEBUG
                + " t: " + currentNode.timestamp
#endif
                ;

            object parentIdentity = currentNodeIdentity;
            graph.AddNode(parentIdentity, label: label, shape: Shape.Rect);

            var familyNode = alternatives;
            var altIdentity = alternatives.Children ?? new object();

            int altIndex = 0;
            do
            {
                graph.AddEdge(
                    parentIdentity,
                    altIdentity,
                    style: Style.Dotted,
                    label: "alt#" + ++altIndex);

                this.currentNodeIdentity = altIdentity;
                this.currentNode = familyNode;
                visited.Add(familyNode);
                familyNode.Accept(this, ignoreAlternatives: true);

                familyNode = familyNode.NextAlternative;
                altIdentity = familyNode;
            }
            while (familyNode != null);
        }

        private string GetLabel(SppfNode node)
        {
            if (node.Id > 0)
            {
                return grammar.Symbols[node.Id].Name + " pos: " + node.Location.Position;
            }

            if (node.NextAlternative != null)
            {
                return grammar.Symbols[node.GetTokenId(grammar)].Name;
            }

            var production = grammar.Productions[-node.Id];
            if (showRules)
            {
                return string.Format(
                        "{0} -> {1}",
                        production.Outcome.Name,
                        string.Join(" ", from s in production.Pattern select s.Name));
            }

            return production.Outcome.Name;
        }
    }
}
