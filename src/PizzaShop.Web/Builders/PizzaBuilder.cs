using PizzaShop.Web.Domain;

namespace PizzaShop.Web.Builders;

/// <summary>
/// Concrete builder. Accumulates the customer's choices step by step, validates them
/// (built-in rules plus any <see cref="WithRule"/> delegates), prices the result and
/// finally assembles a <see cref="Pizza"/>.
/// </summary>
public sealed class PizzaBuilder : IPizzaBuilder
{
    public const int MaxToppingPortions = 8;
    public const int MaxQuantityPerTopping = 3;
    public const int MaxNameLength = 60;

    // Recipe state (cleared by Reset)
    private string _name = string.Empty;
    private PizzaSize _size = PizzaSize.Medium;
    private CrustType _crust = CrustType.Regular;
    private SauceType _sauce = SauceType.Tomato;
    private bool _extraCheese;
    private string? _notes;
    private readonly Dictionary<int, (Topping Topping, int Quantity)> _toppings = new();

    // Builder configuration (kept across Reset)
    private readonly List<Func<PizzaDraft, string?>> _rules = [];
    private readonly List<Action<Pizza>> _onBuilt = [];

    // ---- Plain steps -------------------------------------------------------------------------

    public IPizzaBuilder Named(string name)
    {
        _name = (name ?? string.Empty).Trim();
        return this;
    }

    public IPizzaBuilder OfSize(PizzaSize size)
    {
        _size = size;
        return this;
    }

    public IPizzaBuilder WithCrust(CrustType crust)
    {
        _crust = crust;
        return this;
    }

    public IPizzaBuilder WithSauce(SauceType sauce)
    {
        _sauce = sauce;
        return this;
    }

    public IPizzaBuilder AddTopping(Topping topping, int quantity = 1)
    {
        ArgumentNullException.ThrowIfNull(topping);
        if (quantity <= 0)
        {
            return RemoveTopping(topping);
        }

        _toppings[topping.Id] = (topping, Math.Min(quantity, MaxQuantityPerTopping));
        return this;
    }

    public IPizzaBuilder RemoveTopping(Topping topping)
    {
        ArgumentNullException.ThrowIfNull(topping);
        _toppings.Remove(topping.Id);
        return this;
    }

    public IPizzaBuilder WithExtraCheese(bool extraCheese = true)
    {
        _extraCheese = extraCheese;
        return this;
    }

    public IPizzaBuilder WithNotes(string? notes)
    {
        _notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        return this;
    }

    public IPizzaBuilder Reset()
    {
        _name = string.Empty;
        _size = PizzaSize.Medium;
        _crust = CrustType.Regular;
        _sauce = SauceType.Tomato;
        _extraCheese = false;
        _notes = null;
        _toppings.Clear();
        return this;
    }

    // ---- Delegate based steps (Action) -------------------------------------------------------

    public IPizzaBuilder Configure(Action<IPizzaBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(this);
        return this;
    }

    public IPizzaBuilder When(bool condition, Action<IPizzaBuilder> then, Action<IPizzaBuilder>? otherwise = null)
    {
        ArgumentNullException.ThrowIfNull(then);
        if (condition)
        {
            then(this);
        }
        else
        {
            otherwise?.Invoke(this);
        }

        return this;
    }

    public IPizzaBuilder OnBuilt(Action<Pizza> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _onBuilt.Add(callback);
        return this;
    }

    // ---- Delegate based steps (Func) ---------------------------------------------------------

    public IPizzaBuilder AddToppings(IEnumerable<Topping> toppings, Func<Topping, bool>? where = null, Func<Topping, int>? quantity = null)
    {
        ArgumentNullException.ThrowIfNull(toppings);
        foreach (var topping in toppings)
        {
            if (where is null || where(topping))
            {
                AddTopping(topping, quantity?.Invoke(topping) ?? 1);
            }
        }

        return this;
    }

    public IPizzaBuilder WithRule(Func<PizzaDraft, string?> rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _rules.Add(rule);
        return this;
    }

    // ---- Product -----------------------------------------------------------------------------

    public PizzaDraft Preview() => CreateDraft();

    public Pizza Build()
    {
        var draft = CreateDraft();
        if (!draft.IsValid)
        {
            throw new PizzaBuildException(draft.Problems);
        }

        var pizza = new Pizza
        {
            Name = draft.Name,
            Size = draft.Size,
            Crust = draft.Crust,
            Sauce = draft.Sauce,
            ExtraCheese = draft.ExtraCheese,
            Notes = draft.Notes,
            Price = draft.Price.Total,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var (topping, quantity) in draft.Toppings)
        {
            pizza.Toppings.Add(new PizzaTopping
            {
                Pizza = pizza,
                ToppingId = topping.Id,
                Topping = topping,
                Quantity = quantity
            });
        }

        foreach (var callback in _onBuilt)
        {
            callback(pizza);
        }

        return pizza;
    }

    public TResult Build<TResult>(Func<Pizza, TResult> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return map(Build());
    }

    // ---- Internals ---------------------------------------------------------------------------

    private PizzaDraft CreateDraft()
    {
        var toppings = OrderedToppings();

        var draft = new PizzaDraft(
            Name: string.IsNullOrWhiteSpace(_name) ? "Custom pizza" : _name,
            Size: _size,
            Crust: _crust,
            Sauce: _sauce,
            ExtraCheese: _extraCheese,
            Notes: _notes,
            Toppings: toppings,
            Price: PizzaPricing.Calculate(_size, _crust, _sauce, toppings, _extraCheese),
            Problems: []);

        var problems = ValidateBuiltInRules(toppings);
        foreach (var rule in _rules)
        {
            var problem = rule(draft);
            if (!string.IsNullOrWhiteSpace(problem))
            {
                problems.Add(problem);
            }
        }

        return draft with { Problems = problems };
    }

    private List<(Topping Topping, int Quantity)> OrderedToppings() =>
        [.. _toppings.Values
            .OrderBy(t => t.Topping.Category)
            .ThenBy(t => t.Topping.Name)];

    private List<string> ValidateBuiltInRules(IReadOnlyCollection<(Topping Topping, int Quantity)> toppings)
    {
        var problems = new List<string>();

        if (_name.Length > MaxNameLength)
        {
            problems.Add($"Name must be at most {MaxNameLength} characters.");
        }

        var portions = toppings.Sum(t => t.Quantity);
        if (portions > MaxToppingPortions)
        {
            problems.Add($"At most {MaxToppingPortions} topping portions are allowed (you have {portions}).");
        }

        foreach (var (topping, _) in toppings.Where(t => !t.Topping.IsAvailable))
        {
            problems.Add($"'{topping.Name}' is currently unavailable.");
        }

        if (_crust == CrustType.Stuffed && _size == PizzaSize.Small)
        {
            problems.Add("Stuffed crust is only available on medium and large pizzas.");
        }

        if (_sauce == SauceType.None && toppings.Count == 0 && !_extraCheese)
        {
            problems.Add("A pizza needs at least a sauce, a topping or extra cheese.");
        }

        return problems;
    }
}
