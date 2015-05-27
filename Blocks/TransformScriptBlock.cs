using NoQL.CEP.Adapters;
using NoQL.CEP.Exceptions;

namespace NoQL.CEP.Blocks
{
    public class TransformScriptBlock<InputType, OutputType> : AbstractBlock
    {
        private IScriptTransformAdapter<InputType, OutputType> ScriptTransform { get; set; }

        internal TransformScriptBlock(Processor p, string ScriptName)
            : base(p)
        {
            ScriptTransform = p.ScriptsManager.GetScript<InputType, OutputType>(ScriptName);
        }

        public override bool OnData(object data)
        {
            if (!(data is InputType))
                throw new BlockTypeMismatchException(typeof(InputType), data.GetType(), this);

            OutputType nData = ScriptTransform.OnData((InputType)data);

            SendToChildren(nData);
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