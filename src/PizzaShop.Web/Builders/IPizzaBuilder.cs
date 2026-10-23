using PizzaShop.Web.Domain;

namespace PizzaShop.Web.Builders;

/// <summary>
/// Builder contract (GoF Builder pattern). Every step returns the builder so steps can be chained,
/// and <see cref="Build()"/> produces the immutable <see cref="Pizza"/> product.
/// </summary>
public interface IPizzaBuilder
{
    // ---- Plain steps -------------------------------------------------------------------------

    IPizzaBuilder Named(string name);
    IPizzaBuilder OfSize(PizzaSize size);
    IPizzaBuilder WithCrust(CrustType crust);
    IPizzaBuilder WithSauce(SauceType sauce);
    IPizzaBuilder AddTopping(Topping topping, int quantity = 1);
    IPizzaBuilder RemoveTopping(Topping topping);
    IPizzaBuilder WithExtraCheese(bool extraCheese = true);
    IPizzaBuilder WithNotes(string? notes);

    /// <summary>Clears the recipe (name, size, crust, sauce, toppings, cheese, notes). Rules and callbacks are kept.</summary>
    IPizzaBuilder Reset();

    // ---- Delegate based steps (Action) -------------------------------------------------------

    /// <summary>Runs a block of steps against this builder, e.g. a shared recipe fragment or a method group.</summary>
    IPizzaBuilder Configure(Action<IPizzaBuilder> configure);

    /// <summary>Runs <paramref name="then"/> when the condition holds, otherwise <paramref name="otherwise"/> (if given).</summary>
    IPizzaBuilder When(bool condition, Action<IPizzaBuilder> then, Action<IPizzaBuilder>? otherwise = null);

    /// <summary>Registers a callback invoked with every product that <see cref="Build()"/> creates.</summary>
    IPizzaBuilder OnBuilt(Action<Pizza> callback);

    // ---- Delegate based steps (Func) ---------------------------------------------------------

    /// <summary>
    /// Adds many toppings at once. <paramref name="where"/> filters the candidates (all when null) and
    /// <paramref name="quantity"/> decides the portions per topping (1 when null).
    /// </summary>
    IPizzaBuilder AddToppings(IEnumerable<Topping> toppings, Func<Topping, bool>? where = null, Func<Topping, int>? quantity = null);

    /// <summary>
    /// Adds a custom validation rule. The rule inspects the current draft and returns a problem
    /// description, or null when the draft is fine. Rules survive <see cref="Reset"/>.
    /// </summary>
    IPizzaBuilder WithRule(Func<PizzaDraft, string?> rule);

    /// <summary>Read-only snapshot of the current state, for live previews. Never throws.</summary>
    PizzaDraft Preview();

    /// <summary>Validates and produces the product. Throws <see cref="PizzaBuildException"/> when the recipe is invalid.</summary>
    Pizza Build();

    /// <summary>Builds the product and hands it to <paramref name="map"/>, e.g. to persist it or project it to a DTO.</summary>
    TResult Build<TResult>(Func<Pizza, TResult> map);
}
