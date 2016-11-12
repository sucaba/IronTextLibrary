using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Runtime;
using IronText.Automata.DotNfa;

namespace IronText.Automata.Lalr1
{
    partial class Lalr1DfaProvider
	{
        private StringBuilder DescribeState(MutableDotItemSet state, StringBuilder output, string indent)
        {
            foreach (var item in state)
            {
                output.Append(indent);
                DescribeItem(item, output).AppendLine();
            }

            return output;
        }

        private StringBuilder DescribeItem(DotItem item, StringBuilder output, bool showLookaheads = true)
        {
            var production = grammar.GetProduction(item.ProductionId);

            int start = output.Length;
            output.Append(grammar.GetTokenName(item.Outcome)).Append(" ->");
            for (int i = 0; i != production.InputLength; ++i)
            {
                if (item.Position == i)
                {
                    output.Append(" •");
                }

                output.Append(" ").Append(grammar.GetTokenName(production.Input[i]));
            }

            if (item.IsReduce)
            {
                output.Append(" •");
            }

            int lineLength = output.Length - start;
            const int AlignTo = 20;
            if (lineLength < AlignTo)
            {
                output.Append(new string(' ', AlignTo - lineLength));
            }

            if (showLookaheads)
            {
                output.Append("  |LA = {").Append(string.Join(", ", item.LA.Select(grammar.GetTokenName))).Append("}");
            }

            return output;
        }

        [Conditional("DEBUG")]
        private void PrintPropogation(
            List<DotState>    lr0states,
            DotPoint sourceItemId,
            DotPoint destinationItemId)
        {
            var sourceItem = sourceItemId.Item;
            var destItem = destinationItemId.Item;
            var lookaheads = sourceItem.LA.Except(destItem.LA).Select(grammar.GetTokenName);

            var output = new StringBuilder();
            output.Append(">>> I").Append(sourceItemId.State).Append(": ");
            DescribeItem(sourceItem, output, showLookaheads:false);
            output.Append(" ==[").Append(string.Join(" ", lookaheads)).Append("]==>    ");
            output.Append("I").Append(destinationItemId.State).Append(": ");
            DescribeItem(destItem, output, showLookaheads:true);
            Debug.WriteLine(output);
        }

        [Conditional("DEBUG")]
        private void PrintPropogationTable(List<DotState> lr0states, Dictionary<DotPoint, List<DotPoint>> propogation)
        {
            var output = new StringBuilder();
            output.AppendLine(new string('-', 50));
            output.AppendLine("Propogations:");

            foreach (var pair in propogation)
            {
                output.Append("I").Append(pair.Key.State).Append(": ");
                DescribeItem(pair.Key.Item, output).AppendLine(" -> ");
                foreach (var destId in pair.Value)
                {
                    output.Append("  I").Append(destId.State).Append(": ");
                    DescribeItem(destId.Item, output).AppendLine();
                }
            }
            Debug.Write(output);
        }

        [Conditional("DEBUG")]
        private void PrintStates(string title, List<MutableDotItemSet> states)
        {
            var output = new StringBuilder();

            output.Append(new string('-', 50)).AppendLine(title);
            for (int i = 0; i != states.Count; ++i)
            {
                output.AppendLine("  State I" + i);
                DescribeState(states[i], output, "    ");
            }
            Debug.Write(output);
        }
	}
}
