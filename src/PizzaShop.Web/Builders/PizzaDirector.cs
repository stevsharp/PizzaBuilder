using PizzaShop.Web.Domain;

namespace PizzaShop.Web.Builders;

/// <summary>
/// Director (GoF Builder pattern): knows the recipes for the house classics and drives
/// any <see cref="IPizzaBuilder"/> through the steps. Toppings are looked up by name in the catalog.
/// Shared recipe fragments are expressed as <see cref="Action{IPizzaBuilder}"/> and applied with
/// <see cref="IPizzaBuilder.Configure"/>; size dependent choices use <see cref="IPizzaBuilder.When"/>.
/// </summary>
public sealed class PizzaDirector(IReadOnlyCollection<Topping> catalog)
{
    public static readonly IReadOnlyList<string> Presets = ["Margherita", "Pepperoni", "Veggie Garden", "Meat Lovers", "BBQ Chicken"];

    public IPizzaBuilder Construct(string preset, IPizzaBuilder builder, PizzaSize size = PizzaSize.Medium) => preset switch
    {
        "Margherita" => Margherita(builder, size),
        "Pepperoni" => Pepperoni(builder, size),
        "Veggie Garden" => VeggieGarden(builder, size),
        "Meat Lovers" => MeatLovers(builder, size),
        "BBQ Chicken" => BbqChicken(builder, size),
        _ => throw new ArgumentException($"Unknown preset '{preset}'.", nameof(preset))
    };

    public IPizzaBuilder Margherita(IPizzaBuilder builder, PizzaSize size = PizzaSize.Medium) =>
        builder.Reset()
            .Named("Margherita")
            .OfSize(size)
            .WithCrust(CrustType.Thin)
            .Configure(TomatoAndMozzarella)
            .AddTopping(Find("Fresh basil"));

    public IPizzaBuilder Pepperoni(IPizzaBuilder builder, PizzaSize size = PizzaSize.Medium) =>
        builder.Reset()
            .Named("Pepperoni")
            .OfSize(size)
            .WithCrust(CrustType.Regular)
            .Configure(TomatoAndMozzarella)
            .AddTopping(Find("Pepperoni"), 2);

    public IPizzaBuilder VeggieGarden(IPizzaBuilder builder, PizzaSize size = PizzaSize.Medium) =>
        builder.Reset()
            .Named("Veggie Garden")
            .OfSize(size)
            .WithCrust(CrustType.Thin)
            .WithSauce(SauceType.Pesto)
            .AddTopping(Find("Mozzarella"))
            // Func<Topping, bool> picks the vegetables that belong on this pizza.
            .AddToppings(catalog, where: t => t.Category == ToppingCategory.Vegetable && VeggieGardenVegetables.Contains(t.Name));

    public IPizzaBuilder MeatLovers(IPizzaBuilder builder, PizzaSize size = PizzaSize.Large) =>
        builder.Reset()
            .Named("Meat Lovers")
            .OfSize(size)
            // Stuffed crust does not exist in small, so fall back to regular there.
            .When(size == PizzaSize.Small,
                then: b => b.WithCrust(CrustType.Regular),
                otherwise: b => b.WithCrust(CrustType.Stuffed))
            .Configure(TomatoAndMozzarella)
            .AddTopping(Find("Pepperoni"))
            .AddTopping(Find("Italian sausage"))
            .AddTopping(Find("Ham"))
            .AddTopping(Find("Bacon"))
            .WithExtraCheese();

    public IPizzaBuilder BbqChicken(IPizzaBuilder builder, PizzaSize size = PizzaSize.Medium) =>
        builder.Reset()
            .Named("BBQ Chicken")
            .OfSize(size)
            .WithCrust(CrustType.Regular)
            .WithSauce(SauceType.Bbq)
            .AddTopping(Find("Grilled chicken"))
            .AddTopping(Find("Red onion"))
            .AddTopping(Find("Cheddar"));

    private static readonly HashSet<string> VeggieGardenVegetables =
        new(["Mushrooms", "Bell peppers", "Red onion", "Black olives"], StringComparer.OrdinalIgnoreCase);

    /// <summary>Recipe fragment shared by the tomato based classics (used as an Action&lt;IPizzaBuilder&gt;).</summary>
    private void TomatoAndMozzarella(IPizzaBuilder builder) =>
        builder.WithSauce(SauceType.Tomato).AddTopping(Find("Mozzarella"));

    private Topping Find(string name) =>
        catalog.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException($"Topping '{name}' is missing from the catalog.");
}
