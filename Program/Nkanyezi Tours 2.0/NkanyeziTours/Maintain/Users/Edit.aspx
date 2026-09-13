<%@ Page Title="User" Language="C#" MasterPageFile="~/Maintain/Maintain.master" AutoEventWireup="true"
         CodeBehind="Edit.aspx.cs" Inherits="NkanyeziTours.Maintain.Users.Edit" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MaintainContent" runat="server">

    <h1 class="h3 mb-4"><asp:Literal ID="litHeading" runat="server" Text="Add a user" /></h1>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="User"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <div class="card">
        <div class="card-body">
            <div class="row g-3">

                <div class="col-md-4">
                    <asp:Label ID="lblName" runat="server" AssociatedControlID="txtName"
                               CssClass="form-label">First name <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtName" ToolTip="First name." runat="server" CssClass="form-control" MaxLength="60" />
                    <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName"
                                                ValidationGroup="User" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The first name is required." />
                </div>

                <div class="col-md-4">
                    <asp:Label ID="lblSurname" runat="server" AssociatedControlID="txtSurname"
                               CssClass="form-label">Surname <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtSurname" ToolTip="Surname." runat="server" CssClass="form-control" MaxLength="60" />
                    <asp:RequiredFieldValidator ID="rfvSurname" runat="server" ControlToValidate="txtSurname"
                                                ValidationGroup="User" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The surname is required." />
                </div>

                <div class="col-md-4">
                    <asp:Label ID="lblRole" runat="server" AssociatedControlID="ddlRole"
                               CssClass="form-label">Role <span class="text-danger">*</span></asp:Label>
                    <asp:DropDownList ID="ddlRole" ToolTip="What this person may do. Agents get trips, supervisors assign them, administrators maintain the data." runat="server" CssClass="form-select" />
                </div>

                <div class="col-md-6">
                    <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail"
                               CssClass="form-label">Email <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtEmail" ToolTip="What they sign in with. It has to be unique." runat="server" CssClass="form-control" TextMode="Email"
                                 MaxLength="120" />
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                                                ValidationGroup="User" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The email address is required." />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                                                    ValidationGroup="User" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                                                    ErrorMessage="That does not look like an email address." />
                </div>

                <div class="col-md-6">
                    <asp:Label ID="lblPhone" runat="server" AssociatedControlID="txtPhone"
                               CssClass="form-label">Telephone</asp:Label>
                    <asp:TextBox ID="txtPhone" ToolTip="Contact number." runat="server" CssClass="form-control" MaxLength="20" />
                </div>

                <div class="col-md-6">
                    <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword"
                               CssClass="form-label"><asp:Literal ID="litPasswordLabel" runat="server"
                               Text="Password" /></asp:Label>
                    <asp:TextBox ID="txtPassword" ToolTip="At least 8 characters. Leave this blank to keep their current password." runat="server" CssClass="form-control" TextMode="Password" />
                    <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                                                ValidationGroup="User" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="A password is required for a new account." />
                    <div class="form-text"><asp:Literal ID="litPasswordHelp" runat="server" /></div>
                </div>

                <div class="col-md-6 d-flex align-items-end">
                    <div class="form-check">
                        <asp:CheckBox ID="chkActive" ToolTip="Untick to stop them signing in without deleting the account." runat="server" CssClass="form-check-input" Checked="true" />
                        <asp:Label ID="lblActive" runat="server" AssociatedControlID="chkActive"
                                   CssClass="form-check-label">Allowed to sign in</asp:Label>
                    </div>
                </div>

            </div>

            <hr class="my-4" />

            <div class="d-flex justify-content-end gap-2">
                <asp:HyperLink ID="lnkCancel" ToolTip="Goes back to the list without saving." runat="server" CssClass="btn btn-outline-secondary"
                               NavigateUrl="~/Maintain/Users/Default.aspx">Cancel</asp:HyperLink>
                <asp:Button ID="btnSave" ToolTip="Saves the account." runat="server" Text="Save" CssClass="btn btn-primary"
                            ValidationGroup="User" OnClick="btnSave_Click" />
            </div>
        </div>
    </div>

</asp:Content>
