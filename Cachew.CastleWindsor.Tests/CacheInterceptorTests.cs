using System;
using Castle.DynamicProxy;
using Moq;
using NUnit.Framework;

namespace Cachew.CastleWindsor.Tests
{
    [TestFixture]
    public class CacheInterceptorTests
    {
        [Test]
        [TestCase("GetStuff")]
        [TestCase("getOtherStuff")]
        public void InterceptWithoutArguments_ShouldUseCache(string methodName)
        {
            var cache = new Mock<ICache>();
            var interceptor = new CacheInterceptor(cache.Object, "Get", "get");
            var innovation = new Mock<IInvocation>();
            innovation.SetupGet(x => x.Method.Name).Returns(methodName);
            innovation.SetupGet(x => x.TargetType.Name).Returns("SomeType");
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(string));

            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Never());
            cache.Verify(x => x.Get(new CacheKey("SomeType_" + methodName), It.IsAny<Func<object>>()));
        }

        [Test]
        [TestCase("GetStuff")]
        [TestCase("getOtherStuff")]
        public void InterceptWithArguments_ShouldUseCache(string methodName)
        {
            var cache = new Mock<ICache>();
            var interceptor = new CacheInterceptor(cache.Object, "Get", "get");
            var innovation = new Mock<IInvocation>();
            innovation.SetupGet(x => x.Method.Name).Returns(methodName);
            innovation.SetupGet(x => x.Arguments).Returns(new object[] { "Argument1", "Argument2" });
            innovation.SetupGet(x => x.TargetType.Name).Returns("OtherType");
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(object));

            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Never());
            cache.Verify(x => x.Get(new CacheKey("OtherType_" + methodName, "Argument1", "Argument2"), It.IsAny<Func<object>>()));
        }

        [Test]
        public void Intercept_ShouldCacheWithGetAsDefaultMethodPrefix()
        {
            var cache = new Mock<ICache>();
            var interceptor = new CacheInterceptor(cache.Object);
            var innovation = new Mock<IInvocation>();
            innovation.SetupGet(x => x.Method.Name).Returns("GetStuff");
            innovation.SetupGet(x => x.TargetType.Name).Returns("Type");
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(int));
            
            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Never);
        }

        [Test]
        public void Intercept_ShouldNotUseCacheIfMethodHasDifferentPrefix()
        {
            var cache = new Mock<ICache>();
            var interceptor = new CacheInterceptor(cache.Object, "Get");
            var innovation = new Mock<IInvocation>();
            innovation.SetupGet(x => x.Method.Name).Returns("getStuff");
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(string));

            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Once);
            cache.Verify(x => x.Get(It.IsAny<CacheKey>(), It.IsAny<Func<object>>()), Times.Never);
        }

        [Test]
        public void InterceptVoidMethod_ShouldNotUseCache()
        {
            var cache = new Mock<ICache>();
            var interceptor = new CacheInterceptor(cache.Object, "Get");
            var innovation = new Mock<IInvocation>();
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(void));
            innovation.SetupGet(x => x.Method.Name).Returns("GetStuff");

            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Once);
            cache.Verify(x => x.Get(It.IsAny<CacheKey>(), It.IsAny<Func<object>>()), Times.Never);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Interceptor_ShouldNotBeAbleToBeCreatedWithEmptyPrefixes()
        {
            var cache = new Mock<ICache>();
            var interceptor = new CacheInterceptor(cache.Object, new string[] { });
        }
    }
}
