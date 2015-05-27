using NoQL.CEP.Exceptions;
using System;

namespace NoQL.CEP.Blocks
{
    /// <summary>
    ///     Delay Block accepts data and waits a period before sending it on.
    /// </summary>
    /// <typeparam name="T">The type of data expected</typeparam>
    public class DelayBlock<T> : ExpressionBlock
    {
        public double IntervalMS { get; set; }

        internal DelayBlock(Processor p, double IntervalMS)
            : base(p)
        {
            this.IntervalMS = IntervalMS;
        }

        public override bool OnData(object data)
        {
            if (!(data is T))
                throw new BlockTypeMismatchException(typeof(T), data.GetType(), this);

            NoQL.CEP.Time.Future.Do(() => SendToChildren(data), TimeSpan.FromMilliseconds(IntervalMS));

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