using System;

namespace Cachew
{
    public interface ICache
    {
        object Get<T>(CacheKey key, Func<T> func);
    }
}
