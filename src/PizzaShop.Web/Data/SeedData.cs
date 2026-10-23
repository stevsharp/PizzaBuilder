using PizzaShop.Web.Domain;

namespace PizzaShop.Web.Data;

public static class SeedData
{
    public static IReadOnlyList<Topping> Toppings { get; } =
    [
        new() { Id = 1,  Name = "Mozzarella",       Category = ToppingCategory.Cheese,    Price = 1.00m },
        new() { Id = 2,  Name = "Cheddar",          Category = ToppingCategory.Cheese,    Price = 1.00m },
        new() { Id = 3,  Name = "Gorgonzola",       Category = ToppingCategory.Cheese,    Price = 1.50m },
        new() { Id = 4,  Name = "Parmesan",         Category = ToppingCategory.Cheese,    Price = 1.25m },
        new() { Id = 5,  Name = "Pepperoni",        Category = ToppingCategory.Meat,      Price = 1.50m },
        new() { Id = 6,  Name = "Italian sausage",  Category = ToppingCategory.Meat,      Price = 1.75m },
        new() { Id = 7,  Name = "Ham",              Category = ToppingCategory.Meat,      Price = 1.50m },
        new() { Id = 8,  Name = "Bacon",            Category = ToppingCategory.Meat,      Price = 1.50m },
        new() { Id = 9,  Name = "Grilled chicken",  Category = ToppingCategory.Meat,      Price = 2.00m },
        new() { Id = 10, Name = "Mushrooms",        Category = ToppingCategory.Vegetable, Price = 1.00m },
        new() { Id = 11, Name = "Bell peppers",     Category = ToppingCategory.Vegetable, Price = 0.75m },
        new() { Id = 12, Name = "Red onion",        Category = ToppingCategory.Vegetable, Price = 0.50m },
        new() { Id = 13, Name = "Black olives",     Category = ToppingCategory.Vegetable, Price = 0.75m },
        new() { Id = 14, Name = "Fresh basil",      Category = ToppingCategory.Vegetable, Price = 0.50m },
        new() { Id = 15, Name = "Jalapeños",        Category = ToppingCategory.Vegetable, Price = 0.75m },
        new() { Id = 16, Name = "Pineapple",        Category = ToppingCategory.Vegetable, Price = 0.75m },
        new() { Id = 17, Name = "Artichoke hearts", Category = ToppingCategory.Vegetable, Price = 1.25m, IsAvailable = false },
    ];
}
