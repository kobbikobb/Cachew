using System;

namespace Cachew
{
    internal interface IClock
    {
        TimeSpan GetTimeOfDay();
    }
}
