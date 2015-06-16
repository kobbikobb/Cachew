using System;

namespace Cachew
{
    internal class SystemClock : IClock
    {
        public TimeSpan GetTimeOfDay()
        {
            return DateTime.Now.TimeOfDay;
        }
    }
}
