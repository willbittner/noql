using NoQL.CEP.Blocks;

namespace NoQL.CEP.Adapters
{
    public interface IScriptTransformAdapter
    {
        void SetProcessor(Processor p);
    }

    public interface IScriptTransformAdapter<DataType, OutputType> : IScriptTransformAdapter
    {
        OutputType OnData(DataType data);
    }

    public interface IScriptTransformFactoryAdapter<ArgumentType> : IScriptTransformAdapter<ArgumentType, AbstractBlock>
    {
    }
}