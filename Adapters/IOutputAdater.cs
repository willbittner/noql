namespace NoQL.CEP.Adapters
{
    public interface IOutputAdapter<OutputType>
    {
        void OnOutput(OutputType output);
    }
}