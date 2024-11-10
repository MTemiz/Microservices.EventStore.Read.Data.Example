using Product.Event.Handler.Service.Services;
using Shared.Services;
using Shared.Services.Abstractions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IEventStoreService, EventStoreService>();
builder.Services.AddSingleton<IMongoDbService, MongoDbService>();

builder.Services.AddHostedService<EventStoreBackgroundService>();

var host = builder.Build();
host.Run();