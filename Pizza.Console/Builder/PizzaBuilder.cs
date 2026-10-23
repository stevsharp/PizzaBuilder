using Domain;

using Pizza.Console.Rule;

namespace Builder;

public sealed class PizzaBuilder
{
    private readonly Domain.Pizza _pizza = Domain.Pizza.CreateEmpty();
    private readonly PizzaValidator _validator = new(); 
    public PizzaBuilder Configure(Action<Domain.Pizza> configureAction)
    {
        configureAction?.Invoke(_pizza);
        return this;
    }

    public PizzaBuilder AddIngredient(Func<Domain.Ingredient> ingredientFactory)
    {
        var ingredient = ingredientFactory() ?? throw new ArgumentNullException(nameof(ingredientFactory), "Ingredient factory returned null.");

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
        _validator.ValidateAndThrow(_pizza);

        return _pizza;
    }
}