using System;

namespace Cachew
{
    public interface ICache
    {
        object Get<T>(object key, Func<T> func);
    }
}
