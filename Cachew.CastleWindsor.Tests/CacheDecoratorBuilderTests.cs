using System;
using Moq;
using NUnit.Framework;

namespace Cachew.CastleWindsor.Tests
{
    public interface IDummy
    {
        string GetStuff();
        bool UpdateDatabase();
    }

    [TestFixture]
    public class CacheDecoratorBuilderTests
    {
        private Mock<IDummy> dummyMock;

        [SetUp]
        public void SetUp()
        {
            dummyMock = new Mock<IDummy>();
        }

        [Test]
        public void GetterRetreivesFromCache()
        {
            var decorator = new CacheDecoratorBuilder().BuildFromInterface(dummyMock.Object);
            decorator.GetStuff();
            dummyMock.Verify(x => x.GetStuff(), Times.Once());
        }

        [Test]
        public void MethodWithReturnValueNotCached()
        {
            var decorator = new CacheDecoratorBuilder().BuildFromInterface(dummyMock.Object);
            decorator.UpdateDatabase();
            dummyMock.Verify(x => x.GetStuff(), Times.Never);
        }

        [Test]
        public void BuildWithValues()
        {
            var decorator = new CacheDecoratorBuilder()
                .SetTimeout(TimeSpan.FromMilliseconds(4))
                .SetTimeoutStyle(TimeoutStyle.FixedTimeout)
                .SetMethodPrefixes("Get", "get", "Retreive")
                .BuildFromInterface(dummyMock.Object);

            decorator.GetStuff();
            dummyMock.Verify(x => x.GetStuff(), Times.Once());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullInMethodPrefixes()
        {
            var decorator = new CacheDecoratorBuilder().SetMethodPrefixes(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyMethodPrefixes()
        {
            var decorator = new CacheDecoratorBuilder().SetMethodPrefixes(new string[] { });
        }
    }
}
