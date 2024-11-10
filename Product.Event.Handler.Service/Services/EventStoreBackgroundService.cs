using System.Reflection;
using System.Text.Json;
using MongoDB.Driver;
using Shared.Events;
using Shared.Services.Abstractions;

namespace Product.Event.Handler.Service.Services;

public class EventStoreBackgroundService(IEventStoreService eventStoreService, IMongoDbService mongoDbService)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await eventStoreService.SubscribeToStreamAsync("products-stream",
            async (streamSubscription, resolvedEvent, cancellationToken) =>
            {
                string eventType = resolvedEvent.Event.EventType;

                object @event = JsonSerializer.Deserialize(resolvedEvent.Event.Data.ToArray(),
                    Assembly.Load("Shared").GetTypes().FirstOrDefault((t => t.Name == eventType)));

                var productCollection = mongoDbService.GetCollection<Shared.Models.Product>("Products");

                Shared.Models.Product product = null;

                switch (@event)
                {
                    case NewProductAddedEvent e:
                        bool hasProduct = await (await productCollection.FindAsync(c => c.Id == e.ProductId))
                            .AnyAsync();

                        if (!hasProduct)
                        {
                            await productCollection.InsertOneAsync(new()
                            {
                                Id = e.ProductId,
                                ProductName = e.ProductName,
                                Price = e.InitialPrice,
                                Count = e.InitialCount,
                                IsAvailable = e.IsAvailable
                            });
                        }

                        break;

                    case PriceDecreasedEvent e:
                        product = await (await productCollection.FindAsync(c => c.Id == e.ProductId))
                            .FirstOrDefaultAsync();

                        if (product != null)
                        {
                            product.Price -= e.DecrementAmount;

                            await productCollection.FindOneAndReplaceAsync(c => c.Id == e.ProductId, product);
                        }

                        break;

                    case PriceIncreasedEvent e:
                        product = await (await productCollection.FindAsync(c => c.Id == e.ProductId))
                            .FirstOrDefaultAsync();

                        if (product != null)
                        {
                            product.Price += e.IncrementAmount;

                            await productCollection.FindOneAndReplaceAsync(c => c.Id == e.ProductId, product);
                        }

                        break;

                    case AvailableChangedEvent e:
                        product = await (await productCollection.FindAsync(c => c.Id == e.ProductId))
                            .FirstOrDefaultAsync();

                        if (product != null)
                        {
                            product.IsAvailable = e.IsAvailable;

                            await productCollection.FindOneAndReplaceAsync(c => c.Id == e.ProductId, product);
                        }

                        break;

                    case CountIncreasedEvent e:
                        product = await (await productCollection.FindAsync(c => c.Id == e.ProductId))
                            .FirstOrDefaultAsync();

                        if (product != null)
                        {
                            product.Count += e.IncrementCount;

                            await productCollection.FindOneAndReplaceAsync(c => c.Id == e.ProductId, product);
                        }

                        break;
                    
                    case CountDecreasedEvent e:
                        product = await (await productCollection.FindAsync(c => c.Id == e.ProductId))
                            .FirstOrDefaultAsync();

                        if (product != null)
                        {
                            product.Count -= e.DecreasedCount;

                            await productCollection.FindOneAndReplaceAsync(c => c.Id == e.ProductId, product);
                        }
                        break;

                    default:
                        break;
                }
            });
    }
}