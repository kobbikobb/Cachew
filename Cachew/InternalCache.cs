using System;
using System.Collections.Generic;
using System.Linq;

namespace Cachew
{
    /// <summary>
    /// In memory cache that keeps a linked list of cached items ordered by time 
    /// and can remove expired items by traversing list forward.
    /// Not thread safe.
    /// </summary>
    internal class InternalCache : IInternalCache
    {
        private readonly TimeoutStyle timeoutStyle;
        private readonly TimeSpan timeout;
        private readonly IClock clock;

        private readonly LinkedList<CacheItem> timedList = new LinkedList<CacheItem>();

        public InternalCache(TimeoutStyle timeoutStyle, TimeSpan timeout) : this(timeoutStyle, timeout, new SystemClock())
        {
           
        }

        internal InternalCache(TimeoutStyle timeoutStyle, TimeSpan timeout, IClock clock)
        {
            if (clock == null) throw new ArgumentNullException("clock");

            this.timeoutStyle = timeoutStyle;
            this.timeout = timeout;
            this.clock = clock;
        }

        public bool TryGetValue(CacheKey key, out object value)
        {
            var oldItem = GetCachedItem(key);
            if (oldItem != null)
            {
                value = GetItemValue(oldItem);
                return true;
            }
            value = null;
            return false;
        }
        
        public void Add(CacheKey key, object value)
        {
            var item = new CacheItem()
            {
                Key = key,
                Value = value,
                LastQueried = clock.GetInstant()
            };
            timedList.AddLast(item);
        }
        
        private CacheItem GetCachedItem(CacheKey key)
        {
            return timedList.SingleOrDefault(x => x.Key.Equals(key));
        }

        private object GetItemValue(CacheItem item)
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
            item.LastQueried = clock.GetInstant();
        }

        public void RemoveExpiredItems()
        {
            while (timedList.Count != 0)
            {
                var oldest = timedList.First;
                if (oldest.Value.LastQueried.Add(timeout) < clock.GetInstant())
                {
                    timedList.Remove(oldest);
                }
                else
                {
                    break;
                }
            }
        }

        private class CacheItem
        {
            public CacheKey Key { get; set; }
            public object Value { get; set; }
            public DateTime LastQueried { get; set; }
        }
    }
}