using System;
using System.Collections.Generic;
using System.Linq;
using NetFramework48Console.Models;
using NetFramework48Console.Services;

namespace NetFramework48Console
{
    /// <summary>
    /// Business logic class demonstrating various C# patterns and constructs
    /// </summary>
    public class BusinessLogic
    {
        private readonly ProductService _productService;

        public BusinessLogic()
        {
            _productService = new ProductService();
        }

        /// <summary>
        /// Calculate discounted price for a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="discountRate">Discount rate (0.0 to 1.0)</param>
        /// <returns>Discounted price</returns>
        public decimal CalculateDiscountedPrice(int productId, decimal discountRate)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException($"Product with ID {productId} not found");

            if (discountRate < 0 || discountRate > 1)
                throw new ArgumentOutOfRangeException(nameof(discountRate), "Discount rate must be between 0 and 1");

            return product.Price * (1 - discountRate);
        }

        /// <summary>
        /// Get total value of all products in a category
        /// </summary>
        /// <param name="category">Product category</param>
        /// <returns>Total value</returns>
        public decimal GetCategoryTotalValue(string category)
        {
            var products = _productService.GetProductsByCategory(category);
            return products.Sum(p => p.Price);
        }

        /// <summary>
        /// Get average price for products in a category
        /// </summary>
        /// <param name="category">Product category</param>
        /// <returns>Average price</returns>
        public decimal GetCategoryAveragePrice(string category)
        {
            var products = _productService.GetProductsByCategory(category);
            if (!products.Any())
                return 0;

            return products.Average(p => p.Price);
        }

        /// <summary>
        /// Apply bulk discount to products over a certain price threshold
        /// </summary>
        /// <param name="priceThreshold">Minimum price for discount eligibility</param>
        /// <param name="discountRate">Discount rate to apply</param>
        /// <returns>List of products with updated prices</returns>
        public List<Product> ApplyBulkDiscount(decimal priceThreshold, decimal discountRate)
        {
            var allProducts = _productService.GetAllProducts();
            var discountedProducts = new List<Product>();

            foreach (var product in allProducts.Where(p => p.Price >= priceThreshold))
            {
                var discountedProduct = new Product
                {
                    Id = product.Id,
                    Name = product.Name,
                    Category = product.Category,
                    Price = product.Price * (1 - discountRate)
                };
                discountedProducts.Add(discountedProduct);
            }

            return discountedProducts;
        }

        /// <summary>
        /// Generate product summary report
        /// </summary>
        /// <returns>Report string</returns>
        public string GenerateProductReport()
        {
            var allProducts = _productService.GetAllProducts();
            var categories = allProducts.Select(p => p.Category).Distinct().ToList();

            var report = "Product Summary Report\n";
            report += "=====================\n\n";

            foreach (var category in categories)
            {
                var categoryProducts = allProducts.Where(p => p.Category == category).ToList();
                report += $"Category: {category}\n";
                report += $"  Product Count: {categoryProducts.Count}\n";
                report += $"  Total Value: ${categoryProducts.Sum(p => p.Price):F2}\n";
                report += $"  Average Price: ${categoryProducts.Average(p => p.Price):F2}\n";
                report += $"  Products:\n";

                foreach (var product in categoryProducts.OrderBy(p => p.Name))
                {
                    report += $"    - {product.Name}: ${product.Price:F2}\n";
                }
                report += "\n";
            }

            return report;
        }
    }
}
