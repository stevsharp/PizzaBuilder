
using Domain;

using Builder;

try
{
    bool wantBacon = true;

    Pizza myPizza = new PizzaBuilder()
        .Configure(pizza =>
        {
            pizza.Name = "My Custom Pizza";
            pizza.Crust = "Thin";
            pizza.Size = "Large";
        })
        .AddIngredient(() => new Ingredient { Name = "Cheese", Price = 1.5m })
        .AddIngredient(() => new Ingredient { Name = "Pepperoni", Price = 2.5m })
        .AddIngredientIf(wantBacon, () => new Ingredient { Name = "Bacon", Price = 2.0m })
        .Build();

    // 5. Output to Console
    Console.WriteLine($"=== Pizza Created Successfully! ===");
    Console.WriteLine($"Name: {myPizza.Name}");
    Console.WriteLine($"Crust: {myPizza.Crust}");
    Console.WriteLine($"Size: {myPizza.Size}");
    Console.WriteLine("Ingredients:");

    foreach (var pi in myPizza.PizzaIngredients)
    {
        Console.WriteLine($" - {pi.Ingredient.Name} (${pi.Ingredient.Price})");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while creating the pizza: {ex.Message}"); 
}

Console.ReadLine();