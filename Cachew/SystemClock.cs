using System;

namespace Cachew
{
    internal class SystemClock : IClock
    {
        public DateTime GetInstant()
        {
            return DateTime.UtcNow;
        }
    }
}
