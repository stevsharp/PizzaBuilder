namespace PizzaShop.Web.Domain;

/// <summary>
/// The product produced by <see cref="Builders.PizzaBuilder"/>. Persisted with EF Core.
/// Properties are init-only so a Pizza can only be assembled through the builder.
/// </summary>
public class Pizza
{
    public int Id { get; init; }
    public string Name { get; init; } = "Custom pizza";
    public PizzaSize Size { get; init; }
    public CrustType Crust { get; init; }
    public SauceType Sauce { get; init; }
    public bool ExtraCheese { get; init; }
    public decimal Price { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? Notes { get; init; }

    public List<PizzaTopping> Toppings { get; init; } = [];

    public override string ToString() =>
        $"{Name}: {Size} / {Crust} crust / {Sauce} sauce" +
        (Toppings.Count > 0
            ? " / " + string.Join(", ", Toppings.Select(t => t.Quantity > 1 ? $"{t.Quantity}x {t.Topping.Name}" : t.Topping.Name))
            : string.Empty) +
        (ExtraCheese ? " / extra cheese" : string.Empty) +
        $" = {Money.Format(Price)}";
}
