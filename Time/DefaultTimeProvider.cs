using System;

namespace NoQL.CEP.Time
{
    internal class DefaultTimeProvider : ITimeProvider
    {
        public DateTime GetTime()
        {
            return DateTime.Now;
        }

        public int GetTimeCoefficient()
        {
            return 1;
        }
    }
}