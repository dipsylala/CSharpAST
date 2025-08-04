using System;
using System.Collections.Generic;
using System.Linq;
using NetFramework48Console.Models;
using NetFramework48Console.Services;

namespace NetFramework48Console
{
    /// <summary>
    /// Main program entry point for .NET Framework 4.8 console application
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("NET Framework 4.8 Console Application");
            Console.WriteLine("=====================================");

            // Initialize product service
            var productService = new ProductService();
            
            // Add some sample products
            productService.AddProduct(new Product { Id = 1, Name = "Laptop", Price = 999.99m, Category = "Electronics" });
            productService.AddProduct(new Product { Id = 2, Name = "Mouse", Price = 25.50m, Category = "Electronics" });
            productService.AddProduct(new Product { Id = 3, Name = "Keyboard", Price = 75.00m, Category = "Electronics" });

            // Display all products
            Console.WriteLine("\nAll Products:");
            var allProducts = productService.GetAllProducts();
            foreach (var product in allProducts)
            {
                Console.WriteLine($"ID: {product.Id}, Name: {product.Name}, Price: ${product.Price:F2}, Category: {product.Category}");
            }

            // Find a specific product
            Console.WriteLine("\nFinding product with ID 2:");
            var foundProduct = productService.GetProductById(2);
            if (foundProduct != null)
            {
                Console.WriteLine($"Found: {foundProduct.Name} - ${foundProduct.Price:F2}");
            }

            // Get products by category
            Console.WriteLine("\nElectronics products:");
            var electronics = productService.GetProductsByCategory("Electronics");
            foreach (var product in electronics)
            {
                Console.WriteLine($"- {product.Name}: ${product.Price:F2}");
            }

            // Test business logic
            var businessLogic = new BusinessLogic();
            Console.WriteLine($"\nCalculating discount for product ID 1:");
            var discountedPrice = businessLogic.CalculateDiscountedPrice(1, 0.15m);
            Console.WriteLine($"Original Price: ${allProducts.First().Price:F2}");
            Console.WriteLine($"Discounted Price (15% off): ${discountedPrice:F2}");

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
