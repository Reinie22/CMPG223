<%@ Page Title="Manage Accommodation" Language="C#" AutoEventWireup="true" CodeBehind="AccommodationList.aspx.cs" Inherits="NkanyeziTours.Maintain.Accommodation.AccommodationList" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Manage Accommodation</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Accommodation</h2>

            <asp:Label ID="lblMessage" runat="server" ForeColor="Green" Text="" />

            <p>
                <asp:HyperLink ID="lnkAdd" runat="server" NavigateUrl="~/Maintain/Accommodation/AccommodationForm.aspx">+ Add New Accommodation</asp:HyperLink>
            </p>

            <table>
                <tr>
                    <td>Name</td>
                    <td><asp:TextBox ID="txtSearchName" runat="server" /></td>
                    <td>Type</td>
                    <td>
                        <asp:DropDownList ID="ddlSearchType" runat="server">
                            <asp:ListItem Text="All types" Value="" />
                            <asp:ListItem Text="Hotel" Value="Hotel" />
                            <asp:ListItem Text="Guest House" Value="Guest House" />
                            <asp:ListItem Text="Lodge" Value="Lodge" />
                            <asp:ListItem Text="Resort" Value="Resort" />
                            <asp:ListItem Text="Bed &amp; Breakfast" Value="Bed &amp; Breakfast" />
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>Location (Province)</td>
                    <td>
                        <!-- Bound in code-behind - CONFIRM LookupRepository method name -->
                        <asp:DropDownList ID="ddlSearchProvince" runat="server" />
                    </td>
                    <td>Max Price / Night (R)</td>
                    <td><asp:TextBox ID="txtSearchMaxPrice" runat="server" /></td>
                </tr>
                <tr>
                    <td colspan="4">
                        <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" />
                        <asp:Button ID="btnClearSearch" runat="server" Text="Clear" CausesValidation="false" OnClick="btnClearSearch_Click" />
                    </td>
                </tr>
            </table>

            <asp:GridView ID="gvAccommodations" runat="server"
                AutoGenerateColumns="false"
                DataKeyNames="AccommodationId"
                OnRowCommand="gvAccommodations_RowCommand"
                CellPadding="4" GridLines="Horizontal"
                EmptyDataText="No accommodation records match your search.">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:BoundField DataField="Type" HeaderText="Type" />
                    <asp:BoundField DataField="Location" HeaderText="Location" />
                    <asp:BoundField DataField="StarDisplay" HeaderText="Stars" />
                    <asp:BoundField DataField="PricePerNight" HeaderText="Price / Night" DataFormatString="{0:C}" />
                    <asp:BoundField DataField="Capacity" HeaderText="Capacity" />
                    <asp:BoundField DataField="RoomCount" HeaderText="Rooms" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <%# (bool)Eval("IsActive") ? "Active" : "Inactive" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkEdit" runat="server"
                                CommandName="EditAccommodation"
                                CommandArgument='<%# Eval("AccommodationId") %>'>Edit</asp:LinkButton>

                            <asp:LinkButton ID="lnkDelete" runat="server"
                                Visible='<%# (bool)Eval("IsActive") %>'
                                CommandName="DeleteAccommodation"
                                CommandArgument='<%# Eval("AccommodationId") %>'
                                OnClientClick="return confirm('Delete this accommodation? If it has bookings it will be deactivated instead.');">
                                &nbsp;|&nbsp;Delete</asp:LinkButton>

                            <asp:LinkButton ID="lnkReactivate" runat="server"
                                Visible='<%# !(bool)Eval("IsActive") %>'
                                CommandName="ReactivateAccommodation"
                                CommandArgument='<%# Eval("AccommodationId") %>'>
                                &nbsp;|&nbsp;Reactivate</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>
