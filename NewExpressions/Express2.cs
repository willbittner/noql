using NoQL.CEP.Blocks;
using NoQL.CEP.Blocks.Factories;
using NoQL.CEP.Connections;
using NoQL.CEP.Datastructures;
using NoQL.CEP.Logging;
using NoQL.CEP.YoloQLBlocks;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Timer = System.Timers.Timer;

namespace NoQL.CEP.NewExpressions
{
    public class Express2
    {
        protected static List<WeakReference<Express2>> Expressions = new List<WeakReference<Express2>>();

        protected static IEnumerable<Express2> GetLiveExpressions()
        {
            foreach (var wr in Expressions)
            {
                Express2 ret;
                if (wr.TryGetTarget(out ret))
                    yield return ret;
            }
        }

        public Express2 Creator { get; set; }

        public static BlockFactory BlockFactory { get; set; }

        private string _name;

        public string ComponentName
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private int _ID = 0;

        public int ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }

        public int CepID
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

        public static void Init(ExpressionManager mgr)
        {
            mgr.AddExpression("Join");
            mgr.AddExpression("Where");
            mgr.AddExpression("Query");
            mgr.AddExpression("Select");
            mgr.AddExpression("Perform");
            mgr.AddExpression("Window");
            mgr.AddExpression("Split");
            mgr.AddExpression("Save");
            mgr.AddExpression("Delete");
            mgr.AddExpression("Sum");
            mgr.AddExpression("SumInt");
            mgr.AddExpression("SumLong");
            mgr.AddExpression("SumDecimal");
            mgr.AddExpression("Time");
        }

        protected static Express2<NewType> CreateExpression<NewType>(AbstractBlock inBlock)
        {
            var retexpr = new Express2<NewType>();
            ExprTree.CreateComponent(retexpr, inBlock);
            return retexpr;
        }

        protected Express2()
        {
            Expressions.Add(new WeakReference<Express2>(this));
        }
    }

    public class Express2<InputType> : Express2, INewCEPExpression<InputType>
    {
        public static Express2<InputType> CreateBase<InputType>()
        {
            LambdaBlock<InputType> b = BlockFactory.CreateLambdaBlock<InputType>(x => true, "base");

            return CreateExpression<InputType>(b);
        }

        public INewCEPExpression<InputType> Else()
        {
            throw new NotImplementedException();
        }

        public INewCEPExpression<Pair<InputType, AdjoiningType>> Join<AdjoiningType>(Func<InputType, AdjoiningType> getAdjoiningMember)
        {
            TransformBlock<InputType, Pair<InputType, AdjoiningType>> newBlock = BlockFactory.CreateTransformBlock<InputType, Pair<InputType, AdjoiningType>>(x => new Pair<InputType, AdjoiningType>(x, getAdjoiningMember(x)), "Join");
            OutputBlock.AddChild(newBlock, new Filter<InputType>(type => true));
            return CreateExpression<Pair<InputType, AdjoiningType>>(InputBlock);
        }

        #region INewCEPExpression<InputType> Members

        public INewCEPExpression<IDictionary<IndexType, DataType>> GroupBy<DataType, IndexType>(Func<DataType, IndexType> groupFunc)
        {
            IndexBlock<IndexType, DataType> newBlock = BlockFactory.CreateIndexBlock(groupFunc, true, "Group");
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<IDictionary<IndexType, DataType>>(InputBlock);
        }

        public INewCEPExpression<OutputType> Select<OutputType>(Func<InputType, OutputType> selectFunc, string name = null)
        {
            TransformBlock<InputType, OutputType> newBlock = BlockFactory.CreateTransformBlock(selectFunc, String.IsNullOrEmpty(name) ? "Select" : name);
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<OutputType>(InputBlock);
        }

        public INewCEPExpression<InputType> Where(Func<InputType, bool> whereFunc)
        {
            LambdaBlock<InputType> newBlock = BlockFactory.CreateLambdaBlock<InputType>(x => true, "where");
            OutputBlock.AddChild(newBlock, new Filter<InputType>(whereFunc));
            return CreateExpression<InputType>(InputBlock);
        }

        public INewCEPExpression<InputType> Where(Func<InputType, bool> whereFunc, out INewCEPExpression<InputType> elseBranch)
        {
            Func<InputType, bool> inverseWhere = x => !whereFunc(x);
            var outelseBranch = this.Branch().Where(inverseWhere);
            var retExpr = this.Where(whereFunc);

            elseBranch = outelseBranch;
            return retExpr;
        }

        public INewCEPExpression<InputType> Where(Func<InputType, bool> whereFunc, INewCEPExpression<InputType> elseBranch)
        {
            Func<InputType, bool> inverseWhere = x => !whereFunc(x);
            this.Branch().Where(inverseWhere).Attach(elseBranch);
            var retExpr = this.Where(whereFunc);
            return retExpr;
        }

        public INewCEPExpression<InputType> Name<T>(string name)
        {
            OutputBlock.DebugName = name + "_" + OutputBlock.DebugName;
            //OutputBlock.DebugName = name + "_" + OutputBlock.DebugName;
            return this;
        }

        public INewCEPExpression<InputType> Name(string name)
        {
            OutputBlock.DebugName = name + "_" + OutputBlock.DebugName;
            //OutputBlock.DebugName = name + "_" + OutputBlock.DebugName;
            return this;
        }

        public INewCEPExpression<IEnumerable<InputType>> Window(TimeSpan span)
        {
            TimeWindowBlock<InputType> newBlock = BlockFactory.CreateTimeWindowBlock<InputType>(span, "WindowTimeEvents");
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<IEnumerable<InputType>>(InputBlock);
        }

        public INewCEPExpression<IEnumerable<InputType>> Window(int numEvents)
        {
            EventWindowBlock<InputType> newBlock = BlockFactory.CreateEventWindowBlock<InputType>(numEvents, "WindowNumEvents");
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<IEnumerable<InputType>>(InputBlock);
        }

        public INewCEPExpression<KeyValuePair<InputType, IEnumerable<OutputType>>> Query<OutputType>(string databaseName, string indexName, Func<InputType, object> indexFunc)
        {
            TransformBlock<InputType, KeyValuePair<InputType, IEnumerable<OutputType>>> newBlock = BlockFactory.CreateTransformBlock<InputType, KeyValuePair<InputType, IEnumerable<OutputType>>>(
                                                                                                                                                                                                  (x =>
                                                                                                                                                                                                   {
                                                                                                                                                                                                       object kv = indexFunc(x);
                                                                                                                                                                                                       IRamDB db = BlockFactory.EventProcessor.GetRamDb(databaseName);
                                                                                                                                                                                                       return new KeyValuePair<InputType, IEnumerable<OutputType>>(x, db.GetEnumerable<OutputType>(indexName, kv));
                                                                                                                                                                                                   }), "SimpleQuery");
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<KeyValuePair<InputType, IEnumerable<OutputType>>>(InputBlock);
        }

        public INewCEPExpression<SingletonType> Split<SingletonType>()
        {
            SplitterBlock newBlock = BlockFactory.CreateSplitterBlock("Split");
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<SingletonType>(InputBlock);
        }

        public class SplitTag
        {
            private static int _splitTag = 0;

            public static int NextID
            {
                get
                {
                    return Interlocked.Increment(ref _splitTag);
                }
            }

            public int Tag;
            public int Count;
            public int ReconstructionCount;

            public SplitTag(int count)
            {
                Tag = NextID;
                Count = count;
            }
        }

        //public INewCEPExpression<Pair<SplitTag,SingletonType>> W<SingletonType>()
        //{
        //    this.Select(x => new Pair<SplitTag, InputType>(new SplitTag(), x));
        //    SplitterBlock newBlock = BlockFactory.CreateSplitterBlock("Split");
        //    OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
        //    return CreateExpression<Pair<SplitTag, SingletonType>>(InputBlock);
        //}

        //public INewCEPExpression<IEnumerable<SingletonType>> UnRetainSplit<SingletonType>()
        //{
        //    var input = Yolo.Express<Pair<SplitTag, SingletonType>>();
        //    Yolo.Express<Pair<SplitTag,SingletonType>>().GroupBy<Pair<SplitTag,SingletonType>,int>(x => x.Left.Tag ).Split<KeyValuePair<int,Pair<SplitTag,SingletonType>>>().Where(x => x.Value)
        //}

        public INewCEPExpression<Pair<IEnumerable<SingletonType>, SingletonType>> Explode<SingletonType>()
        {
            var newBlock = BlockFactory.CreateAnonymousBlock<InputType>("Explode");

            newBlock.OnDataEvent = (data, block) =>
                                   {
                                       var dataEnum = (data as IEnumerable<SingletonType>).ToList();

                                       if (dataEnum == null)
                                       {
                                           throw new Exception("Explode block recieved a non-IEnumerable event.");
                                       }

                                       var list = new BlockingCollection<SingletonType>(new ConcurrentQueue<SingletonType>(dataEnum));
                                       if (list.Count <= 0)
                                           return false; //Return false now,
                                       //don't block the thread
                                       //on consuming enumerable
                                       //if queue is empty.

                                       int i = 0;
                                       foreach (var item in list.GetConsumingEnumerable())
                                       {
                                           var breaking = list.Count == 0;
                                           block.SendToChildren(new Pair<IEnumerable<SingletonType>, SingletonType>(dataEnum, item));
                                           if (breaking) break;
                                       }

                                       return false;
                                   };

            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<Pair<IEnumerable<SingletonType>, SingletonType>>(InputBlock);
        }

        public INewCEPExpression<IEnumerable<SingletonType>> Implode<SingletonType>()
        {
            var Queue = new BlockingCollection<SingletonType>();

            var newBlock = BlockFactory.CreateAnonymousBlock<Pair<IEnumerable<SingletonType>, SingletonType>>("Implode");
            newBlock.OnDataEvent = (data, block) =>
                                   {
                                       if (!Queue.Intersect(data.Left).Any() && Queue.Count != 0)
                                       {
                                           block.SendToChildren(Queue);
                                           Queue = new BlockingCollection<SingletonType>();
                                       }
                                       else
                                       {
                                           Queue.Add(data.Right);
                                       }
                                       return false;
                                   };
            OutputBlock.AddChild(newBlock, new Filter<InputType>(type => true));

            return CreateExpression<IEnumerable<SingletonType>>(InputBlock);
        }

        /// <summary>
        /// This block, takes in an input of InputType, and will route to exprs added with addRoute. It will return the input type from
        /// the input expr. the Delete expr, will delete routes with that key. NOTE THIS EXPR RETURNS NULL
        /// </summary>
        /// <typeparam name="RouteKeyType"></typeparam>
        /// <param name="addRouteExpr"></param>
        /// <param name="removeExpr"></param>
        /// <returns></returns>
        public INewCEPExpression<InputType> Route<RouteKeyType>(Func<InputType, RouteKeyType> keyFunc, INewCEPExpression<KeyValuePair<RouteKeyType, INewComponent>> addRouteExpr, INewCEPExpression<RouteKeyType> removeExpr = null)
        {
            ConcurrentDictionary<RouteKeyType, INewCEPExpression<InputType>> routeDict = new ConcurrentDictionary<RouteKeyType, INewCEPExpression<InputType>>();
            var routeblock = BlockFactory.CreateAnonymousBlock<InputType>("RouterExpr");
            routeblock.OnDataEvent = (data, block) =>
            {
                INewCEPExpression<InputType> result;
                if (routeDict.TryGetValue(keyFunc(data), out result))
                {
                    if (result != null) result.Send(data);
                    else
                    {
                        throw new Exception("Route expr found, but is null. WTF?");
                    }
                }
                else
                {
                    throw new Exception("RouteBlock ID=" + block.ComponentID + " Could not find route for Key=" + keyFunc(data) + " data=" + data);
                }
                return false;
            };
            ExprFromBlock<InputType>(routeblock);

            var addRouteBlock = BlockFactory.CreateAnonymousBlock<KeyValuePair<RouteKeyType, INewCEPExpression<InputType>>>("AddRouteExpr");
            addRouteBlock.OnDataEvent = (data, block) =>
                                        {
                                            routeDict[data.Key] = data.Value;
                                            return false;
                                        };
            ExprFromBlock<InputType>(addRouteBlock);
            return null;
        }

        /// <summary>
        /// Takes a stream of input type and caches them. Then when a input of KeyType comes into LookupInput, it will SEAL,
        /// and lookup the value, and will throw exception if there is value for that key, and retunr the value if found
        /// on lookupInput
        /// </summary>
        /// <typeparam name="InputType"></typeparam>
        /// <typeparam name="KeyType"></typeparam>
        /// <param name="LookupInput"></param>
        /// <param name="keyFunc"></param>
        /// <returns></returns>
        public INewCEPExpression<InputType> Cache<KeyType>(INewCEPExpression<KeyType> LookupInput, Func<InputType, KeyType> keyFunc, INewCEPExpression<KeyType> deleteExpr = null)
        {
            ConcurrentDictionary<KeyType, InputType> dict = new ConcurrentDictionary<KeyType, InputType>();
            var newBlock = BlockFactory.CreateAnonymousBlock<InputType>("InputToCache");
            newBlock.OnDataEvent = (data, block) =>
            {
                dict[keyFunc(data)] = data;
                block.SendToChildren(data);
                return false;
            };

            var lookupBlock = BlockFactory.CreateAnonymousBlock<KeyType>("LookupCache");
            lookupBlock.OnDataEvent = (data, block) =>
                                            {
                                                InputType result;
                                                if (dict.TryGetValue(data, out result))
                                                {
                                                    block.SendToChildren(result);
                                                }
                                                else
                                                {
                                                    throw new Exception("CacheBlock ID=" + block.ComponentID + " Could not find Key=" + data);
                                                }
                                                return false;
                                            };
            ExprFromBlock<InputType>(lookupBlock, LookupInput);
            if (deleteExpr != null)
            {
                var deleteBlock = BlockFactory.CreateAnonymousBlock<KeyType>("DeleteCache");
                deleteBlock.OnDataEvent = (data, block) =>
                                          {
                                              InputType outval;
                                              dict.TryRemove(data, out outval);
                                              return true;
                                          };
                ExprFromBlock<KeyType>(deleteBlock, deleteExpr);
            }
            OutputBlock.AddChild(newBlock, new Filter<InputType>(type => true));
            return CreateExpression<InputType>(InputBlock);
        }

        public INewCEPExpression<InputType> Save<InputType>(string DatabaseName)
        {
            if (!BlockFactory.EventProcessor.RamDatabase.ContainsKey(DatabaseName))
            {
                BlockFactory.EventProcessor.RamDatabase[DatabaseName] = RamDBFactory.NewRamDb();
            }
            SaveBlock<InputType> newBlock = BlockFactory.CreateSaveBlock<InputType>(DatabaseName, "Save");
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<InputType>(InputBlock);
        }

        public INewCEPExpression<InputType> Update<InputType>(string DatabaseName, UpdatePolicy policy)
        {
            if (!BlockFactory.EventProcessor.RamDatabase.ContainsKey(DatabaseName))
            {
                BlockFactory.EventProcessor.RamDatabase[DatabaseName] = RamDBFactory.NewRamDb();
            }
            UpdateBlock<InputType> newBlock = BlockFactory.CreateUpdateBlock<InputType>(DatabaseName, "Save", policy);
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<InputType>(InputBlock);
        }

        public INewCEPExpression<InputType> Delete(string DatabaseName)
        {
            if (!BlockFactory.EventProcessor.RamDatabase.ContainsKey(DatabaseName))
            {
                BlockFactory.EventProcessor.RamDatabase[DatabaseName] = RamDBFactory.NewRamDb();
            }
            LambdaBlock<InputType> newBlock = BlockFactory.CreateLambdaBlock<InputType>(x =>
                                                                                        {
                                                                                            BlockFactory.EventProcessor.RamDatabase[DatabaseName].Delete(x);
                                                                                            return true;
                                                                                        }, "Delete");
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<InputType>(InputBlock);
        }

        public INewCEPExpression<InputType> NotIn(INewCEPExpression<InputType> expr)
        {
            throw new NotImplementedException();
        }

        public INewCEPExpression<OutputType> Sum<OutputType>(Func<InputType, OutputType, OutputType> sumFunc) where OutputType : new()
        {
            AggregationBlock<InputType, OutputType> newBlock = BlockFactory.CreateAggregationBlock<InputType, OutputType>("Sum");
            newBlock.AggregationFunction = sumFunc;
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<OutputType>(InputBlock);
        }

        public INewCEPExpression<int> SumInt()
        {
            AggregationBlock<int, int> newBlock = BlockFactory.CreateAggregationBlock<int, int>("SumInt");
            newBlock.AggregationFunction = (x, y) => x + y;
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<int>(InputBlock);
        }

        public INewCEPExpression<long> SumLong()
        {
            AggregationBlock<long, long> newBlock = BlockFactory.CreateAggregationBlock<long, long>("SumLong");
            newBlock.AggregationFunction = (x, y) => x + y;
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<long>(InputBlock);
        }

        public INewCEPExpression<decimal> SumDecimal()
        {
            AggregationBlock<decimal, decimal> newBlock = BlockFactory.CreateAggregationBlock<decimal, decimal>("SumLong");
            newBlock.AggregationFunction = (x, y) => x + y;
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<decimal>(InputBlock);
        }

        public INewCEPExpression<InputType> Perform(Action<InputType> action)
        {
            LambdaBlock<InputType> newBlock = BlockFactory.CreateLambdaBlock<InputType>(
                                                                                        input =>
                                                                                        {
                                                                                            action(input);
                                                                                            return true;
                                                                                        }, "Perform");
            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<InputType>(InputBlock);
        }

        public INewCEPExpression<Pair<DateTime, InputType>> Time()
        {
            TransformBlock<InputType, Pair<DateTime, InputType>> newBlock =
                BlockFactory.CreateTransformBlock<InputType, Pair<DateTime, InputType>>
                (input => new Pair<DateTime, InputType>(BlockFactory.EventProcessor.TimeProvider.GetTime(), input), "Time");

            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<Pair<DateTime, InputType>>(InputBlock);
        }

        public INewCEPExpression<InputType> Delay(double intervalMs)
        {
            var newBlock = BlockFactory.CreateDelayBlock<InputType>(intervalMs, "delay");
            OutputBlock.AddChild(newBlock, new Filter<InputType>(null));
            return CreateExpression<InputType>(InputBlock);
        }

        public INewCEPExpression<object> Timer(double intervalMs)
        {
            Timer timer = new Timer(intervalMs * BlockFactory.EventProcessor.TimeProvider.GetTimeCoefficient());
            timer.Elapsed += (sender, args) => this.OutputBlock.Accept(new object());

            LambdaBlock<InputType> newBlock = BlockFactory.CreateLambdaBlock<InputType>(type => true, "Branch");

            OutputBlock.AddChild(newBlock, new Filter<InputType>(x => true));
            return CreateExpression<object>(InputBlock);
        }

        public INewCEPExpression<Pair<InputType, OtherType>> Merge<OtherType>(INewCEPExpression<OtherType> otherExpression, Func<Pair<InputType, OtherType>, bool> mergeOn = null, bool waitForMerge = true)
        {
            InputType leftLast = default(InputType);
            OtherType rightLast = default(OtherType);

            bool seenLeft = false, seenRight = false;

            TransformBlock<InputType, Pair<InputType, OtherType>> leftNewBlock =
                BlockFactory.CreateTransformBlock<InputType, Pair<InputType, OtherType>>
                    (input =>
                     {
                         leftLast = input;
                         seenLeft = true;
                         return new Pair<InputType, OtherType>(leftLast, rightLast);
                     }, "MergeLeftBlock");

            TransformBlock<OtherType, Pair<InputType, OtherType>> rightNewBlock =
                BlockFactory.CreateTransformBlock<OtherType, Pair<InputType, OtherType>>
                    (input =>
                    {
                        rightLast = input;
                        seenRight = true;
                        return new Pair<InputType, OtherType>(leftLast, rightLast);
                    }, "MergeRightBlock");

            LambdaBlock<Pair<InputType, OtherType>> newBlock = BlockFactory.CreateLambdaBlock<Pair<InputType, OtherType>>
                (x => ((((seenLeft && seenRight) || (!waitForMerge)) && (mergeOn == null || mergeOn(x)))), "MergeBlock");

            otherExpression.OutputBlock.AddChild<OtherType>(rightNewBlock, new Filter<OtherType>(null));

            OutputBlock.AddChild(leftNewBlock, new Filter<InputType>(null));

            leftNewBlock.AddChild<Pair<InputType, OtherType>>(newBlock, new Filter<Pair<InputType, OtherType>>(null));
            rightNewBlock.AddChild<Pair<InputType, OtherType>>(newBlock, new Filter<Pair<InputType, OtherType>>(null));

            return CreateExpression<Pair<InputType, OtherType>>(InputBlock);
        }

        public INewCEPExpression<Pair<IEnumerable<InputType>, TriggerType>> IndexTrigger<TriggerType>(INewCEPExpression<TriggerType> TriggerExpr, Func<InputType, TriggerType, bool> MatchFunction, bool deleteOnMatch = true)
        {
            IRamDB db = new RamDB();
            db.Init<InputType>();

            var retention = this.Perform(db.Add);

            var jointTrigger = TriggerExpr.Branch().Select(trigger =>
            {
                lock (db)
                {
                    var ret = db.GetEnumerable<InputType>().ToList()
                        .Where(x => MatchFunction(x, trigger)).ToList();

                    if (deleteOnMatch)
                    {
                        foreach (var obj in ret)
                        {
                            db.Delete(obj); //remove from RamDB
                        }
                    }

                    return new Pair<Pair<IEnumerable<InputType>, TriggerType>, bool>(new Pair<IEnumerable<InputType>, TriggerType>(ret, trigger), false);
                }
            }, "trigger_transform").Where(x => x.Left.Left.Any());

            retention.Merge(jointTrigger).Select(x => x.Right).Where(x => !x.Right).Perform(x => x.Right = true).Select(x => x.Left);

            return CreateExpression<Pair<IEnumerable<InputType>, TriggerType>>(InputBlock);
        }

        public INewCEPExpression<InputType> Count()
        {
            throw new NotImplementedException();
        }

        public INewCEPExpression<InputType> Log(LogSeverity severity, string message)
        {
            LambdaBlock<InputType> newBlock = BlockFactory.CreateLambdaBlock<InputType>(x =>
            {
                BlockFactory.EventProcessor.LoggingProvider.LogEvent(severity, this.OutputBlock, x, message);
                return true;
            }, "LogBlock");

            OutputBlock.AddChild<InputType>(newBlock, null);
            return CreateExpression<InputType>(InputBlock);
        }

        /// <summary>
        /// On Error "fork"
        /// </summary>
        /// <param name="ErrorAction">A func that will recieve the
        /// offending object, the raised exception and will
        /// return true if the block should pass the error
        /// to the processor's global error handler
        /// or false to stop the propagation of the error
        /// </param>
        /// <returns></returns>
        public INewCEPExpression<InputType> OnError(Action<object, Exception> ErrorAction, bool bubbleToProcessor = false)
        {
            Func<object, Exception, bool> func = (o, exception) =>
                                               {
                                                   ErrorAction(o, exception);
                                                   return bubbleToProcessor;
                                               };
            this.OutputBlock.ErrorAction = func;
            return this;
        }

        public INewCEPExpression<InputType> Branch<InputType>(out INewCEPExpression<InputType> expr)
        {
            LambdaBlock<InputType> newBlock = BlockFactory.CreateLambdaBlock<InputType>(type => true, "Branch");

            expr = CreateExpression<InputType>(newBlock);
            Attach(expr);

            return (INewCEPExpression<InputType>)this;
        }

        public INewCEPExpression<InputType> Branch<InputType>(INewCEPExpression<InputType> expr)
        {
            Attach(expr);
            return (INewCEPExpression<InputType>)this;
        }

        public INewCEPExpression<InputType> Express()
        {
            return this;
        }

        private INewCEPExpression<OutputType> ExprFromBlock<OutputType>(AbstractBlock block, INewComponent inputExpr = null)
        {
            var expr = CreateExpression<InputType>(block);
            if (inputExpr == null)
            {
                Attach(expr);
                return (INewCEPExpression<OutputType>)this;
            }
            else
            {
                inputExpr.Attach(expr);
                return inputExpr as INewCEPExpression<OutputType>;
            }
        }

        private AbstractBlock _finalInputBlock { get; set; }

        private AbstractBlock _finalOutputBlock { get; set; }

        public AbstractBlock OutputBlock
        {
            get
            {
                if (_finalOutputBlock == null) return ExprTree.GetOutputBlock(ID);
                return _finalOutputBlock;
            }
            set
            {
                if (ID == 0) throw new Exception("Bad juju, you never set OuputBlock");
            }
        }

        public AbstractBlock InputBlock
        {
            get
            {
                if (_finalInputBlock == null) return ExprTree.GetInputBlock(ID);
                return _finalInputBlock;
            }
            set
            {
                if (ID == 0) throw new Exception("Bad juju Don't Set Inputblock until you set the Components ID");
                if (_finalInputBlock != null && _finalInputBlock.ComponentID == value.ComponentID) throw new Exception("InputBlock is already set, you can't set it again unless you have a new componentID");
                _finalInputBlock = value;
            }
        }

        protected void ForceAttach(INewComponent component)
        {
            OutputBlock.AddChild(component.InputBlock, new Filter<InputType>(null));
        }

        public INewCEPExpression<InputType> Seal()
        {
            _finalInputBlock = InputBlock;
            _finalOutputBlock = OutputBlock;
            return this;
        }

        public void Attach(INewComponent component)
        {
            if (GetLiveExpressions().Any(x => x.Creator == this))
                throw new Exception("Attaching mid-expression (potentially orphaned expressions?).\nThis is inappropriate use of attach, use branch instead. ");
            ForceAttach(component);
        }

        public void Detach(INewComponent component)
        {
            OutputBlock.RemoveChild(component.InputBlock);
        }

        #endregion INewCEPExpression<InputType> Members

        public INewCEPExpression<InputType> Branch()
        {
            INewCEPExpression<InputType> bexpr;
            Branch(out bexpr);
            return bexpr;
        }

        public INewCEPExpression<InputType> Name(INewComponent name)
        {
            return this.Name(name.ComponentName);
        }

        public INewCEPExpression<InputType> Name<T>(INewComponent name)
        {
            return this.Name(name.ComponentName);
        }

        private BaseInputAdapter adapter;

        public void Send(object data)
        {
            if (adapter == null)
            {
                adapter = new BaseInputAdapter();
                adapter.Attach(this);
            }
            adapter.Send(data);
        }

        public void OnReceive<T>(Action<T> recvFunc)
        {
            BaseOutputAdapter<T> objectOutAdapter = new BaseOutputAdapter<T>();
            objectOutAdapter.SetFunction(recvFunc, this);
        }
    }
}