using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NetFramework48WebForms.Models
{
    [Serializable]
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Product Name")]
        public string Name { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public Product()
        {
            // Default constructor for serialization
        }

        public Product(int id, string name, string description, decimal price, string category)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            Category = category;
            CreatedDate = DateTime.Now;
            IsActive = true;
        }
    }
}
