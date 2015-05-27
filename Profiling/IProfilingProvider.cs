using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NoQL.CEP.Profiling
{
    public interface IProfilingProvider
    {
        IEnumerable<ProfileFrame> GetFrames();

        IEnumerable<FrameStats> GetStats();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AcceptFrame(ProfileFrame Frame);
    }
}