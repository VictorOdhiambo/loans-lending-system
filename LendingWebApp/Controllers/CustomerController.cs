using Microsoft.AspNetCore.Mvc;
using Loan_application_service.Data;
using Loan_application_service.Models;

namespace Loan_application_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly LoanApplicationServiceDbContext _context;

        public CustomerController(LoanApplicationServiceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetCustomers()
        {
            var customers = _context.Customers.ToList();
            return Ok(customers);
        }

        [HttpPost]
        public IActionResult CreateCustomer([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Customers.Add(customer);
            _context.SaveChanges();

            return Created($"api/customer/{customer.CustomerId}", customer);
        }
    }
}
