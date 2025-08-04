using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetFramework48MVC.Models
{
    public class ProductViewModel
    {
        public List<Product> Products { get; set; }
        public int TotalProducts { get; set; }
        public List<string> Categories { get; set; }
        public string SelectedCategory { get; set; }
        public string SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public ProductViewModel()
        {
            Products = new List<Product>();
            Categories = new List<string>();
        }
    }
}
