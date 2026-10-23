using Domain;  

namespace Builder;

public sealed class PizzaBuilder
{
    private readonly Domain.Pizza _pizza = Domain.Pizza.CreateEmpty();

    public PizzaBuilder Configure(Action<Domain.Pizza> configureAction)
    {
        configureAction?.Invoke(_pizza);
        return this;
    }

    public PizzaBuilder AddIngredient(Func<Domain.Ingredient> ingredientFactory)
    {
        var ingredient = ingredientFactory();

        if (ingredient is null)
        {
            throw new ArgumentNullException(nameof(ingredientFactory), "Ingredient factory returned null.");
        }

        _pizza.PizzaIngredients.Add(new Domain.PizzaIngredient
        {
            Pizza = _pizza,
            Ingredient = ingredient
        });
        return this;
    }

    public PizzaBuilder AddIngredientIf(bool condition, Func<Domain.Ingredient> ingredientFactory)
    {
        if (condition)
        {
            AddIngredient(ingredientFactory);
        }
        return this;
    }

    public Domain.Pizza Build()
    {
        if (string.IsNullOrWhiteSpace(_pizza.Name))
            _pizza.Name = "Custom Pizza";

        return _pizza;
    }
}