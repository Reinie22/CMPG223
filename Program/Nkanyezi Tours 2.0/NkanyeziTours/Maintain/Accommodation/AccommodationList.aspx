<%@ Page Title="Manage Accommodation" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="AccommodationList.aspx.cs" Inherits="NkanyeziTours.Maintain.Accommodation.AccommodationList" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="d-flex justify-content-between align-items-center mb-3">
        <h1 class="h3 mb-0">Accommodation</h1>
        <asp:HyperLink ID="lnkAdd" runat="server" CssClass="btn btn-primary"
                       NavigateUrl="~/Maintain/Accommodation/AccommodationForm.aspx">+ Add New Accommodation</asp:HyperLink>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <div class="card mb-3">
        <div class="card-body">
            <asp:Panel ID="pnlSearch" runat="server" DefaultButton="btnSearch">
                <div class="row g-3">
                    <div class="col-md-4">
                        <asp:Label ID="lblSearchName" runat="server" AssociatedControlID="txtSearchName"
                                   CssClass="form-label">Name</asp:Label>
                        <asp:TextBox ID="txtSearchName" runat="server" CssClass="form-control" placeholder="Search by name" />
                    </div>
                    <div class="col-md-4">
                        <asp:Label ID="lblSearchProvince" runat="server" AssociatedControlID="ddlSearchProvince"
                                   CssClass="form-label">Province</asp:Label>
                        <asp:DropDownList ID="ddlSearchProvince" runat="server" CssClass="form-select" />
                    </div>
                    <div class="col-md-4">
                        <asp:Label ID="lblSearchType" runat="server" AssociatedControlID="ddlSearchType"
                                   CssClass="form-label">Type</asp:Label>
                        <asp:DropDownList ID="ddlSearchType" runat="server" CssClass="form-select">
                            <asp:ListItem Text="All types" Value="" />
                            <asp:ListItem Text="Hotel" Value="Hotel" />
                            <asp:ListItem Text="Guest House" Value="Guest House" />
                            <asp:ListItem Text="Lodge" Value="Lodge" />
                            <asp:ListItem Text="Resort" Value="Resort" />
                            <asp:ListItem Text="Bed &amp; Breakfast" Value="Bed &amp; Breakfast" />
                        </asp:DropDownList>
                    </div>
                    <div class="col-md-4">
                        <asp:Label ID="lblSearchMinStars" runat="server" AssociatedControlID="ddlSearchMinStars"
                                   CssClass="form-label">Min Star Rating</asp:Label>
                        <asp:DropDownList ID="ddlSearchMinStars" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Any" Value="" />
                            <asp:ListItem Text="1+" Value="1" />
                            <asp:ListItem Text="2+" Value="2" />
                            <asp:ListItem Text="3+" Value="3" />
                            <asp:ListItem Text="4+" Value="4" />
                            <asp:ListItem Text="5" Value="5" />
                        </asp:DropDownList>
                    </div>
                    <div class="col-md-4">
                        <asp:Label ID="lblSearchMaxPrice" runat="server" AssociatedControlID="txtSearchMaxPrice"
                                   CssClass="form-label">Max Price / Night</asp:Label>
                        <div class="input-group">
                            <span class="input-group-text">R</span>
                            <asp:TextBox ID="txtSearchMaxPrice" runat="server" CssClass="form-control" placeholder="e.g. 2000" />
                        </div>
                        <asp:CompareValidator ID="cvSearchMaxPrice" runat="server" ControlToValidate="txtSearchMaxPrice"
                                              Type="Currency" Operator="GreaterThanEqual" ValueToCompare="0"
                                              Display="Dynamic" CssClass="text-danger small"
                                              ErrorMessage="Enter a valid price." />
                    </div>
                    <div class="col-md-4 d-flex align-items-end gap-2">
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                        <asp:Button ID="btnClearSearch" runat="server" Text="Clear" CssClass="btn btn-outline-secondary"
                                    CausesValidation="false" OnClick="btnClearSearch_Click" />
                    </div>
                </div>
            </asp:Panel>
        </div>
    </div>

    <div class="card">
        <div class="card-body">
            <asp:GridView ID="gvAccommodations" runat="server"
                CssClass="table table-striped table-hover align-middle"
                AutoGenerateColumns="false"
                DataKeyNames="AccommodationId"
                OnRowCommand="gvAccommodations_RowCommand"
                EmptyDataText="No accommodation records match your search.">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:BoundField DataField="Type" HeaderText="Type" />
                    <asp:BoundField DataField="Location" HeaderText="Location" />
                    <asp:BoundField DataField="StarDisplay" HeaderText="Stars" />
                    <asp:BoundField DataField="PricePerNight" HeaderText="Price / Night" DataFormatString="R{0:N2}" />
                    <asp:BoundField DataField="Capacity" HeaderText="Capacity" />
                    <asp:BoundField DataField="RoomCount" HeaderText="Rooms" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class='<%# (bool)Eval("IsActive") ? "badge text-bg-success" : "badge text-bg-secondary" %>'>
                                <%# (bool)Eval("IsActive") ? "Active" : "Inactive" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkEdit" runat="server" CssClass="btn btn-sm btn-outline-primary"
                                CommandName="EditAccommodation"
                                CommandArgument='<%# Eval("AccommodationId") %>'>Edit</asp:LinkButton>

                            <%-- Only one of Delete / Reactivate makes sense at a time --%>
                            <asp:LinkButton ID="lnkDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                Visible='<%# (bool)Eval("IsActive") %>'
                                CommandName="DeleteAccommodation"
                                CommandArgument='<%# Eval("AccommodationId") %>'
                                OnClientClick="return confirm('Delete this accommodation? If it has bookings it will be deactivated instead.');">
                                Delete</asp:LinkButton>

                            <asp:LinkButton ID="lnkReactivate" runat="server" CssClass="btn btn-sm btn-outline-success"
                                Visible='<%# !(bool)Eval("IsActive") %>'
                                CommandName="ReactivateAccommodation"
                                CommandArgument='<%# Eval("AccommodationId") %>'>
                                Reactivate</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

</asp:Content>
