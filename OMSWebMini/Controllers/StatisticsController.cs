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
    public class StatisticsController : ControllerBase
    {
        NorthwindContext northwindContext;

        public StatisticsController(NorthwindContext northwindContext)
        {
            this.northwindContext = northwindContext;
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductsByCategory>>> GetProductsByCategories()
        {
            return await northwindContext.ProductsByCategories.ToListAsync();
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesByEmployee>>> GetSalesByEmployees()
        {
            return await northwindContext.SalesByEmployees.ToListAsync();
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomersByCountry>>> GetCustomersByCountries()
        {
            return await northwindContext.CustomersByCountries.ToListAsync();
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchasesByCustomer>>> GetPurchasesByCustomers()
        {
            return await northwindContext.PurchasesByCustomers.ToListAsync();
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrdersByCountry>>> GetOrdersByCountries()
        {
            return await northwindContext.OrdersByCountries.ToListAsync();
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesByCategory>>> GetSalesByCategories()
        {
            return await northwindContext.SalesByCategories.ToListAsync();
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesByCountry>>> GetSalesByCountries()
        {
            return await northwindContext.SalesByCountries.ToListAsync();
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<decimal>> GetSummary(string summaryType)
        {
            var summary = await northwindContext.Summaries.FirstOrDefaultAsync();

            switch (summaryType)
            {
                case "OverallSales":
                    return summary.OverallSales;
                case "OrdersQuantity":
                    return summary.OrdersQuantity;
                case "AverageCheck":
                    return summary.AverageCheck;
                case "MaxCheck":
                    return summary.MaxCheck;
                case "MinCheck":
                    return summary.MinCheck;
                default:
                    return BadRequest();
            }
        }
    }
}
