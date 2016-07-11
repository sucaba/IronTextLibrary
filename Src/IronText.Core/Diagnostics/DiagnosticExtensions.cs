using IronText.Reflection;
using IronText.Runtime;
using System.IO;
using System.Linq;

namespace IronText.Diagnostics
{
    public static class DiagnosticExtensions
    {
        public static void WriteGraph(this SppfNode node, IGraphView graph, Grammar grammar, bool showRules = false)
        {
            var graphWriter = new SppfGraphWriter(grammar, graph, showRules: showRules);
            graphWriter.WriteGraph(node);
        }

        public static string Describe(this SppfNode node)
        {
            using (var output = new StringWriter())
            {
                node.WriteIndented(null, output, 0);
                return output.ToString();
            }
        }

        public static int GetTokenId(this SppfNode node, Grammar grammar)
        {
            if (node.IsTerminal)
            {
                // TODO: Lexical ambiguities. Return index of resolved symbol.
                return ((Symbol)grammar.Matchers[node.MatcherIndex].Outcome).Index;
            }

            return grammar.Productions[-node.id].Outcome.Index;
        }

        public static void WriteIndented(this SppfNode node, Grammar grammar, TextWriter output, int indentLevel)
        {
            const int IndentStep = 2;

            string indent = new string(' ', indentLevel);
            if (grammar != null)
            {
                if (node.IsTerminal)
                {
                    output.WriteLine("{0}{1} = {2}", indent, "ID", node.MatcherIndex);
                    var matcher = grammar.Matchers[node.MatcherIndex];
                    output.WriteLine("{0}{1} = {2}", indent, "Token", matcher.Outcome.Name);
                }
                else
                {
                    output.WriteLine("{0}{1} = {2}", indent, "ID", node.ProductionIndex);
                    var prod = grammar.Productions[node.ProductionIndex];
                    output.Write("{0}Rule: {1} -> ", indent, prod.Outcome.Name);
                    output.WriteLine(string.Join(" ", prod.Input.Select(s => s.Name)));
                }
            }

            output.WriteLine("{0}{1} = {2}", indent, "Loc", node.Location);
            if (node.Children != null && node.Children.Length != 0)
            {
                output.WriteLine(indent + "Children (" + node.Children.Length + ")");

                int i = 0;
                foreach (var child in node.Children)
                {
                    output.WriteLine(indent + "#" + i++ + ":");
                    child.WriteIndented(grammar, output, indentLevel + 2 * IndentStep);
                }
            }
        }
    }
}
