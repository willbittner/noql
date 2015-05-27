using NoQL.CEP.Blocks;
using System.Threading;

namespace NoQL.CEP.Profiling
{
    public class ProfileFrame
    {
        public static int NextID = 0;

        /// <summary>
        /// Identification of the frame
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Abstract Block being profiled
        /// </summary>
        public AbstractBlock Block { get; set; }

        /// <summary>
        /// Data being passed through the block
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Timed runtime in milliseconds
        /// </summary>
        public double Runtime { get; set; }

        public ProfileFrame(AbstractBlock Block, object Data, double Runtime)
        {
            this.ID = Interlocked.Increment(ref NextID);
            this.Block = Block;
            this.Data = Data;
            this.Runtime = Runtime;
        }
    }
}