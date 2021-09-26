using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMSWebMini.Data;
using OMSWebMini.Model;

namespace OMSWebMini.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly NorthwindContext _context;

        public OrdersController(NorthwindContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        //You may have problem on this endpoint
        //this link may help you: https://stackoverflow.com/questions/59199593/net-core-3-0-possible-object-cycle-was-detected-which-is-not-supported
        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            var detailedOrder = await _context.Orders.Where(o => o.OrderId == id).Include(o => o.OrderDetails).FirstOrDefaultAsync();

            order.OrderDetails = detailedOrder.OrderDetails;

            return order;
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.OrderId)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Orders
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await UpdatePurchasesByCustomer(order);
                await UpdateOrdersByCountries(order);
                await UpdateCustomersByCountry(order);
                await UpdateSalesByCategory(order);
                await UpdateSalesByEmployee(order);
                await UpdateSalesByCountry(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Order>> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var orderDetailsToDelete = (await _context.Orders.Include(o => o.OrderDetails).Where(o => o.OrderId == id).FirstOrDefaultAsync()).OrderDetails;

                _context.OrderDetails.RemoveRange(orderDetailsToDelete);
                await _context.SaveChangesAsync();

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                throw new Exception(e.Message);
            }
        }

        private async Task UpdateOrdersByCountries(Order order)
        {
            var ordersByCountry = await _context.OrdersByCountries.FirstOrDefaultAsync(x => x.CountryName == order.Customer.Country);
            if (ordersByCountry is not null)
                ordersByCountry.OrdersCount++;
            else
                _context.OrdersByCountries.Add(new() { CountryName = order.Customer.Country, OrdersCount = 1 });
        }

        private async Task UpdatePurchasesByCustomer(Order order)
        {
            var purchasesByCustomer = await _context.PurchasesByCustomers.FirstOrDefaultAsync(x => x.CompanyName == order.Customer.CompanyName);
            if (purchasesByCustomer is not null)
                purchasesByCustomer.Purchases += order.OrderDetails.Sum(x => x.Quantity * x.UnitPrice);
            else
                _context.PurchasesByCustomers.Add(new() { CompanyName = order.Customer.CompanyName, Purchases = order.OrderDetails.Sum(x => x.Quantity * x.UnitPrice) });
        }

        private async Task UpdateCustomersByCountry(Order order)
        {
            var customersByCountry = await _context.CustomersByCountries.FirstOrDefaultAsync(x => x.CountryName == order.Customer.Country);
            if (customersByCountry is not null)
                customersByCountry.CustomersCount++;
            else
                _context.CustomersByCountries.Add(new() { CountryName = order.Customer.Country, CustomersCount = 1 });
        }

        private async Task UpdateSalesByCategory(Order order)
        {
            var newSalesByCategories = order
                .OrderDetails
                .GroupBy(x => x.Product.Category.CategoryName)
                .Select(x => new { CategoryName = x.Key, Sales = x.Sum(x => x.UnitPrice * x.Quantity) });

            foreach (var newSalesByCategory in newSalesByCategories)
            {
                var salesByCategory = await _context.SalesByCategories.FirstOrDefaultAsync(x => x.CategoryName == newSalesByCategory.CategoryName);

                if (salesByCategory is null)
                    _context.SalesByCategories.Add(new() { CategoryName = newSalesByCategory.CategoryName, Sales = newSalesByCategory.Sales });
                else
                    salesByCategory.Sales += newSalesByCategory.Sales;
            }
        }

        private async Task UpdateSalesByCountry(Order order)
        {
            var newSalesByCountries = order
                .OrderDetails
                .GroupBy(x => x.Order.Customer.Country)
                .Select(x => new { CountryName = x.Key, Sales = x.Sum(x => x.UnitPrice * x.Quantity) });

            foreach (var newSalesByCountry in newSalesByCountries)
            {
                var salesByCountry = await _context.SalesByCountries.FirstOrDefaultAsync(x => x.CountryName == newSalesByCountry.CountryName);

                if (salesByCountry is null)
                    _context.SalesByCountries.Add(new() { CountryName = newSalesByCountry.CountryName, Sales = newSalesByCountry.Sales });
                else
                    salesByCountry.Sales += newSalesByCountry.Sales;
            }
        }

        private async Task UpdateSalesByEmployee(Order order)
        {
            var newSalesByEmployees = order
                .OrderDetails
                .GroupBy(x => x.Order.Employee)
                .Select(x => new { Id = x.Key.EmployeeId, LastName = x.Key.LastName, Sales = x.Sum(x => x.UnitPrice * x.Quantity) });

            foreach (var newSalesByEmployee in newSalesByEmployees)
            {
                var salesByEmployee = await _context.SalesByEmployees.FirstOrDefaultAsync(x => x.ID == newSalesByEmployee.Id);

                if (salesByEmployee is null)
                    _context.SalesByEmployees.Add(new() { ID = newSalesByEmployee.Id, LastName = newSalesByEmployee.LastName, Sales = newSalesByEmployee.Sales });
                else
                    salesByEmployee.Sales += newSalesByEmployee.Sales;
            }
        }

        private async Task UpdateSummary()
        {
            var overallSales = await _context.OrderDetails.SumAsync(a => a.Quantity * a.UnitPrice);
            var ordersQuantity = await _context.Orders.CountAsync();
            var groupedOrderDetails = _context.OrderDetails.GroupBy(od => od.OrderId);
            var ordersChecks = await groupedOrderDetails.Select(god => new { Sales = god.Sum(a => a.Quantity * a.UnitPrice) }).ToListAsync();
            var maxCheck = ordersChecks.Max(a => a.Sales);
            var averageCheck = ordersChecks.Average(a => a.Sales);
            var minCheck = ordersChecks.Min(a => a.Sales);

            await _context.Summaries.AddAsync(new()
            {
                OverallSales = overallSales,
                OrdersQuantity = ordersQuantity,
                MaxCheck = maxCheck,
                AverageCheck = averageCheck,
                MinCheck = minCheck
            });
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
