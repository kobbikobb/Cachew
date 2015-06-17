namespace Cachew
{
    internal interface IInternalCache
    {
        bool TryGetValue(CacheKey key, out object value);
        void Add(CacheKey key, object value);
        void RemoveExpiredItems();
    }
}