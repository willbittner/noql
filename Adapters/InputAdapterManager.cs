using NoQL.CEP.NewExpressions;
using System.Collections.Generic;

namespace NoQL.CEP.Adapters
{
    public class InputAdapterManager
    {
        public Dictionary<string, BaseInputAdapter> Adapters { get; set; }

        public InputAdapterManager()
        {
            Adapters = new Dictionary<string, BaseInputAdapter>();
        }

        public void Register(string name, BaseInputAdapter adapter)
        {
            Adapters[name] = adapter;
        }

        public BaseInputAdapter Get(string name)
        {
            if (!Adapters.ContainsKey(name))
                return null;
            return Adapters[name];
        }
    }
}