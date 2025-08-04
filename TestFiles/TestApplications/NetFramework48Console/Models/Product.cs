using System;

namespace NetFramework48Console.Models
{
    /// <summary>
    /// Product model class
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Unique identifier for the product
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Product price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Product category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Optional product description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Date when product was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Whether the product is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Stock quantity
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Override ToString for better display
        /// </summary>
        /// <returns>String representation of the product</returns>
        public override string ToString()
        {
            return $"{Name} ({Category}) - ${Price:F2}";
        }

        /// <summary>
        /// Check if product is in stock
        /// </summary>
        /// <returns>True if in stock, false otherwise</returns>
        public bool IsInStock()
        {
            return StockQuantity > 0;
        }

        /// <summary>
        /// Check if product is discounted (price ends in .99 or .95)
        /// </summary>
        /// <returns>True if appears to be a discounted price</returns>
        public bool IsDiscountedPrice()
        {
            var cents = (Price - Math.Floor(Price)) * 100;
            return Math.Abs(cents - 99) < 0.01m || Math.Abs(cents - 95) < 0.01m;
        }
    }
}
