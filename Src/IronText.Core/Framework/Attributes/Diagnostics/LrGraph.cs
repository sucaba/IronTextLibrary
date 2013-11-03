﻿using System.Linq;
using System.Text;
using System.Web;
using IronText.Algorithm;
using IronText.Diagnostics;
using IronText.Extensibility;

namespace IronText.Framework
{
    sealed class LrGraph
    {
        private readonly BnfGrammar     grammar;
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
            int tokenCount = data.TokenCount;

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
                            graph.AddEdge(state.Index, action.State, grammar.TokenName(transition.Token));
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

                var rule = item.Rule;
                output.AppendFormat(
                    @"<tr> <td align=""left"" port=""r0"">&#40;{0}&#41; {1} -&gt; ",
                    item.Rule.Id,
                    TokenToHtml(rule.Left)
                    );

                for (int k = 0; k != rule.Parts.Length; ++k)
                {
                    if (item.Position == k)
                    {
                        output.Append("&bull;");
                    }

                    output.Append(" ").Append(TokenToHtml(rule.Parts[k]));
                }

                if (item.Position == rule.Parts.Length)
                {
                    output.Append("&bull;");
                }

                output.Append(" , ").Append(string.Join(" ", item.Lookaheads.Select(TokenToHtml)));
                output.Append("</td> </tr>");
            }

            output.AppendLine("</table>");
            return output.ToString().Trim();
        }

        private string TokenToHtml(int token)
        {
            var result = HttpUtility.HtmlEncode(grammar.TokenName(token));
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
