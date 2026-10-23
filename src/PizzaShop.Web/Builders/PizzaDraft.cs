using PizzaShop.Web.Domain;

namespace PizzaShop.Web.Builders;

/// <summary>Immutable snapshot of a builder's state, used by the UI to render a live preview.</summary>
public sealed record PizzaDraft(
    string Name,
    PizzaSize Size,
    CrustType Crust,
    SauceType Sauce,
    bool ExtraCheese,
    string? Notes,
    IReadOnlyList<(Topping Topping, int Quantity)> Toppings,
    PriceBreakdown Price,
    IReadOnlyList<string> Problems)
{
    /// <summary>
    /// 
    /// </summary>
    public bool IsValid => Problems.Count == 0;
    /// <summary>
    /// 
    /// </summary>
    public int ToppingPortions => Toppings.Sum(t => t.Quantity);
}
