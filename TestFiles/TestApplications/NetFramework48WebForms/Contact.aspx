<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="NetFramework48WebForms.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %>.</h2>
    <h3>Your contact page.</h3>
    <address>
        One Microsoft Way<br />
        Redmond, WA 98052-6399<br />
        <abbr title="Phone">P:</abbr>
        425.555.0100
    </address>

    <address>
        <strong>Support:</strong>   <a href="mailto:Support@example.com">Support@example.com</a><br />
        <strong>Marketing:</strong> <a href="mailto:Marketing@example.com">Marketing@example.com</a>
    </address>

    <div class="row">
        <div class="col-md-8">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <h4>Contact Form</h4>
                </div>
                <div class="panel-body">
                    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-success">
                        <asp:Label ID="lblMessage" runat="server"></asp:Label>
                    </asp:Panel>

                    <div class="form-group">
                        <asp:Label ID="lblName" runat="server" Text="Name:" AssociatedControlID="txtName" CssClass="control-label"></asp:Label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="Enter your name"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName" 
                                                   ErrorMessage="Name is required" CssClass="text-danger" Display="Dynamic"></asp:RequiredFieldValidator>
                    </div>

                    <div class="form-group">
                        <asp:Label ID="lblEmail" runat="server" Text="Email:" AssociatedControlID="txtEmail" CssClass="control-label"></asp:Label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter your email"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" 
                                                   ErrorMessage="Email is required" CssClass="text-danger" Display="Dynamic"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                                                       ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                                                       ErrorMessage="Please enter a valid email address" CssClass="text-danger" Display="Dynamic"></asp:RegularExpressionValidator>
                    </div>

                    <div class="form-group">
                        <asp:Label ID="lblSubject" runat="server" Text="Subject:" AssociatedControlID="txtSubject" CssClass="control-label"></asp:Label>
                        <asp:TextBox ID="txtSubject" runat="server" CssClass="form-control" placeholder="Subject"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvSubject" runat="server" ControlToValidate="txtSubject" 
                                                   ErrorMessage="Subject is required" CssClass="text-danger" Display="Dynamic"></asp:RequiredFieldValidator>
                    </div>

                    <div class="form-group">
                        <asp:Label ID="lblMessageText" runat="server" Text="Message:" AssociatedControlID="txtMessageText" CssClass="control-label"></asp:Label>
                        <asp:TextBox ID="txtMessageText" runat="server" CssClass="form-control" TextMode="MultiLine" 
                                    Rows="5" placeholder="Your message"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvMessage" runat="server" ControlToValidate="txtMessageText" 
                                                   ErrorMessage="Message is required" CssClass="text-danger" Display="Dynamic"></asp:RequiredFieldValidator>
                    </div>

                    <div class="form-group">
                        <asp:Button ID="btnSend" runat="server" Text="Send Message" CssClass="btn btn-primary" OnClick="btnSend_Click" />
                        <asp:Button ID="btnClear" runat="server" Text="Clear Form" CssClass="btn btn-default" OnClick="btnClear_Click" CausesValidation="false" />
                    </div>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <h4>Business Hours</h4>
                </div>
                <div class="panel-body">
                    <p><strong>Monday - Friday:</strong> 9:00 AM - 5:00 PM</p>
                    <p><strong>Saturday:</strong> 10:00 AM - 2:00 PM</p>
                    <p><strong>Sunday:</strong> Closed</p>
                </div>
            </div>

            <div class="panel panel-info">
                <div class="panel-heading">
                    <h4>Form Statistics</h4>
                </div>
                <div class="panel-body">
                    <p><strong>Page Views:</strong> <asp:Label ID="lblPageViews" runat="server"></asp:Label></p>
                    <p><strong>Form Submissions:</strong> <asp:Label ID="lblSubmissions" runat="server"></asp:Label></p>
                    <p><strong>Last Reset:</strong> <asp:Label ID="lblLastReset" runat="server"></asp:Label></p>
                    <asp:Button ID="btnResetStats" runat="server" Text="Reset Statistics" CssClass="btn btn-info btn-sm" OnClick="btnResetStats_Click" CausesValidation="false" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
