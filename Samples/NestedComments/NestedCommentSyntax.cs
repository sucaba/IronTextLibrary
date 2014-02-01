using System;
using System.Text;
using IronText.Framework;

namespace Samples
{
    [Language]
    [GrammarDocument("NestedCommentSyntax.gram")]
    public class NestedCommentSyntax
    {
        [Outcome] 
        public string Result { get; set; }

        [LanguageService]
        public IScanning Scanning { get; set; }

        [Literal("/*")]
        public void BeginComment(out CommentMode mode)
        {
            mode = new CommentMode(this, Scanning);
        }
    }

    [Vocabulary]
    public class CommentMode
    {
        private StringBuilder comment;
        private readonly NestedCommentSyntax exit;
        private int nestLevel;
        private IScanning scanning;

        public CommentMode(NestedCommentSyntax exit, IScanning scanning)
        {
            this.scanning = scanning;
            this.exit = exit;
            this.comment = new StringBuilder();

            BeginComment();
        }

        [Literal("/*")]
        public void BeginComment()
        {
            ++nestLevel;
            Console.WriteLine("increased comment nest level to {0}", nestLevel);
            comment.Append("/*");
        }

        [Match("(~[*/] | '*' ~'/' | '/' ~'*') +")]
        public void Text(string text)
        {
            comment.Append(text);
        }

        [Literal("*/")]
        public string EndComment(out NestedCommentSyntax mode)
        {
            comment.Append("*/");

            --nestLevel;
            Console.WriteLine("decreased comment nest level to {0}", nestLevel);
            if (nestLevel == 0)
            {
                mode = exit;
                return comment.ToString();
            }
            else
            {
                mode = null;            // null mode means don't change current mode.
                scanning.Skip();        // scanner should skip current token value 
                return null;            // Ignored token value
            }
        }
    }
}
