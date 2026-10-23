namespace Domain;

public class Pizza
{
    public int Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public string Crust { get; set; } = "Thin";
    public required string Size { get; set; } = "Medium";

    public ICollection<PizzaIngredient> PizzaIngredients { get; set; } = new List<PizzaIngredient>();

    /// <summary>
    /// 
    /// </summary>
    public Pizza() {}

    public static Pizza CreateEmpty()
    {
        return new Pizza
        {
            Name = string.Empty,
            Crust = "Thin",
            Size = "Medium",
            PizzaIngredients = []
        };
    }
}
