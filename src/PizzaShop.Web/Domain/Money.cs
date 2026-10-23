using System.Globalization;

namespace PizzaShop.Web.Domain;

public static class Money
{
    public static string Format(decimal value) =>
        string.Create(CultureInfo.InvariantCulture, $"€{value:0.00}");
}
