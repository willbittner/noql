using NoQL.CEP.Connections;
using NoQL.CEP.JobManagers;
using NoQL.CEP.NewExpressions;
using NoQL.CEP.Profiling;
using NoQL.CEP.RemoteHooks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NoQL.CEP.Blocks
{
    public class DebugWrapper
    {
        public object obj;
        private List<string> placesVisited = new List<string>();

        public DebugWrapper(object data)
        {
            obj = data;
        }

        public void Visit(AbstractBlock block)
        {
            placesVisited.Add(block.DebugName);
        }
    }

    [Serializable]
    public class ExpressionBlockPlaceHolder : ExpressionBlock
    {
        public ExpressionBlockPlaceHolder(string name)
        {
            DebugName = name;
        }

        public ExpressionBlockPlaceHolder(Processor p)
            : base(p)
        {
            //ProcessorOwner = p;
        }

        public override bool OnData(object data)
        {
            return true;
        }

        public override Type BlockInputType
        {
            get { throw new NotImplementedException(); }
        }

        public override Type BlockOutputType
        {
            get { throw new NotImplementedException(); }
        }
    }

    [Serializable]
    public abstract class ExpressionBlock : AbstractBlock
    {
        internal List<Connection> ElseChildren = new List<Connection>();
        internal List<Connection> ErrorChildren = new List<Connection>();
        internal List<Connection> LogChildren = new List<Connection>();

        internal ExpressionBlock(Processor p)
            : base(p)
        {
        }

        internal ExpressionBlock()
        {
        }

        public new void Accept(object data)
        {
            try
            {
                OnLog(data);
                if (OnData(data))
                {
                    EmitToChildren(Children, data);
                }
                else
                {
                    EmitToChildren(ElseChildren, data);
                }
            }
            catch (Exception e)
            {
                if (ErrorChildren.Count == 0) throw e;
                OnError(e);
            }
        }


        private void EmitToChildren(List<Connection> children, object data)
        {
            foreach (Connection conn in children)
            {
                if (ProcessorOwner != null)
                {
                    var newJob = Processor.ObjectPool.GetObject<Job>();
                    newJob.Data = data;
                    newJob.JobConnection = conn;
                    ProcessorOwner.JobManager.Schedule(newJob);
                }
                if (ProcessorOwner == null) conn.Emit(data);
            }
        }

        public void OnElse(object data)
        {
            if (ElseChildren.Count == 0) return;
            // EmitToChildren(ElseChildren,data);
        }

        public void OnError(object data)
        {
            if (ErrorChildren.Count == 0) return;
            //     EmitToChildren(ErrorChildren,data);
        }

        public void OnLog(object data)
        {
            if (LogChildren.Count == 0) return;
            //   EmitToChildren(LogChildren,data);
        }

        public bool ValidateConnection(AbstractBlock block)
        {
            if (this == block) throw new Exception("No, you cannot connect your output to you own input, not allowed");
            return true;
        }
    }

    // //<summary>
    // //    Abstract Block describes a node on the traversal graph,
    // //    an abstract Block can pass
    // //</summary>
    //[DebuggerDisplay("Name - {DebugName}")]
    public interface IBlock
    {
        int ComponentID { get; set; }

        bool DebugMode { get; set; }

        string DebugName { get; set; }

        int DebugTabs { get; set; }

        int UniqueID { get; set; }

        object LastData { get; }

        Type BlockInputType { get; }

        Type BlockOutputType { get; }

        /// <summary>
        /// Action invoked on error, returns true
        /// to continue passing the event to the processor's
        /// event handler, returns false to stop
        /// propagation of the error.
        /// </summary>
        Func<object, Exception, bool> ErrorAction { get; set; }

        bool HasError { get; set; }

        string LastException { get; set; }

        bool IsVisited { get; set; }

        void SetProcessor(Processor processor);

        /// <summary>
        ///     Public-facing accept method called by
        /// </summary>
        /// <param name="data"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining, MethodCodeType = MethodCodeType.IL)]
        void Accept(object data);

        [MethodImpl(MethodImplOptions.AggressiveInlining, MethodCodeType = MethodCodeType.IL)]
        void SendToChildren(object data);

        Connection<MessageType> AddChild<MessageType>(AbstractBlock destination, Filter<MessageType> filter);

        void RemoveChild(AbstractBlock destination);

        /// <summary>
        ///     On-data method to be overridden by child classes
        /// </summary>
        /// <param name="data"></param>
        /// <returns> Return true if should continue to children</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining, MethodCodeType = MethodCodeType.IL)]
        bool OnData(object data);

        void PrintPretty(string indent, bool last);

        void SendError(object obj, Exception e);

        string ToString();

        event PropertyChangedEventHandler PropertyChanged;
    }

    [Serializable]
    public abstract class AbstractBlock : INotifyPropertyChanged, IBlock
    {
        private static int nextID;

        public static bool RemoteDebug { get; set; }

        public List<string> DebugStrings = new List<string>();

        /// <summary>
        ///     List of Children connections (the edges from this node to other nodes)
        /// </summary>
        internal List<Connection> Children { get; set; }

        public int ComponentID { get; set; }

        public bool DebugMode { get; set; }

        public string DebugName { get; set; }

        public int DebugTabs { get; set; }

        public int UniqueID { get; set; }

        public bool IsHead = false;
        private bool DirectedQueue = false;
        private bool _isVisited = false;
        private bool _hasError;

        public object LastData { get; protected set; }

        public abstract Type BlockInputType { get; }

        public abstract Type BlockOutputType { get; }

        /// <summary>
        /// Action invoked on error, returns true
        /// to continue passing the event to the processor's
        /// event handler, returns false to stop
        /// propagation of the error.
        /// </summary>
        public Func<object, Exception, bool> ErrorAction { get; set; }

        public string UniqID = Guid.NewGuid().ToString();

        public bool HasError
        {
            get { return _hasError; }
            set
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("HasError"));
                }
                _hasError = value;
            }
        }

        private string _lastException;

        public string LastException
        {
            get { return _lastException; }
            set
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("LastException"));
                }
                _lastException = value;
            }
        }

#if DEBUG

        public bool IsVisited
        {
            get { return _isVisited; }
            set
            {
                if (Children.Count > 0)
                {
                    foreach (Connection conn in Children)
                    {
                        conn.IsActiveWithEvent = true;
                    }
                }
            }
        }

#endif

        [ThreadStatic]
        [NonSerialized]
        private static int depthcount;

        /// <summary>
        ///     The complex even processor that owns this abstract Block. It is used to
        ///     reference the historical db if necessary.
        /// </summary>
        internal Processor ProcessorOwner { get; private set; }

        /// <summary>
        ///     Constructor to create an abstract Block. Any SUB class _MUST_ invoke this constructor
        /// </summary>
        /// <param name="processor"></param>
        internal AbstractBlock(Processor processor)
        {
            ProcessorOwner = processor;
            Init();
        }

        internal void Init()
        {
            Children = new List<Connection>();
            DebugMode = false;
            UniqueID = Interlocked.Increment(ref nextID);
            RemoteDebug = true;
        }

        internal AbstractBlock()
        {
            DebugName = "ERROR-FAKEBLOCK";
            Init();
        }

        public void SetProcessor(Processor processor)
        {
            ProcessorOwner = processor;
        }

        /// <summary>
        ///     Public-facing accept method called by
        /// </summary>
        /// <param name="data"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining, MethodCodeType = MethodCodeType.IL)]
        public void Accept(object data)
        {
            //if (RemoteDebug) DebugStrings.Add(data.ToString());
            Stopwatch sw = new Stopwatch();
            LastData = data;
            try
            {
                if (DebugMode) PrintDebug(" Got Data");
                if (data == null) return;

                if (DebugMode) PrintDebug(" Data Not Null, Is: " + data);
                //Console.WriteLine("{0} is currently accepting [{1}] {2}", this.GetType().Name, data.GetType().Name, data.ToString());
                //if (data.GetType() == typeof (Order) && ((Order) data).ID == "yomamma")
                //{
                //    Console.WriteLine("Gotcha");
                //}
                sw.Start();
                if (OnData(data)) //Call the ondata method, and if it returns true...
                {
                    if (DebugMode) PrintDebug(" Is SendToChildren data: " + data);
                    SendToChildren(data);
                }
                sw.Stop();
            }
            catch (Exception e)
            {
                HasError = true;
                LastException = e.Message;
                SendError(data, e);
            }
            finally
            {
                Processor.ProfilingProvider.AcceptFrame(new ProfileFrame(this, data, sw.Elapsed.TotalMilliseconds));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining, MethodCodeType = MethodCodeType.IL)]
        public void SendToChildren(object data)
        {
            if (data == null) return;
            if (Children.Count == 0) return;

#if DEBUG
            if (DebugMode) PrintDebug(" In SendToChildren, Sending: " + data);
#endif

            foreach (Connection connection in Children)
            {
                if (ProcessorOwner == null && connection.Evaluate(data))
                {
                    connection.Emit(data);
                    continue;
                }

                if (connection.Evaluate(data))
                {
                    var newJob = Processor.ObjectPool.GetObject<Job>();
                    newJob.Data = data;
                    newJob.JobConnection = connection;
                    ProcessorOwner.JobManager.Schedule(newJob);
                }
            }
        }

        private void PrintDebug(string msg)
        {
#if DEBUG
            Console.WriteLine(PrintDebugTabs() + "DEBUG: Block: " + DebugName + " " +
                              msg);
#endif
        }

        private string PrintDebugTabs()
        {
            string tabs = "";
            for (int i = 0; i < DebugTabs; i++)
            {
                tabs += "\t";
            }
            tabs = "|->";
            return tabs;
        }

        public Connection<MessageType> AddChild<MessageType>(AbstractBlock destination, Filter<MessageType> filter)
        {
            return ExprTree.BlockAddChild(this, destination, filter);
        }

        public void RemoveChild(AbstractBlock destination)
        {
            lock (Children)
            {
                Children.RemoveAll(child => child.Destination == destination);
            }
        }

        /// <summary>
        ///     On-data method to be overridden by child classes
        /// </summary>
        /// <param name="data"></param>
        /// <returns> Return true if should continue to children</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining, MethodCodeType = MethodCodeType.IL)]
        public abstract bool OnData(object data);

        public void PrintPretty(string indent, bool last)
        {
            Console.Write(indent);
            if (last)
            {
                Console.Write("\\-");
                indent += "  ";
            }
            else
            {
                Console.Write("|-");
                indent += "| ";
            }

            Console.WriteLine(GetType().Name);

            for (int i = 0; i < Children.Count; i++)
                Children[i].PrintPretty(indent, i == Children.Count - 1);
        }

        public void SendError(object obj, Exception e)
        {
            if (ErrorAction != null)
            {
                if (ErrorAction(obj, e))
                {
                    ProcessorOwner.ErrorHandler(this, e);
                    return;
                }
            }
            else
                ProcessorOwner.ErrorHandler(this, e);
        }

        public override string ToString()
        {
            return string.Format("{0}", DebugName);
        }

        //[NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}