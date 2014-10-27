using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Cachew.Tests
{
    [TestFixture]
    public class CacheConcurencyTests
    {
        [Test]
        [TestCase(TimeoutStyle.FixedTimeout)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery)]
        public void CacheShouldWaitForResultOfAnotherThread(TimeoutStyle timeoutStyle)
        {
            //Arrange
            var dummy = new Mock<IDummy>();
            
            var key = new CacheKey("TheKey");
            var cache = new Cache(timeoutStyle, TimeSpan.FromMinutes(1));
            const string result = "TheResult";
            
            dummy.Setup(x => x.GetStuff()).Returns(result);

            var task1 = new Task(() => cache.Get(key, dummy.Object.GetStuff));
            var task2 = new Task(() => cache.Get(key, dummy.Object.GetStuff));

            //Act
            task1.Start();
            task2.Start();
       
            //Assert
            task1.Wait();
            task2.Wait();
            dummy.Verify(x => x.GetStuff(), Times.Once);
        }
    }
}
