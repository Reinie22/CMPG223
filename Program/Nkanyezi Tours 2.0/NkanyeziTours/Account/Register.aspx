<%@ Page Title="Create an account" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="Register.aspx.cs" Inherits="NkanyeziTours.Account.Register" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="row justify-content-center">
        <div class="col-md-7 col-lg-6">

            <h1 class="h3 mb-1">Create an account</h1>
            <p class="text-muted mb-4">
                Plan your own trip, pick where you would like to stay and what you
                would like to do, and one of our agents will take it from there.
            </p>

            <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert alert-danger py-2">
                <asp:Literal ID="litError" runat="server" />
            </asp:Panel>

            <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="SignUp"
                                   CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

            <div class="card">
                <div class="card-body">
                    <div class="row g-3">

                        <div class="col-md-6">
                            <asp:Label ID="lblName" runat="server" AssociatedControlID="txtName"
                                       CssClass="form-label">First name <span class="text-danger">*</span></asp:Label>
                            <asp:TextBox ID="txtName" ToolTip="Your first name, as it should appear on your booking." runat="server" CssClass="form-control" MaxLength="60" />
                            <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName"
                                                        ValidationGroup="SignUp" Display="Dynamic"
                                                        CssClass="text-danger small"
                                                        ErrorMessage="Please give your first name." />
                        </div>

                        <div class="col-md-6">
                            <asp:Label ID="lblSurname" runat="server" AssociatedControlID="txtSurname"
                                       CssClass="form-label">Surname <span class="text-danger">*</span></asp:Label>
                            <asp:TextBox ID="txtSurname" ToolTip="Your surname, as it appears on your ID or passport." runat="server" CssClass="form-control" MaxLength="60" />
                            <asp:RequiredFieldValidator ID="rfvSurname" runat="server" ControlToValidate="txtSurname"
                                                        ValidationGroup="SignUp" Display="Dynamic"
                                                        CssClass="text-danger small"
                                                        ErrorMessage="Please give your surname." />
                        </div>

                        <div class="col-12">
                            <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail"
                                       CssClass="form-label">Email <span class="text-danger">*</span></asp:Label>
                            <asp:TextBox ID="txtEmail" ToolTip="This will be your username when you sign in." runat="server" CssClass="form-control"
                                         TextMode="Email" MaxLength="120" />
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                                                        ValidationGroup="SignUp" Display="Dynamic"
                                                        CssClass="text-danger small"
                                                        ErrorMessage="Please give your email address." />
                            <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                                                            ValidationGroup="SignUp" Display="Dynamic"
                                                            CssClass="text-danger small"
                                                            ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                                                            ErrorMessage="That does not look like an email address." />
                            <div class="form-text">This will be your username.</div>
                        </div>

                        <div class="col-12">
                            <asp:Label ID="lblPhone" runat="server" AssociatedControlID="txtPhone"
                                       CssClass="form-label">Telephone <span class="text-danger">*</span></asp:Label>
                            <asp:TextBox ID="txtPhone" ToolTip="How your travel agent reaches you about your trip." runat="server" CssClass="form-control" MaxLength="20" />
                            <asp:RequiredFieldValidator ID="rfvPhone" runat="server" ControlToValidate="txtPhone"
                                                        ValidationGroup="SignUp" Display="Dynamic"
                                                        CssClass="text-danger small"
                                                        ErrorMessage="Telephone is required." />
                            <asp:RegularExpressionValidator ID="revPhone" runat="server"
                                                            ControlToValidate="txtPhone"
                                                            ValidationGroup="SignUp" Display="Dynamic"
                                                            ValidationExpression="^\s*\+?[0-9\s\-\(\)]{7,20}\s*$"
                                                            CssClass="text-danger small"
                                                            ErrorMessage="That does not look like a phone number." />
                            <div class="form-text">So that your agent can contact you.</div>
                        </div>

                        <div class="col-md-6">
                            <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword"
                                       CssClass="form-label">Password <span class="text-danger">*</span></asp:Label>
                            <asp:TextBox ID="txtPassword" ToolTip="At least 8 characters. Use something you do not use on other sites." runat="server" CssClass="form-control"
                                         TextMode="Password" />
                            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                                                        ValidationGroup="SignUp" Display="Dynamic"
                                                        CssClass="text-danger small"
                                                        ErrorMessage="Please choose a password." />

                            <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="txtPassword"
                                                            ValidationGroup="SignUp" Display="Dynamic"
                                                            CssClass="text-danger small"
                                                            ValidationExpression="^.{8,}$"
                                                            ErrorMessage="Your password must be at least eight characters." />
                            <div class="form-text">At least eight characters.</div>
                        </div>

                        <div class="col-md-6">
                            <asp:Label ID="lblConfirm" runat="server" AssociatedControlID="txtConfirm"
                                       CssClass="form-label">Retype password <span class="text-danger">*</span></asp:Label>
                            <asp:TextBox ID="txtConfirm" ToolTip="Type the same password again so a typo cannot lock you out." runat="server" CssClass="form-control"
                                         TextMode="Password" />
                            <asp:RequiredFieldValidator ID="rfvConfirm" runat="server"
                                                        ControlToValidate="txtConfirm"
                                                        ValidationGroup="SignUp" Display="Dynamic"
                                                        CssClass="text-danger small"
                                                        ErrorMessage="Please retype your password." />
                            <asp:CompareValidator ID="cvConfirm" runat="server"
                                                  ControlToValidate="txtConfirm"
                                                  ControlToCompare="txtPassword"
                                                  ValidationGroup="SignUp" Display="Dynamic"
                                                  CssClass="text-danger small"
                                                  ErrorMessage="The two passwords do not match." />
                        </div>

                    </div>

                    <hr class="my-4" />

                    <div class="d-grid">
                        <asp:Button ID="btnSignUp" ToolTip="Creates a client account. Staff accounts are made by an administrator." runat="server" Text="Create my account"
                                    CssClass="btn btn-primary"
                                    ValidationGroup="SignUp" OnClick="btnSignUp_Click" />
                    </div>
                </div>
            </div>

            <p class="text-center text-muted small mt-3">
                Already have an account?
                <asp:HyperLink ID="lnkSignIn" ToolTip="Already have an account? Sign in instead." runat="server"
                               NavigateUrl="~/Account/Login.aspx">Sign in</asp:HyperLink>
            </p>

        </div>
    </div>

</asp:Content>
