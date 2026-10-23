namespace PizzaShop.Web.Domain;

/// <summary>Join entity between a built pizza and a catalog topping, with quantity (1 = normal, 2 = double, ...).</summary>
public class PizzaTopping
{
    public int PizzaId { get; set; }
    public Pizza Pizza { get; set; } = null!;

    public int ToppingId { get; set; }
    public Topping Topping { get; set; } = null!;

    public int Quantity { get; set; } = 1;
}
