using System.IO;
using IronText.Algorithm;

namespace IronText.Lib.RegularAst
{
    class AstNodeWriter : IAstNodeVisitor<TextWriter>
    {
        private const string IndentUnit = "  ";

        private readonly TextWriter writer;
        private int indentLevel = 0;

        public AstNodeWriter(TextWriter writer)
        {
            this.writer = writer;
        }

        public TextWriter Visit(CharSetNode node)
        {
            NewLine();
            WriteIntSet(node.Characters);
            return writer;
        }

        private void WriteIntSet(IntSet intSet)
        {
            writer.Write("{");
            bool first = true;
            foreach (var interval in intSet.EnumerateIntervals())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.Write(" ");
                }

                WriteInterval(interval);
            }

            writer.Write("}");
        }

        private void WriteInterval(IntInterval interval)
        {
            WriteChar(interval.First);
            if (interval.First != interval.Last)
            {
                writer.Write("..");
                WriteChar(interval.Last);
            }
        }

        private void WriteChar(int value)
        {
            char ch = (char)value;
            if (!char.IsControl(ch) && !char.IsSurrogate(ch))
            {
                writer.Write('\'');
                writer.Write(ch);
                writer.Write('\'');
            }
            else
            {
                writer.Write("'\\U{0,4:X}'", value);
            }
        }

        public TextWriter Visit(ActionNode node)
        {
            NewLine();
            writer.Write("Action(");
            writer.Write(node.Action);
            writer.Write(")");
            return writer;
        }

        public TextWriter Visit(CatNode node)
        {
            NewLine();
            writer.Write("cat:");
            IncreaseIndent();
            try
            {
                foreach (var child in node.Children)
                {
                    child.Accept(this);
                }
            }
            finally
            {
                DecreaseIndent();
            }

            return writer;
        }

        public TextWriter Visit(OrNode node)
        {
            NewLine();
            writer.Write("or:");
            IncreaseIndent();
            try
            {
                foreach (var child in node.Children)
                {
                    child.Accept(this);
                }
            }
            finally
            {
                DecreaseIndent();
            }

            return writer;
        }

        public TextWriter Visit(RepeatNode node)
        {
            NewLine();
            writer.Write("repeat from={0} to={1}:", node.MinCount, node.MaxCount);
            IncreaseIndent();
            try
            {
                node.Inner.Accept(this);
            }
            finally
            {
                DecreaseIndent();
            }

            return writer;
        }

        private void WriteIndent()
        {
            int i = indentLevel;
            while (i-- != 0)
            {
                writer.Write(IndentUnit);
            }
        }

        private void IncreaseIndent()
        {
            ++indentLevel;
        }

        private void DecreaseIndent()
        {
            --indentLevel;
        }

        private void NewLine()
        {
            writer.WriteLine();
            WriteIndent();
        }
    }
}
