using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Timers;
using NoQL.CEP.Blocks;
using NoQL.CEP.Connections;
using GraphSharp.Algorithms.Layout;
using GraphSharp.Algorithms.Layout.Simple.Hierarchical;
using QuickGraph;
using Size = System.Windows.Size;

namespace NoQL.CEP.Windows
{
    public interface IYoloQLWindow
    {
        void LoadComponent(YoloGraph graph);
    }

    public static class NodeAndEdgeKeepr
    {
        public static List<Connection> Connections = new List<Connection>();
        public static YoloGraph Graph = new YoloGraph(true);
        public static bool NeedsUpdate = false;
        public static YoloGraph OldGraph = new YoloGraph();
        public static List<AbstractBlock> Vertexes = new List<AbstractBlock>();
        public static IYoloQLWindow Window;

        public static void AddEdge(Connection edge)
        {
            edge.PropertyChanged += ConnnectionPropertyChanged;
            NeedsUpdate = true;
            Graph.AddEdge(edge);
            //TimedUpdated.callback();
        }

        public static void AddVertex(AbstractBlock block)
        {
            Graph.AddVertex(block);
            NeedsUpdate = true;
            block.PropertyChanged += ConnnectionPropertyChanged;
            //TimedUpdated.callback();
        }

        public static void ConnnectionPropertyChanged(object obj, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "IsActiveWithEvent") NeedsUpdate = true;
            if (args.PropertyName == "HasError") NeedsUpdate = true;
        }
    }


    public class TimedUpdated
    {
        public TimedUpdated()
        {
            var timer1 = new Timer();
            timer1.Interval = 3000;
            timer1.Elapsed += callback;
            timer1.Start();
        }

        public static void callback(object obj, ElapsedEventArgs args)
        {
            if (NodeAndEdgeKeepr.Window != null && NodeAndEdgeKeepr.Graph != null && NodeAndEdgeKeepr.NeedsUpdate)
            {
                NodeAndEdgeKeepr.Window.LoadComponent(NodeAndEdgeKeepr.Graph);
                NodeAndEdgeKeepr.NeedsUpdate = false;
            }
        }
    }



    [Serializable]
    public class YoloGraph : BidirectionalGraph<AbstractBlock, Connection>, INotifyPropertyChanged
    {
        public LayoutAlgorithmBase<AbstractBlock, Connection, YoloGraph> LayoutAlgorithm { get; set; }
        private bool isInGUI = false;

        public YoloGraph(bool isInGUI, bool bs)
            : base()
        {
            isInGUI = isInGUI;
        }

        public YoloGraph()
        {
        }

        public YoloGraph(bool allowParallelEdges)
            : base(allowParallelEdges)
        {
        }

        public override bool AddEdge(Connection e)
        {
            lock (this)
            {
                FirePropertyChanged("Edges");

                return base.AddEdge(e);
            }
        }

        public override bool AddVertex(AbstractBlock v)
        {
            lock (this)
            {
                FirePropertyChanged("Vertices");
                return base.AddVertex(v);
            }
        }

        public Connection[] EdgesToArray()
        {
            lock (this)
            {
                return Edges.ToArray();
            }
        }

        public void Layout()
        {
            LayoutAlgorithm = new EfficientSugiyamaLayoutAlgorithm<AbstractBlock, Connection, YoloGraph>(this,
                new EfficientSugiyamaLayoutParameters() { VertexDistance = 20 },
                new Dictionary<AbstractBlock, Size>());
            LayoutAlgorithm.Compute();
        }

        private void FirePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public AbstractBlock[] VerticesToArray()
        {
            lock (this)
            {
                return Vertices.ToArray();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}