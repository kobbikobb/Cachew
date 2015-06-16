using System;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Cachew.Tests
{
    [TestFixture]
    public class CacheTests
    {
        private Fixture fixture;
        
        [SetUp]
        public void SetUp()
        {
            fixture = new Fixture();
        }

        [Test]
        public void GetShouldReturnValueFromFunction()
        {
            //Arrange
            var key = CreateAnyCacheKey();
            var cache = CreateAnyCache();
            var expected = fixture.Create<string>();

            var dummyMock = new Mock<IDummy>();
            var dummy = dummyMock.Object;
            dummyMock.Setup(x => x.GetStuff()).Returns(expected);

            //Act
            var result = cache.Get(key, dummy.GetStuff);

            //Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetShouldUseCacheForSecondQuery()
        {
            //Arrange
            var key = CreateAnyCacheKey();            
            var cache = CreateAnyCache();
            var expected = fixture.Create<string>();

            var dummyMock = new Mock<IDummy>();
            var dummy = dummyMock.Object;
            dummyMock.Setup(x => x.GetStuff()).Returns(expected);

            //Act
            var result1 = cache.Get(key, dummy.GetStuff);
            var result2 = cache.Get(key, dummy.GetStuff);

            //Assert
            Assert.AreEqual(expected, result1);
            Assert.AreEqual(expected, result2);
            dummyMock.Verify(x => x.GetStuff(), Times.Exactly(1));
        }

        [Test]
        public void CacheShouldExpireItems()
        {
            //Arrange
            var internalCache = new Mock<IInternalCache>();
            var timer = new FixedTimer();
            var sut = new Cache(internalCache.Object, timer);

            //Act
            timer.InvokeElapsed();
            
            //Assert
            internalCache.Verify(x => x.RemoveExpiredItems());
        }

        private Cache CreateAnyCache()
        {
            return fixture.Create<Cache>();
        }

        private CacheKey CreateAnyCacheKey()
        {
            return fixture.Create<CacheKey>();
        }
    }
}
