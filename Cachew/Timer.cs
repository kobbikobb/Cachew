using System;

namespace Cachew
{
    public class Timer : ITimer
    {
        private readonly System.Timers.Timer timer;

        public Timer(double interval)
        {
            timer = new System.Timers.Timer(interval);
            timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Elapsed != null)
            {
                Elapsed(sender, e);
            }
        }    

        public event EventHandler<EventArgs> Elapsed;

        public void Start()
        {
            timer.Start();
        }
    }
}
