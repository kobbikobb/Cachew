using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Cachew.Tests
{
    [TestFixture]
    public class CacheKeyTests
    {
        [Test]
        [TestCase("GetObjects")]
        [TestCase("GetBar")]
        [TestCase("GetBoo")]
        public void TwoKeysWithSameNameAndNoParametersAreEqual(string name)
        {
            var key1 = new CacheKey(name);
            var key2 = new CacheKey(name);

            Assert.AreEqual(key1, key2);
        }

        [Test]
        [TestCase("GetObjects", "GetObjectS")]
        [TestCase("GetBar", "GetFoo")]
        [TestCase("GetBoo", "")]
        public void TwoKeysWithDifferentNameAndNoParametersAreNotEqual(string name1, string name2)
        {
            var key1 = new CacheKey(name1);
            var key2 = new CacheKey(name2);

            Assert.AreNotEqual(key1, key2);
        }

        [Test]
        [TestCase("GetObjects", 3, "argument")]
        [TestCase("GetBar", 45, "king")]
        [TestCase("GetBoo", 65, "pirate")]
        public void TwoKeysWithSameNameAndSameParametersAreEqual(string name, int argument1, string argument2)
        {
            var key1 = new CacheKey(name, argument1, argument2);
            var key2 = new CacheKey(name, argument1, argument2);

            Assert.AreEqual(key1, key2);
        }

        [Test]
        [TestCase("GetObjects", 3, 4, "argument", "argument")]
        [TestCase("GetObjects", 3, 3, "argument", "argumenT")]
        [TestCase("GetBar", 45, 4, "king", "king")]
        [TestCase("GetBar", 45, 45, "king", "spider")]
        public void TwoKeysWithSameNameAndDifferentParametersAreNotEqual(
            string name, int intArgument1, int intArgument2, string stringArgument1, string stringArgument2)
        {
            var key1 = new CacheKey(name, intArgument1, stringArgument1);
            var key2 = new CacheKey(name, intArgument2, stringArgument2);

            Assert.AreNotEqual(key1, key2);
        }

        [Test]
        public void TwoKeysWithSameNameAndSameParameterClassesAreEqual()
        {
            var key1 = new CacheKey("Name", 1, new {File="SomeFile.txt", Source="Target"});
            var key2 = new CacheKey("Name", 1, new { File = "SomeFile.txt", Source = "Target" });

            Assert.AreEqual(key1, key2);
        }

        [Test]
        public void TwoKeysWithSameNameAndDifferentParameterClassesAreNotEqual()
        {
            var key1 = new CacheKey("Name", 1, new { File = "SomeFile.txt", Source = "Target" });
            var key2 = new CacheKey("Name", 1, new { File = "SomeFile.txt", Source = "Target2" });

            Assert.AreNotEqual(key1, key2);
        }

        [Test]
        public void KeyNameCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CacheKey(null));
        }
    }
}
