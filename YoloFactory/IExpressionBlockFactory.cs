using NoQL.CEP.Blocks;
using NoQL.CEP.Blocks.Factories;
using NoQL.CEP.Connections;
using NoQL.CEP.YoloQLBlocks;
using System;
using System.Collections.Generic;

namespace NoQL.CEP.YoloFactory
{
    public interface ICEPExpression
    {
        ICEPExpression ChildExpression { get; set; }

        ICEPExpression ParentExpression { get; set; }

        AbstractBlock ThisBlock { get; set; }

        AbstractBlock Compile();
    }

    public interface ICEPExpression<InputType> : ICEPExpression
    {
        ICEPExpression<InputType> Count();

        ICEPExpression<IDictionary<IndexType, DataType>> GroupBy<DataType, IndexType>(Func<DataType, IndexType> groupFunc);

        ICEPExpression<InputType> NotIn(CEPExpression<InputType> expr);

        ICEPExpression<InputType> Perform(Action<InputType> action);

        ICEPExpression<QueryResultType> Query<IndexType, DataType, QueryResultType>(string DatabaseName,
                                                                                    Func<IndexType, DataType, QueryResultType> selectorFunction);

        ICEPExpression<InputType> Save<InputType>(string DatabaseName);

        ICEPExpression<OutputType> Select<OutputType>(Func<InputType, OutputType> selectFunc);

        ICEPExpression<SingletonType> Split<SingletonType>();

        ICEPExpression<InputType> Sum(Func<InputType, object> sumFunc);

        ICEPExpression<InputType> Where(Func<InputType, bool> whereFunc);

        ICEPExpression<IEnumerable<InputType>> Window(TimeSpan span);

        ICEPExpression<IEnumerable<InputType>> Window(int numEvents);
    }

    public class CEPExpression<InputType> : ICEPExpression<InputType>
    {
        #region Delegates

        public delegate void HandleCEPOutputDelegate(object output);

        #endregion Delegates

        public event HandleCEPOutputDelegate CEPOutputEvent;

        private AbstractBlock _tail;
        private BlockFactory blockFactory;
        private string name = "Tree";
        private Processor processor;

        internal AbstractBlock Tail
        {
            get { return _tail; }
            set { }
        }

        internal CEPExpression(Processor p)
        {
            processor = p;
            blockFactory = p.CreateBlockFactory();
        }

        private static CEPExpression<Out> CreateChildExpr<Out>(CEPExpression<InputType> parent, AbstractBlock block, string newname)
        {
            var ret = new CEPExpression<Out>(parent.processor);
            ret.processor = parent.processor;
            parent.ChildExpression = ret;
            ret.blockFactory = parent.blockFactory;
            //ret.CEPOutputEvent = parent.CEPOutputEvent;
            ret.name = newname;
            ret.ThisBlock = block;
            ret.ParentExpression = parent;
            return ret;
        }

        #region ICEPExpression<InputType> Members

        public ICEPExpression ParentExpression { get; set; }

        public ICEPExpression ChildExpression { get; set; }

        public AbstractBlock ThisBlock { get; set; }

        public AbstractBlock Compile()
        {
            var blocks = new Queue<AbstractBlock>();

            ICEPExpression currentExpr = ChildExpression;
            while (currentExpr != null)
            {
                blocks.Enqueue(currentExpr.ThisBlock);
                if (currentExpr.ChildExpression != null)
                    currentExpr = currentExpr.ChildExpression;
                else break;
            }

            AbstractBlock topBlock = blocks.Dequeue();
            AbstractBlock currentBlock = topBlock;

            foreach (AbstractBlock b in blocks)
            {
                currentBlock.AddChild(b, new Filter<object>(null));
                currentBlock = b;
            }
            _tail = blockFactory.CreateLambdaBlock<object>(o =>
                                                           {
                                                               if (CEPOutputEvent != null) CEPOutputEvent(o);
                                                               return true;
                                                           }, "tail");

            currentBlock.AddChild<object>(_tail, null);
            return topBlock;
        }

        public ICEPExpression<IDictionary<IndexType, DataType>> GroupBy<DataType, IndexType>(Func<DataType, IndexType> groupFunc)
        {
            IndexBlock<IndexType, DataType> block = blockFactory.CreateIndexBlock(groupFunc, true, name + "Group");

            return CreateChildExpr<IDictionary<IndexType, DataType>>(this, block, name + "GroupBy.");
        }

        public ICEPExpression<OutputType> Select<OutputType>(Func<InputType, OutputType> selectFunc)
        {
            TransformBlock<InputType, OutputType> block = blockFactory.CreateTransformBlock(selectFunc, name + "Select");

            return CreateChildExpr<OutputType>(this, block, name + "Select.");
        }

        public ICEPExpression<InputType> Where(Func<InputType, bool> whereFunc)
        {
            LambdaBlock<InputType> block = blockFactory.CreateLambdaBlock(whereFunc, name + "Where");

            return CreateChildExpr<InputType>(this, block, name + "Where.");
        }

        public ICEPExpression<IEnumerable<InputType>> Window(TimeSpan span)
        {
            TimeWindowBlock<InputType> block = blockFactory.CreateTimeWindowBlock<InputType>(span, name + "WindowTimeEvents");

            return CreateChildExpr<IEnumerable<InputType>>(this, block, name + "WindowTime.");
        }

        public ICEPExpression<IEnumerable<InputType>> Window(int numEvents)
        {
            EventWindowBlock<InputType> block = blockFactory.CreateEventWindowBlock<InputType>(numEvents, name + "WindowNumEvents");

            return CreateChildExpr<IEnumerable<InputType>>(this, block, name + "WindowEvents.");
        }

        //public CEPExpression<QueryResultType> Query<IndexType, QueryResultType>(string DatabaseName,
        //    Func<IndexType, QueryResultType, QueryResultType> selectorFunction)
        //{
        //    var Block = BlockFactory.CreateQueryBlock<IndexType, QueryResultType, QueryResultType>(selectorFunction,DatabaseName,name + "Query");
        //    return CreateChildExpr<QueryResultType>(this, Block, "Query.");
        //}
        public ICEPExpression<QueryResultType> Query<IndexType, DataType, QueryResultType>(string DatabaseName,
                                                                                           Func<IndexType, DataType, QueryResultType> selectorFunction)
        {
            QueryBlock<IndexType, DataType, QueryResultType> block = blockFactory.CreateQueryBlock(selectorFunction, DatabaseName, name + "Query");
            return CreateChildExpr<QueryResultType>(this, block, "Query.");
        }

        public ICEPExpression<SingletonType> Split<SingletonType>()
        {
            SplitterBlock block = blockFactory.CreateSplitterBlock("Split");
            return CreateChildExpr<SingletonType>(this, block, "Split.");
        }

        public ICEPExpression<InputType> Save<InputType>(string DatabaseName)
        {
            SaveBlock<InputType> block = blockFactory.CreateSaveBlock<InputType>(DatabaseName, name + "Save");
            return CreateChildExpr<InputType>(this, block, "Save.");
        }

        public ICEPExpression<InputType> NotIn(CEPExpression<InputType> expr)
        {
            throw new NotImplementedException();
        }

        public ICEPExpression<InputType> Sum(Func<InputType, object> sumFunc)
        {
            throw new NotImplementedException();
        }

        public ICEPExpression<InputType> Count()
        {
            throw new NotImplementedException();
        }

        public ICEPExpression<InputType> Perform(Action<InputType> action)
        {
            LambdaBlock<InputType> block = blockFactory.CreateLambdaBlock<InputType>(
                                                                                     input =>
                                                                                     {
                                                                                         action(input);
                                                                                         return true;
                                                                                     }, "Perform");

            return CreateChildExpr<InputType>(this, block, "Perform.");
        }

        #endregion ICEPExpression<InputType> Members
    }
}