using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronText.Runtime.RIGLR.GraphStructuredStack;
using IronText.Collections;
using System.Diagnostics;

namespace IronText.Runtime
{
    class GenericParserDiagnostics<T>
    {
        [Conditional("DIAGNOSTICS")]
        public void StartInput(MessageData alternateInput)
        {
            Debug.WriteLine($"------- input Token={alternateInput.Token} text={alternateInput.Text}");
        }

        [Conditional("DIAGNOSTICS")]
        public void SetCurrentProcess(Process<T> process)
        {
            Debug.WriteLine($"Process {Describe(process)}:");
        }

        [Conditional("DIAGNOSTICS")]
        public void Action(ParserInstruction instruction)
        {
            Debug.WriteLine($"  -> {instruction}");
        }

        [Conditional("DIAGNOSTICS")]
        public void GotoPos(int pos)
        {
            Debug.WriteLine($"  ...  -> {pos}");
        }

        [Conditional("DIAGNOSTICS")]
        public void ProcessReduction(Reduction<T> reduction)
        {
            SetCurrentProcess(reduction.Process);
            Debug.WriteLine($"-> process reduce P{reduction.Production.Index}");
        }

        private static string Describe(Process<T> process)
        {
            var output = new StringBuilder();
            Describe(process.InstructionState, new[] { process.CallStack }, output, new List<int>());
            return output.ToString();
        }

        private static void Describe(
            int                         state,
            IEnumerable<CallStackNode<T>> callStacks,
            StringBuilder               output,
            List<int>                   visited)
        {
            output.Append("{ S").Append(state).Append(' ');

            if (!visited.Contains(state))
            {
                output.Append(": ");
                visited.Add(state);

                int count = 0;
                foreach (var stack in callStacks)
                {
                    if (count++ != 0)
                    {
                        output.Append(" |");
                    }

                    if (stack == null)
                    {
                        output.Append("{}");
                    }
                    else
                    {
                        Describe(
                            stack.State,
                            stack.BackLink.AllAlternatives().Select(l => l.Prior),
                            output,
                            visited);
                    }
                }

                visited.Remove(state);
            }

            output.Append("}");
        }
    }
}
