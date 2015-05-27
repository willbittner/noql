using NoQL.CEP.Blocks;
using NoQL.CEP.Blocks.Factories;
using NoQL.CEP.Connections;
using NoQL.CEP.JobManagers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NoQL.CEP.NewExpressions
{
    public class YoloList<T> : YoloEnumerable<T>, IList<T> where T : IComparable
    {
        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(T item)
        {
            CollectionOp<T> op = new CollectionOp<T>();
            op.OpType = CollectionOpTypes.Add;
            op.Value = item;
            WaitForOp(op);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public new int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(T item)
        {
            CollectionOp<T> op = new CollectionOp<T>();
            op.OpType = CollectionOpTypes.Remove;
            op.Value = item;
            input.Send(item);
            return true;
        }

        private T WaitForOp(CollectionOp<T> op)
        {
            bool doneWaiting = false;
            T val = default(T);

            var tempoutputBlock = BlockFactory.CreateLambdaBlock<CollectionOp<T>>(x =>
            {
                if (x.OpID == op.OpID)
                {
                    op = x;
                    val = x.Value;
                    doneWaiting = true;
                }

                return true;
            }, "YoloEnumerableOutput");
            outputBlock.AddChild(tempoutputBlock, new Filter<CollectionOp<T>>(null));
            input.Send(op);
            while (!doneWaiting)
            {
            }
            outputBlock.RemoveChild(tempoutputBlock);
            return val;
        }
    }

    public class YoloEnumerable<T> : Express2<T>, IEnumerable<T> where T : IComparable
    {
        protected AbstractBlock outputBlock;
        protected CollectionBlock<T> firstBlock;
        protected BaseInputAdapter input = new BaseInputAdapter("YoloEnumInputBase");

        public YoloEnumerable()
        {
            outputBlock = BlockFactory.CreateLambdaBlock<CollectionOp<T>>(x => true, "YoloEnumerableOutput");
            firstBlock = BlockFactory.CreateCollectionBlock<T>(outputBlock);

            ExprTree.CreateComponent(this, firstBlock);

            ExprTree.CreateIndependentBlock(outputBlock);
            input.Attach(this);
            Yolo.DumpGraph();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new YoloEnumerator<T>(outputBlock, input);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new YoloEnumerator<T>(outputBlock, input);
        }
    }

    public class YoloEnumerator<T> : Express2, IEnumerator<T>
    {
        private AbstractBlock outputBlock;
        private AbstractBlock parentOutput;
        private T currentValue;

        private CollectionOp<T> _colOp;

        private CollectionOp<T> EnumOp
        {
            get { return _colOp; }
            set
            {
                _colOp = value;
                parentOutput.RemoveChild(outputBlock);
            }
        }

        private int listPosition = -1;

        public YoloEnumerator(AbstractBlock output, BaseInputAdapter inputer)
        {
            CollectionOp<T> enumOp = new CollectionOp<T>();
            enumOp.OpType = CollectionOpTypes.Enumerate;
            parentOutput = output;
            outputBlock = BlockFactory.CreateLambdaBlock<CollectionOp<T>>(x =>
                                                                          {
                                                                              if (x.OpID == enumOp.OpID)
                                                                              {
                                                                                  EnumOp = x;
                                                                                  output.RemoveChild(outputBlock);
                                                                              }

                                                                              return true;
                                                                          }, "YoloEnumerableOutput");
            output.AddChild(outputBlock, new Filter<CollectionOp<T>>(null));
            inputer.Send(enumOp);
        }

        public T Current
        {
            get { return EnumOp.EnumCollection[listPosition]; }
        }

        public void Dispose()
        {
        }

        object System.Collections.IEnumerator.Current
        {
            get { return (object)currentValue; }
        }

        public bool MoveNext()
        {
            if (listPosition >= EnumOp.EnumCollection.Count - 1) return false;
            listPosition++;
            return true;
        }

        public void Reset()
        {
            listPosition = 0;
        }
    }

    public enum CollectionOpTypes
    {
        Add = 1,
        Remove = 2,
        Update = 3,
        Find = 4,
        Enumerate = 5
    }

    public class CollectionOp<T>
    {
        private int _opID;
        private static int _nextOpID = 0;

        public int OpID { get { return _opID; } }

        public CollectionOp()
        {
            _opID = Interlocked.Increment(ref _nextOpID);
        }

        public CollectionOpTypes OpType { get; set; }

        public T Value { get; set; }

        public Func<T, bool> FindFunction { get; set; }

        public List<T> EnumCollection = new List<T>();
    }

    public class CollectionBlock<T> : AbstractBlock where T : IComparable
    {
        private static int _nextID = -1;
        private ConcurrentDictionary<int, Connection> otherConnectionBlocks = new ConcurrentDictionary<int, Connection>();
        private Connection outputConnection;
        private T value;
        private BlockFactory Factory;

        private static int nextID
        {
            get
            {
                if (_nextID == -1) return Interlocked.Increment(ref _nextID);
                else return _nextID;
            }
        }

        public int CollectionBlockID { get; set; }

        public CollectionBlock(AbstractBlock output, Processor p)
            : base(p)
        {
            Factory = new BlockFactory(p);
            var filter = new Filter<CollectionOp<T>>(null);

            var connection = (new Connection<CollectionOp<T>> { ConnectionFilter = filter, Destination = output });
            connection.Source = this;
            outputConnection = connection;
            var a = nextID;
        }

        public override bool OnData(object data)
        {
            var op = (CollectionOp<T>)data;
            switch (op.OpType)
            {
                case CollectionOpTypes.Add:
                    if (EqualityComparer<T>.Default.Equals(value, default(T)))
                    {
                        value = op.Value;
                        SendToOutput(op);
                    }
                    else
                    {
                        if (Children.Count == 0)
                        {
                            AddChild(Factory.CreateCollectionBlock<T>(outputConnection.Destination), new Filter<CollectionOp<T>>(null));
                        }
                        return true;
                    }
                    break;

                case CollectionOpTypes.Find:

                    if (op.FindFunction(value))
                    {
                        op.Value = value;
                        SendToOutput(op);
                    }
                    else return true;

                    break;

                case CollectionOpTypes.Remove:
                    if (op.FindFunction == null)
                    {
                        if (op.Value.Equals(value))
                        {
                            op.Value = value;
                            value = default(T);
                            SendToOutput(op);
                        }
                    }
                    else if (op.FindFunction(value))
                    {
                        op.Value = value;
                        value = default(T);
                        SendToOutput(op);
                    }
                    else return true;
                    break;

                case CollectionOpTypes.Update:
                    throw new NotImplementedException();
                    break;

                case CollectionOpTypes.Enumerate:
                    if (value != null) op.EnumCollection.Add(value);
                    if (Children.Count == 0) SendToOutput(op);
                    else return true;
                    break;
            }
            return false;
        }

        private void SendToOutput(CollectionOp<T> op)
        {
            if (outputConnection.Evaluate(op))
            {
                var newJob = Processor.ObjectPool.GetObject<Job>();
                newJob.Data = op;
                newJob.JobConnection = outputConnection;
                ProcessorOwner.JobManager.Schedule(newJob);
            }
        }

        public override Type BlockInputType
        {
            get { return typeof(T); }
        }

        public override Type BlockOutputType
        {
            get { return typeof(IEnumerable<T>); }
        }
    }
}