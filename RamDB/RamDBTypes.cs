namespace NoQL.CEP
{
    public enum UpdatePolicy
    {
        UPDATE_ONLY = 0,
        UPDATE_OR_INSERT = 1
    }

    public class LookupSpecification
    {
        public string IndexName { get; set; }

        public object IndexValue { get; set; }

        public LookupSpecification(string IndexName, object IndexValue)
        {
            this.IndexName = IndexName;
            this.IndexValue = IndexValue;
        }
    }
}