using RestAPI.Models;

namespace RestAPI.Services
{
    /// <summary>
    /// Service interface for product operations
    /// </summary>
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(CreateProductRequest request);
        Task<Product?> UpdateProductAsync(int id, UpdateProductRequest request);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
    }

    /// <summary>
    /// In-memory implementation of product service
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly List<Product> _products = new();
        private int _nextId = 1;

        public ProductService()
        {
            // Seed with some sample data
            SeedSampleData();
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            await Task.Delay(10); // Simulate async operation
            return _products.Where(p => p.IsActive).ToList();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            await Task.Delay(10);
            return _products.FirstOrDefault(p => p.Id == id && p.IsActive);
        }

        public async Task<Product> CreateProductAsync(CreateProductRequest request)
        {
            await Task.Delay(10);

            var product = new Product
            {
                Id = _nextId++,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Category = request.Category,
                Stock = request.Stock
            };

            _products.Add(product);
            return product;
        }

        public async Task<Product?> UpdateProductAsync(int id, UpdateProductRequest request)
        {
            await Task.Delay(10);

            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return null;

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.Category = request.Category;
            product.Stock = request.Stock;
            product.IsActive = request.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            return product;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            await Task.Delay(10);

            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return false;

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            return true;
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            await Task.Delay(10);

            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllProductsAsync();

            return _products.Where(p => p.IsActive &&
                (p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                 p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                 p.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            await Task.Delay(10);

            return _products.Where(p => p.IsActive &&
                p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private void SeedSampleData()
        {
            _products.AddRange(new[]
            {
                new Product { Id = _nextId++, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Category = "Electronics", Stock = 10 },
                new Product { Id = _nextId++, Name = "Mouse", Description = "Wireless mouse", Price = 29.99m, Category = "Electronics", Stock = 50 },
                new Product { Id = _nextId++, Name = "Book", Description = "Programming guide", Price = 49.99m, Category = "Books", Stock = 20 }
            });
        }
    }
}
