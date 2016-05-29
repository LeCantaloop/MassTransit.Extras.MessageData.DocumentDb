# MassTransit.Extras.MessageData.DocumentDb

A library for [MassTransit][mt] to use [Azure DocumentDB][docdb] to store large message payloads.

[![Build status](https://ci.appveyor.com/api/projects/status/5r0447j1ysbd531e?svg=true)](https://ci.appveyor.com/project/MattKotsenas/masstransit-extras-messagedata-documentdb)
[![Install from nuget](http://img.shields.io/nuget/v/MassTransit.Extras.MessageData.DocumentDb.svg?style=flat-square)](https://www.nuget.org/packages/MassTransit.Extras.MessageData.DocumentDb)

## Why?

Sometimes your services need to send large payloads. Putting the big payload in the message either
slows down your whole app, or breaks it entirely if your message bus has a maximum message size.

One solution to this problem is the [claim check][claim-check] pattern; store the big fields of
your message in an external datastore and send a message with just links to your big fields.

## Getting Started

Install the package using nuget or the Visual Studio Package Manager console

```cmd
nuget install MassTransit.Extras.MessageData.DocumentDb
```
```powershell
PM> Install-Package MassTransit.Extras.MessageData.DocumentDb
```

### Creating the DocumentClient
Now that we have the package, we next create a `DocumentClient` that talks to the DocumentDB database

```csharp
var client = new DocumentClient(new Uri("https://mydb.documents.azure.com:443/"), "secret-key");
var repo = new DocumentDbRepository(client, "database-name", "collection-name");
```

### Creating a message

Next we'll create a message that contains a big data field

```csharp
public class BigMessage
{
    public string FirstProperty { get; set; }
    public double SecondProperty { get; set; }
    public MessageData<byte[]> BigProperty { get; set; }
}
```

and fill it with some data

```csharp
var ttl = TimeSpan.FromMinutes(10);
var bigData = new byte[] { 1, 2, 3 };
var payload = await repo.PutBytes(bigData, ttl);
var message = new BigMessage
{
    FirstProperty = "This data will be in the message itself",
    SecondProperty = 7.0,
    BigProperty = payload
};
```

When we create the message we need to use our repository to put the payload into DocumentDB. The repository
gives us back a `MessageData` object that contains a `Uri` for retrieving the data later. We also set the
time to live on the message, which will come in handy later.

### Setting up MassTransit

Next let's set up MassTransit. All we need to do is tell the endpoint to use our `repo` with our
`BigMessage`s

```csharp
var bus = Bus.Factory.CreateUsingInMemory(cfg =>
{
    // Configured your endpoint here
    cfg.ReceiveEndpoint("test_queue", e =>
    {
        e.UseMessageData<BigMessage>(repo);
        e.Consumer<BigMessageConsumer>();
    });
});
bus.Start();
```

### Sending and receiving messages

Finally we can start sending and receiving messages. To send just

```csharp
bus.Publish(message);
```

and to receive we create a consumer like this

```csharp
public class BigMessageConsumer : IConsumer<BigMessage>
{
    public async Task Consume(ConsumeContext<BigMessage> context)
    {
        var payload = await context.Message.BigProperty.Value;
        // Do stuff
    }
}
```

## Deleting old messages

Depending on your needs you may need to delete old `MessageData` payloads from your DocumentDB
to keep it from filling up. To have DocumentDB auto-delete old messages you need to

1. Set the time to live value in the `Put` call, as we did in the example
2. Enable time to live tracking for your DocumentDB collection. You can find more information on
enabling that [here][docdb-ttl]

Of course this also means if your consumers aren't consuming messages fast enough they may get
an exception when trying to retrieve `MessageData` payloads.

## DocumentClient best practices

If your app will be sending a high rate of messages it's important to use the `DocumentClient`
object correctly. First, make sure you're following the DocumentDB [performance best practices][docdb-perf],
namely using direct mode and sharing your client across your AppDomain. `DocumentClient`
implements `IDisposable`, so to facilitate sharing the `DocumentDbRepository` supports a
`DocumentClientReference` wrapper. This class has a property `IsOwned`, with a default value of `true`;
set it to `false` if you would like to manage the lifetime of the `DocumentClient`
yourself.

Here's an example using [Autofac][autofac]. In this sample Autofac creates a singleton `DocumentClient`
and is a responsible for cleaning up all objects when the lifetime goes out-of-scope.

```csharp
var builder = new ContainerBuilder();
builder.Register(ctx =>
{
    return new DocumentClient(new Uri("https://mydb.documents.azure.com:443/"), "secret-key");
}).AsSelf().SingleInstance();

builder.Register(ctx =>
{
    var client = ctx.Resolve<DocumentClient>();
    var reference = new DocumentDbReference
    {
        Client = client,
        IsOwned = false
    };
    return new DocumentDbRepository(reference, "database-name", "collection-name");
}).AsRegisteredInterfaces();

using (var lifetimeScope = builder.Build())
{
    var repo = lifetimeScope.Resolve<IMessageDataRepository>();
    // Do stuff with repo
}
```

[mt]: http://masstransit-project.com/
[autofac]: http://autofac.org/
[docdb]: https://azure.microsoft.com/en-us/services/documentdb/
[claim-check]: http://www.enterpriseintegrationpatterns.com/patterns/messaging/StoreInLibrary.html        
[docdb-ttl]: https://azure.microsoft.com/en-us/documentation/articles/documentdb-time-to-live/
[docdb-perf]: https://azure.microsoft.com/en-us/blog/performance-tips-for-azure-documentdb-part-1-2/
