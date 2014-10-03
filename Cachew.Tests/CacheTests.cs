using System;
using Moq;
using NUnit.Framework;

namespace Cachew.Tests
{
    public interface IDummy
    {
        string GetStuff();
    }

    [TestFixture]
    public class CacheTests
    {
        private string expected;
        private Mock<IDummy> dummyMock;
        private IDummy dummy;
        private string key;
        private FixedClock clock;
        
        [SetUp]
        public void SetUp()
        {
            expected = "return string";
            dummyMock = new Mock<IDummy>();
            dummy = dummyMock.Object;
            key = "key";
            clock = new FixedClock(new TimeSpan(0));

            dummyMock.Setup(x => x.GetStuff()).Returns(expected);
            Clock.Instance = clock;
        }

        [Test]
        [TestCase(TimeoutStyle.FixedTimeout, 2, 1)]
        [TestCase(TimeoutStyle.FixedTimeout, 4, 1)]
        [TestCase(TimeoutStyle.FixedTimeout, 5, 2)]
        [TestCase(TimeoutStyle.FixedTimeout, 6, 2)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 2, 1)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 4, 1)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 5, 2)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 6, 2)]
        public void IsCacheUsed(TimeoutStyle style, int interval, int expectedCount)
        {
            var cache = new Cache(style, new TimeSpan(5));

            var actual1 = cache.Get(key, dummy.GetStuff);
            clock.Add(new TimeSpan(interval));
            var actual2 = cache.Get(key, dummy.GetStuff);

            Assert.AreEqual(expected, actual1);
            Assert.AreEqual(expected, actual2);
            dummyMock.Verify(x => x.GetStuff(), Times.Exactly(expectedCount));
        }

        [Test]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 4, 4, 1)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 4, 5, 2)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 4, 6, 2)]
        [TestCase(TimeoutStyle.FixedTimeout, 4, 4, 2)]
        [TestCase(TimeoutStyle.FixedTimeout, 4, 5, 2)]
        [TestCase(TimeoutStyle.FixedTimeout, 4, 6, 2)]
        public void IsCacheRenewed(TimeoutStyle style, int interval1, int interval2, int expectedCount)
        {
            var cache = new Cache(style, new TimeSpan(5));

            var actual1 = cache.Get(key, dummy.GetStuff);
            clock.Add(new TimeSpan(interval1));
            var actual2 = cache.Get(key, dummy.GetStuff);
            clock.Add(new TimeSpan(interval2));
            var actual3 = cache.Get(key, dummy.GetStuff);

            Assert.AreEqual(expected, actual1);
            Assert.AreEqual(expected, actual2);
            Assert.AreEqual(expected, actual3);
            dummyMock.Verify(x => x.GetStuff(), Times.Exactly(expectedCount));
        }

        [Test]
        [TestCase(TimeoutStyle.FixedTimeout)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery)]
        public void SomeItemsExpired(TimeoutStyle style)
        {
            var cache = new Cache(style, new TimeSpan(5));

            cache.Get(1, dummy.GetStuff);
            cache.Get(2, dummy.GetStuff);
            cache.Get(3, dummy.GetStuff);

            clock.Add(new TimeSpan(3));

            cache.Get(4, dummy.GetStuff);
            cache.Get(5, dummy.GetStuff);
            cache.Get(6, dummy.GetStuff);

            clock.Add(new TimeSpan(2));

            cache.Get(1, dummy.GetStuff);
            cache.Get(2, dummy.GetStuff);
            cache.Get(3, dummy.GetStuff);
            cache.Get(4, dummy.GetStuff);
            cache.Get(5, dummy.GetStuff);
            cache.Get(6, dummy.GetStuff);

            dummyMock.Verify(x => x.GetStuff(), Times.Exactly(9));
        }

        [Test]
        public void SomeItemsExpiredSomeRenewed()
        {
            var cache = new Cache(TimeoutStyle.RenewTimoutOnQuery, new TimeSpan(5));

            cache.Get(1, dummy.GetStuff);
            cache.Get(2, dummy.GetStuff);
            cache.Get(3, dummy.GetStuff);
            cache.Get(4, dummy.GetStuff);
            cache.Get(5, dummy.GetStuff);
            cache.Get(6, dummy.GetStuff);

            clock.Add(new TimeSpan(3));

            cache.Get(2, dummy.GetStuff);
            cache.Get(3, dummy.GetStuff);
            cache.Get(5, dummy.GetStuff);

            clock.Add(new TimeSpan(2));

            cache.Get(1, dummy.GetStuff);
            cache.Get(2, dummy.GetStuff);
            cache.Get(3, dummy.GetStuff);
            cache.Get(4, dummy.GetStuff);
            cache.Get(5, dummy.GetStuff);
            cache.Get(6, dummy.GetStuff);

            dummyMock.Verify(x => x.GetStuff(), Times.Exactly(9));
        }
    }
}
