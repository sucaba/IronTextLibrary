using IronText.Framework;

namespace IronText.Lib.Shared
{
    public interface Push<TNs> { }
    public interface Pop<TNs> { }

    [Vocabulary]
    public abstract class NsBase<TNs>
    {
        internal SNode<IFrame<TNs>> FrameNode;

        public NsBase(IFrame<TNs> frame = null)
        {
            FrameNode = new SNode<IFrame<TNs>> { Value = frame ?? new Frame<TNs>() };
        }

        public IFrame<TNs> Frame {  get { return FrameNode.Value; } set { FrameNode.Value = value; } }

        [Produce]
        public Push<TNs> PushFrame() { FrameNode = new SNode<IFrame<TNs>> { Value = new Frame<TNs>(), Next = FrameNode }; return null; }

        [Produce]
        public Pop<TNs> PopFrame() { FrameNode = FrameNode.Next; return null; }

        public Def<TNs> Generate() { return Frame.Define(); }

        public Def<TNs> Generate(string idn) { return Frame.Define(idn); }
    }

    internal class SNode<T>
    {
        public T Value;
        public SNode<T> Next;
    }
}
