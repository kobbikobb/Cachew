using System;
using System.Collections.Generic;
using System.Linq;

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

        private readonly LinkedList<CacheItem> timedList = new LinkedList<CacheItem>();

        public Cache(TimeoutStyle timeoutStyle, TimeSpan timeout)
        {
            this.timeoutStyle = timeoutStyle;
            this.timeout = timeout;
        }

        public object Get<T>(CacheKey key, Func<T> func)
        {
            RemoveExpiredItems();

            var oldItem = timedList.SingleOrDefault(x => x.Key.Equals(key));
            if (oldItem != null)
            {
                if (timeoutStyle == TimeoutStyle.RenewTimoutOnQuery)
                {
                    RenewItem(oldItem);
                }
                return oldItem.Value;
            }

            var newItem = new CacheItem(key, func());
            timedList.AddLast(newItem);
            return newItem.Value;

        }

        private void RenewItem(CacheItem valueObject)
        {
            timedList.Remove(valueObject);
            timedList.AddLast(valueObject);
            valueObject.LastQueried = GetTimeOfDay();
        }

        private void RemoveExpiredItems()
        {
            while (timedList.Count != 0)
            {
                var oldest = timedList.First.Value;
                if (oldest.LastQueried.Add(timeout) <= GetTimeOfDay())
                {
                    timedList.RemoveFirst();
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

