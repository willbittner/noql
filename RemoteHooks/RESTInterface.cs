using Funq;
using NoQL.CEP.Blocks;
using NoQL.CEP.Connections;
using NoQL.CEP.NewExpressions;
using NoQL.CEP.Profiling;
using NoQL.YoloLambda;
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.WebHost.Endpoints.Support;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace NoQL.CEP.RemoteHooks
{


    public static class RESTStartup
    {
        private static RESTInterface Interface;
   

        public static void StartService()
        {
            StopService();
            Interface = new RESTInterface();
            Interface.StartService();
        }

        public static void StopService()
        {
            if (Interface != null)
            {
                try
                {
                    Interface.Stop();
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger(typeof(RESTStartup)).Debug("Stop interface exception", ex);
                }
            }
        }
    }

    public class StaticFileHandler : EndpointHandlerBase
    {
        protected static readonly Dictionary<string, string> ExtensionContentType;

        protected FileInfo fi;

        static StaticFileHandler()
        {
            ExtensionContentType = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { ".text", "text/plain" },
            { ".js", "text/javascript" },
            { ".css", "text/css" },
            { ".html", "text/html" },
            { ".htm", "text/html" },
            { ".png", "image/png" },
            { ".ico", "image/x-icon" },
            { ".gif", "image/gif" },
            { ".bmp", "image/bmp" },
            { ".jpg", "image/jpeg" }
        };
        }

        public string BaseDirectory { protected set; get; }

        public string Prefix { protected set; get; }

        public StaticFileHandler(string baseDirectory, string prefix)
        {
            BaseDirectory = baseDirectory;
            Prefix = prefix;
        }

        private StaticFileHandler(FileInfo fi)
        {
            this.fi = fi;
        }

        public static StaticFileHandler Factory(string baseDirectory, string prefix, string pathInfo)
        {
            if (!pathInfo.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var fn = baseDirectory + "/" + pathInfo.Substring(prefix.Length - 1);

            var fi = new System.IO.FileInfo(fn);

            if (!fi.Exists)
            {
                return null;
            }

            return new StaticFileHandler(fi);
        }

        public override void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            var bytes = File.ReadAllBytes(fi.FullName);
            // timeStamp = fi.LastWriteTime;
            httpRes.OutputStream.Write(bytes, 0, bytes.Length);
            httpRes.Flush();
            httpRes.AddHeader("Date", DateTime.Now.ToString("R"));
            httpRes.AddHeader("Content-Type", ExtensionContentType.ContainsKey(fi.Extension) ? ExtensionContentType[fi.Extension] : "text/plain");
        }

        public override object CreateRequest(IHttpRequest request, string operationName)
        {
            return null;
        }

        public override object GetResponse(IHttpRequest httpReq, IHttpResponse httpRes, object request)
        {
            return null;
        }
    }

    public class RESTInterface : AppHostHttpListenerBase
    {
        public RESTInterface()
            : base("YoloCEP Remote Debugging Hooks", typeof(CEPService).Assembly)
        {
            CatchAllHandlers.Add(
                     (httpMethod, pathInfo, filePath) =>
                         StaticFileHandler.Factory(
                             Environment.CurrentDirectory + "\\web\\",
                             "/",
                             pathInfo
                     )
                 );
        }

        public override void Configure(Container container)
        {
            LogManager.LogFactory = new ConsoleLogFactory();
        }

        public void StartService()
        {
            Parallel.Invoke(
                            () =>
                            {
                                Init();
                                Start("http://*:8081/");
                            }
                )
                ;
        }
    }

    [Route("/graph/edges", "GET")]
    public class GetEdges : IReturn<SimpleEdge[]>
    {
    }

    [Route("/graph/verticies", "GET")]
    public class GetVertices : IReturn<SimpleVertex[]>
    {
    }

    [Route("/express", "POST")]
    [Route("/graph/express", "POST")]
    public class PostExpr : IReturn<string>
    {
    }

    [Route("/graph/gexf", "GET")]
    [Route("/graph/gexf.gexf", "GET")]
    public class GetGexf : IReturn<string>
    {
    }

    [Route("/debug/blockdebug/{BlockID}", "GET")]
    public class GetBlockDebug : IReturn<string>
    {
        public int BlockID { get; set; }
    }

    [Route("/profiling/frames", "GET")]
    public class GetProfileFrames : IReturn<IList<ProfileFrame>>
    {
    }

    [Route("/profiling/frames/byblock", "GET")]
    public class GetFrameStats : IReturn<IList<FrameStats>>
    {
    }

    [Route("/database", "GET")]
    public class GetDatabases : IReturn<IList<string>>
    {
    }

    [Route("/database/{Name}", "GET")]
    public class GetDatabaseByName : IReturn<ArrayList>
    {
        public string Name { get; set; }
    }

    public class CEPService : Service
    {
        public object Get(GetDatabases db)
        {
            return Processor.RamDatabases.Keys.ToList();
        }

        public object Get(GetDatabaseByName name)
        {
            return Processor.RamDatabases[name.Name].GetEnumerable();
        }

        public object Get(GetProfileFrames query)
        {
            return Processor.ProfilingProvider.GetFrames();
        }

        public object Get(GetFrameStats query)
        {
            return Processor.ProfilingProvider.GetStats();
        }

        public object Get(GetVertices query)
        {
            var verts = new List<SimpleVertex>();
            foreach (AbstractBlock vertex in ExprTree.Tree.Vertices.ToArray())
            {
                verts.Add(new SimpleVertex(vertex));
            }
            //Response.AddHeader("Access-Control-Allow-Origin", "*");
            return verts.ToArray();
        }

        public object Get(GetEdges query)
        {
            var edges = new List<SimpleEdge>();
            foreach (Connection edge in ExprTree.Tree.Edges.ToArray())
            {
                edges.Add(new SimpleEdge(edge));
            }
            //Response.AddHeader("Access-Control-Allow-Origin", "*");
            return edges.ToArray();
        }

        public string Get(GetGexf g0)
        {
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            Response.ContentType = "text/xml";
            Random r = new Random(Environment.TickCount);
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n\r" +
                      "<gexf xmlns=\"http://www.gexf.net/1.2draft\" xmlns:viz=\"http://www.gexf.net/1.2draft/viz\" " +
                      "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.gexf.net/1.2draft http://www.gexf.net/1.2draft/gexf.xsd\"  version=\"1.2\">" +
                      "<meta lastmodifieddate=\"2009-03-20\">" +
                      "<creator>Gexf.net</creator><description>NoQL Visual Debugger</description></meta><graph mode=\"static\" defaultedgetype=\"directed\">");
            sb.Append("<attributes class=\"edge\"><attribute id=\"0\" title=\"LastData\" type=\"string\"/></attributes>");
            sb.Append("<attributes class=\"node\"><attribute id=\"0\" title=\"LastData\" type=\"string\"/></attributes>");
            sb.Append("<nodes>");
            ExprTree.ViewModel.Layout.UpdateLayout();
           ExprTree.Tree.Vertices.ToList().ForEach(x =>
            {
                var vert = new SimpleVertex(x);
                string vertString = String.Format("<node id=\"{0}\" label=\"{1}\">", vert.UniqID, vert.Name);
                if (ExprTree.ViewModel.Layout.LayoutAlgorithm.VertexPositions.ContainsKey(x))
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

            return sb.ToString();
        }

        public string Post(PostExpr s)
        {
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            string exprString = Request.GetParam("expr");
            string errors;
            INewCEPExpression expr = YoloMaker.MakeExpr(exprString, out errors);
            if (expr == null)
            {
                Response.StatusCode = 400;
                return errors;
            }

            return "success";
        }

        public string Get(GetBlockDebug blockdebug)
        {
            var blocks = ExprTree.Tree.Vertices.Where(block => block.UniqueID == blockdebug.BlockID);
            if (blocks.Count() > 1) throw new Exception("multiple blocks with same ID in GetDebugBlocks");
            var ablock = blocks.FirstOrDefault();
            ablock.DebugMode = true;
            string retstring = "<html><table>";
            for (int i = 0; i < ablock.DebugStrings.Count; i++)
            {
                var curstring = ablock.DebugStrings[i];
                retstring += "<tr><td>" + curstring + "</td></tr>";
            }
            retstring += "</table></html>";
            return retstring;
        }
    }

    [Serializable]
    public class SimpleVertex
    {
        public string Name { get; set; }

        public Dictionary<string, object> OtherFields { get; set; }

        public string UniqID { get; set; }

        public object LastObject { get; set; }

        public List<string> debugStringList { get; set; }

        public SimpleVertex()
        {
        }

        public SimpleVertex(AbstractBlock ab)
        {
            UniqID = ab.UniqueID.ToString();
            Name = ab.DebugName ?? ab.GetType().Name;
            LastObject = ab.LastData;
            debugStringList = ab.DebugStrings;
        }
    }

    [Serializable]
    public class SimpleEdge
    {
        public string Destination { get; set; }

        public Dictionary<string, object> OtherFields { get; set; }

        public string Source { get; set; }

        public string UniqID { get; set; }

        public SimpleEdge()
        {
        }

        public SimpleEdge(Connection c)
        {
            UniqID = c.UniqueID.ToString();

            if (c.Source != null)
            {
                Source = c.Source.UniqueID.ToString();
            }
            if (c.Destination != null)
            {
                Destination = c.Destination.UniqueID.ToString();
            }
        }
    }
}