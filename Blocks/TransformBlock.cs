using NoQL.CEP.Exceptions;
using System;

namespace NoQL.CEP.Blocks
{
    /// <summary>
    ///     Transform Block behaves like LambdaBlock however instead of simply performing an
    ///     action on the data, the lambda function can return a different object
    ///     to be passed to child objects, or retun null to stop
    ///     CEP-tree call propagation
    /// </summary>
    /// <typeparam name="InputType">The type of data expected</typeparam>
    /// <typeparam name="OutputType">The type of data send to child objects</typeparam>
    public class TransformBlock<InputType, OutputType> : AbstractBlock
    {
        public override Type BlockInputType
        {
            get { return typeof(InputType); }
        }

        public override Type BlockOutputType
        {
            get { return typeof(InputType); }
        }

        public Func<InputType, OutputType> LambdaFunction { get; set; }

        internal TransformBlock(Processor p, Func<InputType, OutputType> lambdaFunc)
            : base(p)
        {
            if (lambdaFunc == null)
                throw new ArgumentNullException("lambdaFunc");
            LambdaFunction = lambdaFunc;
        }

        public override bool OnData(object data)
        {
            if (!(data is InputType))
                throw new BlockTypeMismatchException(typeof(InputType), data.GetType(), this);

            OutputType next = LambdaFunction((InputType)data);
            if (next != null)
                SendToChildren(next);
            return false;
        }
    }
}