namespace IronText.Algorithm
{
    internal struct Edge
    {
        public Edge(int from, int to)
        {
            From = from;
            To   = to;
        }

        public int  From     { get; private set; }

        public int  To       { get; private set; }

        public Edge Opposite { get { return new Edge(To, From); } }
    }
}
