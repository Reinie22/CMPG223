<%@ Page Title="Sign in" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="Login.aspx.cs" Inherits="NkanyeziTours.Account.Login" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="row justify-content-center">
        <div class="col-md-5">
            <div class="card shadow-sm">
                <div class="card-body p-4">

                    <h1 class="h4 mb-1">Sign in</h1>
                    <p class="text-muted small mb-4">Nkanyezi Tours staff and client portal</p>

                    <%-- The red warning panel. Hidden until there is something to say. --%>
                    <asp:Panel ID="pnlError" runat="server" Visible="false"
                               CssClass="alert alert-danger py-2">
                        <asp:Literal ID="litError" runat="server" />
                    </asp:Panel>

                    <div class="mb-3">
                        <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail"
                                   CssClass="form-label">Email address <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"
                                     TextMode="Email" MaxLength="150" />
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                                                    ControlToValidate="txtEmail"
                                                    ValidationGroup="SignIn"
                                                    CssClass="text-danger small"
                                                    Display="Dynamic"
                                                    ErrorMessage="Enter your email address." />
                    </div>

                    <div class="mb-3">
                        <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword"
                                   CssClass="form-label">Password <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control"
                                     TextMode="Password" />
                        <asp:RequiredFieldValidator ID="rfvPassword" runat="server"
                                                    ControlToValidate="txtPassword"
                                                    ValidationGroup="SignIn"
                                                    CssClass="text-danger small"
                                                    Display="Dynamic"
                                                    ErrorMessage="Enter your password." />
                    </div>

                    <div class="form-check mb-4">
                        <asp:CheckBox ID="chkRemember" runat="server" CssClass="form-check-input" />
                        <asp:Label ID="lblRemember" runat="server" AssociatedControlID="chkRemember"
                                   CssClass="form-check-label">Keep me signed in</asp:Label>
                    </div>

                    <div class="d-grid">
                        <asp:Button ID="btnSignIn" runat="server" Text="Sign in"
                                    CssClass="btn btn-primary"
                                    ValidationGroup="SignIn"
                                    OnClick="btnSignIn_Click" />
                    </div>

                </div>
            </div>

            <p class="text-center text-muted small mt-3">
                Seeded test accounts use the password <code>Password123</code>
            </p>
        </div>
    </div>

</asp:Content>
