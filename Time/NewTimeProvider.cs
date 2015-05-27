using System;

namespace NoQL.CEP.Time
{
    internal class newTimeProvider : ITimeProvider
    {
        private static DateTime _currentTime = new DateTime();
        private static object lockobject = new object();

        private static DateTime CurrentTime
        {
            get
            {
                lock (lockobject)
                {
                    return _currentTime;
                }
            }
            set
            {
                lock (lockobject)
                {
                    _currentTime = value;
                }
            }
        }

        public DateTime GetTime()
        {
            return CurrentTime;
        }

        public void SetTime(DateTime time)
        {
            CurrentTime = time;
        }

        public int GetTimeCoefficient()
        {
            throw new NotImplementedException();
        }
    }
}