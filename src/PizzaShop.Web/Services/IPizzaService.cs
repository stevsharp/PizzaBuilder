using PizzaShop.Web.Domain;

namespace PizzaShop.Web.Services;

public interface IPizzaService
{
    Task<IReadOnlyList<Topping>> GetToppingsAsync(bool includeUnavailable = false, CancellationToken cancellationToken = default);

    /// <summary>Persists a pizza produced by the builder and returns it with its generated Id.</summary>
    Task<Pizza> SaveAsync(Pizza pizza, CancellationToken cancellationToken = default);

    /// <summary>All saved pizzas, newest first, projected to the read model.</summary>
    Task<IReadOnlyList<PizzaSummary>> GetSavedPizzasAsync(CancellationToken cancellationToken = default);

    Task<PizzaSummary?> GetPizzaAsync(int id, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
