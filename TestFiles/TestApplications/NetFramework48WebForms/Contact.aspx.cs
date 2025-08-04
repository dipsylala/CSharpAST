using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NetFramework48WebForms
{
    public partial class Contact : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadStatistics();
            }
            IncrementPageViews();
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                // Simulate sending the message
                ProcessContactForm();
                ClearForm();
                ShowSuccessMessage("Thank you! Your message has been sent successfully.");
                IncrementSubmissions();
                LoadStatistics();
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            pnlMessage.Visible = false;
        }

        protected void btnResetStats_Click(object sender, EventArgs e)
        {
            Application["PageViews"] = 0;
            Application["FormSubmissions"] = 0;
            Application["LastReset"] = DateTime.Now;
            LoadStatistics();
            ShowSuccessMessage("Statistics have been reset.");
        }

        private void ProcessContactForm()
        {
            // In a real application, this would send an email or save to database
            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string subject = txtSubject.Text.Trim();
            string message = txtMessageText.Text.Trim();

            // Log the contact form submission (simulate processing)
            System.Diagnostics.Debug.WriteLine($"Contact Form Submitted: {name} ({email}) - {subject}");
        }

        private void ClearForm()
        {
            txtName.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtSubject.Text = string.Empty;
            txtMessageText.Text = string.Empty;
        }

        private void ShowSuccessMessage(string message)
        {
            lblMessage.Text = message;
            pnlMessage.Visible = true;
        }

        private void IncrementPageViews()
        {
            if (Application["PageViews"] == null)
                Application["PageViews"] = 0;
            
            Application["PageViews"] = (int)Application["PageViews"] + 1;
        }

        private void IncrementSubmissions()
        {
            if (Application["FormSubmissions"] == null)
                Application["FormSubmissions"] = 0;
            
            Application["FormSubmissions"] = (int)Application["FormSubmissions"] + 1;
        }

        private void LoadStatistics()
        {
            lblPageViews.Text = (Application["PageViews"] ?? 0).ToString();
            lblSubmissions.Text = (Application["FormSubmissions"] ?? 0).ToString();
            
            DateTime lastReset = (DateTime)(Application["LastReset"] ?? DateTime.Now);
            lblLastReset.Text = lastReset.ToString("g");
        }
    }
}
