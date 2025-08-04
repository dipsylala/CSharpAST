using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NetFramework48WebForms
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadServerInfo();
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadServerInfo();
        }

        private void LoadServerInfo()
        {
            lblServerTime.Text = DateTime.Now.ToString("F");
            lblAppName.Text = HttpContext.Current.Request.ApplicationPath;
            lblFrameworkVersion.Text = Environment.Version.ToString();
        }
    }
}
