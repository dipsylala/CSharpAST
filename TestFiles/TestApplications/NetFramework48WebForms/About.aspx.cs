using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NetFramework48WebForms
{
    public partial class About : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadPageInfo();
            }
        }

        protected void btnShowServerVars_Click(object sender, EventArgs e)
        {
            LoadServerVariables();
            pnlServerVars.Visible = true;
        }

        private void LoadPageInfo()
        {
            lblSessionId.Text = Session.SessionID;
            lblUserAgent.Text = Request.UserAgent ?? "Not Available";
        }

        private void LoadServerVariables()
        {
            var serverVars = new DataTable();
            serverVars.Columns.Add("Variable", typeof(string));
            serverVars.Columns.Add("Value", typeof(string));

            var interestingVars = new[] 
            {
                "SERVER_NAME", "SERVER_PORT", "REQUEST_METHOD", "PATH_INFO",
                "QUERY_STRING", "CONTENT_TYPE", "CONTENT_LENGTH", "HTTP_HOST",
                "HTTP_USER_AGENT", "REMOTE_ADDR", "REMOTE_HOST", "LOCAL_ADDR"
            };

            foreach (string varName in interestingVars)
            {
                string value = Request.ServerVariables[varName] ?? "Not Available";
                serverVars.Rows.Add(varName, value);
            }

            gvServerVars.DataSource = serverVars;
            gvServerVars.DataBind();
        }
    }
}
