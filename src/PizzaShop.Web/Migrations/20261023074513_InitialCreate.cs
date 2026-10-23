using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PizzaShop.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pizzas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Size = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Crust = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Sauce = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ExtraCheese = table.Column<bool>(type: "INTEGER", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pizzas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Toppings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Toppings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PizzaToppings",
                columns: table => new
                {
                    PizzaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToppingId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PizzaToppings", x => new { x.PizzaId, x.ToppingId });
                    table.ForeignKey(
                        name: "FK_PizzaToppings_Pizzas_PizzaId",
                        column: x => x.PizzaId,
                        principalTable: "Pizzas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PizzaToppings_Toppings_ToppingId",
                        column: x => x.ToppingId,
                        principalTable: "Toppings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Toppings",
                columns: new[] { "Id", "Category", "IsAvailable", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Cheese", true, "Mozzarella", 1.00m },
                    { 2, "Cheese", true, "Cheddar", 1.00m },
                    { 3, "Cheese", true, "Gorgonzola", 1.50m },
                    { 4, "Cheese", true, "Parmesan", 1.25m },
                    { 5, "Meat", true, "Pepperoni", 1.50m },
                    { 6, "Meat", true, "Italian sausage", 1.75m },
                    { 7, "Meat", true, "Ham", 1.50m },
                    { 8, "Meat", true, "Bacon", 1.50m },
                    { 9, "Meat", true, "Grilled chicken", 2.00m },
                    { 10, "Vegetable", true, "Mushrooms", 1.00m },
                    { 11, "Vegetable", true, "Bell peppers", 0.75m },
                    { 12, "Vegetable", true, "Red onion", 0.50m },
                    { 13, "Vegetable", true, "Black olives", 0.75m },
                    { 14, "Vegetable", true, "Fresh basil", 0.50m },
                    { 15, "Vegetable", true, "Jalapeños", 0.75m },
                    { 16, "Vegetable", true, "Pineapple", 0.75m },
                    { 17, "Vegetable", false, "Artichoke hearts", 1.25m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pizzas_CreatedAt",
                table: "Pizzas",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PizzaToppings_ToppingId",
                table: "PizzaToppings",
                column: "ToppingId");

            migrationBuilder.CreateIndex(
                name: "IX_Toppings_Name",
                table: "Toppings",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PizzaToppings");

            migrationBuilder.DropTable(
                name: "Pizzas");

            migrationBuilder.DropTable(
                name: "Toppings");
        }
    }
}
