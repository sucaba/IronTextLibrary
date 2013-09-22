using System;
using System.Collections.Generic;
using System.IO;

namespace IronText.Diagnostics
{
    /// <summary>
    /// GraphWizard file writer
    /// </summary>
    public class GvGraphView : IDisposable, IGraphView
    {
        private StreamWriter writer;

        private readonly Dictionary<object,Node>     nodes       = new Dictionary<object,Node>();

        public GvGraphView(string outputPath)
        {
            this.writer = new StreamWriter(outputPath);
        }

        ~GvGraphView()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (writer != null && disposing)
            {
                writer.Dispose();
                writer = null;
            }
        }

        public void BeginDigraph(string name)
        {
            writer.WriteLine("digraph {0} {{", name);
        }

        public void EndDigraph()
        {
            foreach (var node in nodes.Values)
            {
                if (node.PostponeGeneration)
                {
                    node.PostponeGeneration = false;
                    GenerateNode(node);
                }
            }

            writer.WriteLine("}");
        }

        public void BeginCluster(string clusterName)
        {
            writer.WriteLine(
                "subgraph cluster{0} {{ graph [style=dotted label=\"{0}\"]", Escape(clusterName));
        }

        public void EndCluster() { writer.WriteLine("}"); }

        public void AddNode(
            object identity,
            string label = null,
            Style style = Style.Default,
            Shape shape = Shape.Default,
            GraphColor color = GraphColor.Default)
        {
            Node node = GetOrAddNode(identity, label, style, shape, color);

            if (node.PostponeGeneration)
            {
                return;
            }

            GenerateNode(node);
        }

        private void GenerateNode(Node node)
        {
            node.PostponeGeneration = false;
            
            var label =  node.Label ?? "node #" + node.Index;

            writer.WriteLine(
                "  node_{0} [label={1}, style={2}, shape={3}, color=\"{4}\"]",
                node.Index,
                node.Shape == Shape.Mrecord ? "<" + label + ">" : ("\"" + Escape(label) + "\""),
                StyleToText(node.Style),
                ShapeToText(node.Shape),
                ColorToText(node.Color));
        }

        private Node GetOrAddNode(object identity)
        {
            return GetOrAddNode(identity, null, Style.Default, Shape.Default, GraphColor.Default);
        }

        private Node GetOrAddNode(object identity, string label, Style style, Shape shape, GraphColor color)
        {
            Node node;
            if (!nodes.TryGetValue(identity, out node))
            {
                int index = nodes.Count;
                node = new Node 
                       { 
                           Index = index,
                           Label = label,
                           Style = style,
                           Shape = shape,
                           Color = color,
                           PostponeGeneration = label == null,
                       };
                nodes.Add(identity, node);
            }
            else
            {
                if (node.Label == null)
                {
                    node.Label = label;
                }

                if (node.Style == Style.Default)
                {
                    node.Style = style;
                }

                if (node.Shape == Shape.Default)
                {
                    node.Shape = shape;
                }

                if (node.Color == GraphColor.Default)
                {
                    node.Color = color;
                }
            }

            return node;
        }

        public void SetGraphProperties(
            RankDir value = RankDir.TopToBottom,
            Style style = Style.Default,
            string label = null,
            Ordering ordering = Ordering.None)
        {
            string text = null;
            switch (value)
            {
                case RankDir.BottomToTop: text = "BT"; break;
                case RankDir.TopToBottom: text = "TB"; break;
                case RankDir.RightToLeft: text = "RL"; break;
                case RankDir.LeftToRight: text = "LR"; break;
            }

            if (text != null)
            {
                writer.WriteLine("graph [rankdir={0}]", text);
            }

            if (label != null)
            {
                writer.WriteLine("graph [label=\"{0}\"]", Escape(label));
            }

            if (style != Style.Default)
            {
                writer.WriteLine("graph [style={0}]", StyleToText(style));
            }

            if (ordering != Ordering.None)
            {
                writer.WriteLine("graph [ordering={0}]", OrderingToText(ordering));
            }
        }

        private string StyleToText(Style style)
        {
            switch (style)
            {
                case Style.Dotted: return "dotted";
                case Style.Dashed: return "dashed";
                case Style.Invisible: return "invis";
                case Style.Bold: return "bold";
                case Style.Default:
                case Style.Solid: 
                default:
                    return "solid";
            }
        }
        
        public void SetNodeProperties(Shape shape)
        {
            writer.WriteLine("node [shape={0}]", ShapeToText(shape));
        }

        public void AddEdge(
            object fromNodeIdentity, 
            object toNodeIdentity, 
            string label = null,
            Style style = Style.Default)
        {
            writer.Write(
                "node_{0} -> node_{1} ",
                GetOrAddNode(fromNodeIdentity).Index,
                GetOrAddNode(toNodeIdentity).Index
                );

            if (label != null || style != Style.Default)
            {
                writer.WriteLine("[ label = \"{0}\", style = {1} ]", Escape(label), StyleToText(style));
            }
            else
            {
                writer.WriteLine();
            }
        }

        private string ShapeToText(Shape shape)
        {
            switch (shape)
            {
                case Shape.Default:
                case Shape.Ellipse: return "ellipse";
                case Shape.Point:   return "point";
                case Shape.Circle:  return "circle";
                case Shape.Rect:    return "rect";
                case Shape.Mrecord: return "Mrecord";
                default:
                    throw new NotSupportedException();
            }
        }

        class Node 
        {
            public string Label;
            public int    Index;
            public bool   PostponeGeneration;
            public Style Style;
            public Shape Shape;
            public GraphColor Color;
        }

        private static string Escape(string s)
        {
            if (s == null)
            {
                return "";
            }

            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        public string OrderingToText(Ordering value)
        {
            switch (value)
            {
                case Ordering.Out: return "out";
                case Ordering.In: return "in";
                case Ordering.None:
                default:
                    return null;
            }
        }

        private string ColorToText(GraphColor color)
        {
            string name = Enum.GetName(typeof(GraphColor), color);
            return name.ToLower();
        }

    }
}
