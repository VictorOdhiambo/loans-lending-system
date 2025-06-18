using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Loan_application_service.Data;
using Loan_application_service.Models;

namespace Loan_application_service.Controllers
{
    public class LoanProductController : Controller
    {
        private readonly LoanApplicationServiceDbContext _context;

        public LoanProductController(LoanApplicationServiceDbContext context)
        {
            _context = context;
        }

        // GET: loan_product
        public async Task<IActionResult> Index()
        {
            return View(await _context.LoanProducts.ToListAsync());
        }

        // GET: loan_product/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan_product = await _context.LoanProducts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loan_product == null)
            {
                return NotFound();
            }

            return View(loan_product);
        }

        // GET: loan_product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: loan_product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LoanName,InterestRate,MinimumAmount,MaximumAmount,RepaymentPeriodMonths")] LoanProduct loan_product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loan_product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(loan_product);
        }

        // GET: loan_product/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan_product = await _context.LoanProducts.FindAsync(id);
            if (loan_product == null)
            {
                return NotFound();
            }
            return View(loan_product);
        }

        // POST: loan_product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,LoanName,InterestRate,MinimumAmount,MaximumAmount,RepaymentPeriodMonths")] LoanProduct loan_product)
        {
            if (id != loan_product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loan_product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!loan_productExists(loan_product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(loan_product);
        }

        // GET: loan_product/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan_product = await _context.LoanProducts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loan_product == null)
            {
                return NotFound();
            }

            return View(loan_product);
        }

        // POST: loan_product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var loan_product = await _context.LoanProducts.FindAsync(id);
            if (loan_product != null)
            {
                _context.LoanProducts.Remove(loan_product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool loan_productExists(long id)
        {
            return _context.LoanProducts.Any(e => e.Id == id);
        }
    }
}
