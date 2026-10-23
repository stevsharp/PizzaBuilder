using PizzaShop.Web.Domain;

namespace PizzaShop.Web.Services;

/// <summary>One topping line of a saved pizza, flattened from the join entity and the catalog item.</summary>
public sealed record PizzaToppingLine(int ToppingId, string Name, ToppingCategory Category, int Quantity);

/// <summary>
/// Read model for saved pizzas. Queries project straight into this shape with <c>Select</c>,
/// so only the needed columns are read and no entity graph is tracked.
/// </summary>
public sealed record PizzaSummary(
    int Id,
    string Name,
    PizzaSize Size,
    CrustType Crust,
    SauceType Sauce,
    bool ExtraCheese,
    decimal Price,
    DateTime CreatedAt,
    string? Notes,
    IReadOnlyList<PizzaToppingLine> Toppings)
{
    public string ToppingsText
    {
        get
        {
            var parts = Toppings
                .Select(t => t.Quantity > 1 ? $"{t.Quantity}× {t.Name}" : t.Name)
                .ToList();

            if (ExtraCheese)
            {
                parts.Add("extra cheese");
            }

            return parts.Count == 0 ? "—" : string.Join(", ", parts);
        }
    }
}
