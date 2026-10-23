# PizzaBuilder

A Blazor Web App (.NET 9, interactive server rendering) that lets customers compose a pizza step by step.
The domain uses the **Builder pattern** to assemble pizzas, and **EF Core 9** with SQLite to persist them.

## Projects

| Path | Purpose |
| --- | --- |
| `src/PizzaShop.Web` | Blazor app, domain, builder, EF Core data access |
| `tests/PizzaShop.Tests` | xUnit tests for the builder, director and EF Core service |

## Running

```bash
dotnet run --project src/PizzaShop.Web
```

On first start the app applies the EF Core migrations, which create `pizzashop.db` and seed the topping catalog.

```bash
dotnet test
```

## Builder pattern

| Role | Type |
| --- | --- |
| Builder interface | `Builders/IPizzaBuilder` |
| Concrete builder | `Builders/PizzaBuilder` (fluent steps, validation, pricing, `Preview()` for the live UI, `Build()` for the product) |
| Director | `Builders/PizzaDirector` (house recipes such as Margherita or Meat Lovers, driven through any builder) |
| Product | `Domain/Pizza` (init-only, so it can only be assembled by the builder) |

```csharp
var pizza = new PizzaBuilder()
    .Named("Friday night")
    .OfSize(PizzaSize.Large)
    .WithCrust(CrustType.Stuffed)
    .WithSauce(SauceType.Bbq)
    .AddTopping(pepperoni, quantity: 2)
    .AddTopping(mushrooms)
    .WithExtraCheese()
    .Build();
```

### Delegate based steps

Besides the plain steps, the builder accepts `Action` and `Func` delegates:

| Step | Delegate | Purpose |
| --- | --- | --- |
| `Configure(Action<IPizzaBuilder>)` | Action | Apply a block of steps or a method group (shared recipe fragments in the director) |
| `When(bool, Action<IPizzaBuilder> then, Action<IPizzaBuilder>? otherwise)` | Action | Conditional steps, e.g. no stuffed crust on small pizzas |
| `OnBuilt(Action<Pizza>)` | Action | Callback for every product `Build()` creates (the page logs it) |
| `AddToppings(IEnumerable<Topping>, Func<Topping,bool>? where, Func<Topping,int>? quantity)` | Func | Bulk add with a filter and a per-topping quantity |
| `WithRule(Func<PizzaDraft, string?>)` | Func | Custom validation rule, surfaced in `Preview()` and enforced by `Build()` (the page adds a budget rule) |
| `Build<T>(Func<Pizza, T>)` | Func | Build and map/persist in one call, e.g. `Build(p => service.SaveAsync(p))` |

```csharp
var pizza = new PizzaBuilder()
    .WithRule(d => d.Price.Total > 20m ? "Over budget." : null)
    .OnBuilt(p => logger.LogInformation("Built {Pizza}", p))
    .Configure(b => b.WithCrust(CrustType.Thin).WithSauce(SauceType.Pesto))
    .AddToppings(catalog, where: t => t.Category == ToppingCategory.Vegetable, quantity: t => t.Name == "Mushrooms" ? 2 : 1)
    .When(isHungry, b => b.OfSize(PizzaSize.Large).WithExtraCheese())
    .Build();
```

Rules and callbacks configure the builder itself, so they survive `Reset()`; the recipe does not.

The Blazor page injects a transient `IPizzaBuilder`, applies one builder step per UI interaction, and
renders `Preview()` for the running price and validation problems. Saving calls `Build()` and hands the
product to `IPizzaService`, which stores it with EF Core.

## EF Core

- `Data/PizzaDbContext` maps `Pizza`, `Topping` and the `PizzaTopping` join entity (with quantity).
- Enums are stored as strings, the topping catalog is seeded with `HasData`.
- `IDbContextFactory<PizzaDbContext>` is used so each operation gets a short-lived context, as recommended for Blazor Server.
- Reads use projections, not `Include`: a single `Expression<Func<Pizza, PizzaSummary>>` in `PizzaService` maps a pizza and its topping lines into the `PizzaSummary` read model, so queries fetch only the needed columns and track nothing.
- Migrations live in `src/PizzaShop.Web/Migrations`. Add one with:

```bash
dotnet ef migrations add <Name> --project src/PizzaShop.Web
```
