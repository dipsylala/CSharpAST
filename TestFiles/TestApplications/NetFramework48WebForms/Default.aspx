<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="NetFramework48WebForms._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>.NET Framework 4.8</h1>
        <p class="lead">This is a test WebForms application built with .NET Framework 4.8 for CSharpAST analysis testing.</p>
        <p><a href="Products/ProductList.aspx" class="btn btn-primary btn-lg">View Products &raquo;</a></p>
    </div>

    <div class="row">
        <div class="col-md-4">
            <h2>Getting started</h2>
            <p>
                This application demonstrates classic ASP.NET WebForms patterns including:
            </p>
            <ul>
                <li>ASPX Pages with Code-Behind</li>
                <li>Master Pages and Content Pages</li>
                <li>Server Controls and ViewState</li>
                <li>PostBack Event Model</li>
            </ul>
            <p><a class="btn btn-default" href="About.aspx">Learn more &raquo;</a></p>
        </div>
        <div class="col-md-4">
            <h2>Products</h2>
            <p>Explore the Products section to see CRUD operations in action with traditional WebForms patterns.</p>
            <p><a class="btn btn-default" href="Products/ProductList.aspx">View Products &raquo;</a></p>
        </div>
        <div class="col-md-4">
            <h2>AST Analysis</h2>
            <p>This application serves as a test case for CSharpAST to analyze .NET Framework 4.8 WebForms applications.</p>
            <p><a class="btn btn-default" href="Contact.aspx">Contact &raquo;</a></p>
        </div>
    </div>

    <div class="row">
        <div class="col-md-12">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <h3>Server Information</h3>
                </div>
                <div class="panel-body">
                    <p><strong>Server Time:</strong> <asp:Label ID="lblServerTime" runat="server"></asp:Label></p>
                    <p><strong>Application Name:</strong> <asp:Label ID="lblAppName" runat="server"></asp:Label></p>
                    <p><strong>Framework Version:</strong> <asp:Label ID="lblFrameworkVersion" runat="server"></asp:Label></p>
                    <asp:Button ID="btnRefresh" runat="server" Text="Refresh Info" CssClass="btn btn-info" OnClick="btnRefresh_Click" />
                </div>
            </div>
        </div>
    </div>

</asp:Content>
