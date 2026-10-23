namespace Pizza.Console.Rule;

public class PizzaValidator
{
    public void ValidateAndThrow(Domain.Pizza pizza)
    {
        // 1. Check required properties
        if (string.IsNullOrWhiteSpace(pizza.Name))
            throw new InvalidOperationException("A pizza must be given a name.");

        // 2. Enforce minimum ingredients rule
        if (!pizza.PizzaIngredients.Any())
            throw new InvalidOperationException("A pizza must have at least one ingredient.");

        // 3. Enforce custom business rules (e.g., conflicting ingredients)
        bool hasPineapple = pizza.PizzaIngredients.Any(pi => pi.Ingredient?.Name == "Pineapple");
        bool hasAnchovies = pizza.PizzaIngredients.Any(pi => pi.Ingredient?.Name == "Anchovies");

        if (hasPineapple && hasAnchovies)
        {
            throw new InvalidOperationException("Culinary violation: Pineapple and Anchovies cannot be on the same pizza!");
        }
    }
}
