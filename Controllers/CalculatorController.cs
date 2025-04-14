using BSFiberCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace BSFiberCore.Controllers
{
    public class CalculatorController : Controller
    {
        // GET: CalculatorController
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Calculate(Calculator calculator)
        {
            if (ModelState.IsValid)
            {
                double perimeter = calculator.CalculatePerimeter();
                ViewBag.Perimeter = perimeter;
                return View("Index");
            }
            return View("Index");
        }

        // GET: CalculatorController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CalculatorController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CalculatorController/Create
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

        // GET: CalculatorController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CalculatorController/Edit/5
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

        // GET: CalculatorController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CalculatorController/Delete/5
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
