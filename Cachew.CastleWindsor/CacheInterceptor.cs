using System;
using System.Linq;
using Castle.DynamicProxy;

namespace Cachew.CastleWindsor
{
    public class CacheInterceptor : IInterceptor
    {
        private readonly ICache cache;
        private readonly string[] methodPrefixes;

        public CacheInterceptor(ICache cache, params string[] methodPrefixes)
        {
            if (cache == null) throw new ArgumentNullException("cache");
            if (methodPrefixes == null) throw new ArgumentNullException("methodPrefixes");
            if (methodPrefixes.Length == 0) throw new ArgumentException("You must specify 1 or more method prefixes", "methodPrefixes");

            this.cache = cache;
            this.methodPrefixes = methodPrefixes;
        }

        public CacheInterceptor(ICache cache) : this(cache, "Get")
        {

        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.ReturnType == typeof(void) || methodPrefixes.All(x => !invocation.Method.Name.StartsWith(x)))
            {
                invocation.Proceed();
                return;
            }

            invocation.ReturnValue = cache.Get(
                GetCacheKey(invocation),
                () =>
                {
                    invocation.Proceed();
                    return invocation.ReturnValue;
                });
        }

        private static CacheKey GetCacheKey(IInvocation invocation)
        {
            var methodKey = invocation.TargetType.Name + "_" + invocation.Method.Name;
            return new CacheKey(methodKey, invocation.Arguments);
        }
    }
}
