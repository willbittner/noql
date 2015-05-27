namespace NoQL.CEP.Profiling
{
    public class FrameStats
    {
        public string Block { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public double Avg { get; set; }

        public double StdDev { get; set; }

        public int Count { get; set; }
    }
}