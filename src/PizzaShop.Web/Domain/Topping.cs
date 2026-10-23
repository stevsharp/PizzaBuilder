namespace PizzaShop.Web.Domain;

/// <summary>Catalog item: a topping the customer can put on a pizza.</summary>
public class Topping
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ToppingCategory Category { get; set; }
    /// <summary>Price for one portion on a medium pizza; scaled by size.</summary>
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
}
