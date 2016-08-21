namespace IronText.Compiler.Analysis
{
    public struct DotItemTransition
    {
        private readonly DotItem priorItem;

        public DotItemTransition(int token, DotItem priorItem)
        {
            this.Token = token;
            this.priorItem = priorItem;
        }

        public int Token { get; }

        public DotItem GetNextItem() => priorItem.CreateNextItem(Token);
    }
}
