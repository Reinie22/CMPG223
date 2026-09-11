<%@ Page Title="Accommodation" Language="C#" AutoEventWireup="true" CodeBehind="AccommodationForm.aspx.cs" Inherits="NkanyeziTours.Maintain.Accommodation.AccommodationForm" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Accommodation</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2><asp:Literal ID="litFormTitle" runat="server" Text="Add Accommodation" /></h2>

            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" Text="" />

            <asp:ValidationSummary ID="valSummary" runat="server" ForeColor="Red" />

            <table>
                <tr>
                    <td>Name</td>
                    <td>
                        <asp:TextBox ID="txtName" runat="server" />
                        <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName" ErrorMessage="Name is required." Display="Dynamic" ForeColor="Red" />
                    </td>
                </tr>
                <tr>
                    <td>Description</td>
                    <td>
                        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3" Columns="40" />
                    </td>
                </tr>
                <tr>
                    <td>Type</td>
                    <td>
                        <asp:DropDownList ID="ddlType" runat="server">
                            <asp:ListItem Text="Hotel" Value="Hotel" />
                            <asp:ListItem Text="Guest House" Value="Guest House" />
                            <asp:ListItem Text="Lodge" Value="Lodge" />
                            <asp:ListItem Text="Resort" Value="Resort" />
                            <asp:ListItem Text="Bed &amp; Breakfast" Value="Bed &amp; Breakfast" />
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>Address</td>
                    <td>
                        <asp:TextBox ID="txtAddress" runat="server" />
                        <asp:RequiredFieldValidator ID="rfvAddress" runat="server" ControlToValidate="txtAddress" ErrorMessage="Address is required." Display="Dynamic" ForeColor="Red" />
                    </td>
                </tr>
                <tr>
                    <td>City</td>
                    <td>
                        <asp:TextBox ID="txtCity" runat="server" />
                        <asp:RequiredFieldValidator ID="rfvCity" runat="server" ControlToValidate="txtCity" ErrorMessage="City is required." Display="Dynamic" ForeColor="Red" />
                    </td>
                </tr>
                <tr>
                    <td>Province</td>
                    <td>
                        <!-- Bound in code-behind - CONFIRM LookupRepository method name -->
                        <asp:DropDownList ID="ddlProvince" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td>Star Rating</td>
                    <td>
                        <asp:DropDownList ID="ddlStarRating" runat="server">
                            <asp:ListItem Text="Not rated" Value="" />
                            <asp:ListItem Text="1" Value="1" />
                            <asp:ListItem Text="2" Value="2" />
                            <asp:ListItem Text="3" Value="3" />
                            <asp:ListItem Text="4" Value="4" />
                            <asp:ListItem Text="5" Value="5" />
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>Price Per Night (R)</td>
                    <td>
                        <asp:TextBox ID="txtPricePerNight" runat="server" />
                        <asp:RequiredFieldValidator ID="rfvPrice" runat="server" ControlToValidate="txtPricePerNight" ErrorMessage="Price is required." Display="Dynamic" ForeColor="Red" />
                        <asp:CompareValidator ID="cvPrice" runat="server" ControlToValidate="txtPricePerNight" Type="Currency" Operator="GreaterThanEqual" ValueToCompare="0" ErrorMessage="Price must be a positive number." Display="Dynamic" ForeColor="Red" />
                    </td>
                </tr>
                <tr>
                    <td>Capacity (guests)</td>
                    <td>
                        <asp:TextBox ID="txtCapacity" runat="server" />
                        <asp:RequiredFieldValidator ID="rfvCapacity" runat="server" ControlToValidate="txtCapacity" ErrorMessage="Capacity is required." Display="Dynamic" ForeColor="Red" />
                        <asp:CompareValidator ID="cvCapacity" runat="server" ControlToValidate="txtCapacity" Type="Integer" Operator="GreaterThan" ValueToCompare="0" ErrorMessage="Capacity must be a positive whole number." Display="Dynamic" ForeColor="Red" />
                    </td>
                </tr>
                <tr>
                    <td>Amenities</td>
                    <td>
                        <asp:TextBox ID="txtAmenities" runat="server" TextMode="MultiLine" Rows="2" Columns="40" placeholder="e.g. WiFi, Pool, Breakfast included" />
                    </td>
                </tr>
            </table>

            <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" />
            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>
    </form>
</body>
</html>
