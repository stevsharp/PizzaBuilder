using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Web.Data;
using PizzaShop.Web.Domain;

namespace PizzaShop.Web.Services;

/// <summary>
/// EF Core backed persistence. Uses <see cref="IDbContextFactory{TContext}"/> so each operation gets
/// a short-lived DbContext, which is the recommended approach for Blazor Server circuits.
/// Reads project into <see cref="PizzaSummary"/> instead of loading entity graphs with Include.
/// </summary>
public sealed class PizzaService(IDbContextFactory<PizzaDbContext> contextFactory) : IPizzaService
{
    /// <summary>
    /// Single projection shared by every read. EF Core translates the nested Select into one query
    /// that joins PizzaToppings and Toppings and returns only the columns the read model needs.
    /// </summary>
    private static readonly Expression<Func<Pizza, PizzaSummary>> ToSummary = p => new PizzaSummary(
        p.Id,
        p.Name,
        p.Size,
        p.Crust,
        p.Sauce,
        p.ExtraCheese,
        p.Price,
        p.CreatedAt,
        p.Notes,
        p.Toppings
            .OrderBy(pt => pt.Topping.Category)
            .ThenBy(pt => pt.Topping.Name)
            .Select(pt => new PizzaToppingLine(pt.ToppingId, pt.Topping.Name, pt.Topping.Category, pt.Quantity))
            .ToList());

    public async Task<IReadOnlyList<Topping>> GetToppingsAsync(bool includeUnavailable = false, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = db.Toppings.AsNoTracking();
        if (!includeUnavailable)
        {
            query = query.Where(t => t.IsAvailable);
        }

        return await query
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Pizza> SaveAsync(Pizza pizza, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pizza);
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        // The toppings referenced by the pizza already exist in the catalog; attach them as Unchanged
        // so that Add() below does not try to insert them again.
        db.AttachRange(pizza.Toppings.Select(pt => pt.Topping).DistinctBy(t => t.Id));

        db.Pizzas.Add(pizza);
        await db.SaveChangesAsync(cancellationToken);
        return pizza;
    }

    public async Task<IReadOnlyList<PizzaSummary>> GetSavedPizzasAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Pizzas
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Select(ToSummary)
            .ToListAsync(cancellationToken);
    }

    public async Task<PizzaSummary?> GetPizzaAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Pizzas
            .Where(p => p.Id == id)
            .Select(ToSummary)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await db.Pizzas.Where(p => p.Id == id).ExecuteDeleteAsync(cancellationToken);
    }
}
