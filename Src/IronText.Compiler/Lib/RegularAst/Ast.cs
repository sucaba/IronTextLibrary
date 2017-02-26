using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronText.Algorithm;

namespace IronText.Lib.RegularAst
{
    public abstract class AstNode
    {
        public static readonly AstNode Empty = new CatNode(new AstNode[0]);

        public static AstNode Stub
        {
            get { return CharSetNode.Create('\0'); }
        }

        public static AstNode Cat(IEnumerable<AstNode> children) 
        {
            return new CatNode(children); 
        }

        public static AstNode Cat(params AstNode[] children) 
        {
            if (children.Length == 1)
            {
                return children[0];
            }

            return new CatNode(children); 
        }

        public static AstNode Or(IEnumerable<AstNode> children) 
        {
            return new OrNode(children); 
        }

        public static AstNode Or(params AstNode[] children) 
        {
            return new OrNode(children); 
        }

        public abstract TResult Accept<TResult>(IAstNodeVisitor<TResult> visitor);
        public abstract TResult Accept<TResult,T>(IAstNodeVisitor<TResult,T> visitor, T value);

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                return Accept(new AstNodeWriter(writer)).ToString();
            }
        }
    }

    public class LookaheadNode : CharSetNode
    {
        public LookaheadNode(IntSet intSet) : base(intSet) { }
    }

    public class LookbackNode : CharSetNode
    {
        public LookbackNode(IntSet intSet) : base(intSet) { }
    }

    public class CharSetNode : AstNode
    {
        public readonly IntSet Characters;

        public static CharSetNode Create(int ch) { return new CharSetNode(ch); }
        public static CharSetNode Create(IntSet cset) { return new CharSetNode(cset); }
        public static CharSetNode CreateRange(int from, int to) 
        { 
            return new CharSetNode(UnicodeIntSetType.Instance.Range(from, to));
        }

        public static CharSetNode Union(IEnumerable<CharSetNode> nodes)
        {
            var all = UnicodeIntSetType.Instance.Mutable();
            foreach (var node in nodes)
            {
                all.AddAll(node.Characters);
            }

            return new CharSetNode(all.CompleteAndDestroy());
        }

        public CharSetNode Complement() { return new CharSetNode(Characters.Complement()); }

        protected CharSetNode(int ch) : this(UnicodeIntSetType.Instance.Of(ch)) { }
        protected CharSetNode(IntSet cset) { this.Characters = cset; }

        public override TResult Accept<TResult>(IAstNodeVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public override TResult Accept<TResult,T>(IAstNodeVisitor<TResult,T> visitor, T value)
        {
            return visitor.Visit(this, value);
        }
    }

    public class ActionNode : AstNode
    {
        public readonly int Action;

        public static ActionNode Create(int action) { return new ActionNode(action); }
        
        private ActionNode(int action) { this.Action = action; }

        public override TResult Accept<TResult>(IAstNodeVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public override TResult Accept<TResult,T>(IAstNodeVisitor<TResult,T> visitor, T value)
        {
            return visitor.Visit(this, value);
        }
    }

    public class CatNode : AstNode
    {
        public readonly List<AstNode> Children;

        public CatNode(IEnumerable<AstNode> children) : this(children.ToList()) { }

        public CatNode(List<AstNode> children) { Children = children ?? new List<AstNode>(); }

        public override TResult Accept<TResult>(IAstNodeVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public override TResult Accept<TResult,T>(IAstNodeVisitor<TResult,T> visitor, T value)
        {
            return visitor.Visit(this, value);
        }
    }

    public class OrNode : AstNode
    {
        public readonly List<AstNode> Children;

        public OrNode(IEnumerable<AstNode> children) : this(children.ToList()) { }
        public OrNode(List<AstNode> children) { Children = children ?? new List<AstNode>(); }

        public override TResult Accept<TResult>(IAstNodeVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public override TResult Accept<TResult,T>(IAstNodeVisitor<TResult,T> visitor, T value)
        {
            return visitor.Visit(this, value);
        }
    }

    public class RepeatNode : AstNode
    {
        public readonly AstNode Inner;
        public readonly int MinCount;
        public readonly int MaxCount;

        public static RepeatNode ZeroOrMore(AstNode inner) { return new RepeatNode(inner, 0, int.MaxValue); }
        public static RepeatNode OneOrMore(AstNode inner) { return new RepeatNode(inner, 1, int.MaxValue); }
        public static RepeatNode Optional(AstNode inner) { return new RepeatNode(inner, 0, 1); }

        public RepeatNode(AstNode inner, int minCount, int maxCount)
        {
            Inner = inner;
            MinCount = minCount;
            MaxCount = maxCount;
        }

        public override TResult Accept<TResult>(IAstNodeVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public override TResult Accept<TResult,T>(IAstNodeVisitor<TResult,T> visitor, T value)
        {
            return visitor.Visit(this, value);
        }

        internal int InnerCompilationCount { get { return MaxCount == int.MaxValue ? MinCount + 1 : MaxCount; } }
    }
}
