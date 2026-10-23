namespace PizzaShop.Web.Domain;

/// <summary>Single place that knows how a pizza is priced. Used by the builder.</summary>
public static class PizzaPricing
{
    public static decimal BasePrice(PizzaSize size) => size switch
    {
        PizzaSize.Small => 7.00m,
        PizzaSize.Medium => 9.50m,
        PizzaSize.Large => 12.00m,
        _ => throw new ArgumentOutOfRangeException(nameof(size))
    };

    public static decimal CrustSurcharge(CrustType crust) => crust switch
    {
        CrustType.Thin => 0.00m,
        CrustType.Regular => 0.00m,
        CrustType.Stuffed => 2.50m,
        CrustType.GlutenFree => 2.00m,
        _ => throw new ArgumentOutOfRangeException(nameof(crust))
    };

    public static decimal SauceSurcharge(SauceType sauce) => sauce switch
    {
        SauceType.None => 0.00m,
        SauceType.Tomato => 0.00m,
        SauceType.Bbq => 0.50m,
        SauceType.White => 0.75m,
        SauceType.Pesto => 1.00m,
        _ => throw new ArgumentOutOfRangeException(nameof(sauce))
    };

    /// <summary>Toppings cost more on bigger pizzas because more of them is used.</summary>
    public static decimal ToppingMultiplier(PizzaSize size) => size switch
    {
        PizzaSize.Small => 0.8m,
        PizzaSize.Medium => 1.0m,
        PizzaSize.Large => 1.3m,
        _ => throw new ArgumentOutOfRangeException(nameof(size))
    };

    public const decimal ExtraCheesePrice = 1.50m;

    public static PriceBreakdown Calculate(
        PizzaSize size,
        CrustType crust,
        SauceType sauce,
        IEnumerable<(Topping Topping, int Quantity)> toppings,
        bool extraCheese)
    {
        var multiplier = ToppingMultiplier(size);
        var toppingsTotal = toppings.Sum(t => t.Topping.Price * t.Quantity * multiplier);

        return new PriceBreakdown(
            Base: BasePrice(size),
            Crust: CrustSurcharge(crust),
            Sauce: SauceSurcharge(sauce),
            Toppings: Math.Round(toppingsTotal, 2, MidpointRounding.AwayFromZero),
            ExtraCheese: extraCheese ? ExtraCheesePrice * multiplier : 0m);
    }
}
