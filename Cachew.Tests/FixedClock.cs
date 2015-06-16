using System;

namespace Cachew.Tests
{
    internal class FixedClock : Clock
    {
        private TimeSpan fixedTimeOfDay;

        public FixedClock(TimeSpan fixedTimeOfDay)
        {
            this.fixedTimeOfDay = fixedTimeOfDay;
        }

        protected override TimeSpan TimeOfDay
        {
            get { return fixedTimeOfDay; }
        }

        public void Add(TimeSpan timeSpan)
        {
            fixedTimeOfDay = fixedTimeOfDay.Add(timeSpan);
        }
    }
}
