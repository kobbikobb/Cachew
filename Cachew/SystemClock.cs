using System;

namespace Cachew
{
    internal class SystemClock : IClock
    {
        public DateTime GetNow()
        {
            return DateTime.Now;
        }
    }
}
