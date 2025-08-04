<%@ Page Title="Products" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ProductList.aspx.cs" Inherits="NetFramework48WebForms.Products.ProductList" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Products</h2>

    <div class="row">
        <div class="col-md-12">
            <p>
                <asp:HyperLink ID="lnkAddProduct" runat="server" NavigateUrl="~/Products/AddProduct.aspx" CssClass="btn btn-primary">Add New Product</asp:HyperLink>
            </p>
            
            <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-info">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <div class="panel panel-default">
                <div class="panel-heading">
                    <h4>Product Summary</h4>
                </div>
                <div class="panel-body">
                    <p><strong>Total Products:</strong> <asp:Label ID="lblTotalProducts" runat="server"></asp:Label></p>
                    <p><strong>Categories:</strong> <asp:Label ID="lblCategories" runat="server"></asp:Label></p>
                </div>
            </div>
        </div>
    </div>

    <div class="row">
        <div class="col-md-12">
            <asp:GridView ID="gvProducts" runat="server" CssClass="table table-striped table-hover" 
                         AutoGenerateColumns="false" DataKeyNames="Id" 
                         OnRowCommand="gvProducts_RowCommand" 
                         OnRowDeleting="gvProducts_RowDeleting"
                         OnRowEditing="gvProducts_RowEditing"
                         OnRowUpdating="gvProducts_RowUpdating"
                         OnRowCancelingEdit="gvProducts_RowCancelingEdit"
                         BorderStyle="None" GridLines="None">
                <HeaderStyle CssClass="bg-primary text-white" />
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="ID" ReadOnly="true" />
                    <asp:BoundField DataField="Name" HeaderText="Product Name" />
                    <asp:BoundField DataField="Description" HeaderText="Description" />
                    <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="{0:C}" />
                    <asp:BoundField DataField="Category" HeaderText="Category" />
                    <asp:CheckBoxField DataField="IsActive" HeaderText="Active" />
                    <asp:BoundField DataField="CreatedDate" HeaderText="Created" DataFormatString="{0:MM/dd/yyyy}" ReadOnly="true" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:HyperLink ID="lnkDetails" runat="server" 
                                          NavigateUrl='<%# "~/Products/ProductDetails.aspx?id=" + Eval("Id") %>' 
                                          CssClass="btn btn-info btn-sm">Details</asp:HyperLink>
                            <asp:LinkButton ID="btnEdit" runat="server" CommandName="Edit" CommandArgument='<%# Eval("Id") %>' 
                                           CssClass="btn btn-warning btn-sm">Edit</asp:LinkButton>
                            <asp:LinkButton ID="btnDelete" runat="server" CommandName="Delete" CommandArgument='<%# Eval("Id") %>' 
                                           CssClass="btn btn-danger btn-sm" 
                                           OnClientClick="return confirm('Are you sure you want to delete this product?');">Delete</asp:LinkButton>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:LinkButton ID="btnUpdate" runat="server" CommandName="Update" CommandArgument='<%# Eval("Id") %>' 
                                           CssClass="btn btn-success btn-sm">Update</asp:LinkButton>
                            <asp:LinkButton ID="btnCancel" runat="server" CommandName="Cancel" CommandArgument='<%# Eval("Id") %>' 
                                           CssClass="btn btn-default btn-sm">Cancel</asp:LinkButton>
                        </EditItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="alert alert-info">
                        <strong>No products found.</strong> 
                        <asp:HyperLink ID="lnkAddFirst" runat="server" NavigateUrl="~/Products/AddProduct.aspx" CssClass="alert-link">Add the first product</asp:HyperLink>.
                    </div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

    <div class="row">
        <div class="col-md-12">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <h4>Quick Actions</h4>
                </div>
                <div class="panel-body">
                    <asp:Button ID="btnRefresh" runat="server" Text="Refresh List" CssClass="btn btn-default" OnClick="btnRefresh_Click" />
                    <asp:Button ID="btnAddSample" runat="server" Text="Add Sample Data" CssClass="btn btn-info" OnClick="btnAddSample_Click" />
                    <asp:Button ID="btnClearAll" runat="server" Text="Clear All Products" CssClass="btn btn-danger" 
                               OnClick="btnClearAll_Click" OnClientClick="return confirm('Are you sure you want to delete all products?');" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
