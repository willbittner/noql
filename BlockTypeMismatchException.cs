using NoQL.CEP.Blocks;
using System;

namespace NoQL.CEP.Exceptions
{
    internal class BlockTypeMismatchException : Exception
    {
        private string _message;

        public Type ActualType { get; set; }

        public Type ExpectedType { get; set; }

        public override string Message
        {
            get { return OffendingBlock.DebugName ?? OffendingBlock.GetType().Name + " expected a " + ExpectedType.Name + " but recieved a " + ActualType.Name; }
        }

        public AbstractBlock OffendingBlock { get; set; }

        public BlockTypeMismatchException(Type ExpectedType, Type ActualType, AbstractBlock OffendingBlock)
        {
            this.ExpectedType = ExpectedType;
            this.ActualType = ActualType;
            this.OffendingBlock = OffendingBlock;
        }
    }
}