using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OMSWebMini.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using OMSWebMini.Model;

namespace OMSWebMini
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().
                AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "Version #1",
                    Title = "OMSWebMini",
                });
            });

            services.AddDbContext<NorthwindContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SqlConnection")), ServiceLifetime.Singleton);

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, NorthwindContext northwindContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //To call await this method track this issue - https://github.com/dotnet/aspnetcore/issues/24142
            //This issue is about async startup
            SeedStatistics(northwindContext).GetAwaiter().GetResult();
            SeedSummary(northwindContext).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Seed statistics table if such table is empty
        /// </summary>
        /// <param name="northwindContext"></param>
        private async Task SeedStatistics(NorthwindContext northwindContext)
        {
            await SeedProductsByCategories(northwindContext);
            await SeedSalesByEmployees(northwindContext);
            await SeedCustomersByCountries(northwindContext);
            await SeedPurchasesByCustomers(northwindContext);
            await SeedOrdersByCountries(northwindContext);
            await SeedSalesByCategories(northwindContext);
            await SeedSalesByCountries(northwindContext);

            await northwindContext.SaveChangesAsync();
        }

        private async Task SeedProductsByCategories(NorthwindContext northwindContext)
        {
            if (await northwindContext.ProductsByCategories.CountAsync() != 0) return;

            var productsByCategories = await northwindContext
                .Categories
                .Select(category => new ProductsByCategory
                {
                    CategoryName = category.CategoryName,
                    ProductsCount = category.Products.Count
                })
                .ToListAsync();

            await northwindContext.ProductsByCategories.AddRangeAsync(productsByCategories);
        }

        private async Task SeedSalesByEmployees(NorthwindContext northwindContext)
        {
            if (await northwindContext.SalesByEmployees.CountAsync() != 0) return;

            var employees = await northwindContext
                .Employees
                .Include(employee => employee.Orders)
                .ThenInclude(employeeOrder => employeeOrder.OrderDetails)
                .AsSplitQuery()
                .ToListAsync();

            var salesByEmployees = await Task.Run(
                () => employees
                .Select(employee => new SalesByEmployee
                {
                    ID = employee.EmployeeId,
                    LastName = employee.LastName,
                    Sales = employee.Orders.Sum(order => order.OrderDetails.Sum(orderDetail => orderDetail.Quantity * orderDetail.UnitPrice))
                })
                .ToList());

            await northwindContext.SalesByEmployees.AddRangeAsync(salesByEmployees);
        }

        private async Task SeedCustomersByCountries(NorthwindContext northwindContext)
        {
            if (await northwindContext.CustomersByCountries.CountAsync() != 0) return;

            var groupedCustomers = northwindContext
                .Customers
                .GroupBy(customer => customer.Country);

            var customersByCountries = await groupedCustomers
                .Select(customerGroup => new CustomersByCountry
                {
                    CountryName = customerGroup.Key,
                    CustomersCount = customerGroup.Count()
                })
                .ToListAsync();

            await northwindContext.CustomersByCountries.AddRangeAsync(customersByCountries);
        }

        private async Task SeedPurchasesByCustomers(NorthwindContext northwindContext)
        {
            if (await northwindContext.PurchasesByCustomers.CountAsync() != 0) return;

            var customers = await northwindContext
                .Customers
                .Include(customer => customer.Orders)
                .ThenInclude(customerOrder => customerOrder.OrderDetails)
                .AsSplitQuery()
                .ToListAsync();

            var purchasesByCustomers = await Task.Run(
                () => customers
                .Select(customer => new PurchasesByCustomer
                {
                    CompanyName = customer.CompanyName,
                    Purchases = customer.Orders.Sum(order => order.OrderDetails.Sum(orderDetail => orderDetail.Quantity * orderDetail.UnitPrice))
                })
                .ToList());

            await northwindContext.PurchasesByCustomers.AddRangeAsync(purchasesByCustomers);
        }

        private async Task SeedOrdersByCountries(NorthwindContext northwindContext)
        {
            if (await northwindContext.OrdersByCountries.CountAsync() != 0) return;

            var groupedOrders = northwindContext
                .Orders
                .GroupBy(order => order.Customer.Country);

            var ordersByCountries = await groupedOrders
                .Select(orderGroup => new OrdersByCountry
                {
                    CountryName = orderGroup.Key,
                    OrdersCount = orderGroup.Count()
                })
                .ToListAsync();

            await northwindContext.OrdersByCountries.AddRangeAsync(ordersByCountries);
        }

        private async Task SeedSalesByCategories(NorthwindContext northwindContext)
        {
            if (await northwindContext.SalesByCategories.CountAsync() != 0) return;

            var groupedOrderDetail = northwindContext
                .OrderDetails
                .GroupBy(orderDetail => orderDetail.Product.Category.CategoryName);

            var salesByCategories = await groupedOrderDetail
                .Select(orderDetailGroup => new SalesByCategory
                {
                    CategoryName = orderDetailGroup.Key,
                    Sales = orderDetailGroup.Sum(orderDetail => orderDetail.Quantity * orderDetail.UnitPrice)
                })
                .ToListAsync();

            await northwindContext.SalesByCategories.AddRangeAsync(salesByCategories);
        }

        private async Task SeedSalesByCountries(NorthwindContext northwindContext)
        {
            if (await northwindContext.SalesByCountries.CountAsync() != 0) return;

            var groupedOrderDetails = northwindContext
                .OrderDetails
                .GroupBy(orderDetail => orderDetail.Order.Customer.Country);

            var salesByCountries = await groupedOrderDetails
                .Select(orderDetailGroup => new SalesByCountry
                {
                    CountryName = orderDetailGroup.Key,
                    Sales = orderDetailGroup.Sum(orderDetail => orderDetail.Quantity * orderDetail.UnitPrice)
                })
                .ToListAsync();

            await northwindContext.SalesByCountries.AddRangeAsync(salesByCountries);
        }

        private async Task SeedSummary(NorthwindContext northwindContext)
        {
            var overallSales = await northwindContext.OrderDetails.SumAsync(a => a.Quantity * a.UnitPrice);
            var ordersQuantity = await northwindContext.Orders.CountAsync();
            var groupedOrderDetails = northwindContext.OrderDetails.GroupBy(od => od.OrderId);
            var ordersChecks = await groupedOrderDetails.Select(god => new { Sales = god.Sum(a => a.Quantity * a.UnitPrice) }).ToListAsync();
            var maxCheck = ordersChecks.Max(a => a.Sales);
            var averageCheck = ordersChecks.Average(a => a.Sales);
            var minCheck = ordersChecks.Min(a => a.Sales);

            await northwindContext.Summaries.AddAsync(new()
            {
                OverallSales = overallSales,
                OrdersQuantity = ordersQuantity,
                MaxCheck = maxCheck,
                AverageCheck = averageCheck,
                MinCheck = minCheck
            });
        }
    }
}
