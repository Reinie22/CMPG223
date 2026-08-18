<%@ Page Title="Attractions" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="Default.aspx.cs" Inherits="NkanyeziTours.Maintain.Attractions.Default" %>

<%--
    THE ATTRACTION LIST - the page to copy when you build your own.

    Notice what is NOT in this markup: no SqlDataSource, no connection string,
    no SQL. The grid is filled in the code-behind from AttractionRepository.

    Dragging a SqlDataSource onto a page from the toolbox and typing a query
    into its wizard would put SQL back into the markup. Do not do it. It would
    be duplicated across 49 pages by 8 people and the repositories would become
    dead code.
--%>
<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="d-flex justify-content-between align-items-center mb-3">
        <div>
            <h1 class="h3 mb-0">Attractions</h1>
            <p class="text-muted mb-0 small">Places clients can visit on a trip</p>
        </div>
        <asp:HyperLink ID="lnkAdd" runat="server" CssClass="btn btn-primary"
                       NavigateUrl="~/Maintain/Attractions/Edit.aspx">Add attraction</asp:HyperLink>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <%-- Filter bar --%>
    <div class="card mb-3">
        <div class="card-body">
            <div class="row g-2 align-items-end">
                <div class="col-md-3">
                    <asp:Label ID="lblName" runat="server" AssociatedControlID="txtName"
                               CssClass="form-label small">Name contains</asp:Label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control form-control-sm" />
                </div>
                <div class="col-md-3">
                    <asp:Label ID="lblCategory" runat="server" AssociatedControlID="ddlCategory"
                               CssClass="form-label small">Category</asp:Label>
                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select form-select-sm" />
                </div>
                <div class="col-md-3">
                    <asp:Label ID="lblProvince" runat="server" AssociatedControlID="ddlProvince"
                               CssClass="form-label small">Province</asp:Label>
                    <asp:DropDownList ID="ddlProvince" runat="server" CssClass="form-select form-select-sm" />
                </div>
                <div class="col-md-2">
                    <asp:Label ID="lblMaxRate" runat="server" AssociatedControlID="txtMaxRate"
                               CssClass="form-label small">Max rate</asp:Label>
                    <asp:TextBox ID="txtMaxRate" runat="server" CssClass="form-control form-control-sm" />
                </div>
                <div class="col-md-1 d-grid">
                    <asp:Button ID="btnSearch" runat="server" Text="Search"
                                CssClass="btn btn-sm btn-outline-primary" OnClick="btnSearch_Click" />
                </div>
            </div>
            <div class="form-check mt-2">
                <asp:CheckBox ID="chkIncludeInactive" runat="server" CssClass="form-check-input"
                              AutoPostBack="true" OnCheckedChanged="btnSearch_Click" />
                <asp:Label ID="lblIncludeInactive" runat="server" AssociatedControlID="chkIncludeInactive"
                           CssClass="form-check-label small">Show deactivated attractions</asp:Label>
            </div>
        </div>
    </div>

    <asp:GridView ID="gvAttractions" runat="server"
                  AutoGenerateColumns="false"
                  DataKeyNames="AttractionId"
                  CssClass="table table-striped table-hover align-middle"
                  EmptyDataText="No attractions match those filters."
                  GridLines="None"
                  AllowPaging="true" PageSize="20"
                  OnPageIndexChanging="gvAttractions_PageIndexChanging"
                  OnRowCommand="gvAttractions_RowCommand">
        <Columns>
            <asp:BoundField DataField="Name"         HeaderText="Name" />
            <asp:BoundField DataField="CategoryName" HeaderText="Category" />
            <asp:BoundField DataField="City"         HeaderText="City" />
            <asp:BoundField DataField="ProvinceName" HeaderText="Province" />
            <asp:BoundField DataField="Rate"         HeaderText="Rate"
                            DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
            <asp:BoundField DataField="Quantity"     HeaderText="Capacity / slot"
                            ItemStyle-CssClass="text-end" />

            <asp:TemplateField HeaderText="Active">
                <ItemTemplate>
                    <span class='badge <%# (bool)Eval("IsActive") ? "bg-success" : "bg-secondary" %>'>
                        <%# (bool)Eval("IsActive") ? "Active" : "Deactivated" %>
                    </span>
                </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="">
                <ItemTemplate>
                    <asp:HyperLink runat="server" CssClass="btn btn-sm btn-outline-secondary"
                                   NavigateUrl='<%# "~/Maintain/Attractions/Edit.aspx?id=" + Eval("AttractionId") %>'>Edit</asp:HyperLink>
                    <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-danger"
                                    CommandName="DeleteAttraction"
                                    CommandArgument='<%# Eval("AttractionId") %>'
                                    OnClientClick="return confirm('Delete this attraction?');">Delete</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>
