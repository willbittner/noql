using NoQL.CEP.Exceptions;
using System;

namespace NoQL.CEP.Blocks
{
    /// <summary>
    ///     Lambda Block accepts data an executes an assigned Lambda function
    ///     against the data. The lambda function "replaces" the
    ///     OnData function in AbstractBlock and should return
    ///     true if execution should continue down the CEP-tree
    ///     or false to cancel execution down the CEP-tree
    /// </summary>
    /// <typeparam name="T">The type of data expected</typeparam>
    public class LambdaBlock<T> : ExpressionBlock
    {
        public Func<T, bool> LambdaFunction { get; set; }

        internal LambdaBlock(Processor p, Func<T, bool> lambdaFunc)
            : base(p)
        {
            if (lambdaFunc == null)
                throw new ArgumentNullException("lambdaFunc");
            LambdaFunction = lambdaFunc;
        }

        public override bool OnData(object data)
        {
            if (!(data is T))
                throw new BlockTypeMismatchException(typeof(T), data.GetType(), this);

            return LambdaFunction((T)data);
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