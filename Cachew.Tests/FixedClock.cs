using System;

namespace Cachew.Tests
{
    internal class FixedClock : IClock
    {
        private DateTime fixedTimeOfDay;

        public FixedClock(DateTime value)
        {
            this.fixedTimeOfDay = value;
        }

        public DateTime GetNow()
        {
             return fixedTimeOfDay; 
        }

        internal void Add(TimeSpan value)
        {
            fixedTimeOfDay = fixedTimeOfDay.Add(value);
        }

        internal void SetNow(DateTime value)
        {
            fixedTimeOfDay = value;
        }
    }
}
