using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Web.Builders;
using PizzaShop.Web.Data;
using PizzaShop.Web.Domain;
using PizzaShop.Web.Services;

namespace PizzaShop.Tests;

/// <summary>Round-trips builder output through EF Core using an in-memory SQLite database.</summary>
public sealed class PizzaServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PizzaService _service;

    public PizzaServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PizzaDbContext>().UseSqlite(_connection).Options;
        using (var db = new PizzaDbContext(options))
        {
            db.Database.EnsureCreated();
        }

        _service = new PizzaService(new TestContextFactory(options));
    }

    [Fact]
    public async Task GetToppings_ReturnsSeededCatalog()
    {
        var available = await _service.GetToppingsAsync();
        var all = await _service.GetToppingsAsync(includeUnavailable: true);

        Assert.Equal(SeedData.Toppings.Count(t => t.IsAvailable), available.Count);
        Assert.Equal(SeedData.Toppings.Count, all.Count);
    }

    [Fact]
    public async Task Save_ThenLoad_RoundTripsPizzaWithToppings()
    {
        var catalog = await _service.GetToppingsAsync();
        var pizza = new PizzaDirector(catalog).Pepperoni(new PizzaBuilder(), PizzaSize.Large).Build();

        var saved = await _service.SaveAsync(pizza);
        var loaded = await _service.GetPizzaAsync(saved.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Pepperoni", loaded.Name);
        Assert.Equal(PizzaSize.Large, loaded.Size);
        Assert.Equal(pizza.Price, loaded.Price);
        Assert.Equal(2, loaded.Toppings.Count);
        Assert.Equal(2, loaded.Toppings.Single(t => t.Name == "Pepperoni").Quantity);
        Assert.Equal("Mozzarella, 2× Pepperoni", loaded.ToppingsText);
        Assert.Equal(ToppingCategory.Cheese, loaded.Toppings[0].Category);

        // Saving must not duplicate catalog toppings.
        Assert.Equal(SeedData.Toppings.Count, (await _service.GetToppingsAsync(includeUnavailable: true)).Count);
    }

    [Fact]
    public async Task GetSavedPizzas_ProjectsNewestFirstWithToppingLines()
    {
        var catalog = await _service.GetToppingsAsync();
        var director = new PizzaDirector(catalog);
        var first = await _service.SaveAsync(director.Margherita(new PizzaBuilder()).Build());
        var second = await _service.SaveAsync(director.BbqChicken(new PizzaBuilder()).Build());

        var list = await _service.GetSavedPizzasAsync();

        Assert.Equal([second.Id, first.Id], list.Select(p => p.Id));
        Assert.Equal(["Cheddar", "Grilled chicken", "Red onion"], list[0].Toppings.Select(t => t.Name));
        Assert.Equal(SauceType.Bbq, list[0].Sauce);
    }

    [Fact]
    public async Task Delete_RemovesPizzaAndItsToppingRows()
    {
        var catalog = await _service.GetToppingsAsync();
        var saved = await _service.SaveAsync(new PizzaBuilder().AddTopping(catalog[0]).Build());

        await _service.DeleteAsync(saved.Id);

        Assert.Null(await _service.GetPizzaAsync(saved.Id));
        Assert.Empty(await _service.GetSavedPizzasAsync());
    }

    public void Dispose() => _connection.Dispose();

    private sealed class TestContextFactory(DbContextOptions<PizzaDbContext> options) : IDbContextFactory<PizzaDbContext>
    {
        public PizzaDbContext CreateDbContext() => new(options);
    }
}
