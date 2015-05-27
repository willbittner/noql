using System;

namespace NoQL.CEP.Time
{
    public interface ITimeProvider
    {
        int GetTimeCoefficient();

        DateTime GetTime();
    }
}