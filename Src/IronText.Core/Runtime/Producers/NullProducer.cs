
namespace IronText.Framework
{
    public class NullProducer<T> : IProducer<T>
    {
        public static readonly NullProducer<T> Instance = new NullProducer<T>();

        public ReductionOrder ReductionOrder { get { return ReductionOrder.Unordered; } }

        public T Result
        {
            get { return default(T); }
            set { }
        }

        public T CreateLeaf(Msg envelope, MsgData msg) { return default(T); }

        public T CreateBranch(Framework.BnfRule rule, Algorithm.ArraySlice<T> parts, IStackLookback<T> lookback)
        {
            return default(T);
        }

        public T Merge(T alt1, T alt2, IStackLookback<T> lookback) { return default(T); }

        public T GetDefault(int nonTerm, IStackLookback<T> lookback) { return default(T); }

        public void FillEpsilonSuffix(
            int ruleId,
            int prefixSize,
            T[] buffer,
            int destIndex,
            IStackLookback<T> lookback)
        {
        }

        public IProducer<T> GetErrorRecoveryProducer() { return this; }
    }
}
