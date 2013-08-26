using System;
using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Lib.RegularAst;

namespace IronText.Automata.Regular
{
    class PositionBuilder : IAstNodeVisitor<object,object>
    {
        public readonly List<RegularPositionInfo> Positions = new List<RegularPositionInfo>();

        public object Visit(CharSetNode node, object value)
        {
            Positions.Add(
                new RegularPositionInfo 
                { 
                    Action = new Nullable<int>(),
                    Characters = node.Characters.Clone(),
                });

            return this;
        }

        public object Visit(ActionNode node, object value)
        {
            Positions.Add(
                new RegularPositionInfo 
                {
                    Action = node.Action,
                    Characters = SparseIntSetType.Instance.Empty,
                });
            return this;
        }

        public object Visit(CatNode node, object value)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this, null);
            }

            return this;
        }

        public object Visit(OrNode node, object value)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this, null);
            }

            return this;
        }

        public object Visit(RepeatNode node, object value)
        {
            for (int i = 0; i != node.InnerCompilationCount; ++i)
            {
                node.Inner.Accept(this, null);
            }

            return this;
        }
    }
}
