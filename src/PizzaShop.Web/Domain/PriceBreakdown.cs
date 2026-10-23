namespace PizzaShop.Web.Domain;

public sealed record PriceBreakdown(
    decimal Base,
    decimal Crust,
    decimal Sauce,
    decimal Toppings,
    decimal ExtraCheese)
{
    public decimal Total => Base + Crust + Sauce + Toppings + ExtraCheese;
}
