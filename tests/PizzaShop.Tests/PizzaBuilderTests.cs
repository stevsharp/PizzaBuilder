using PizzaShop.Web.Builders;
using PizzaShop.Web.Data;
using PizzaShop.Web.Domain;

namespace PizzaShop.Tests;

public class PizzaBuilderTests
{
    private static Topping Get(string name) => SeedData.Toppings.Single(t => t.Name == name);

    [Fact]
    public void Build_WithDefaults_ProducesMediumRegularTomatoPizza()
    {
        var pizza = new PizzaBuilder().Build();

        Assert.Equal("Custom pizza", pizza.Name);
        Assert.Equal(PizzaSize.Medium, pizza.Size);
        Assert.Equal(CrustType.Regular, pizza.Crust);
        Assert.Equal(SauceType.Tomato, pizza.Sauce);
        Assert.Empty(pizza.Toppings);
        Assert.Equal(9.50m, pizza.Price);
    }

    [Fact]
    public void Build_PricesEveryComponent()
    {
        // Large: base 12.00, topping multiplier 1.3
        // Stuffed crust +2.50, BBQ sauce +0.50
        // Pepperoni 1.50 x2 + Mushrooms 1.00 = 4.00 * 1.3 = 5.20
        // Extra cheese 1.50 * 1.3 = 1.95
        var pizza = new PizzaBuilder()
            .Named("Test")
            .OfSize(PizzaSize.Large)
            .WithCrust(CrustType.Stuffed)
            .WithSauce(SauceType.Bbq)
            .AddTopping(Get("Pepperoni"), 2)
            .AddTopping(Get("Mushrooms"))
            .WithExtraCheese()
            .Build();

        Assert.Equal(12.00m + 2.50m + 0.50m + 5.20m + 1.95m, pizza.Price);
        Assert.Equal(2, pizza.Toppings.Count);
        Assert.Equal(2, pizza.Toppings.Single(t => t.ToppingId == Get("Pepperoni").Id).Quantity);
    }

    [Fact]
    public void AddTopping_SameToppingTwice_ReplacesQuantityAndCapsIt()
    {
        var draft = new PizzaBuilder()
            .AddTopping(Get("Ham"), 1)
            .AddTopping(Get("Ham"), 10)
            .Preview();

        var ham = Assert.Single(draft.Toppings);
        Assert.Equal(PizzaBuilder.MaxQuantityPerTopping, ham.Quantity);
    }

    [Fact]
    public void AddTopping_WithZeroQuantity_RemovesTopping()
    {
        var draft = new PizzaBuilder()
            .AddTopping(Get("Ham"))
            .AddTopping(Get("Ham"), 0)
            .Preview();

        Assert.Empty(draft.Toppings);
    }

    [Fact]
    public void Build_TooManyPortions_Throws()
    {
        var builder = new PizzaBuilder();
        foreach (var topping in SeedData.Toppings.Where(t => t.IsAvailable).Take(5))
        {
            builder.AddTopping(topping, 2); // 10 portions > 8
        }

        var ex = Assert.Throws<PizzaBuildException>(() => builder.Build());
        Assert.Contains(ex.Problems, p => p.Contains("portions"));
        Assert.False(builder.Preview().IsValid);
    }

    [Fact]
    public void Build_SmallStuffedCrust_IsRejected()
    {
        var builder = new PizzaBuilder().OfSize(PizzaSize.Small).WithCrust(CrustType.Stuffed);

        var ex = Assert.Throws<PizzaBuildException>(() => builder.Build());
        Assert.Contains(ex.Problems, p => p.Contains("Stuffed crust"));
    }

    [Fact]
    public void Build_UnavailableTopping_IsRejected()
    {
        var builder = new PizzaBuilder().AddTopping(Get("Artichoke hearts"));

        Assert.Throws<PizzaBuildException>(() => builder.Build());
    }

    [Fact]
    public void Build_NoSauceNoToppingsNoCheese_IsRejected()
    {
        var builder = new PizzaBuilder().WithSauce(SauceType.None);

        Assert.Throws<PizzaBuildException>(() => builder.Build());
        Assert.True(builder.WithExtraCheese().Preview().IsValid);
    }

    [Fact]
    public void Reset_ClearsEverything()
    {
        var builder = new PizzaBuilder()
            .Named("Something")
            .OfSize(PizzaSize.Large)
            .AddTopping(Get("Bacon"))
            .WithExtraCheese()
            .WithNotes("notes");

        var draft = builder.Reset().Preview();

        Assert.Equal("Custom pizza", draft.Name);
        Assert.Equal(PizzaSize.Medium, draft.Size);
        Assert.Empty(draft.Toppings);
        Assert.False(draft.ExtraCheese);
        Assert.Null(draft.Notes);
    }

    [Fact]
    public void Build_ReturnsIndependentProducts()
    {
        var builder = new PizzaBuilder().AddTopping(Get("Bacon"));

        var first = builder.Build();
        var second = builder.AddTopping(Get("Ham")).Build();

        Assert.NotSame(first, second);
        Assert.Single(first.Toppings);
        Assert.Equal(2, second.Toppings.Count);
    }

    [Theory]
    [InlineData("Margherita", 2)]
    [InlineData("Pepperoni", 2)]
    [InlineData("Veggie Garden", 5)]
    [InlineData("Meat Lovers", 5)]
    [InlineData("BBQ Chicken", 3)]
    public void Director_BuildsEveryPreset(string preset, int expectedToppings)
    {
        var director = new PizzaDirector(SeedData.Toppings);

        var pizza = director.Construct(preset, new PizzaBuilder()).Build();

        Assert.Equal(preset, pizza.Name);
        Assert.Equal(expectedToppings, pizza.Toppings.Count);
        Assert.True(pizza.Price > 0);
    }

    [Fact]
    public void Director_UnknownPreset_Throws()
    {
        var director = new PizzaDirector(SeedData.Toppings);

        Assert.Throws<ArgumentException>(() => director.Construct("Hawaiian", new PizzaBuilder()));
    }
}
