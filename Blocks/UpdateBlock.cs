using NoQL.CEP.Exceptions;

namespace NoQL.CEP.Blocks
{
    /// <summary>
    ///     Save Block saves a Block to a RamDB
    /// </summary>
    public class UpdateBlock<MessageType> : AbstractBlock
    {
        private IRamDB Database { get; set; }

        private UpdatePolicy Policy { get; set; }

        internal UpdateBlock(Processor p, IRamDB database, UpdatePolicy policy)
            : base(p)
        {
            Database = database;
            Policy = policy;
        }

        public override bool OnData(object data)
        {
            if (!(data is MessageType))
                throw new BlockTypeMismatchException(typeof(MessageType), data.GetType(), this);

            Database.Update((MessageType)data, Policy);
            return true;
        }

        public override System.Type BlockInputType
        {
            get { throw new System.NotImplementedException(); }
        }

        public override System.Type BlockOutputType
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}