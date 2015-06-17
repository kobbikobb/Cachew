﻿using System;

namespace Cachew.Tests
{
    internal class FixedTimer : ITimer
    {
        public event EventHandler<EventArgs> Elapsed;

        public void Start()
        {
            
        }

        public void InvokeElapsed()
        {
            if (Elapsed != null)
            {
                Elapsed(this, new EventArgs());
            }
        }
    }
}
