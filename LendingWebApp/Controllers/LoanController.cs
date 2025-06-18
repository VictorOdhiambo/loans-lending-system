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
    public class LoanController : Controller
    {
        private readonly LoanApplicationServiceDbContext _context;

        public LoanController(LoanApplicationServiceDbContext context)
        {
            _context = context;
        }

        // GET: Loan_application
        public async Task<IActionResult> Index()
        {
            return View(await _context.LoanApplication.ToListAsync());
        }

        // GET: Loan_application/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan_application = await _context.LoanApplication
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loan_application == null)
            {
                return NotFound();
            }

            return View(loan_application);
        }

        // GET: Loan_application/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Loan_application/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RequestedAmount,LoanStatus")] LoanApplication loan_application)
        {
            if (ModelState.IsValid)
            {
                loan_application.ApplicationDate = DateTime.Now;
                loan_application.LoanStatus = "Requested";
                _context.Add(loan_application);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(loan_application);
        }

        // GET: Loan_application/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan_application = await _context.LoanApplication.FindAsync(id);
            if (loan_application == null)
            {
                return NotFound();
            }
            return View(loan_application);
        }

        // POST: Loan_application/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,RequestedAmount,ApplicationDate,LoanStatus,ApprovalDate,DisbursementDate")] LoanApplication loan_application)
        {
            if (id != loan_application.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loan_application);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Loan_applicationExists(loan_application.Id))
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
            return View(loan_application);
        }

        // GET: Loan_application/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan_application = await _context.LoanApplication
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loan_application == null)
            {
                return NotFound();
            }

            return View(loan_application);
        }

        // POST: Loan_application/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var loan_application = await _context.LoanApplication.FindAsync(id);
            if (loan_application != null)
            {
                _context.LoanApplication.Remove(loan_application);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Loan_applicationExists(long id)
        {
            return _context.LoanApplication.Any(e => e.Id == id);
        }
    }
}
