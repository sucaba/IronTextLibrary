using System.Linq;
using System.Text;
using System.Web;
using IronText.Algorithm;
using IronText.Diagnostics;
using IronText.Extensibility;
using IronText.Reflection;
using IronText.Reporting;
using IronText.Runtime;

namespace IronText.Framework
{
    sealed class LrGraph
    {
        private readonly Grammar grammar;
        private readonly IReportData data;

        public LrGraph(IReportData data)
        {
            this.data       = data;
            this.grammar    = data.Grammar;
        }

        public void WriteGv(string path)
        {
            using (var graph = new GvGraphView(path))
            {
                WriteGv(graph);
            }
        }

        public void WriteGv(IGraphView graph)
        {
            graph.BeginDigraph("LRFSM");
            //graph.SetGraphProperties(RankDir.LeftToRight);

            int stateCount = data.ParserAutomata.States.Count;
            int tokenCount = data.Grammar.Symbols.Count;

            foreach (var state in data.ParserAutomata.States)
            {
                graph.AddNode(state.Index, shape: Shape.Mrecord, label: StateToHtml(state.Index));
            }

            foreach (var state in data.ParserAutomata.States)
            {
                foreach (var transition in state.Transitions)
                {
                    foreach (var action in transition.Actions)
                    {
                        if (action.Kind == ParserActionKind.Shift)
                        {
                            graph.AddEdge(state.Index, action.State, grammar.Symbols[transition.Token].Name);
                        }
                    }
                }
            }

            graph.EndDigraph();
        }

        private string StateToHtml(int i)
        {
            var output = new StringBuilder();
            var state = data.ParserAutomata.States[i];
            output.AppendFormat(
                @"
<table border=""0"" cellborder=""0"" cellpadding=""3"" bgcolor=""white"">
<tr> <td bgcolor=""black"" align=""center"" colspan=""2""><font color=""white"">State #{0}</font></td> </tr>",
                StateName(i));
            int limit = 20;

            foreach (var item in state.DotItems)
            {
                if (0 == limit--)
                {
                    output.AppendLine("<tr><td><b>...</b></td></tr>");
                    break;
                }

                var rule = item.Production;
                output.AppendFormat(
                    @"<tr> <td align=""left"" port=""r0"">&#40;{0}&#41; {1} -&gt; ",
                    item.Production.Index,
                    TokenToHtml(rule.OutcomeToken)
                    );

                for (int k = 0; k != rule.PatternTokens.Length; ++k)
                {
                    if (item.Position == k)
                    {
                        output.Append("&bull;");
                    }

                    output.Append(" ").Append(TokenToHtml(rule.PatternTokens[k]));
                }

                if (item.Position == rule.PatternTokens.Length)
                {
                    output.Append("&bull;");
                }

                output.Append(" , ").Append(string.Join(" ", item.LA.Select(TokenToHtml)));
                output.Append("</td> </tr>");
            }

            output.AppendLine("</table>");
            return output.ToString().Trim();
        }

        private string TokenToHtml(int token)
        {
            var result = HttpUtility.HtmlEncode(grammar.Symbols[token].Name);
            result = result.Replace("{", "&#123;");
            result = result.Replace("}", "&#125;");
            return result;
        }

        private static string StateName(int correct)
        {
            return correct.ToString();
            /*
            switch (correct)
            {
                case 0: return "0";
                case 1: return "2";
                case 2: return "1";
                case 3: return "3";
                case 4: return "5";
                case 5: return "4";
                case 6: return "7";
                case 7: return "6";
            }

            throw new InvalidOperationException();
            */
        }
    }
}
