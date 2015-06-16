using System;

namespace Cachew.Tests
{
    internal class FixedClock : IClock
    {
        private TimeSpan fixedTimeOfDay;

        public FixedClock(TimeSpan fixedTimeOfDay)
        {
            this.fixedTimeOfDay = fixedTimeOfDay;
        }

        public TimeSpan GetTimeOfDay()
        {
             return fixedTimeOfDay; 
        }

        internal void Add(TimeSpan timeSpan)
        {
            fixedTimeOfDay = fixedTimeOfDay.Add(timeSpan);
        }
    }
}
