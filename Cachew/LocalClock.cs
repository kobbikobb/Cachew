using System;

namespace Cachew
{
    internal class LocalClock : Clock
    {
        protected override TimeSpan TimeOfDay
        {
            get { return DateTime.Now.TimeOfDay; }
        }
    }
}
