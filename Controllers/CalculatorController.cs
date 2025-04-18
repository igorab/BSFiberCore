using BSFiberCore.Data;
using BSFiberCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace BSFiberCore.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalculatorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Calculator
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet]
        [HttpPost]
        public IActionResult GeomCalculate(Calculator calculator)
        {
            if (ModelState.IsValid)
            {
                double area = calculator.CalculateArea();
                ViewBag.Area = area;

                double perimeter = calculator.CalculatePerimeter();
                ViewBag.Perimeter = perimeter;
                
                return View();
            }
            return View();
        }
        
        public IActionResult Details(int id)
        {
            return View();
        }

        // GET: Calculator/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Calculator/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Calculator/Edit
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Calculator/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Calculator/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
