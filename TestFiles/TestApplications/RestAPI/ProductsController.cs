using Microsoft.AspNetCore.Mvc;
using RestAPI.Models;
using RestAPI.Services;

namespace RestAPI.Controllers
{
    /// <summary>
    /// API Controller for product management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Product>>>> GetAllProducts()
        {
            try
            {
                _logger.LogInformation("Fetching all products");
                var products = await _productService.GetAllProductsAsync();
                return Ok(ApiResponse<IEnumerable<Product>>.SuccessResult(products, "Products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return StatusCode(500, ApiResponse<IEnumerable<Product>>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Product>>> GetProduct(int id)
        {
            try
            {
                _logger.LogInformation("Fetching product with ID: {ProductId}", id);
                var product = await _productService.GetProductByIdAsync(id);
                
                if (product == null)
                {
                    return NotFound(ApiResponse<Product>.ErrorResult("Product not found"));
                }

                return Ok(ApiResponse<Product>.SuccessResult(product, "Product retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product with ID: {ProductId}", id);
                return StatusCode(500, ApiResponse<Product>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Product>>> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(ApiResponse<Product>.ErrorResult("Validation failed", errors));
                }

                _logger.LogInformation("Creating new product: {ProductName}", request.Name);
                var product = await _productService.CreateProductAsync(request);
                
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id },
                    ApiResponse<Product>.SuccessResult(product, "Product created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, ApiResponse<Product>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Product>>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(ApiResponse<Product>.ErrorResult("Validation failed", errors));
                }

                _logger.LogInformation("Updating product with ID: {ProductId}", id);
                var product = await _productService.UpdateProductAsync(id, request);
                
                if (product == null)
                {
                    return NotFound(ApiResponse<Product>.ErrorResult("Product not found"));
                }

                return Ok(ApiResponse<Product>.SuccessResult(product, "Product updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
                return StatusCode(500, ApiResponse<Product>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(int id)
        {
            try
            {
                _logger.LogInformation("Deleting product with ID: {ProductId}", id);
                var success = await _productService.DeleteProductAsync(id);
                
                if (!success)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Product not found"));
                }

                return Ok(ApiResponse<object>.SuccessResult(null, "Product deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Search products
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Product>>>> SearchProducts([FromQuery] string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching products with term: {SearchTerm}", searchTerm);
                var products = await _productService.SearchProductsAsync(searchTerm);
                return Ok(ApiResponse<IEnumerable<Product>>.SuccessResult(products, "Search completed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                return StatusCode(500, ApiResponse<IEnumerable<Product>>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Product>>>> GetProductsByCategory(string category)
        {
            try
            {
                _logger.LogInformation("Fetching products for category: {Category}", category);
                var products = await _productService.GetProductsByCategoryAsync(category);
                return Ok(ApiResponse<IEnumerable<Product>>.SuccessResult(products, "Products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products for category: {Category}", category);
                return StatusCode(500, ApiResponse<IEnumerable<Product>>.ErrorResult("Internal server error"));
            }
        }
    }
}
