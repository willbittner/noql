#if DEBUG

#endif

using NoQL.CEP.Blocks;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace NoQL.CEP.Connections
{
    public abstract class Connection : DependencyObject, INotifyPropertyChanged, IEdge<AbstractBlock>
    {
        public static int nextID = 0;
        public int UniqueID = 0;
        public string UniqID = Guid.NewGuid().ToString();

        public object LastData { get; protected set; }

        public abstract Type GetDestinationType();

        public abstract void Emit(object data);

        public abstract void PrintPretty(string indent, bool last);

        public AbstractBlock Destination { get; set; }

        private AbstractBlock _source;

        public AbstractBlock Source
        {
            get { return _source; }
            set { _source = value; }
        }

        public abstract bool Evaluate(object data);

        public bool ConnectsToHeadBlock { get; set; }

        public bool _IsActiveWithEventProperty;
        public static DependencyProperty IsActiveWithEventProperty;

        public bool InGUI = false;

        public void InitOnGui()
        {
            InGUI = true;
            //IsActiveWithEventProperty = DependencyProperty.Register("IsActiveWithEvent", typeof(Boolean), typeof(Connection),
            //new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsActiveWithEvent
        {
            get
            {
                try
                {
                    return _IsActiveWithEventProperty;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            set
            {
                try
                {
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("IsActiveWithEvent"));
                    }
                    _IsActiveWithEventProperty = value;
                    //Execute.OnUIThread(() => SetValue(IsActiveWithEventProperty, value));
                }
                catch (Exception e)
                {
                    throw e;
                }

                //this.OnPropertyChanged(new DependencyPropertyChangedEventArgs(IsActiveWithEventProperty, false, value));
            }
        }

        AbstractBlock IEdge<AbstractBlock>.Source
        {
            get { return _source; }
        }

        public AbstractBlock Target
        {
            get { return Destination; }
        }
    }

    /// <summary>
    ///     A connection describes a graph-like edge between Abstract Blocks
    /// </summary>
    [DebuggerDisplay("{Source.DebugName} -> {Target.DebugName}")]
    public class Connection<T> : Connection
    {
        /// <summary>
        ///     The filter object to decide if the destination
        ///     should accept this data
        /// </summary>

        public string ConnectionString
        {
            get
            {
                return string.Format("{0}-{1} Connected\n\r\n\rLast Data Seen:{2}", Source.DebugName, Destination.DebugName,
                                     LastData ?? "<null>"
                    );
            }
            set { }
        }

        public Connection()
        {
            UniqueID = Interlocked.Increment(ref nextID);
        }

        public Filter<T> ConnectionFilter { get; set; }

        public bool ConnectsToHeadBlock { get; set; }

        public override bool Evaluate(object data)
        {
            return ConnectionFilter == null || (ConnectionFilter.CheckType(data) && ConnectionFilter.IsFit((T)data));
        }

        /// <summary>
        ///     Public facing method called by ConnectionJob processor to push
        ///     execute the filter and if allowed, push data into
        ///     the destination.
        /// </summary>
        /// <param name="data"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Emit(object data)
        {
            if (data == null) return;
            if (Destination.ProcessorOwner == null)
            {
                Console.WriteLine("Null Processor Owner");
                throw new Exception("Null Processor Owner");
            }
            Destination.Accept(data);

            IsActiveWithEvent = true;
            this.LastData = data;
        }

        public override Type GetDestinationType()
        {
            return typeof(T);
        }

        public override void PrintPretty(string indent, bool last)
        {
            Destination.PrintPretty(indent, last);
        }

        public AbstractBlock Target
        {
            get { return Destination; }
            set { Destination = value; }
        }

        public override string ToString()
        {
            return string.Format("{0}", ConnectionString);
        }
    }

    public interface IIndexConnection<InputType, IndexType>
    {
        int UniqueID { get; }

        Type ConnectionInputType { get; }

        InputType LastValue { get; set; }

        AbstractBlock Source { get; set; }

        InputType LastData { get; }

        AbstractBlock GetConnection(IndexType type);

        bool CheckType(AbstractBlock blockToAdd);

        void AddConnection(AbstractBlock block, IndexType index);

        void AddConnection(AbstractBlock block);

        void EmitIndex(InputType data);

        void EmitRegular(InputType data);

        void Emit(InputType data);

        void PrintPretty(string indent, bool last);

        bool Evaluate(InputType data);

        void RemoveConnection(AbstractBlock block, IndexType type);

        void RemoveConnection(AbstractBlock block);
    }

    public class IndexConnection<InputType, IndexType> : IIndexConnection<InputType, IndexType>
    {
        private Dictionary<IndexType, AbstractBlock> _idxConnections = new Dictionary<IndexType, AbstractBlock>();
        private List<AbstractBlock> _regConnections = new List<AbstractBlock>();
        public static int nextID = 0;
        private int _uniqid = 0;

        public int UniqueID { get { return nextID; } }

        private Func<InputType, IndexType> _getKeyFunc;

        public Type ConnectionInputType { get { return typeof(InputType); } }

        private InputType _lastValue;

        public InputType LastValue { get; set; }

        public AbstractBlock Source { get; set; }

        private ReaderWriterLockSlim _regBlockLock = new ReaderWriterLockSlim();
        private ReaderWriterLockSlim _indexBlockLock = new ReaderWriterLockSlim();

        public IndexConnection(Func<InputType, IndexType> keyfunc, AbstractBlock source)
        {
            _uniqid = Interlocked.Increment(ref nextID);
            _getKeyFunc = keyfunc;
        }

        public AbstractBlock GetConnection(IndexType type)
        {
            AbstractBlock connOut;
            _indexBlockLock.EnterReadLock();
            try
            {
                if (!_idxConnections.TryGetValue(type, out connOut))
                {
                }
                return connOut;
            }
            finally
            {
                _indexBlockLock.ExitReadLock();
            }

            return connOut;
        }

        public bool CheckType(AbstractBlock blockToAdd)
        {
            if (!Source.BlockOutputType.IsInstanceOfType(blockToAdd.BlockInputType))
            {
                throw new Exception("Types do not match OutputType: " + Source.BlockOutputType + " InputType of next block: " + blockToAdd.BlockInputType);
                return false;
            }
            return true;
        }

        public void AddConnection(AbstractBlock block, IndexType index)
        {
            _indexBlockLock.EnterWriteLock();
            if (!CheckType(block)) return;
            try
            {
                if (_idxConnections.ContainsKey(index)) throw new Exception("Block already exists for that key");

                _idxConnections[index] = block;
            }
            finally
            {
                _indexBlockLock.ExitWriteLock();
            }
        }

        public void AddConnection(AbstractBlock block)
        {
            if (!CheckType(block)) return;
            _regBlockLock.EnterWriteLock();
            try
            {
                _regConnections.Add(block);
            }
            finally
            {
                _regBlockLock.ExitWriteLock();
            }
        }

        public InputType LastData
        {
            get { return _lastValue; }
        }

        public void EmitIndex(InputType data)
        {
            _indexBlockLock.EnterReadLock();

            try
            {
                if (_getKeyFunc == null || !_idxConnections.ContainsKey(_getKeyFunc(data))) return;
                AbstractBlock blockout;
                _idxConnections.TryGetValue(_getKeyFunc(data), out blockout);
                if (blockout != null) blockout.Accept(data);
            }
            finally
            {
                _indexBlockLock.ExitReadLock();
            }
        }

        public void EmitRegular(InputType data)
        {
            _regBlockLock.EnterWriteLock();
            try
            {
                foreach (var block in _regConnections)
                {
                    block.Accept(data);
                }
            }
            finally
            {
                _regBlockLock.ExitReadLock();
            }
        }

        public void Emit(InputType data)
        {
            if (data == null) return;
            if (Source.ProcessorOwner == null)
            {
                Console.WriteLine("Null Processor Owner");
                throw new Exception("Null Processor Owner");
            }
            EmitIndex(data);
            EmitRegular(data);
        }

        public void PrintPretty(string indent, bool last)
        {
            throw new NotImplementedException();
        }

        public bool Evaluate(InputType data)
        {
            return true;
        }

        public void RemoveConnection(AbstractBlock block, IndexType type)
        {
            _indexBlockLock.EnterWriteLock();
            try
            {
                if (!_idxConnections.Remove(type))
                {
                }
            }
            finally
            {
                _indexBlockLock.ExitWriteLock();
            }
        }

        public void RemoveConnection(AbstractBlock block)
        {
            _regBlockLock.EnterWriteLock();
            try
            {
                _regConnections.Remove(block);
            }
            finally
            {
                _regBlockLock.ExitWriteLock();
            }
        }
    }
}