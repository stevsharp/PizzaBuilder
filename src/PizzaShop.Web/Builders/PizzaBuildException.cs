namespace PizzaShop.Web.Builders;

public sealed class PizzaBuildException(IReadOnlyList<string> problems)
    : InvalidOperationException("The pizza cannot be built: " + string.Join("; ", problems))
{
    public IReadOnlyList<string> Problems { get; } = problems;
}
