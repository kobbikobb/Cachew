using System;
using System.Threading;

namespace Cachew
{
    public enum TimeoutStyle
    {
        FixedTimeout,
        RenewTimoutOnQuery
    }

    /// <summary>
    /// Keeps a linked list of cached items ordered by time and removes expired items by traversing list forward.
    /// </summary>
    public class Cache : ICache
    {
        private readonly ITimer expirationTimer;
        private readonly IInternalCache internalCache;

        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();
        
        public Cache(TimeoutStyle timeoutStyle, TimeSpan timeout) : 
            this(new InternalCache(timeoutStyle, timeout), new Timer(5000))
        {
          
        }

        internal Cache(IInternalCache iternalCache, ITimer expirationTimer)
        {
            this.expirationTimer = expirationTimer;
            this.internalCache = iternalCache;

            this.expirationTimer.Elapsed += ExpirationTimerElapsed;
            this.expirationTimer.Start();
        }

        internal Cache(TimeoutStyle timeoutStyle, TimeSpan timeout, ITimer expirationTimer)
        {
            this.expirationTimer = expirationTimer;
            this.internalCache = new InternalCache(timeoutStyle, timeout);

            this.expirationTimer.Elapsed += ExpirationTimerElapsed;
            this.expirationTimer.Start();
        }

        public object Get<T>(CacheKey key, Func<T> func)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                object existingValue;
                if (internalCache.TryGetValue(key, out existingValue))
                    return existingValue;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }

            readerWriterLockSlim.EnterWriteLock();
            try
            {
                object existingValue;
                if (internalCache.TryGetValue(key, out existingValue))
                    return existingValue;

                var newValue = func();
                internalCache.Add(key, newValue);
                return newValue;
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        private void ExpirationTimerElapsed(object sender, EventArgs e)
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                internalCache.RemoveExpiredItems();
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }
    }
}

