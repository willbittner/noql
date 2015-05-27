using System;

namespace NoQL.CEP
{
    public static class Utilities
    {
        public static int NumberOfProcessors
        {
            get { return Environment.ProcessorCount; }
        }
    }
}