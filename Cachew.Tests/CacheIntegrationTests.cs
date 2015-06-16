using System;
using Moq;
using NUnit.Framework;

namespace Cachew.Tests
{
    [TestFixture]
    public class CacheIntegrationTests
    {
        private string expected;
        private Mock<IDummy> dummyMock;
        private IDummy dummy;
        private CacheKey key;
        private FixedClock fixedClock;

        [SetUp]
        public void SetUp()
        {
            expected = "return string";
            dummyMock = new Mock<IDummy>();
            dummy = dummyMock.Object;
            key = new CacheKey("MethodName");
            fixedClock = new FixedClock(DateTime.Today);

            dummyMock.Setup(x => x.GetStuff()).Returns(expected);
        }

        [Test]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 4, 5, 1)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 4, 6, 2)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 4, 6, 2)]
        [TestCase(TimeoutStyle.FixedTimeout, 4, 4, 2)]
        [TestCase(TimeoutStyle.FixedTimeout, 4, 5, 2)]
        [TestCase(TimeoutStyle.FixedTimeout, 4, 6, 2)]
        public void GetShouldRenewStyleRenewTimoutOnQuery(TimeoutStyle style, int interval1, int interval2, int expectedCount)
        {
            var timer = new FixedTimer();
            var cache = new Cache(new InternalCache(style, new TimeSpan(5), fixedClock), timer);

            var actual1 = cache.Get(key, dummy.GetStuff);
            fixedClock.Add(new TimeSpan(interval1));
            timer.InvokeElapsed();
            var actual2 = cache.Get(key, dummy.GetStuff);
            fixedClock.Add(new TimeSpan(interval2));
            timer.InvokeElapsed();
            var actual3 = cache.Get(key, dummy.GetStuff);

            Assert.AreEqual(expected, actual1);
            Assert.AreEqual(expected, actual2);
            Assert.AreEqual(expected, actual3);
            dummyMock.Verify(x => x.GetStuff(), Times.Exactly(expectedCount));
        }

        [Test]
        [TestCase(TimeoutStyle.FixedTimeout)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery)]
        public void GetShouldExpireSomeItems(TimeoutStyle style)
        {
            var timer = new FixedTimer();
            var cache = new Cache(new InternalCache(style, new TimeSpan(5), fixedClock), timer);

            cache.Get(new CacheKey("1"), dummy.GetStuff);
            cache.Get(new CacheKey("2"), dummy.GetStuff);
            cache.Get(new CacheKey("3"), dummy.GetStuff);

            fixedClock.Add(new TimeSpan(3));
            timer.InvokeElapsed();

            cache.Get(new CacheKey("4"), dummy.GetStuff);
            cache.Get(new CacheKey("5"), dummy.GetStuff);
            cache.Get(new CacheKey("6"), dummy.GetStuff);

            fixedClock.Add(new TimeSpan(3));
            timer.InvokeElapsed();

            cache.Get(new CacheKey("1"), dummy.GetStuff);
            cache.Get(new CacheKey("2"), dummy.GetStuff);
            cache.Get(new CacheKey("3"), dummy.GetStuff);
            cache.Get(new CacheKey("4"), dummy.GetStuff);
            cache.Get(new CacheKey("5"), dummy.GetStuff);
            cache.Get(new CacheKey("6"), dummy.GetStuff);

            dummyMock.Verify(x => x.GetStuff(), Times.Exactly(9));
        }

        [Test]
        public void GetShouldExpireSomeItemsAndRenewOthers()
        {
            var timer = new FixedTimer();
            var cache = new Cache(new InternalCache(TimeoutStyle.RenewTimoutOnQuery, new TimeSpan(5), fixedClock), timer);

            cache.Get(new CacheKey("1"), dummy.GetStuff);
            cache.Get(new CacheKey("2"), dummy.GetStuff);
            cache.Get(new CacheKey("3"), dummy.GetStuff);
            cache.Get(new CacheKey("4"), dummy.GetStuff);
            cache.Get(new CacheKey("5"), dummy.GetStuff);
            cache.Get(new CacheKey("6"), dummy.GetStuff);

            fixedClock.Add(new TimeSpan(3));
            timer.InvokeElapsed();

            cache.Get(new CacheKey("2"), dummy.GetStuff);
            cache.Get(new CacheKey("3"), dummy.GetStuff);
            cache.Get(new CacheKey("5"), dummy.GetStuff);

            fixedClock.Add(new TimeSpan(3));
            timer.InvokeElapsed();

            cache.Get(new CacheKey("1"), dummy.GetStuff);
            cache.Get(new CacheKey("2"), dummy.GetStuff);
            cache.Get(new CacheKey("3"), dummy.GetStuff);
            cache.Get(new CacheKey("4"), dummy.GetStuff);
            cache.Get(new CacheKey("5"), dummy.GetStuff);
            cache.Get(new CacheKey("6"), dummy.GetStuff);

            dummyMock.Verify(x => x.GetStuff(), Times.Exactly(9));
        }

        [Test]
        public void CacheIsNotUsedWithMethodsOfSameNameAndDifferentClassParameters()
        {
            var cache = new Cache(TimeoutStyle.RenewTimoutOnQuery, new TimeSpan(5));

            cache.Get(new CacheKey("Name", new { Gender = "Male", Age = 23 }), dummy.GetStuff);
            cache.Get(new CacheKey("Name", new { Gender = "Male", Age = 24 }), dummy.GetStuff);

            dummyMock.Verify(x => x.GetStuff(), Times.Exactly(2));
        }

        [Test]
        public void CacheIsUsedWithMethodsOfSameNameAndSameClassParameters()
        {
            var cache = new Cache(TimeoutStyle.RenewTimoutOnQuery, new TimeSpan(5));

            cache.Get(new CacheKey("Name", new { Gender = "Male", Age = 23 }), dummy.GetStuff);
            cache.Get(new CacheKey("Name", new { Gender = "Male", Age = 23 }), dummy.GetStuff);

            dummyMock.Verify(x => x.GetStuff(), Times.Once);
        }
    }
}
