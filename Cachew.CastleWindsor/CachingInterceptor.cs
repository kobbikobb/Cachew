using System;
using System.Linq;
using Castle.DynamicProxy;

namespace Cachew.CastleWindsor
{
    public class CachingInterceptor : IInterceptor
    {
        private readonly ICache cache;
        private readonly string[] methodPrefixes;

        public CachingInterceptor(ICache cache, params string[] methodPrefixes)
        {
            if (cache == null) throw new ArgumentNullException("cache");
            if (methodPrefixes == null) throw new ArgumentNullException("methodPrefixes");
            if (methodPrefixes.Length == 0) throw new ArgumentException("You must specify 1 or more method prefixes", "methodPrefixes");

            this.cache = cache;
            this.methodPrefixes = methodPrefixes;
        }

        public CachingInterceptor(ICache cache) : this(cache, "Get")
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
                GetMethodKey(invocation),
                () =>
                {
                    invocation.Proceed();
                    return invocation.ReturnValue;
                });
        }

        private static object GetMethodKey(IInvocation invocation)
        {
            string arguments = null;
            if (invocation.Arguments.Length > 0)
            {
                arguments = invocation.Arguments.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y);
            }

            return new { Method = invocation.Method.Name, Arguments = arguments };
        }
    }
}
