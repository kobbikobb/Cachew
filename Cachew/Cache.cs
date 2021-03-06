﻿using System;

namespace Cachew
{
    public enum TimeoutStyle
    {
        FixedTimeout,
        RenewTimoutOnQuery
    }

    /// <summary>
    /// A simple thread safe cache with a GetOrAdd style function and automatic removal of expired items.
    /// </summary>
    public class Cache : ICache
    {
        private readonly ITimer expirationTimer;
        private readonly IInternalCache internalCache;

        private readonly LockManager lockManager = new LockManager();

        public Cache() : this(TimeoutStyle.RenewTimoutOnQuery, TimeSpan.FromMinutes(5))
        {
            
        }

        public Cache(TimeoutStyle timeoutStyle, TimeSpan timeout) : 
            this(new InternalCache(timeoutStyle, timeout), new SystemTimer(5000))
        {
          
        }

        internal Cache(IInternalCache iternalCache, ITimer expirationTimer)
        {
            if (iternalCache == null) throw new ArgumentNullException("iternalCache");
            if (expirationTimer == null) throw new ArgumentNullException("expirationTimer");
            
            this.internalCache = iternalCache;

            this.expirationTimer = expirationTimer;
            this.expirationTimer.Elapsed += ExpirationTimerElapsed;
            this.expirationTimer.Start();
        }

        public object Get<T>(CacheKey key, Func<T> func)
        {
            using (lockManager.EnterRead())
            {
                object existingValue;
                if (internalCache.TryGetValue(key, out existingValue))
                {
                    return existingValue;
                }
            }

            using (lockManager.EnterWrite())
            {
                object existingValue;
                if (internalCache.TryGetValue(key, out existingValue))
                {
                    return existingValue;
                }

                var newValue = func();
                internalCache.Add(key, newValue);
                return newValue;
            }
        }

        private void ExpirationTimerElapsed(object sender, EventArgs e)
        {
            using (lockManager.EnterWrite())
            {
                internalCache.RemoveExpiredItems();                
            }
        }
    }
}

