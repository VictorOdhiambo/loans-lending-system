using AutoMapper;
using Loan_application_service.Data;
using Loan_application_service.DTOs;
using Loan_application_service.Models;
using Loan_application_service.Models;
using Loan_application_service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static Loan_application_service.Enums.RepaymentFrequency;

namespace Loan_application_service.Controllers
{
    [Route("/loan_product/")]
    public class LoanProductController : Controller
    {

        private IMapper _mapper;

        private readonly LoanApplicationServiceDbContext _context;
        private ILoanProductRepository _loanProductRepository;

        public LoanProductController(LoanApplicationServiceDbContext context, IMapper mapper, ILoanProductRepository loanProductRepository  )
        {

            _context = context;

            _mapper = mapper;
            _loanProductRepository = loanProductRepository;
        }

        //list all the loan products

        [HttpGet("/loan_product/")]
        public  ActionResult GetAll()
        {

            var products =  _loanProductRepository.GetAll();

           
            List<loanproductDto> dto = _mapper.Map<List<loanproductDto>>(products);
            
            return View(dto);

           
        }

        



        //get loan product by id
        [HttpGet("/loan_product/{id:long}")]
        public ActionResult EditLoanProduct(int id)
        {
            
            LoanProduct loanproduct = _loanProductRepository.GetById(id);
           
            loanproductDto loandto = _mapper.Map<loanproductDto>(loanproduct);

            ViewData["RepaymentFrequencyList"] = Enum.GetValues(typeof(paymentFrequency))
    .Cast<paymentFrequency>()
    .Select(e => new SelectListItem
    {
        Value = e.ToString(),
        Text = e.ToString()
    }).ToList();



            return View(loandto);
            

            

        }

        [Authorize(Roles ="user")]
        [HttpGet("/loan_product/create")]
        public ActionResult CreateLoan ()
        {
            ViewData["RepaymentFrequencyList"] = Enum.GetValues(typeof(paymentFrequency))
    .Cast<paymentFrequency>()
    .Select(e => new SelectListItem
    {
        Value = e.ToString(),
        Text = e.ToString()
    }).ToList();
            return View();
        }

        //create a loan product
        [HttpPost("/loan_product/create")]
        public async Task<ActionResult> Create (loanproductDto dto)
        {

            if (ModelState.IsValid)
            {
                //return BadRequest(ModelState);


                var entity = _mapper.Map<LoanProduct>(dto);

                await _loanProductRepository.Insert(entity);
                await _loanProductRepository.SaveAsync();

                var resultDto = _mapper.Map<loanproductDto>(entity);

                return RedirectToAction("GetAll", resultDto);
                // return CreatedAtAction(nameof(GetById), new { id = resultDto.ProductId }, resultDto);

                //return View(resultDto);
            }
            return View();
            

        }

        //update loan product
        [HttpPost("/loan_product/{id:long}")]
        
        public ActionResult EditLoanProduct (loanproductDto dto)
        {

            if (ModelState.IsValid)
            {
                var existing = _loanProductRepository.GetById(dto.ProductId);
                if (existing== null)
                {
                    return NotFound();
                }
                _mapper.Map(dto, existing);
                existing.UpdatedAt = DateTime.Now;
                _loanProductRepository.Update(existing);
                _loanProductRepository.SaveAsync();
                return RedirectToAction("GetAll");
            }
            return View();

            
        }

        [HttpGet("/loan_product/delete/{id:long}")]

        public ActionResult Delete (int id)
        {

            LoanProduct product = _loanProductRepository.GetById(id);
            loanproductDto dto = _mapper.Map<loanproductDto>(product);
            return View(dto);
        }

        //delete a loan product
        [HttpDelete("/loan_product/delete/{id:long}")]
        public ActionResult LoanDelete(int id)
        {

            _loanProductRepository.Delete(id);
            _loanProductRepository.SaveAsync();

            return RedirectToAction("GetAll");

        }


        // get loan charges
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
        

        // Get all charges for a loan product using the new LoanChargeMapper relationship
        [HttpGet("/loan_product/{id}/charges")]
        public async Task<IActionResult> GetChargesByProductId(int id)
        {
            var mappers = await _context.LoanChargeMapper
                .Include(m => m.LoanCharge)
                .Where(m => m.LoanProductId == id)
                .ToListAsync();

            if (mappers == null || mappers.Count == 0)
                return NotFound();

            var dtos = _mapper.Map<List<LoanChargeDto>>(mappers.Select(m => m.LoanCharge));
            return Ok(dtos);
        }

        // Add a charge to a loan product using the LoanChargeMapper
        [HttpPost("/loan_product/{productId}/charge")]
        public async Task<IActionResult> AddChargeToProduct(int productId, [FromBody] LoanChargeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _context.LoanProduct.FindAsync(productId);
            if (product == null)
                return NotFound("Loan product not found.");

            var charge = _mapper.Map<LoanCharge>(dto);
            await _context.LoanCharge.AddAsync(charge);
            await _context.SaveChangesAsync();

            var mapper = new LoanChargeMapper
            {
                LoanProductId = productId,
                LoanProduct = product,
                LoanChargeId = charge.Id,
                LoanCharge = charge
            };
            await _context.LoanChargeMapper.AddAsync(mapper);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChargesByProductId), new { id = productId }, dto);
        }
    }
}