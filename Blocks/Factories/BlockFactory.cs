using NoQL.CEP.NewExpressions;
using NoQL.CEP.YoloQLBlocks;
using System;
using System.Collections.Generic;

namespace NoQL.CEP.Blocks.Factories
{
    public class BlockFactory
    {
        public Processor EventProcessor { get; private set; }

        internal BlockFactory(Processor p)
        {
            EventProcessor = p;
        }

        public AggregationBlock<MessageType, AggregationType> CreateAggregationBlock<MessageType, AggregationType>(string name) where AggregationType : new()
        {
            var block =
                new AggregationBlock<MessageType, AggregationType>(EventProcessor);
            SetDebugName(block, name);
            return block;
        }

        public CollectionBlock<T> CreateCollectionBlock<T>(AbstractBlock outputblock) where T : IComparable
        {
            var col = new CollectionBlock<T>(outputblock, EventProcessor);
            SetDebugName(col, "CollectionName");
            return col;
        }

        public EventWindowBlock<T> CreateEventWindowBlock<T>(int windowSize, string name)
        {
            var windowBlock = new EventWindowBlock<T>(EventProcessor, windowSize);
            SetDebugName(windowBlock, name);
            return windowBlock;
        }

        public IndexBlock<IndexType, DataType> CreateIndexBlock<IndexType, DataType>(Func<DataType, IndexType> IndexFunction, bool isAggregator, string name)
        {
            var block = new IndexBlock<IndexType, DataType>(EventProcessor, IndexFunction, isAggregator);
            SetDebugName(block, name);
            return block;
        }

        public IntervalBlock<MessageType> CreateIntervalBlock<MessageType>(int intervalMS, string name)
        {
            var block = new IntervalBlock<MessageType>(EventProcessor, intervalMS);
            SetDebugName(block, name);
            return block;
        }

        public LambdaBlock<MessageType> CreateLambdaBlock<MessageType>(Func<MessageType, bool> LambdaFunction, string name)
        {
            var block = new LambdaBlock<MessageType>(EventProcessor, LambdaFunction);
            SetDebugName(block, name);
            return block;
        }

        public QueryBlock<EventInputType, DatabaseQueryType, QueryReturnType> CreateQueryBlock<EventInputType, DatabaseQueryType, QueryReturnType>
            (Func<EventInputType, IEnumerable<DatabaseQueryType>, IEnumerable<QueryReturnType>> queryFunction, string dbName, string name)
        {
            var block = new QueryBlock<EventInputType, DatabaseQueryType, QueryReturnType>(EventProcessor, EventProcessor.GetRamDb(dbName), queryFunction);
            SetDebugName(block, name);
            return block;
        }

        public QueryBlock<EventInputType, DatabaseQueryType, QueryReturnType> CreateQueryBlock<EventInputType, DatabaseQueryType, QueryReturnType>
            (Func<EventInputType, DatabaseQueryType, QueryReturnType> queryFunction, string dbName, string name)
        {
            var block = new QueryBlock<EventInputType, DatabaseQueryType, QueryReturnType>(EventProcessor, EventProcessor.GetRamDb(dbName), queryFunction);
            SetDebugName(block, name);
            return block;
        }

        public SaveBlock<MessageType> CreateSaveBlock<MessageType>(string dbName, string name)
        {
            var block = new SaveBlock<MessageType>(EventProcessor, EventProcessor.GetRamDb(dbName));
            SetDebugName(block, name);
            return block;
        }

        public TransformScriptBlock<ScriptInType, ScriptOutType> CreateScriptBlock<ScriptInType, ScriptOutType>(string scriptName, string name)
        {
            var block = new TransformScriptBlock<ScriptInType, ScriptOutType>(EventProcessor, scriptName);
            SetDebugName(block, name);
            return block;
        }

        public SignalBlock<MessageType> CreateSignalBlock<MessageType>(string name)
        {
            var block = new SignalBlock<MessageType>(EventProcessor);
            SetDebugName(block, name);
            return block;
        }

        public SplitterBlock CreateSplitterBlock(string name)
        {
            var block = new SplitterBlock(EventProcessor);
            SetDebugName(block, name);
            return block;
        }

        public TimeWindowBlock<T> CreateTimeWindowBlock<T>(TimeSpan windowSize, string name)
        {
            var windowBlock = new TimeWindowBlock<T>(EventProcessor, windowSize);
            SetDebugName(windowBlock, name);
            return windowBlock;
        }

        public TransformBlock<MessageType, OutputType> CreateTransformBlock<MessageType, OutputType>(Func<MessageType, OutputType> LambdaFunction, string name)
        {
            var block = new TransformBlock<MessageType, OutputType>(EventProcessor, LambdaFunction);
            SetDebugName(block, name);
            return block;
        }

        public UpdateBlock<MessageType> CreateUpdateBlock<MessageType>(string dbName, string name, UpdatePolicy policy)
        {
            var block = new UpdateBlock<MessageType>(EventProcessor, EventProcessor.GetRamDb(dbName), policy);
            SetDebugName(block, name);
            return block;
        }

        public DelayBlock<MessageType> CreateDelayBlock<MessageType>(double IntervalMS, string name)
        {
            var block = new DelayBlock<MessageType>(EventProcessor, IntervalMS);
            SetDebugName(block, name);
            return block;
        }

        public AnonymousBlock<MessageType> CreateAnonymousBlock<MessageType>(string name)
        {
            var block = new AnonymousBlock<MessageType>(EventProcessor);
            SetDebugName(block, name);
            return block;
        }

        public static void SetDebugName(AbstractBlock block, string name)
        {
            block.DebugName = name;
            if (name == "") throw new Exception("Cant ahve a null Block debug name");
        }
    }
}