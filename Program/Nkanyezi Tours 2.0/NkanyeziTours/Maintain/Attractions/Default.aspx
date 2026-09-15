<%@ Page Title="Attractions" Language="C#" MasterPageFile="~/Maintain/Maintain.master" AutoEventWireup="true"
         CodeBehind="Default.aspx.cs" Inherits="NkanyeziTours.Maintain.Attractions.Default" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MaintainContent" runat="server">

    <div class="d-flex justify-content-between align-items-center mb-3">
        <div>
            <h1 class="h3 mb-0">Attractions</h1>
            <p class="text-muted mb-0 small">Places clients can visit on a trip</p>
        </div>
        <asp:HyperLink ID="lnkAdd" ToolTip="Capture a new activity clients can add to a trip." runat="server" CssClass="btn btn-primary"
                       NavigateUrl="~/Maintain/Attractions/Edit.aspx">Add attraction</asp:HyperLink>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <div class="card mb-3">
        <div class="card-body">
            <div class="row g-2 align-items-end">
                <div class="col-md-3">
                    <asp:Label ID="lblName" runat="server" AssociatedControlID="txtName"
                               CssClass="form-label small">Name contains</asp:Label>
                    <asp:TextBox ID="txtName" ToolTip="Part of the name is enough." runat="server" CssClass="form-control form-control-sm" />
                </div>
                <div class="col-md-3">
                    <asp:Label ID="lblCategory" runat="server" AssociatedControlID="ddlCategory"
                               CssClass="form-label small">Category</asp:Label>
                    <asp:DropDownList ID="ddlCategory" ToolTip="Show only one kind of activity." runat="server" CssClass="form-select form-select-sm" />
                </div>
                <div class="col-md-3">
                    <asp:Label ID="lblProvince" runat="server" AssociatedControlID="ddlProvince"
                               CssClass="form-label small">Province</asp:Label>
                    <asp:DropDownList ID="ddlProvince" ToolTip="Show only activities in one province." runat="server" CssClass="form-select form-select-sm" />
                </div>
                <div class="col-md-2">
                    <asp:Label ID="lblMaxRate" runat="server" AssociatedControlID="txtMaxRate"
                               CssClass="form-label small">Max rate</asp:Label>
                    <asp:TextBox ID="txtMaxRate" ToolTip="Hide anything that costs more than this per person." runat="server" CssClass="form-control form-control-sm" />
                </div>
                <div class="col-md-1 d-grid gap-1">
                    <asp:Button ID="btnSearch" ToolTip="Applies the filters above." runat="server" Text="Search"
                                CssClass="btn btn-sm btn-outline-primary" OnClick="btnSearch_Click" />
                    <asp:Button ID="btnClear" ToolTip="Clears every filter and shows the whole list again." runat="server" Text="Clear"
                                CssClass="btn btn-sm btn-outline-secondary" CausesValidation="false" OnClick="btnClear_Click" />
                </div>
            </div>
            <div class="form-check mt-2">
                <asp:CheckBox ID="chkIncludeInactive" ToolTip="Also show activities that have been withdrawn from sale." runat="server" CssClass="form-check-input"
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
                    <asp:LinkButton ID="lnkDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                    ToolTip="Removes the activity. If clients have booked or chosen it, it is deactivated instead."
                                    CommandName="DeleteAttraction"
                                    CommandArgument='<%# Eval("AttractionId") %>'
                                    Visible='<%# (bool)Eval("IsActive") %>'
                                    data-confirm="Delete this attraction?">Delete</asp:LinkButton>
                    <asp:LinkButton ID="lnkReactivate" runat="server" CssClass="btn btn-sm btn-outline-success"
                                    ToolTip="Puts the activity back on sale."
                                    CommandName="ReactivateAttraction"
                                    CommandArgument='<%# Eval("AttractionId") %>'
                                    Visible='<%# !(bool)Eval("IsActive") %>'>Reactivate</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>
