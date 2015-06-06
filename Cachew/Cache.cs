using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly TimeoutStyle timeoutStyle;
        private readonly TimeSpan timeout;
        private ReaderWriterLock readerWriterLock = new ReaderWriterLock();
        private readonly LinkedList<CacheItem> timedList = new LinkedList<CacheItem>();

        public Cache(TimeoutStyle timeoutStyle, TimeSpan timeout)
        {
            this.timeoutStyle = timeoutStyle;
            this.timeout = timeout;
        }

        public object Get<T>(CacheKey key, Func<T> func)
        {
            RemoveExpiredItems();

            readerWriterLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                var oldItem = GetCachedItem<T>(key);
                if (oldItem != null)
                    return GetItemValue<T>(oldItem);

                readerWriterLock.UpgradeToWriterLock(Timeout.Infinite);

                oldItem = GetCachedItem<T>(key);
                if (oldItem != null)
                    return GetItemValue<T>(oldItem);

                var newItem = new CacheItem(key, func());
                timedList.AddLast(newItem);
                return newItem.Value;
            }
            finally
            {
                readerWriterLock.ReleaseLock();
            }

        }

        private CacheItem GetCachedItem<T>(CacheKey key)
        {
            return timedList.SingleOrDefault(x => x.Key.Equals(key));
        }

        private object GetItemValue<T>(CacheItem item)
        {
            if (timeoutStyle == TimeoutStyle.RenewTimoutOnQuery)
            {
                RenewItem(item);
            }
            return item.Value;
        }

        private void RenewItem(CacheItem item)
        {
            timedList.Remove(item);
            timedList.AddLast(item);
            item.LastQueried = GetTimeOfDay();
        }

        private void RemoveExpiredItems()
        {
            while (timedList.Count != 0)
            {
                var oldest = timedList.First;
                if (oldest.Value.LastQueried.Add(timeout) <= GetTimeOfDay())
                {
                    readerWriterLock.AcquireWriterLock(Timeout.Infinite);
                    try
                    {
                        timedList.Remove(oldest);
                    }
                    finally
                    {
                        readerWriterLock.ReleaseWriterLock();
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private class CacheItem
        {
            public CacheKey Key { get; private set; }
            public object Value { get; private set; }
            public TimeSpan LastQueried { get; set; }

            public CacheItem(CacheKey key, object value)
            {
                Key = key;
                Value = value;
                LastQueried = GetTimeOfDay();
            }
        }

        private static TimeSpan GetTimeOfDay()
        {
            return Clock.GetTimeOfDay();
        }
    }
}

