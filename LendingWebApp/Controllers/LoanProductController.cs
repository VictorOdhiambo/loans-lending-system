using AutoMapper;
using Loan_application_service.Data;
using Loan_application_service.DTOs;
using Loan_application_service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Loan_application_service.Controllers
{
    [Route("/loan_product/")]
    public class LoanProductController : Controller
    {

        private IMapper _mapper;

        private readonly LoanApplicationServiceDbContext _context;

        public LoanProductController(LoanApplicationServiceDbContext context, IMapper mapper)
        {

            _context = context;

            _mapper = mapper;
        }

        //list all the loan products

        [HttpGet("/loan_product/")]
        public IActionResult GetAll()
        {
            var product = _context.LoanProduct.ToList();
            List<loanproductDto> dto = _mapper.Map<List<loanproductDto>>(product);
            return Ok(dto);

        }


        //get loan product by id
        [HttpGet("/loan_product/{id:long}")]
        public async Task<ActionResult<loanproductDto>> GetById(int id)
        {
            var loanproduct = await _context.FindAsync<LoanProduct>(id);
            if (loanproduct == null)
            {
                return NotFound();
            }
            loanproductDto loandto = _mapper.Map<loanproductDto>(loanproduct);
            return Ok(loandto);

        }

        //create a loan product
        [HttpPost("/loan_product/create")]
        public async Task<ActionResult<loanproductDto>> Create([FromBody] loanproductDto dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _mapper.Map<LoanProduct>(dto);
            _context.LoanProduct.Add(entity);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<loanproductDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.ProductId }, resultDto);

        }


        //update loan product
        [HttpPut("/loan_product/update/{id:long}")]
        public async Task<IActionResult> Update(int id, [FromBody] loanproductDto dto)
        {

            if (!ModelState.IsValid || id != dto.ProductId)
                return BadRequest(ModelState);

            var existing = await _context.LoanProduct.FindAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);

            try
            {
                existing.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "A database error occurred.");
            }

            return Ok(dto);

        }


        //delete a loan product
        [HttpDelete("/loan_product/delete/{id:long}")]
        public async Task<IActionResult> Delete(int id)
        {

            var product = await _context.LoanProduct.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.LoanProduct.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product has been deleted" });

        }


        // get loan charge
        [HttpGet("/loan_product/charges")]
        public async Task<IActionResult> GetCharge()

        {
            var charges = await _context.LoanCharge.ToListAsync();
            
            List <LoanChargeDto> dto = _mapper.Map<List<LoanChargeDto>>(charges);
            return Ok(dto);


        }

        //Get loan charges by loan product id 

        [HttpGet("/loan_product/{id}/charges")]
        public async Task<IActionResult> GetChargeById(int id)
        {
            var charge = await _context.LoanCharge.FirstOrDefaultAsync(charge => charge.Id == id);

            if (charge == null)
            {
                return NotFound();
            }

            LoanChargeDto dto = _mapper.Map<LoanChargeDto>(charge);
            return Ok(dto);
        }
        // add a loan charge
        [HttpPost("/loan_product/charge/create")]
       public async Task<IActionResult> AddCharge( [FromBody]LoanChargeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var id = dto.LoanProductId;
            var loanproduct = await _context.LoanProduct.FindAsync(id);
            if (loanproduct == null)
            {
                return NotFound();
            }

            LoanCharge charge = _mapper.Map<LoanCharge>(dto);
            await _context.LoanCharge.AddAsync(charge);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetChargeById", charge.Id );
        }
    }



}