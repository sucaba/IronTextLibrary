using System.Linq;
using System.Text;
using IronText.Diagnostics;

namespace IronText.Reporting.Rendering
{
    class ScannerAutomataGraphRenderer : IScannerAutomataRenderer
    {
        private readonly IGraphView view;

        public ScannerAutomataGraphRenderer(IGraphView view)
        {
            this.view = view;
        }

        public void DescribeGraph(IScannerAutomata automata)
        {
            view.BeginDigraph("tdfa");

            view.SetGraphProperties(rankDir: RankDir.LeftToRight);
            foreach (var S in automata.States)
            {
                GraphColor color = S.IsNewline ? GraphColor.Green : GraphColor.Default;
                if (S.IsAccepting)
                {
                    view.AddNode(S.Index, GetStateName(S), style: Style.Bold, color: color);
                }
                else
                {
                    view.AddNode(S.Index, GetStateName(S), color: color);
                }
            }

            foreach (var S in automata.States)
            {
                foreach (var t in S.Transitions)
                {
                    var charSet = string.Join(", ", t.CharRanges);
                    view.AddEdge(S.Index, t.Destination.Index, charSet);
                }

                if (S.TunnelState != null)
                {
                    view.AddEdge(S.Index, GetStateName(S.TunnelState), style: Style.Dotted);
                }
            }

            view.EndDigraph();
        }

        private static string GetStateName(IScannerState S)
        {
            var output = new StringBuilder();
            output.Append(S.Index);
            if (S.Actions.Count != 0)
            {
                output.Append(" [");
                output.Append(string.Join(",", S.Actions));
                output.Append("]");
            }

            return output.ToString();
        }
    }
}
