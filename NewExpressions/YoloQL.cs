using NoQL.CEP.Blocks;
using NoQL.CEP.Connections;
using NoQL.CEP.RemoteHooks;
using NoQL.TempGUI.DesignStudio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace NoQL.CEP.NewExpressions
{
    public static class Extensions
    {
        public static INewCEPExpression<object> Input(this INewCEPExpression<object> expr, Func<object, object> action, INewCEPExpression exprToAttachTo)
        {
            //Do block logic here
            BaseInputAdapter adapter = new BaseInputAdapter();
            var perfExpress = Yolo.Express().Perform(x => adapter.Send(action(x)));
            adapter.Attach(exprToAttachTo as INewComponent);
            adapter.Send(action(new object()));
            return perfExpress;
        }
    }

    public class NewComponentManager : INewComponentManager
    {
        private ConcurrentDictionary<string, INewComponent> components = new ConcurrentDictionary<string, INewComponent>();

        #region INewComponentManager Members

        public void Register(string name, int protectionCode)
        {
            if (components.ContainsKey(name)) throw new Exception("That component already exsists, cant register, name: " + name);
            components[name] = new NewComponent(name);
            components[name].ComponentName = name;
        }

        INewComponent INewComponentManager.Get(string name)
        {
            INewComponent component;
            if (components.TryGetValue(name, out component)) return component;
            throw new Exception("Component not found in manager");
            //Register(Yolo.Express(),name);
            return components[name];
        }

        public INewComponent Get(INewComponent comp)
        {
            return comp;
        }

        #endregion INewComponentManager Members

        public System.Collections.Generic.List<INewComponent> GetAll()
        {
            List<INewComponent> returnList = new List<INewComponent>();
            foreach (var obj in components)
            {
                returnList.Add(obj.Value);
            }
            return returnList;
        }
    }

    public class ExpressionManager
    {
        public List<YoloItem> Expressions = new List<YoloItem>();

        public void AddExpression(string name)
        {
            YoloItem item = new YoloItem(YoloItemType.Extension);
            item.Name = name;
            Expressions.Add(item);
        }

        public ExpressionManager()
        {
            Express2.Init(this);
        }
    }

    public static class Yolo
    {
        public static ExpressionManager Expressions = new ExpressionManager();
        public static INewComponentManager Manager = new NewComponentManager();

        public static INewCEPExpression<object> Express()
        //We should never use this. Or at least minimize use.
        {
            //return NewAbstractCEPExpression<object>.StaticExpress(true);
            return Express2<object>.CreateBase<object>();
        }

        public static INewCEPExpression<object> Input(Func<object, object> GenerateInputFunc, INewCEPExpression exprToSendInputTo)
        {
            return Yolo.Express().Input(GenerateInputFunc, exprToSendInputTo);
        }

        public static INewCEPExpression<InputValue> Express<InputValue>()
        {
            return Express2<InputValue>.CreateBase<InputValue>();
        }

        public static IRamDB GetRamDB(string DatabaseName)
        {
            return Processor.RamDatabases[DatabaseName];
        }

        //public static void SetTimeProvider(ITimeProvider provider)
        //{
        //    Processor.TimeProvider = provider;
        //}
        public static void DumpGraph()
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n\r" +
                      "<gexf xmlns=\"http://www.gexf.net/1.2draft\" xmlns:viz=\"http://www.gexf.net/1.2draft/viz\" " +
                      "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.gexf.net/1.2draft http://www.gexf.net/1.2draft/gexf.xsd\"  version=\"1.2\">" +
                      "<meta lastmodifieddate=\"2009-03-20\">" +
                      "<creator>Gexf.net</creator><description>Dashx.CEP</description></meta><graph mode=\"static\" defaultedgetype=\"directed\">");
            sb.Append("<attributes class=\"edge\"><attribute id=\"0\" title=\"LastData\" type=\"string\"/></attributes>");
            sb.Append("<attributes class=\"node\"><attribute id=\"0\" title=\"LastData\" type=\"string\"/></attributes>");
            sb.Append("<nodes>");
            //ExprTree.Tree.Layout();

            ExprTree.Tree.Vertices.ToList().ForEach(x =>
            {
                var vert = new SimpleVertex(x);
                string vertString = String.Format("<node id=\"{0}\" label=\"{1}\">", vert.UniqID, vert.Name);
                if (ExprTree.Tree.Vertices.Any<AbstractBlock>(z => z.UniqID == x.UniqID) )
                {
                    var pt = ExprTree.ViewModel.Layout.LayoutAlgorithm.VertexPositions[x];
                    vertString += string.Format("<viz:position x=\"{0}\" y=\"{1}\" z=\"0\" />", pt.X, pt.Y);
                }
                vertString += string.Format("<viz:size value=\"2\" />");
                vertString += string.Format("<viz:shape value=\"disc\"/>");

                if (x.LastException != null)
                    vertString += string.Format("<viz:color r=\"254\" g=\"0\" b=\"0\" a=\"0.6\"/>");
                else if (x.LastData != null)
                    vertString += string.Format("<viz:color r=\"0\" g=\"0\" b=\"254\" a=\"0.6\"/>");
                else
                    vertString += string.Format("<viz:color r=\"239\" g=\"173\" b=\"66\" a=\"0.6\"/>");

                vertString += String.Format("<attvalues><attvalue for=\"LastData\" value=\"{0}\" /><attvalue for=\"LastEx\" value=\"{1}\" /></attvalues>",
                    x.LastData ?? "NULL", x.LastException ?? "NULL");
                vertString += string.Format("</node>");

                sb.Append(vertString);
            });
            sb.Append("</nodes><edges>");

            List<SimpleEdge> edges = new List<SimpleEdge>();
            ExprTree.Tree.Edges.ToList().ForEach(x =>
            {
                var edge = new SimpleEdge(x);
                edges.Add(edge);
                string edgeString = String.Format("<edge id=\"{0}\" source=\"{1}\" target=\"{2}\">", edge.UniqID, edge.Source, edge.Destination);
                if (x.IsActiveWithEvent)
                    edgeString += String.Format("<color r=\"0\" g=\"254\" b=\"0\" a=\"0.6\"/>");
                edgeString += String.Format("<attvalues><attvalue for=\"0\" value=\"{0}\" /></attvalues>", x.LastData ?? "NULL");
                edgeString += String.Format("</edge>");
                //Avoid parallel edges as GEXF/Gephi has no spec for that.
                if (edges.AsQueryable().Any(e2 => e2.Source == edge.Source && e2.Destination == edge.Destination))
                    sb.Append(edgeString);
            });

            sb.Append("</edges></graph></gexf>");
            System.IO.File.WriteAllText(@"web\test.gexf", sb.ToString());
        }
    }
}