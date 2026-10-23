namespace Domain;

public class Ingredient
{
    public int Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public required decimal Price { get; set; }

    public ICollection<PizzaIngredient> PizzaIngredients { get; set; } = [];
}
