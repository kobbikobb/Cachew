using System.Threading;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cachew.Tests
{
    [TestFixture]
    public class CacheThreadingTests
    {
        [Test]
        public void RemovingItemsWhileOtherThreadsAreQueryingShouldNotThrowAnException()
        {
            //Arrange
            var cache = new Cache(TimeoutStyle.FixedTimeout, TimeSpan.FromTicks(1));
            var startTime = DateTime.Now.AddMilliseconds(5);
            Exception lastException = null;

            Action<string> action = (string key) =>
            {
                while (DateTime.Now < startTime)
                {
                    //do nothing
                }
                for (int i = 0; i < 100; i++)
                {
                    try
                    {
                        cache.Get<string>(new CacheKey(key), () => null);
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                    }
                }
            };

            var thread1 = new Thread(new ThreadStart(() => action("thread1")));
            var thread2 = new Thread(new ThreadStart(() => action("thread2")));

            //Act
            thread1.Start();
            thread2.Start();

            action.Invoke("This");

            //Assert
            if (lastException != null)
                throw lastException;
        }
    }
}
