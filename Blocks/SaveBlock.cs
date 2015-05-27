using NoQL.CEP.Exceptions;

namespace NoQL.CEP.Blocks
{
    /// <summary>
    ///     Save Block saves a Block to a RamDB
    /// </summary>
    public class SaveBlock<MessageType> : AbstractBlock
    {
        private IRamDB Database { get; set; }

        internal SaveBlock(Processor p, IRamDB database)
            : base(p)
        {
            Database = database;
        }

        public override bool OnData(object data)
        {
            if (!(data is MessageType))
                throw new BlockTypeMismatchException(typeof(MessageType), data.GetType(), this);

            Database.Add((MessageType)data);
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