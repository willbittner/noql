using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dashx.CEP.Profiling
{
    public class NullProfilingProvider : IProfilingProvider
    {
        public IEnumerable<ProfileFrame> GetFrames()
        {
            return new List<ProfileFrame>();
        }

        public IEnumerable<FrameStats> GetStats()
        {
            return new List<FrameStats>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AcceptFrame(ProfileFrame Frame)
        {
           
        }
    }
}
