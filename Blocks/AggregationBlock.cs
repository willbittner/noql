using NoQL.CEP.Exceptions;
using System;

namespace NoQL.CEP.Blocks
{
    public class AggregationBlock<MessageType, AggregationType> : AbstractBlock where AggregationType : new()
    {
        public bool FirstRun = true;

        /// <summary>
        ///     Aggregation Function: Gives the message type for the new data,
        ///     and the current aggregation value, and returns the new aggregation value
        /// </summary>
        public Func<MessageType, AggregationType, AggregationType> AggregationFunction { get; set; }

        public AggregationType AggregationValue { get; set; }

        internal AggregationBlock(Processor p)
            : base(p)
        {
            Initialize();
        }

        /// <summary>
        ///     Initializes aggregation value
        /// </summary>
        private void Initialize()
        {
            lock (this)
            {
                AggregationValue = new AggregationType();
            }
        }

        public override bool OnData(object data)
        {
            if (!(data is MessageType))
                throw new BlockTypeMismatchException(typeof(MessageType), data.GetType(), this);

            if (AggregationFunction != null)
            {
                //Aggregate value and send new value to children
                AggregationValue = AggregationFunction((MessageType)data, AggregationValue);
                SendToChildren(AggregationValue);
            }
            //Do NOT send recieved data to children, we already sent
            //the new value to children
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