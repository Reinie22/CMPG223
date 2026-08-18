<%@ Page Title="Attraction" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="Edit.aspx.cs" Inherits="NkanyeziTours.Maintain.Attractions.Edit" %>

<%--
    ADD / EDIT ATTRACTION - the form page to copy.

    One page does both jobs. With no id in the address bar it adds a new
    record; with ?id=7 it loads number 7 and saves over it. Two nearly
    identical pages would be two places to fix every bug.
--%>
<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-3"><asp:Literal ID="litHeading" runat="server" /></h1>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="Attraction"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <div class="card">
        <div class="card-body">
            <div class="row g-3">

                <div class="col-md-6">
                    <asp:Label ID="lblName" runat="server" AssociatedControlID="txtName"
                               CssClass="form-label">Name <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="120" />
                    <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName"
                                                ValidationGroup="Attraction" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Name is required." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblCategory" runat="server" AssociatedControlID="ddlCategory"
                               CssClass="form-label">Category</asp:Label>
                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" />
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
                    <asp:Label ID="lblRate" runat="server" AssociatedControlID="txtRate"
                               CssClass="form-label">Rate <span class="text-danger">*</span></asp:Label>
                    <div class="input-group">
                        <span class="input-group-text">R</span>
                        <asp:TextBox ID="txtRate" runat="server" CssClass="form-control" />
                    </div>
                    <asp:RequiredFieldValidator ID="rfvRate" runat="server" ControlToValidate="txtRate"
                                                ValidationGroup="Attraction" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Rate is required." />
                    <%--
                        The database also has CHECK (Rate > 0). This validator is
                        so the user gets a friendly message instead of a
                        SqlException - it is not the real guarantee.
                    --%>
                    <asp:RangeValidator ID="rvRate" runat="server" ControlToValidate="txtRate"
                                        ValidationGroup="Attraction" Display="Dynamic"
                                        Type="Currency" MinimumValue="0.01" MaximumValue="99999999"
                                        CssClass="text-danger small"
                                        ErrorMessage="Rate must be a number greater than zero." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblQuantity" runat="server" AssociatedControlID="txtQuantity"
                               CssClass="form-label">Capacity per time slot <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" />
                    <div class="form-text">How many people the attraction takes in one slot</div>
                    <asp:RequiredFieldValidator ID="rfvQuantity" runat="server" ControlToValidate="txtQuantity"
                                                ValidationGroup="Attraction" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Capacity is required." />
                    <asp:RangeValidator ID="rvQuantity" runat="server" ControlToValidate="txtQuantity"
                                        ValidationGroup="Attraction" Display="Dynamic"
                                        Type="Integer" MinimumValue="1" MaximumValue="1000000"
                                        CssClass="text-danger small"
                                        ErrorMessage="Capacity must be a whole number greater than zero." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblUnit" runat="server" AssociatedControlID="txtUnit"
                               CssClass="form-label">Unit of measure</asp:Label>
                    <asp:TextBox ID="txtUnit" runat="server" CssClass="form-control"
                                 MaxLength="30" placeholder="per person" />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblDuration" runat="server" AssociatedControlID="txtDuration"
                               CssClass="form-label">Duration (minutes)</asp:Label>
                    <asp:TextBox ID="txtDuration" runat="server" CssClass="form-control" />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblOpens" runat="server" AssociatedControlID="txtOpens"
                               CssClass="form-label">Opens at</asp:Label>
                    <asp:TextBox ID="txtOpens" runat="server" CssClass="form-control"
                                 TextMode="Time" placeholder="08:00" />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblCloses" runat="server" AssociatedControlID="txtCloses"
                               CssClass="form-label">Closes at</asp:Label>
                    <asp:TextBox ID="txtCloses" runat="server" CssClass="form-control"
                                 TextMode="Time" placeholder="17:00" />
                </div>

                <div class="col-md-6">
                    <asp:Label ID="lblSeason" runat="server" AssociatedControlID="txtSeason"
                               CssClass="form-label">Best season</asp:Label>
                    <asp:TextBox ID="txtSeason" runat="server" CssClass="form-control" MaxLength="50" />
                </div>

            </div>

            <hr class="my-4" />

            <div class="d-flex justify-content-end gap-2">
                <asp:HyperLink ID="lnkCancel" runat="server" CssClass="btn btn-outline-secondary"
                               NavigateUrl="~/Maintain/Attractions/Default.aspx">Cancel</asp:HyperLink>
                <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary"
                            ValidationGroup="Attraction" OnClick="btnSave_Click" />
            </div>
        </div>
    </div>

</asp:Content>
