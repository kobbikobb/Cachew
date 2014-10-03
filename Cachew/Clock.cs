using System;

namespace Cachew
{
    internal abstract class Clock
    {
        protected abstract TimeSpan TimeOfDay { get; }

        private static Clock instance;
        public static Clock Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LocalClock();
                }
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        public static TimeSpan GetTimeOfDay()
        {
            return Instance.TimeOfDay;
        }
    }
}
