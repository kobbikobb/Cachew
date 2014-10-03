using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;

namespace Cachew.CastleWindsor.Tests
{
    public interface IRepo
    {
        string GetStuff();
    }

    public class Repo : IRepo
    {
        public string GetStuff()
        {
            Console.WriteLine("Get stuff called");

            return "Stuff";
        }
    }

    public class Service
    {
        private readonly IRepo repo;

        public Service(IRepo repo)
        {
            this.repo = repo;
        }

        public void Invoke()
        {
            Console.WriteLine(repo.GetStuff());
        }
    }

    [TestFixture]
    public class CachingInterceptorIntegrationTests
    {
        [Test]
        public void ResolveServiceWithInterceptor()
        {
            var container = new WindsorContainer();
            container.Register(Component.For<CachingInterceptor>()
                .Instance(new CachingInterceptor(new Cache(TimeoutStyle.RenewTimoutOnQuery, TimeSpan.FromSeconds(3)))));
            container.Register(Component.For<IRepo>().ImplementedBy<Repo>().Interceptors<CachingInterceptor>());
            container.Register(Component.For<Service>());

            var result = container.Resolve<Service>();

            result.Invoke();
            result.Invoke();
        }
    }
}
