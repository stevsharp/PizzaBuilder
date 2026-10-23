namespace PizzaShop.Web.Domain;

public enum PizzaSize
{
    Small = 1,
    Medium = 2,
    Large = 3
}

public enum CrustType
{
    Thin = 1,
    Regular = 2,
    Stuffed = 3,
    GlutenFree = 4
}

public enum SauceType
{
    None = 0,
    Tomato = 1,
    Bbq = 2,
    White = 3,
    Pesto = 4
}

public enum ToppingCategory
{
    Meat = 1,
    Vegetable = 2,
    Cheese = 3
}

public static class EnumDisplay
{
    public static string Label(this PizzaSize size) => size switch
    {
        PizzaSize.Small => "Small (25 cm)",
        PizzaSize.Medium => "Medium (30 cm)",
        PizzaSize.Large => "Large (35 cm)",
        _ => size.ToString()
    };

    public static string Label(this CrustType crust) => crust switch
    {
        CrustType.GlutenFree => "Gluten free",
        _ => crust.ToString()
    };

    public static string Label(this SauceType sauce) => sauce switch
    {
        SauceType.None => "No sauce",
        SauceType.Bbq => "BBQ",
        SauceType.White => "White (garlic cream)",
        _ => sauce.ToString()
    };

    public static string Label(this ToppingCategory category) => category switch
    {
        ToppingCategory.Vegetable => "Vegetables",
        _ => category.ToString()
    };
}
