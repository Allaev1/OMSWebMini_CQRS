using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OMSWebMini.Migrations
{
    public partial class AddStatisticsTablesMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomersByCountries",
                columns: table => new
                {
                    CountryName = table.Column<string>(type: "nchar(100)", fixedLength: true, maxLength: 100, nullable: false),
                    CustomersCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomersByCountries", x => x.CountryName);
                });

            migrationBuilder.CreateTable(
                name: "OrdersByCountries",
                columns: table => new
                {
                    CountryName = table.Column<string>(type: "nchar(100)", fixedLength: true, maxLength: 100, nullable: false),
                    OrdersCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdersByCountries", x => x.CountryName);
                });

            migrationBuilder.CreateTable(
                name: "ProductsByCategories",
                columns: table => new
                {
                    CategoryName = table.Column<string>(type: "nchar(100)", fixedLength: true, maxLength: 100, nullable: false),
                    ProductsCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsByCategories", x => x.CategoryName);
                });

            migrationBuilder.CreateTable(
                name: "PurchasesByCustomers",
                columns: table => new
                {
                    CompanyName = table.Column<string>(type: "nchar(100)", fixedLength: true, maxLength: 100, nullable: false),
                    Purchases = table.Column<decimal>(type: "decimal(18,10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchasesByCustomers", x => x.CompanyName);
                });

            migrationBuilder.CreateTable(
                name: "SalesByCategories",
                columns: table => new
                {
                    CategoryName = table.Column<string>(type: "nchar(100)", fixedLength: true, maxLength: 100, nullable: false),
                    Sales = table.Column<decimal>(type: "decimal(18,10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesByCategories", x => x.CategoryName);
                });

            migrationBuilder.CreateTable(
                name: "SalesByCountries",
                columns: table => new
                {
                    CountryName = table.Column<string>(type: "nchar(100)", fixedLength: true, maxLength: 100, nullable: false),
                    Sales = table.Column<decimal>(type: "decimal(18,10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesByCountries", x => x.CountryName);
                });

            migrationBuilder.CreateTable(
                name: "SalesByEmployees",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastName = table.Column<string>(type: "nchar(100)", fixedLength: true, maxLength: 100, nullable: true),
                    Sales = table.Column<decimal>(type: "decimal(18,10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesByEmployees", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Summaries",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    OverallSales = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    OrdersQuantity = table.Column<int>(type: "int", nullable: false),
                    MaxCheck = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    MinCheck = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    AverageCheck = table.Column<decimal>(type: "decimal(18,10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Summaries", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomersByCountries");

            migrationBuilder.DropTable(
                name: "OrdersByCountries");

            migrationBuilder.DropTable(
                name: "ProductsByCategories");

            migrationBuilder.DropTable(
                name: "PurchasesByCustomers");

            migrationBuilder.DropTable(
                name: "SalesByCategories");

            migrationBuilder.DropTable(
                name: "SalesByCountries");

            migrationBuilder.DropTable(
                name: "SalesByEmployees");

            migrationBuilder.DropTable(
                name: "Summaries");
        }
    }
}
