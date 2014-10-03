using System;
using Castle.DynamicProxy;

namespace Cachew.CastleWindsor
{
    /// <summary>
    /// A helper for constructing cache decorated classes.
    /// </summary>
    public class CacheDecoratorBuilder
    {
        private readonly ProxyGenerator generator;
        private TimeoutStyle timeoutStyle;
        private TimeSpan timeout;
        private string[] methodPrefixes;

        public CacheDecoratorBuilder()
        {
            generator = new ProxyGenerator();
            timeoutStyle = TimeoutStyle.RenewTimoutOnQuery;
            timeout = TimeSpan.FromSeconds(2);
            methodPrefixes = new[] { "Get" };
        }

        public CacheDecoratorBuilder SetTimeoutStyle(TimeoutStyle value)
        {
            timeoutStyle = value;
            return this;
        }

        public CacheDecoratorBuilder SetTimeout(TimeSpan value)
        {
            timeout = value;
            return this;
        }

        public CacheDecoratorBuilder SetMethodPrefixes(params string[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length == 0)
                throw new ArgumentException("You must specify 1 or more method prefixes", "values");

            methodPrefixes = values;
            return this;
        }

        public T BuildFromInterface<T>(T t) where T : class
        {
            var cache = new Cache(timeoutStyle, timeout);
            var interceptor = new CachingInterceptor(cache, methodPrefixes);
            return generator.CreateInterfaceProxyWithTarget(t, interceptor);
        }
    }
}
