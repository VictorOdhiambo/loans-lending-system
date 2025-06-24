using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // List all customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAllCustomers()
        {
            return await _context.customer.ToListAsync();
        }

        // View customer details
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.customer.FindAsync(id);
            if (customer == null)
                return NotFound();

            return customer;
        }

        // Create new customer
        [HttpPost]
        public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
        {
            _context.customer.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, customer);
        }

        // Edit existing customer
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerId)
                return BadRequest();

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.customer.Any(e => e.CustomerId == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }
    }
}
