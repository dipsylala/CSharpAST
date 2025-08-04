<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="NetFramework48WebForms.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %>.</h2>
    <h3>Your application description page.</h3>
    <p>Use this area to provide additional information about your WebForms application.</p>

    <div class="row">
        <div class="col-md-8">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <h4>Application Information</h4>
                </div>
                <div class="panel-body">
                    <dl class="dl-horizontal">
                        <dt>Application Type:</dt>
                        <dd>ASP.NET WebForms</dd>
                        
                        <dt>Framework Version:</dt>
                        <dd>.NET Framework 4.8</dd>
                        
                        <dt>Development Purpose:</dt>
                        <dd>CSharpAST Analysis Testing</dd>
                        
                        <dt>Key Features:</dt>
                        <dd>
                            <ul>
                                <li>Master Pages and Content Pages</li>
                                <li>Server Controls with ViewState</li>
                                <li>PostBack Event Model</li>
                                <li>Code-Behind Pattern</li>
                                <li>ASP.NET Web Optimization</li>
                            </ul>
                        </dd>
                    </dl>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="panel panel-info">
                <div class="panel-heading">
                    <h4>Technical Details</h4>
                </div>
                <div class="panel-body">
                    <p><strong>Session ID:</strong></p>
                    <p><asp:Label ID="lblSessionId" runat="server" CssClass="text-muted"></asp:Label></p>
                    
                    <p><strong>User Agent:</strong></p>
                    <p><asp:Label ID="lblUserAgent" runat="server" CssClass="text-muted small"></asp:Label></p>
                    
                    <p><strong>Server Variables:</strong></p>
                    <asp:Button ID="btnShowServerVars" runat="server" Text="Show Server Variables" 
                               CssClass="btn btn-info btn-sm" OnClick="btnShowServerVars_Click" />
                </div>
            </div>
        </div>
    </div>

    <div class="row">
        <div class="col-md-12">
            <asp:Panel ID="pnlServerVars" runat="server" Visible="false" CssClass="panel panel-default">
                <div class="panel-heading">
                    <h4>Server Variables</h4>
                </div>
                <div class="panel-body">
                    <asp:GridView ID="gvServerVars" runat="server" CssClass="table table-striped table-condensed"
                                  AutoGenerateColumns="true" BorderStyle="None" GridLines="None">
                    </asp:GridView>
                </div>
            </asp:Panel>
        </div>
    </div>
</asp:Content>
