using System;
using System.Collections.Generic;
using System.Linq;

namespace Cachew
{
    internal class InternalCache : IInternalCache
    {
        private readonly TimeoutStyle timeoutStyle;
        private readonly TimeSpan timeout;

        private readonly LinkedList<CacheItem> timedList = new LinkedList<CacheItem>();

        public InternalCache(TimeoutStyle timeoutStyle, TimeSpan timeout)
        {
            this.timeoutStyle = timeoutStyle;
            this.timeout = timeout;
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
            var item = new CacheItem(key, value);
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
            item.LastQueried = GetTimeOfDay();
        }

        public void RemoveExpiredItems()
        {
            while (timedList.Count != 0)
            {
                var oldest = timedList.First;
                if (oldest.Value.LastQueried.Add(timeout) < GetTimeOfDay())
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
            public CacheKey Key { get; private set; }
            public object Value { get; private set; }
            public TimeSpan LastQueried { get; set; }

            public CacheItem(CacheKey key, object value)
            {
                Key = key;
                Value = value;
                LastQueried = Clock.GetTimeOfDay();
            }
        }

        private static TimeSpan GetTimeOfDay()
        {
            return Clock.GetTimeOfDay();
        }
    }
}