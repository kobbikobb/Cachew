using System;
using Castle.DynamicProxy;
using Moq;
using NUnit.Framework;

namespace Cachew.CastleWindsor.Tests
{
    [TestFixture]
    public class CachingInterceptorTests
    {
        [Test]
        [TestCase("GetStuff")]
        [TestCase("getOtherStuff")]
        public void InterceptShouldUseCache_WithoutArguments(string methodName)
        {
            var cache = new Mock<ICache>();
            var innovation = new Mock<IInvocation>();
            var interceptor = new CachingInterceptor(cache.Object, "Get", "get");
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(string));
            innovation.SetupGet(x => x.Method.Name).Returns(methodName);

            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Never());
            var key = new { Method = methodName, Arguments = (string)null };
            cache.Verify(x => x.Get(new CacheKey(methodName), It.IsAny<Func<object>>()));
        }

        [Test]
        [TestCase("GetStuff")]
        [TestCase("getOtherStuff")]
        public void InterceptShouldUseCache_WithArguments(string methodName)
        {
            var cache = new Mock<ICache>();
            var innovation = new Mock<IInvocation>();
            var interceptor = new CachingInterceptor(cache.Object, "Get", "get");
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(string));
            innovation.SetupGet(x => x.Method.Name).Returns(methodName);
            innovation.SetupGet(x => x.Arguments).Returns(new object[] { "Argument1", "Argument2" });

            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Never());
            cache.Verify(x => x.Get(new CacheKey(methodName, "Argument1", "Argument2"), It.IsAny<Func<object>>()));
        }

        [Test]
        public void InterceptShouldNotUseCache()
        {
            var cache = new Mock<ICache>();
            var innovation = new Mock<IInvocation>();
            var interceptor = new CachingInterceptor(cache.Object, "Get");
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(string));
            innovation.SetupGet(x => x.Method.Name).Returns("getStuff");

            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Once);
            cache.Verify(x => x.Get(It.IsAny<CacheKey>(), It.IsAny<Func<object>>()), Times.Never);
        }

        [Test]
        public void InterceptorShouldNotUseCache_VoidMethod()
        {
            var cache = new Mock<ICache>();
            var innovation = new Mock<IInvocation>();
            var interceptor = new CachingInterceptor(cache.Object, "Get");
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(void));
            innovation.SetupGet(x => x.Method.Name).Returns("GetStuff");

            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Once);
            cache.Verify(x => x.Get(It.IsAny<CacheKey>(), It.IsAny<Func<object>>()), Times.Never);
        }

        [Test]
        public void DefaultPrefixIsGet()
        {
            var cache = new Mock<ICache>();
            var innovation = new Mock<IInvocation>();
            var interceptor = new CachingInterceptor(cache.Object);
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(string));
            innovation.SetupGet(x => x.Method.Name).Returns("GetStuff");

            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Never);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MethodPrefixesCanNotBeEmpty()
        {
            var cache = new Mock<ICache>();
            var interceptor = new CachingInterceptor(cache.Object, new string[] { });
        }
    }
}
