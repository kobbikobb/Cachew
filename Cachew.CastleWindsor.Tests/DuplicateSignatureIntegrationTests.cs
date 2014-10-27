using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;

namespace Cachew.CastleWindsor.Tests
{
    public interface IMachine
    {
        int CallCount { get; }
        string GetKey();
    }

    public class Machine : IMachine
    {
        public int CallCount { get; private set; }

        public string GetKey()
        {
            CallCount++;
            return "Machine";
        }
    }

    public interface IToys
    {
        int CallCount { get; }
        string GetKey();
    }

    public class Toys : IToys
    {
        public int CallCount { get; private set; }

        public string GetKey()
        {
            CallCount++;
            return "Toys";
        }
    }

    [TestFixture]
    public class DuplicateSignatureIntegrationTests
    {
        [Test]
        [TestCase(TimeoutStyle.FixedTimeout)]
        [TestCase(TimeoutStyle.RenewTimoutOnQuery)]
        public void InterceptShouldNotUseCacheForDifferentClassesWithSameSignature(TimeoutStyle timeoustStyle)
        {
            //Arrange
            var cache = new Cache(TimeoutStyle.FixedTimeout, TimeSpan.FromMinutes(1));

            var container = new WindsorContainer();
            container.Register(Component.For<CacheInterceptor>()
                .Instance(new CacheInterceptor(cache)));
            container.Register(Component.For<IMachine>().ImplementedBy<Machine>().Interceptors<CacheInterceptor>());
            container.Register(Component.For<IToys>().ImplementedBy<Toys>().Interceptors<CacheInterceptor>());

            var machine = container.Resolve<IMachine>();
            var toys = container.Resolve<IToys>();

            //Act - Assert
            Assert.AreEqual("Machine", machine.GetKey(), "First machine key");
            Assert.AreEqual("Toys", toys.GetKey(), "First toys key");
            Assert.AreEqual("Machine", machine.GetKey(), "Second machine key");
            Assert.AreEqual("Toys", toys.GetKey(), "Second toys key");
            Assert.AreEqual(1, machine.CallCount, "Machine call count");
            Assert.AreEqual(1, toys.CallCount, "Toys call count");
        }
    }
}
