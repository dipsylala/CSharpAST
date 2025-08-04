using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NetFramework48MVC.Models;

namespace NetFramework48MVC.Controllers
{
    public class ProductsController : Controller
    {
        // Sample data for demonstration
        private static List<Product> _products = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Category = "Electronics" },
            new Product { Id = 2, Name = "Smartphone", Description = "Latest smartphone model", Price = 699.99m, Category = "Electronics" },
            new Product { Id = 3, Name = "Desk Chair", Description = "Ergonomic office chair", Price = 299.99m, Category = "Furniture" }
        };

        // GET: Products
        public ActionResult Index()
        {
            var viewModel = new ProductViewModel
            {
                Products = _products,
                TotalProducts = _products.Count,
                Categories = _products.Select(p => p.Category).Distinct().ToList()
            };
            return View(viewModel);
        }

        // GET: Products/Details/5
        public ActionResult Details(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    product.Id = _products.Max(p => p.Id) + 1;
                    _products.Add(product);
                    return RedirectToAction("Index");
                }

                return View(product);
            }
            catch
            {
                return View();
            }
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingProduct = _products.FirstOrDefault(p => p.Id == id);
                    if (existingProduct != null)
                    {
                        existingProduct.Name = product.Name;
                        existingProduct.Description = product.Description;
                        existingProduct.Price = product.Price;
                        existingProduct.Category = product.Category;
                    }
                    return RedirectToAction("Index");
                }
                return View(product);
            }
            catch
            {
                return View();
            }
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var product = _products.FirstOrDefault(p => p.Id == id);
                if (product != null)
                {
                    _products.Remove(product);
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
