using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NetFramework48WebForms.Models;

namespace NetFramework48WebForms.Products
{
    public partial class ProductList : Page
    {
        private List<Product> Products
        {
            get 
            { 
                if (Application["Products"] == null)
                {
                    Application["Products"] = GetSampleProducts();
                }
                return (List<Product>)Application["Products"];
            }
            set { Application["Products"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadProducts();
                LoadSummary();
            }
        }

        protected void gvProducts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int productId = Convert.ToInt32(e.CommandArgument);
            
            switch (e.CommandName)
            {
                case "ViewDetails":
                    Response.Redirect($"~/Products/ProductDetails.aspx?id={productId}");
                    break;
            }
        }

        protected void gvProducts_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int productId = (int)gvProducts.DataKeys[e.RowIndex].Value;
            var product = Products.FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                Products.Remove(product);
                LoadProducts();
                LoadSummary();
                ShowMessage($"Product '{product.Name}' has been deleted.");
            }
        }

        protected void gvProducts_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvProducts.EditIndex = e.NewEditIndex;
            LoadProducts();
        }

        protected void gvProducts_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int productId = (int)gvProducts.DataKeys[e.RowIndex].Value;
            var product = Products.FirstOrDefault(p => p.Id == productId);
            
            if (product != null)
            {
                // Get the updated values from the GridView
                var row = gvProducts.Rows[e.RowIndex];
                product.Name = ((TextBox)row.Cells[1].Controls[0]).Text;
                product.Description = ((TextBox)row.Cells[2].Controls[0]).Text;
                product.Price = decimal.Parse(((TextBox)row.Cells[3].Controls[0]).Text);
                product.Category = ((TextBox)row.Cells[4].Controls[0]).Text;
                product.IsActive = ((CheckBox)row.Cells[5].Controls[0]).Checked;
                
                gvProducts.EditIndex = -1;
                LoadProducts();
                LoadSummary();
                ShowMessage($"Product '{product.Name}' has been updated.");
            }
        }

        protected void gvProducts_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvProducts.EditIndex = -1;
            LoadProducts();
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
            LoadSummary();
            ShowMessage("Product list refreshed.");
        }

        protected void btnAddSample_Click(object sender, EventArgs e)
        {
            AddSampleProducts();
            LoadProducts();
            LoadSummary();
            ShowMessage("Sample products added.");
        }

        protected void btnClearAll_Click(object sender, EventArgs e)
        {
            Products.Clear();
            LoadProducts();
            LoadSummary();
            ShowMessage("All products have been cleared.");
        }

        private void LoadProducts()
        {
            gvProducts.DataSource = Products;
            gvProducts.DataBind();
        }

        private void LoadSummary()
        {
            lblTotalProducts.Text = Products.Count.ToString();
            var categories = Products.Select(p => p.Category).Distinct().ToList();
            lblCategories.Text = string.Join(", ", categories);
        }

        private void ShowMessage(string message)
        {
            lblMessage.Text = message;
            pnlMessage.Visible = true;
        }

        private List<Product> GetSampleProducts()
        {
            return new List<Product>
            {
                new Product(1, "Laptop", "High-performance laptop", 999.99m, "Electronics"),
                new Product(2, "Smartphone", "Latest smartphone model", 699.99m, "Electronics"),
                new Product(3, "Desk Chair", "Ergonomic office chair", 299.99m, "Furniture")
            };
        }

        private void AddSampleProducts()
        {
            var maxId = Products.Any() ? Products.Max(p => p.Id) : 0;
            var newProducts = new[]
            {
                new Product(maxId + 1, "Tablet", "10-inch tablet with high resolution display", 399.99m, "Electronics"),
                new Product(maxId + 2, "Standing Desk", "Adjustable height standing desk", 599.99m, "Furniture"),
                new Product(maxId + 3, "Wireless Mouse", "Ergonomic wireless mouse", 29.99m, "Accessories")
            };

            Products.AddRange(newProducts);
        }
    }
}
