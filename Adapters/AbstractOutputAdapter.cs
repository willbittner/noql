using NoQL.CEP.Blocks;

namespace NoQL.CEP.Adapters
{
    public class AbstractOutputAdapter<OutputType> : AbstractBlock
    {
        private static IOutputAdapter<OutputType> outputAdapter;

        internal AbstractOutputAdapter(Processor p, IOutputAdapter<OutputType> Adapter)
            : base(p)
        {
            outputAdapter = Adapter;
            DebugName = "CSVOutputAdapter";
        }

        public override bool OnData(object output)
        {
            outputAdapter.OnOutput((OutputType)output);
            return false;
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