using PizzaShop.Web.Builders;
using PizzaShop.Web.Data;
using PizzaShop.Web.Domain;

namespace PizzaShop.Tests;

/// <summary>Covers the Action/Func based steps of <see cref="PizzaBuilder"/>.</summary>
public class PizzaBuilderDelegateTests
{
    private static Topping Get(string name) => SeedData.Toppings.Single(t => t.Name == name);

    [Fact]
    public void Configure_AppliesTheBlockToTheSameBuilder()
    {
        static void SpicyBase(IPizzaBuilder b) => b.WithSauce(SauceType.Bbq).AddTopping(Get("Jalapeños"));

        var draft = new PizzaBuilder()
            .Configure(SpicyBase)
            .Configure(b => b.OfSize(PizzaSize.Large))
            .Preview();

        Assert.Equal(SauceType.Bbq, draft.Sauce);
        Assert.Equal(PizzaSize.Large, draft.Size);
        Assert.Single(draft.Toppings);
    }

    [Fact]
    public void Configure_NullAction_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new PizzaBuilder().Configure(null!));
    }

    [Theory]
    [InlineData(true, CrustType.Stuffed)]
    [InlineData(false, CrustType.Thin)]
    public void When_RunsThenOrOtherwise(bool condition, CrustType expected)
    {
        var draft = new PizzaBuilder()
            .When(condition,
                then: b => b.WithCrust(CrustType.Stuffed),
                otherwise: b => b.WithCrust(CrustType.Thin))
            .Preview();

        Assert.Equal(expected, draft.Crust);
    }

    [Fact]
    public void When_FalseWithoutOtherwise_ChangesNothing()
    {
        var draft = new PizzaBuilder().When(false, b => b.WithExtraCheese()).Preview();

        Assert.False(draft.ExtraCheese);
    }

    [Fact]
    public void AddToppings_UsesPredicateAndQuantitySelector()
    {
        var draft = new PizzaBuilder()
            .AddToppings(SeedData.Toppings,
                where: t => t.Category == ToppingCategory.Cheese,
                quantity: t => t.Name == "Mozzarella" ? 2 : 1)
            .Preview();

        var cheeses = SeedData.Toppings.Count(t => t.Category == ToppingCategory.Cheese);
        Assert.Equal(cheeses, draft.Toppings.Count);
        Assert.Equal(2, draft.Toppings.Single(t => t.Topping.Name == "Mozzarella").Quantity);
        Assert.All(draft.Toppings.Where(t => t.Topping.Name != "Mozzarella"), t => Assert.Equal(1, t.Quantity));
    }

    [Fact]
    public void AddToppings_WithoutDelegates_AddsEverythingOnce()
    {
        var meats = SeedData.Toppings.Where(t => t.Category == ToppingCategory.Meat).ToList();

        var draft = new PizzaBuilder().AddToppings(meats).Preview();

        Assert.Equal(meats.Count, draft.Toppings.Count);
        Assert.All(draft.Toppings, t => Assert.Equal(1, t.Quantity));
    }

    [Fact]
    public void WithRule_AddsCustomProblem_AndBlocksBuild()
    {
        var builder = new PizzaBuilder()
            .WithRule(d => d.Toppings.Any(t => t.Topping.Name == "Pineapple") ? "No pineapple in this house." : null)
            .AddTopping(Get("Pineapple"));

        var draft = builder.Preview();
        Assert.False(draft.IsValid);
        Assert.Contains("No pineapple in this house.", draft.Problems);

        var ex = Assert.Throws<PizzaBuildException>(() => builder.Build());
        Assert.Contains("No pineapple in this house.", ex.Problems);

        Assert.True(builder.RemoveTopping(Get("Pineapple")).Preview().IsValid);
    }

    [Fact]
    public void WithRule_SeesPriceOfCurrentDraft()
    {
        var builder = new PizzaBuilder()
            .WithRule(d => d.Price.Total > 10m ? "Too expensive." : null)
            .OfSize(PizzaSize.Small);

        Assert.True(builder.Preview().IsValid);
        Assert.False(builder.OfSize(PizzaSize.Large).Preview().IsValid);
    }

    [Fact]
    public void WithRule_SurvivesReset()
    {
        var builder = new PizzaBuilder()
            .WithRule(d => d.Size == PizzaSize.Large ? "No large pizzas today." : null)
            .OfSize(PizzaSize.Large);

        Assert.False(builder.Preview().IsValid);

        builder.Reset().OfSize(PizzaSize.Large);

        Assert.False(builder.Preview().IsValid);
    }

    [Fact]
    public void WithRule_ReturningNullOrWhitespace_IsIgnored()
    {
        var draft = new PizzaBuilder()
            .WithRule(_ => null)
            .WithRule(_ => "   ")
            .Preview();

        Assert.True(draft.IsValid);
    }

    [Fact]
    public void OnBuilt_IsInvokedForEveryProduct()
    {
        var built = new List<Pizza>();
        var builder = new PizzaBuilder().OnBuilt(built.Add).OnBuilt(p => built.Add(p));

        var first = builder.Build();
        var second = builder.Named("Second").Build();

        Assert.Equal([first, first, second, second], built);
    }

    [Fact]
    public void OnBuilt_IsNotInvokedWhenBuildFails()
    {
        var calls = 0;
        var builder = new PizzaBuilder()
            .OnBuilt(_ => calls++)
            .OfSize(PizzaSize.Small)
            .WithCrust(CrustType.Stuffed);

        Assert.Throws<PizzaBuildException>(() => builder.Build());
        Assert.Equal(0, calls);
    }

    [Fact]
    public void BuildWithMap_ProjectsTheProduct()
    {
        var summary = new PizzaBuilder()
            .Named("Mapped")
            .AddTopping(Get("Ham"), 2)
            .Build(p => $"{p.Name} ({p.Toppings.Sum(t => t.Quantity)} portions)");

        Assert.Equal("Mapped (2 portions)", summary);
    }

    [Fact]
    public async Task BuildWithMap_WorksWithAsyncContinuations()
    {
        var pizza = await new PizzaBuilder().Build(async p =>
        {
            await Task.Yield();
            return p;
        });

        Assert.Equal("Custom pizza", pizza.Name);
    }

    [Fact]
    public void BuildWithMap_InvalidRecipe_ThrowsBeforeMapping()
    {
        var mapped = false;
        var builder = new PizzaBuilder().WithRule(_ => "never");

        Assert.Throws<PizzaBuildException>(() => builder.Build(p => mapped = true));
        Assert.False(mapped);
    }

    [Fact]
    public void Director_MeatLovers_FallsBackToRegularCrustOnSmall()
    {
        var director = new PizzaDirector(SeedData.Toppings);

        var small = director.MeatLovers(new PizzaBuilder(), PizzaSize.Small).Build();
        var large = director.MeatLovers(new PizzaBuilder(), PizzaSize.Large).Build();

        Assert.Equal(CrustType.Regular, small.Crust);
        Assert.Equal(CrustType.Stuffed, large.Crust);
    }
}
