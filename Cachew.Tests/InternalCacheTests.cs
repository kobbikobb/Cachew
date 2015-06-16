using System;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Cachew.Tests
{
    [TestFixture]
    public class InternalCacheTests
    {
        private Fixture fixture;

        [SetUp]
        public void SetUp()
        {
            fixture = new Fixture();
        }

        [Test]
        public void TryGetValue_ShouldReturnExistingValues()
        {
            var sut = CreateAnyInternalCache();
            var key = CreateAnyCacheKey();

            object value;
            var result = sut.TryGetValue(key, out value);

            Assert.IsFalse(result, "Cache should not find value");
            Assert.IsNull(value, "Value should be null");
        }

        [Test]
        public void TryGetValue_ShouldNotReturnValuesThatDontExist()
        {
            var sut = CreateAnyInternalCache();
            var key = CreateAnyCacheKey();
            var existingValue = fixture.Create<string>();
            sut.Add(key, existingValue);

            object value;
            var result = sut.TryGetValue(key, out value);

            Assert.IsTrue(result, "Cache should find value");
            Assert.AreEqual(value, existingValue, "Returned value invalid");
        }

        [Test]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 1, 2)]
        [TestCase(TimeoutStyle.FixedTimeout, 1, 2)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 3, 4)]
        [TestCase(TimeoutStyle.FixedTimeout, 4, 5)]
        public void RemoveExpiredItems_ShouldRemoveExpiredItems(TimeoutStyle style, int timeoutTicks, int elapsedTicks)
        {
            //Arrange
            var fixedClock = new FixedClock(new TimeSpan(0));
            var sut = new InternalCache(style, new TimeSpan(timeoutTicks), fixedClock);

            var key = CreateAnyCacheKey();
            var existingValue = fixture.Create<string>();
            sut.Add(key, existingValue);
            fixedClock.Add(new TimeSpan(elapsedTicks));

            //Act
            sut.RemoveExpiredItems();

            //Assert
            object value;
            var result = sut.TryGetValue(key, out value);
            Assert.IsFalse(result, "Cache should not find value");
        }

        [Test]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 1, 1)]
        [TestCase(TimeoutStyle.FixedTimeout, 2, 1)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery, 4, 3)]
        [TestCase(TimeoutStyle.FixedTimeout, 5, 4)]
        public void RemoveExpiredItems_ShouldNotRemoveItemsThatAreNotExpired(TimeoutStyle style, int timeoutTicks, int elapsedTicks)
        {
            //Arrange
            var fixedClock = new FixedClock(new TimeSpan(0));
            var sut = new InternalCache(style, new TimeSpan(timeoutTicks), fixedClock);

            var key = CreateAnyCacheKey();
            var existingValue = fixture.Create<string>();
            sut.Add(key, existingValue);
            fixedClock.Add(new TimeSpan(elapsedTicks));

            //Act
            sut.RemoveExpiredItems();

            //Assert
            object value;
            var result = sut.TryGetValue(key, out value);
            Assert.IsTrue(result, "Cache should find value");
        }

        [Test]
        [TestCase(2, 2, 2)]
        [TestCase(4, 2, 3)]
        [TestCase(10, 9, 9)]
        public void RemoveExpiredItems_ShouldRenewItemsWithRenewTimoutOnQuery(int timeoutTicks, int elapsedTicks1, int elapsedTicks2)
        {
            //Arrange
            var fixedClock = new FixedClock(new TimeSpan(0));
            var sut = new InternalCache(TimeoutStyle.RenewTimoutOnQuery, new TimeSpan(timeoutTicks), fixedClock);

            var key = CreateAnyCacheKey();
            var existingValue = fixture.Create<string>();
            sut.Add(key, existingValue);
            fixedClock.Add(new TimeSpan(elapsedTicks1));

            object value1;
            sut.TryGetValue(key, out value1);
            fixedClock.Add(new TimeSpan(elapsedTicks2));

            //Act
            sut.RemoveExpiredItems();

            //Assert
            object value2;
            var result = sut.TryGetValue(key, out value2);
            Assert.IsTrue(result, "Cache should find value");
        }

        [Test]
        [TestCase(2, 2, 2)]
        [TestCase(4, 2, 3)]
        [TestCase(10, 9, 9)]
        public void RemoveExpiredItems_ShouldNotRenewItemsWithFixteTimeout(int timeoutTicks, int elapsedTicks1, int elapsedTicks2)
        {
            //Arrange
            var fixedClock = new FixedClock(new TimeSpan(0));
            var sut = new InternalCache(TimeoutStyle.FixedTimeout, new TimeSpan(timeoutTicks), fixedClock);

            var key = CreateAnyCacheKey();
            var existingValue = fixture.Create<string>();
            sut.Add(key, existingValue);
            fixedClock.Add(new TimeSpan(elapsedTicks1));

            object value1;
            sut.TryGetValue(key, out value1);
            fixedClock.Add(new TimeSpan(elapsedTicks2));

            //Act
            sut.RemoveExpiredItems();

            //Assert
            object value2;
            var result = sut.TryGetValue(key, out value2);
            Assert.IsFalse(result, "Cache should find value");
        }

        private InternalCache CreateAnyInternalCache()
        {
            return fixture.Create<InternalCache>();
        }

        private CacheKey CreateAnyCacheKey()
        {
            return fixture.Create<CacheKey>();
        }
    }
}
