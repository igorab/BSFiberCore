using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BSFiberCore.Data;
using BSFiberCore.Models;

namespace BSFiberCore.Controllers
{
    public class FibersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FibersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Fibers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Fiber.ToListAsync());
        }

        // GET: Fibers/ShowSearchForm
        public async Task<IActionResult> ShowSearchForm()
        {
            return View();
        }

        // Post: Fibers/ShowSearchForm
        public async Task<IActionResult> ShowSearchResults(String SearchPhrase)
        {
            return View("Index", await _context.Fiber.Where(j => j.FiberQ.Contains(SearchPhrase)).ToListAsync());
        }

        // GET: Fibers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fiber = await _context.Fiber.FirstOrDefaultAsync(m => m.Id == id);

            if (fiber == null)
            {
                return NotFound();
            }

            return View(fiber);
        }

        // GET: Fibers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Fibers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FiberQ,FiberAns")] Fiber fiber)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fiber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fiber);
        }

        // GET: Fibers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fiber = await _context.Fiber.FindAsync(id);
            if (fiber == null)
            {
                return NotFound();
            }
            return View(fiber);
        }

        // POST: Fibers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FiberQ,FiberAns")] Fiber fiber)
        {
            if (id != fiber.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fiber);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FiberExists(fiber.Id))
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
            return View(fiber);
        }

        // GET: Fibers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fiber = await _context.Fiber.FirstOrDefaultAsync(m => m.Id == id);

            if (fiber == null)
            {
                return NotFound();
            }

            return View(fiber);
        }

        // POST: Fibers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fiber = await _context.Fiber.FindAsync(id);
            if (fiber != null)
            {
                _context.Fiber.Remove(fiber);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FiberExists(int id)
        {
            return _context.Fiber.Any(e => e.Id == id);
        }
    }
}
