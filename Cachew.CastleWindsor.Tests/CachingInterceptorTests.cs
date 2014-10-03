using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using Moq;
using NUnit.Framework;

namespace Cachew.CastleWindsor.Tests
{
    [TestFixture]
    public class CachingInterceptorTests
    {
        private Mock<ICache> cache;
        private Mock<IInvocation> innovation;

        [SetUp]
        public void SetUp()
        {
            cache = new Mock<ICache>();
            innovation = new Mock<IInvocation>();
        }

        [Test]
        [TestCase("GetStuff")]
        [TestCase("getOtherStuff")]
        public void InterceptShouldUseCache_WithoutArguments(string methodName)
        {
            var interceptor = new CachingInterceptor(cache.Object, "Get", "get");
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(string));
            innovation.SetupGet(x => x.Method.Name).Returns(methodName);

            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Never());
            var key = new { Method = methodName, Arguments = (string)null };
            cache.Verify(x => x.Get(It.Is<object>(y => MatchKeys(key, y)), It.IsAny<Func<object>>()));
        }

        [Test]
        [TestCase("GetStuff")]
        [TestCase("getOtherStuff")]
        public void InterceptShouldUseCache_WithArguments(string methodName)
        {
            var interceptor = new CachingInterceptor(cache.Object, "Get", "get");
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(string));
            innovation.SetupGet(x => x.Method.Name).Returns(methodName);
            innovation.SetupGet(x => x.Arguments).Returns(new object[] { "Argument1", "Argument2" });

            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Never());
            var key = new { Method = methodName, Arguments = "Argument1,Argument2" };
            cache.Verify(x => x.Get(It.Is<object>(y => MatchKeys(key, y)), It.IsAny<Func<object>>()));
        }

        [Test]
        public void InterceptShouldNotUseCache()
        {
            var interceptor = new CachingInterceptor(cache.Object, "Get");
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(string));
            innovation.SetupGet(x => x.Method.Name).Returns("getStuff");

            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Once);
            cache.Verify(x => x.Get(It.IsAny<object>(), It.IsAny<Func<object>>()), Times.Never);
        }

        [Test]
        public void InterceptorShouldNotUseCache_VoidMethod()
        {
            var interceptor = new CachingInterceptor(cache.Object, "Get");
            innovation.SetupGet(x => x.Method.ReturnType).Returns(typeof(void));
            innovation.SetupGet(x => x.Method.Name).Returns("GetStuff");

            interceptor.Intercept(innovation.Object);

            innovation.Verify(x => x.Proceed(), Times.Once);
            cache.Verify(x => x.Get(It.IsAny<object>(), It.IsAny<Func<object>>()), Times.Never);
        }

        [Test]
        public void DefaultPrefixIsGet()
        {
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
            var interceptor = new CachingInterceptor(cache.Object, new string[] { });
        }

        private bool MatchKeys(object keyLeft, object keyRight)
        {
            return Equals(GetPropertyValue(keyLeft, "Method"), GetPropertyValue(keyRight, "Method")) &&
                   Equals(GetPropertyValue(keyLeft, "Arguments"), GetPropertyValue(keyRight, "Arguments"));
        }

        private object GetPropertyValue(object obj, string propertyName)
        {
            var propertyInfo = obj.GetType().GetProperty(propertyName);
            return propertyInfo.GetValue(obj, null);
        }

        [Test]
        public void KeysAreEqual()
        {
            var key1 = new { Method = "GetStuff", Arguments = "Argument1,Argument2" };
            var key2 = new { Method = "GetStuff", Arguments = "Argument1,Argument2" };

            Assert.AreEqual(key1, key2);
        }
    }
}
