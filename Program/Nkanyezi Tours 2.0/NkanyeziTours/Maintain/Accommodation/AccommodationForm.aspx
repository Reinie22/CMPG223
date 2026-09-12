<%@ Page Title="Accommodation" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="AccommodationForm.aspx.cs" Inherits="NkanyeziTours.Maintain.Accommodation.AccommodationForm" %>

<%--
    ADD / EDIT ACCOMMODATION - mirrors Maintain/Attractions/Edit.aspx.

    One page does both jobs. With no id in the address bar it adds a new
    record; with ?id=7 it loads number 7 and saves over it.
--%>
<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-3"><asp:Literal ID="litHeading" runat="server" Text="Add Accommodation" /></h1>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="Accommodation"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <asp:Panel ID="pnlForm" runat="server">
        <div class="card">
            <div class="card-body">
                <div class="row g-3">

                    <div class="col-md-6">
                        <asp:Label ID="lblName" runat="server" AssociatedControlID="txtName"
                                   CssClass="form-label">Name <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="150" />
                        <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName"
                                                    ValidationGroup="Accommodation" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Name is required." />
                    </div>

                    <div class="col-md-3">
                        <asp:Label ID="lblType" runat="server" AssociatedControlID="ddlType"
                                   CssClass="form-label">Type</asp:Label>
                        <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Hotel" Value="Hotel" />
                            <asp:ListItem Text="Guest House" Value="Guest House" />
                            <asp:ListItem Text="Lodge" Value="Lodge" />
                            <asp:ListItem Text="Resort" Value="Resort" />
                            <asp:ListItem Text="Bed &amp; Breakfast" Value="Bed &amp; Breakfast" />
                        </asp:DropDownList>
                    </div>

                    <div class="col-md-3">
                        <asp:Label ID="lblProvince" runat="server" AssociatedControlID="ddlProvince"
                                   CssClass="form-label">Province</asp:Label>
                        <asp:DropDownList ID="ddlProvince" runat="server" CssClass="form-select" />
                    </div>

                    <div class="col-12">
                        <asp:Label ID="lblDescription" runat="server" AssociatedControlID="txtDescription"
                                   CssClass="form-label">Description</asp:Label>
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control"
                                     TextMode="MultiLine" Rows="3" MaxLength="1000" />
                    </div>

                    <div class="col-md-6">
                        <asp:Label ID="lblAddress" runat="server" AssociatedControlID="txtAddress"
                                   CssClass="form-label">Address</asp:Label>
                        <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" MaxLength="200" />
                    </div>

                    <div class="col-md-6">
                        <asp:Label ID="lblCity" runat="server" AssociatedControlID="txtCity"
                                   CssClass="form-label">City</asp:Label>
                        <asp:TextBox ID="txtCity" runat="server" CssClass="form-control" MaxLength="80" />
                    </div>

                    <div class="col-md-3">
                        <asp:Label ID="lblStarRating" runat="server" AssociatedControlID="ddlStarRating"
                                   CssClass="form-label">Star Rating</asp:Label>
                        <asp:DropDownList ID="ddlStarRating" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Not rated" Value="" />
                            <asp:ListItem Text="1" Value="1" />
                            <asp:ListItem Text="2" Value="2" />
                            <asp:ListItem Text="3" Value="3" />
                            <asp:ListItem Text="4" Value="4" />
                            <asp:ListItem Text="5" Value="5" />
                        </asp:DropDownList>
                    </div>

                    <div class="col-md-3">
                        <asp:Label ID="lblPricePerNight" runat="server" AssociatedControlID="txtPricePerNight"
                                   CssClass="form-label">Price per night <span class="text-danger">*</span></asp:Label>
                        <div class="input-group">
                            <span class="input-group-text">R</span>
                            <asp:TextBox ID="txtPricePerNight" runat="server" CssClass="form-control" />
                        </div>
                        <asp:RequiredFieldValidator ID="rfvPrice" runat="server" ControlToValidate="txtPricePerNight"
                                                    ValidationGroup="Accommodation" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Price is required." />
                        <%--
                            The database also has CHECK (PricePerNight >= 0). This
                            validator is so the user gets a friendly message instead
                            of a SqlException - it is not the real guarantee.
                        --%>
                        <asp:RangeValidator ID="rvPrice" runat="server" ControlToValidate="txtPricePerNight"
                                            ValidationGroup="Accommodation" Display="Dynamic"
                                            Type="Currency" MinimumValue="0.01" MaximumValue="99999999"
                                            CssClass="text-danger small"
                                            ErrorMessage="Price must be a positive number." />
                    </div>

                    <div class="col-md-3">
                        <asp:Label ID="lblCapacity" runat="server" AssociatedControlID="txtCapacity"
                                   CssClass="form-label">Capacity (guests) <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtCapacity" runat="server" CssClass="form-control" />
                        <asp:RequiredFieldValidator ID="rfvCapacity" runat="server" ControlToValidate="txtCapacity"
                                                    ValidationGroup="Accommodation" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Capacity is required." />
                        <asp:RangeValidator ID="rvCapacity" runat="server" ControlToValidate="txtCapacity"
                                            ValidationGroup="Accommodation" Display="Dynamic"
                                            Type="Integer" MinimumValue="1" MaximumValue="1000000"
                                            CssClass="text-danger small"
                                            ErrorMessage="Capacity must be a whole number greater than zero." />
                    </div>

                    <div class="col-12">
                        <asp:Label ID="lblAmenities" runat="server" AssociatedControlID="txtAmenities"
                                   CssClass="form-label">Amenities</asp:Label>
                        <asp:TextBox ID="txtAmenities" runat="server" CssClass="form-control"
                                     TextMode="MultiLine" Rows="2" MaxLength="500"
                                     placeholder="e.g. WiFi, Pool, Breakfast included" />
                    </div>

                </div>

                <hr class="my-4" />

                <div class="d-flex justify-content-end gap-2">
                    <asp:HyperLink ID="lnkCancel" runat="server" CssClass="btn btn-outline-secondary"
                                   NavigateUrl="~/Maintain/Accommodation/AccommodationList.aspx">Cancel</asp:HyperLink>
                    <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary"
                                ValidationGroup="Accommodation" OnClick="btnSave_Click" />
                </div>
            </div>
        </div>
    </asp:Panel>

</asp:Content>
