using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Product.Application.Models.ViewModels;
using Shared.Events;
using Shared.Services.Abstractions;

namespace Product.Application.Controllers;

public class ProductsController(IEventStoreService eventStoreService, IMongoDbService mongoDbService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var productCollection = mongoDbService.GetCollection<Shared.Models.Product>("Products");

        var productCursorList = await productCollection.FindAsync(_ => true);

        var productList = await productCursorList.ToListAsync();

        return View(productList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductVM vm)
    {
        NewProductAddedEvent newProductAddedEvent = new()
        {
            ProductId = Guid.NewGuid().ToString(),
            ProductName = vm.ProductName,
            InitialCount = vm.Count,
            InitialPrice = vm.Price,
            IsAvailable = vm.IsAvailable
        };

        await eventStoreService.AppendToStreamAsync("products-stream", new[]
        {
            eventStoreService.GenerateEventData(newProductAddedEvent)
        });

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string productId)
    {
        var productsCollection = mongoDbService.GetCollection<Shared.Models.Product>("Products");

        var product = await (await productsCollection.FindAsync(c => c.Id == productId)).FirstOrDefaultAsync();

        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> CountUpdate(Shared.Models.Product model, int durum)
    {
        var productsCollection = mongoDbService.GetCollection<Shared.Models.Product>("Products");

        var product = await (await productsCollection.FindAsync(c => c.Id == model.Id)).FirstOrDefaultAsync();

        if (durum == 1)
        {
            CountDecreasedEvent countDecreasedEvent = new()
            {
                ProductId = model.Id,
                DecreasedCount = model.Count
            };

            await eventStoreService.AppendToStreamAsync("products-stream",
                new[] { eventStoreService.GenerateEventData(countDecreasedEvent) });
        }
        else if (durum == 0)
        {
            CountIncreasedEvent countIncreasedEvent = new()
            {
                ProductId = model.Id,
                IncrementCount = model.Count
            };

            await eventStoreService.AppendToStreamAsync("products-stream",
                new[] { eventStoreService.GenerateEventData(countIncreasedEvent) });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> PriceUpdate(Shared.Models.Product model, int durum)
    {
        var productsCollection = mongoDbService.GetCollection<Shared.Models.Product>("Products");

        var product = await (await productsCollection.FindAsync(c => c.Id == model.Id)).FirstOrDefaultAsync();

        if (durum == 1)
        {
            PriceDecreasedEvent priceDecreasedEvent = new()
            {
                ProductId = model.Id,
                DecrementAmount = model.Price
            };

            await eventStoreService.AppendToStreamAsync("products-stream",
                new[] { eventStoreService.GenerateEventData(priceDecreasedEvent) });
        }
        else if (durum == 0)
        {
            PriceIncreasedEvent priceIncreasedEvent = new()
            {
                ProductId = model.Id,
                IncrementAmount = model.Price
            };

            await eventStoreService.AppendToStreamAsync("products-stream",
                new[] { eventStoreService.GenerateEventData(priceIncreasedEvent) });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AvailableUpdate(Shared.Models.Product model)
    {
        var productsCollection = mongoDbService.GetCollection<Shared.Models.Product>("Products");

        var product = await (await productsCollection.FindAsync(c => c.Id == model.Id)).FirstOrDefaultAsync();

        if (product.IsAvailable != model.IsAvailable)
        {
            AvailableChangedEvent availableChangedEvent = new()
            {
                ProductId = model.Id,
                IsAvailable = model.IsAvailable
            };

            await eventStoreService.AppendToStreamAsync("products-stream",
                new[] { eventStoreService.GenerateEventData(availableChangedEvent) });
        }

        return RedirectToAction(nameof(Index));
    }
}