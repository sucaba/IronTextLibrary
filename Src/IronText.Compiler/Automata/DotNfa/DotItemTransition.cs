using System;

namespace IronText.Automata.DotNfa
{
    public abstract class DotItemTransition
    {
        public DotItemTransition(int token)
        {
            this.Token = token;
        }

        public int Token { get; }

        public abstract DotItem CreateNextItem();
    }

    public class DotItemGotoTransition : DotItemTransition
    {
        private readonly DotItem priorItem;

        public DotItemGotoTransition(int token, DotItem priorItem)
            : base(token)
        {
            this.priorItem = priorItem;
        }

        public override DotItem CreateNextItem() => priorItem.Goto(Token);
    }

    public class DotItemAcceptTransition : DotItemTransition
    {
        public DotItemAcceptTransition(int token)
            : base(token)
        {
        }

        public override DotItem CreateNextItem()
        {
            throw new NotSupportedException();
        }
    }

    public class DotItemReduceTransition : DotItemTransition
    {
        public DotItemReduceTransition(int token, int productionId)
            : base(token)
        {
            this.ProductionId = productionId;
        }

        public int ProductionId { get; }

        public override DotItem CreateNextItem()
        {
            throw new NotSupportedException();
        }
    }
}
