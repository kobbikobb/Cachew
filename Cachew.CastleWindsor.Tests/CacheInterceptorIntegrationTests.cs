using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Moq;
using NUnit.Framework;

namespace Cachew.CastleWindsor.Tests
{
    public class StuffInfo
    {
        public string Property1 { get; set; }
        public string Property2 { get; set; }

        protected bool Equals(StuffInfo other)
        {
            return string.Equals(Property1, other.Property1) && string.Equals(Property2, other.Property2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StuffInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Property1 != null ? Property1.GetHashCode() : 0) * 397) ^ (Property2 != null ? Property2.GetHashCode() : 0);
            }
        }
    }

    public interface IRepo
    {
        string GetStuff();
        string GetStuff(StuffInfo stuffInfo);
    }

    public class Repo : IRepo
    {
        public string GetStuff()
        {
            Console.WriteLine("Get stuff called");

            return "Stuff";
        }

        public string GetStuff(StuffInfo stuffInfo)
        {
            Console.WriteLine("Get stuff called");

            return "Stuff: " + stuffInfo.Property1 + " " + stuffInfo.Property2;
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
    public class CacheInterceptorIntegrationTests
    {
        [Test]
        public void ResolveServiceWithInterceptor()
        {
            var container = new WindsorContainer();
            container.Register(Component.For<CacheInterceptor>()
                .Instance(new CacheInterceptor(new Cache(TimeoutStyle.RenewTimoutOnQuery, TimeSpan.FromSeconds(3)))));
            container.Register(Component.For<IRepo>().ImplementedBy<Repo>().Interceptors<CacheInterceptor>());
            container.Register(Component.For<Service>());

            var result = container.Resolve<Service>();

            result.Invoke();
            result.Invoke();
        }

        [Test]
        public void ClassWithCacheShouldUseCache()
        {
            var repo = new Mock<IRepo>();
            var repoWithCache = new CacheDecoratorBuilder().BuildFromInterface(repo.Object);
            
            repoWithCache.GetStuff();
            repoWithCache.GetStuff();

            repo.Verify(x => x.GetStuff(), Times.Once);
        }

        [Test]
        public void ClassWithCacheShouldUseCacheIfParameterClassesAreEqual()
        {
            var repo = new Mock<IRepo>();
            var repoWithCache = new CacheDecoratorBuilder().BuildFromInterface(repo.Object);

            repoWithCache.GetStuff(new StuffInfo() { Property1 = "1", Property2 = "3" });
            repoWithCache.GetStuff(new StuffInfo() { Property1 = "1", Property2 = "3" });

            repo.Verify(x => x.GetStuff(It.IsAny<StuffInfo>()), Times.Once);
        }

        [Test]
        public void ClassWithCacheShouldNotUseCacheIfParameterClassIsDifferent()
        {
            var repo = new Mock<IRepo>();
            var repoWithCache = new CacheDecoratorBuilder().BuildFromInterface(repo.Object);

            repoWithCache.GetStuff(new StuffInfo() { Property1 = "1", Property2 = "2"} );
            repoWithCache.GetStuff(new StuffInfo() { Property1 = "1", Property2 = "3" });

            repo.Verify(x => x.GetStuff(It.IsAny<StuffInfo>()), Times.Exactly(2));
        }
    }
}
