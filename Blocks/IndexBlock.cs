using NoQL.CEP.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoQL.CEP.Blocks
{
    /// <summary>
    ///     Index Block accepts type DataType, indexes it by a KeyFunction() definition
    ///     and emits down the CEP-Tree a ConcurrentDictionary&lt;IndexType,DataType&gt;
    /// </summary>
    /// <typeparam name="IndexType">The type by which data should be indexed</typeparam>
    /// <typeparam name="DataType">The datat type stored in the index</typeparam>
    public class IndexBlock<IndexType, DataType> : AbstractBlock
    {
        private bool isAggregator;

        public ConcurrentDictionary<IndexType, List<DataType>> AggregateDictionary { get; set; }

        public ConcurrentDictionary<IndexType, DataType> Dictionary { get; set; }

        public Func<DataType, IndexType> KeyFunction { get; set; }

        internal IndexBlock(Processor p, Func<DataType, IndexType> keyFunction, bool aggregateAndEmitListOnIndex)
            : base(p)
        {
            KeyFunction = keyFunction;
            isAggregator = aggregateAndEmitListOnIndex;

            Dictionary = new ConcurrentDictionary<IndexType, DataType>();
            AggregateDictionary = new ConcurrentDictionary<IndexType, List<DataType>>();
        }

        public override bool OnData(object data)
        {
            if (!(data is DataType))
                throw new BlockTypeMismatchException(typeof(DataType), data.GetType(), this);

            var d = (DataType)data;

            // :HACK: lol
            if (isAggregator)
            {
                List<DataType> temp;
                AggregateDictionary.TryGetValue(KeyFunction(d), out temp);
                if (temp == null) AggregateDictionary[KeyFunction(d)] = new List<DataType>();
                AggregateDictionary[KeyFunction(d)].Add(d);
                SendToChildren(AggregateDictionary);
            }
            else
            {
                Dictionary[KeyFunction(d)] = d;
                SendToChildren(Dictionary);
            }

            return false;
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
}