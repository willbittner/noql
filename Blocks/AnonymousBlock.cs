using NoQL.CEP.Exceptions;
using System;

namespace NoQL.CEP.Blocks
{
    public class AnonymousBlock<T> : AbstractBlock
    {
        public AnonymousBlock(Processor p)
            : base(p)
        {
        }

        public delegate bool OnDataDelegate(T data, AnonymousBlock<T> block);

        public OnDataDelegate OnDataEvent { get; set; }

        public override bool OnData(object data)
        {
            if (!(data is T))
                throw new BlockTypeMismatchException(typeof(T), data.GetType(), this);

            return OnDataEvent != null && OnDataEvent((T)data, this);
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