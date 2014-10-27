Cachew
======

Simple in memory caching library with castle windsor interceptor

Cached methods can be cleared after a fixed time(FixedTimeout) or when a method has not been called over a period of time(RenewTimeoutOnQuery).

Class to cache:
```c#
public interface IServer
{
  int GetStuff(int id);
}
public class Server : IServer
{
    public int GetStuff(int id)
    {
      ...
    }
}
```

How to cache a method manually:
```c#
var server = new Server();
var cache = new Cache(TimeoutStyle.RenewTimoutOnQuery, TimeSpan.FromSeconds(3));
var result = cache.Get(new CacheKey("GetStuff", 3), () => server.GetStuff(3));
```

How to add cache decorator with the help of a castle windsor interceptor:
```c#
var container = new WindsorContainer();
container.Register(Component.For<CacheInterceptor>()
  .Instance(new CacheInterceptor(new Cache(TimeoutStyle.RenewTimoutOnQuery, TimeSpan.FromSeconds(3)))));
container.Register(Component.For<IServer>().ImplementedBy<Server>().Interceptors<CacheInterceptor>());

var server = container.Resolve<Server>();

var result = server.GetStuff(3);
```

How to create a cached version of a class with a decorator builder:
```c#
IServer server = new Server();
var decoratedServer = new CacheDecoratorBuilder().BuildFromInterface(server);
decoratedServer.GetStuff(3);
```
            


