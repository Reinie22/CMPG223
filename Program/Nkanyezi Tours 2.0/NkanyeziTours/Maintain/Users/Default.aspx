<%@ Page Title="Users" Language="C#" MasterPageFile="~/Maintain/Maintain.master" AutoEventWireup="true"
         CodeBehind="Default.aspx.cs" Inherits="NkanyeziTours.Maintain.Users._Default" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MaintainContent" runat="server">

    <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
        <h1 class="h3 mb-0">Users and access</h1>
        <asp:HyperLink ID="lnkAdd" ToolTip="Create a staff or client account." runat="server" CssClass="btn btn-primary"
                       NavigateUrl="~/Maintain/Users/Edit.aspx">+ Add a user</asp:HyperLink>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <div class="card mb-3">
        <div class="card-body">
            <div class="row g-3 align-items-end">
                <div class="col-md-5">
                    <asp:Label ID="lblSearchText" runat="server" AssociatedControlID="txtSearchText"
                               CssClass="form-label">Name or email</asp:Label>
                    <asp:TextBox ID="txtSearchText" ToolTip="Search on name, surname or email." runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-4">
                    <asp:Label ID="lblRole" runat="server" AssociatedControlID="ddlRole"
                               CssClass="form-label">Role</asp:Label>
                    <asp:DropDownList ID="ddlRole" ToolTip="Show only one kind of user." runat="server" CssClass="form-select" />
                </div>
                <div class="col-md-3 d-flex gap-2">
                    <asp:Button ID="btnSearch" ToolTip="Applies the filters above." runat="server" Text="Search" CssClass="btn btn-outline-primary"
                                OnClick="btnSearch_Click" />
                    <asp:Button ID="btnClear" ToolTip="Clears every filter and shows everyone again." runat="server" Text="Clear" CssClass="btn btn-outline-secondary"
                                CausesValidation="false" OnClick="btnClear_Click" />
                </div>
            </div>
        </div>
    </div>

    <div class="card">
        <div class="card-body">
            <asp:GridView ID="gvUsers" runat="server"
                          AutoGenerateColumns="false"
                          DataKeyNames="UserId"
                          OnRowCommand="gvUsers_RowCommand"
                          CssClass="table table-striped align-middle"
                          EmptyDataText="No users match that search."
                          GridLines="None">
                <Columns>
                    <asp:BoundField DataField="FullName" HeaderText="Name" />
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:BoundField DataField="ContactNumber" HeaderText="Telephone" />
                    <asp:TemplateField HeaderText="Role">
                        <ItemTemplate>
                            <asp:Label ID="lblRoleCell" runat="server" CssClass="badge text-bg-secondary"
                                       Text='<%# Eval("RoleName") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <asp:Label ID="lblStatus" runat="server"
                                       CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge text-bg-success" : "badge text-bg-danger" %>'
                                       Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Deactivated" %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <div class="d-flex gap-1">
                                <asp:HyperLink ID="lnkEdit" ToolTip="Change this person’s details, role or password." runat="server"
                                               CssClass="btn btn-sm btn-outline-primary text-nowrap" Text="Edit"
                                               NavigateUrl='<%# "~/Maintain/Users/Edit.aspx?id=" + Eval("UserId") %>' />
                                <asp:LinkButton ID="lnkDeactivate" ToolTip="Stops this person signing in. Their trips and history are kept." runat="server"
                                                CssClass="btn btn-sm btn-outline-danger text-nowrap"
                                                CommandName="DeactivateUser"
                                                CommandArgument='<%# Eval("UserId") %>'
                                                Visible='<%# Convert.ToBoolean(Eval("IsActive")) %>'
                                                Text="Deactivate"
                                                data-confirm="Deactivate this account? They will not be able to sign in." />
                                <asp:LinkButton ID="lnkActivate" ToolTip="Lets this person sign in again." runat="server"
                                                CssClass="btn btn-sm btn-outline-success text-nowrap"
                                                CommandName="ActivateUser"
                                                CommandArgument='<%# Eval("UserId") %>'
                                                Visible='<%# !Convert.ToBoolean(Eval("IsActive")) %>'
                                                Text="Reactivate" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <p class="text-muted small mb-0 mt-2">
                Accounts are never deleted. Deactivating keeps the trips and bookings
                the person captured, which a delete would take with it.
            </p>
        </div>
    </div>

</asp:Content>
