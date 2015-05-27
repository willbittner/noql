using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dashx.CEP.Blocks;

namespace Dashx.CEP.Exceptions
{
    class BlockTypeMismatchException : Exception
    {

        public Type ExpectedType { get; set; }
        public Type ActualType { get; set; }
        public AbstractBlock OffendingBlock { get; set; }

        public BlockTypeMismatchException(Type ExpectedType, Type ActualType, AbstractBlock OffendingBlock)
        {
            this.ExpectedType = ExpectedType;
            this.ActualType = ActualType;
            this.OffendingBlock = OffendingBlock;
        }

        public override string Message
        {
            get { return OffendingBlock.DebugName ?? OffendingBlock.GetType().Name + " expected a " + ExpectedType.Name + " but recieved a " + ActualType.Name; }
        }
    }
}
