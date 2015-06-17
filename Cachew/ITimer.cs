using System;

namespace Cachew
{
    public interface ITimer
    {
        event EventHandler<EventArgs> Elapsed;
        void Start();
    }
}