using System;
using System.Collections.Generic;
using System.Linq;
using NetFramework48Console.Models;

namespace NetFramework48Console.Services
{
    /// <summary>
    /// Service class for managing products
    /// </summary>
    public class ProductService
    {
        private readonly List<Product> _products;

        /// <summary>
        /// Initialize the product service with an empty product list
        /// </summary>
        public ProductService()
        {
            _products = new List<Product>();
        }

        /// <summary>
        /// Add a new product
        /// </summary>
        /// <param name="product">Product to add</param>
        /// <returns>True if added successfully</returns>
        public bool AddProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Product name is required", nameof(product));

            if (product.Price <= 0)
                throw new ArgumentException("Product price must be greater than 0", nameof(product));

            // Check if product with same ID already exists
            if (_products.Any(p => p.Id == product.Id))
                return false;

            _products.Add(product);
            return true;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <returns>List of all products</returns>
        public List<Product> GetAllProducts()
        {
            return new List<Product>(_products); // Return a copy
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product if found, null otherwise</returns>
        public Product GetProductById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        /// <param name="category">Category name</param>
        /// <returns>List of products in the specified category</returns>
        public List<Product> GetProductsByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return new List<Product>();

            return _products.Where(p => 
                string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        /// <param name="product">Updated product</param>
        /// <returns>True if updated successfully</returns>
        public bool UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct == null)
                return false;

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Category = product.Category;
            existingProduct.Description = product.Description;
            existingProduct.IsActive = product.IsActive;
            existingProduct.StockQuantity = product.StockQuantity;

            return true;
        }

        /// <summary>
        /// Delete a product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>True if deleted successfully</returns>
        public bool DeleteProduct(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return false;

            _products.Remove(product);
            return true;
        }

        /// <summary>
        /// Search products by name (partial match)
        /// </summary>
        /// <param name="searchterm">Search term</param>
        /// <returns>List of matching products</returns>
        public List<Product> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Product>();

            return _products.Where(p => 
                p.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        /// <summary>
        /// Get products within a price range
        /// </summary>
        /// <param name="minPrice">Minimum price</param>
        /// <param name="maxPrice">Maximum price</param>
        /// <returns>List of products within the price range</returns>
        public List<Product> GetProductsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            return _products.Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .ToList();
        }

        /// <summary>
        /// Get product count
        /// </summary>
        /// <returns>Total number of products</returns>
        public int GetProductCount()
        {
            return _products.Count;
        }
    }
}
