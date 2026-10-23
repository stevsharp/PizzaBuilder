using Microsoft.EntityFrameworkCore;
using PizzaShop.Web.Builders;
using PizzaShop.Web.Components;
using PizzaShop.Web.Data;
using PizzaShop.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<PizzaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PizzaDb") ?? "Data Source=pizzashop.db"));

// A builder is stateful, so every consumer gets its own instance.
builder.Services.AddTransient<IPizzaBuilder, PizzaBuilder>();
builder.Services.AddScoped<IPizzaService, PizzaService>();

var app = builder.Build();

// Apply pending EF Core migrations (creates the SQLite file and seeds the topping catalog on first run).
await using (var scope = app.Services.CreateAsyncScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PizzaDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
