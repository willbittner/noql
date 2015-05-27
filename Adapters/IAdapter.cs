namespace NoQL.CEP.Adapters
{
    public interface IAdapter
    {
        Processor CEP { get; set; }
    }
}