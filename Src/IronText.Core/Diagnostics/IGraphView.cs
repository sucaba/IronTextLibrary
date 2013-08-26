using System;

namespace IronText.Diagnostics
{
    public enum RankDir
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    public enum Shape
    {
        Default,
        Ellipse,
        Rect,
        Circle,
        Mrecord,
        Point,
    }

    public enum Style
    {
        Default,
        Solid,
        Dotted,
        Dashed,
        Invisible,
        Bold
    }

    public enum GraphColor
    {
        Black,
        White,
        Green,
        Red,
        Yellow,
        Blue,
        Magenta,
        Cyan,
        Brown,
        Grey,

        Default = Black,
    }

    public enum Ordering
    {
        None,
        Out,
        In
    }
    
    public interface IGraphView : IDisposable
    {
        void BeginDigraph(string name);
        void EndDigraph();

        void BeginCluster(string clusterName);
        void EndCluster();

        void SetGraphProperties(
            RankDir rankDir = RankDir.TopToBottom,
            Style style = Style.Default,
            string label = null,
            Ordering ordering = Ordering.None);

        void SetNodeProperties(Shape shape);

        void AddNode(
            object identity,
            string label = null,
            Style style = Style.Default,
            Shape shape = Shape.Default,
            GraphColor color = GraphColor.Default);

        void AddEdge(
            object fromNodeIdentity,
            object toNodeIdentity,
            string label = null,
            Style style = Style.Default);
    }
}
