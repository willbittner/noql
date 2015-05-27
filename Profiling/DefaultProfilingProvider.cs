using NoQL.CEP.Blocks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NoQL.CEP.Profiling
{
    public class DefaultProfilingProvider : IProfilingProvider
    {
        private const int DEFAULT_CAPACITY = 10000;

        private ConcurrentQueue<ProfileFrame> Frames;

        public DefaultProfilingProvider()
        {
            Frames = new ConcurrentQueue<ProfileFrame>();
        }

        public int Capacity { get; set; }

        public IEnumerable<ProfileFrame> GetFrames()
        {
            return Frames;
        }

        public void AcceptFrame(ProfileFrame Frame)
        {
            if (Frames.Count > Capacity)
            {
                ProfileFrame t = null;
                Frames.TryDequeue(out t);
            }
            Frames.Enqueue(Frame);
        }

        public IEnumerable<FrameStats> GetStats()
        {
            var frameGroup = Frames.GroupBy(frame => frame.Block, frame => frame.Runtime);

            foreach (IGrouping<AbstractBlock, double> group in frameGroup)
            {
                IList<double> times = group.ToList();

                yield return new FrameStats()
                {
                    Avg = times.Average(),
                    Block = group.Key.DebugName,
                    Min = times.Min(),
                    Max = times.Max(),
                    Count = times.Count(),
                    StdDev = times.StdDev()
                };
            }
        }
    }
}