<%@ Page Title="Vehicle" Language="C#" MasterPageFile="~/Maintain/Maintain.master" AutoEventWireup="true"
         CodeBehind="Edit.aspx.cs" Inherits="NkanyeziTours.Maintain.Vehicles.Edit" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MaintainContent" runat="server">

    <h1 class="h3 mb-4"><asp:Literal ID="litHeading" runat="server" Text="Add a vehicle" /></h1>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="Vehicle"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <div class="card">
        <div class="card-body">
            <div class="row g-3">

                <div class="col-md-3">
                    <asp:Label ID="lblReg" runat="server" AssociatedControlID="txtReg"
                               CssClass="form-label">Registration <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtReg" ToolTip="The registration number. It has to be unique." runat="server" CssClass="form-control" MaxLength="20" />
                    <asp:RequiredFieldValidator ID="rfvReg" runat="server" ControlToValidate="txtReg"
                                                ValidationGroup="Vehicle" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The registration number is required." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblMake" runat="server" AssociatedControlID="txtMake"
                               CssClass="form-label">Make <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtMake" ToolTip="Manufacturer, for example: Toyota." runat="server" CssClass="form-control" MaxLength="50" />
                    <asp:RequiredFieldValidator ID="rfvMake" runat="server" ControlToValidate="txtMake"
                                                ValidationGroup="Vehicle" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Make is required." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblModel" runat="server" AssociatedControlID="txtModel"
                               CssClass="form-label">Model <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtModel" ToolTip="Model, for example: Corolla." runat="server" CssClass="form-control" MaxLength="50" />
                    <asp:RequiredFieldValidator ID="rfvModel" runat="server" ControlToValidate="txtModel"
                                                ValidationGroup="Vehicle" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Model is required." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblColour" runat="server" AssociatedControlID="txtColour"
                               CssClass="form-label">Colour <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtColour" ToolTip="Colour, so the client can pick it out in the car park." runat="server" CssClass="form-control" MaxLength="30" />
                    <asp:RequiredFieldValidator ID="rfvColour" runat="server" ControlToValidate="txtColour"
                                                ValidationGroup="Vehicle" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Colour is required." />
                </div>

                <div class="col-12">
                    <asp:Label ID="lblDescription" runat="server" AssociatedControlID="txtDescription"
                               CssClass="form-label">Description</asp:Label>
                    <asp:TextBox ID="txtDescription" ToolTip="A short description for the hire cards." runat="server" CssClass="form-control" MaxLength="200"
                                 placeholder="e.g. Compact hatchback, air conditioning, manual" />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblSeats" runat="server" AssociatedControlID="txtSeats"
                               CssClass="form-label">Seats <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtSeats" ToolTip="How many people it seats, driver included." runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator ID="rfvSeats" runat="server" ControlToValidate="txtSeats"
                                                ValidationGroup="Vehicle" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Seats is required." />
                    <asp:RangeValidator ID="rvSeats" runat="server" ControlToValidate="txtSeats"
                                        ValidationGroup="Vehicle" Display="Dynamic"
                                        Type="Integer" MinimumValue="1" MaximumValue="100"
                                        CssClass="text-danger small"
                                        ErrorMessage="Seats must be a whole number of at least one." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblDepot" runat="server" AssociatedControlID="txtDepot"
                               CssClass="form-label">Depot town <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtDepot" ToolTip="The branch the car lives at." runat="server" CssClass="form-control" MaxLength="80"
                                 placeholder="e.g. Cape Town" />
                    <asp:RequiredFieldValidator ID="rfvDepot" runat="server" ControlToValidate="txtDepot"
                                                ValidationGroup="Vehicle" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Depot town is required." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblRegion" runat="server" AssociatedControlID="ddlRegion"
                               CssClass="form-label">Region <span class="text-danger">*</span></asp:Label>
                    <asp:DropDownList ID="ddlRegion" ToolTip="The province this car works in. Clients are only offered cars in the province of their leg, because cars are not moved between provinces." runat="server" CssClass="form-select" />
                    <div class="form-text">
                        Clients are only offered cars from the region their leg is in.
                        We do not move cars between provinces.
                    </div>
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblPrice" runat="server" AssociatedControlID="txtPrice"
                               CssClass="form-label">Per day <span class="text-danger">*</span></asp:Label>
                    <div class="input-group">
                        <span class="input-group-text">R</span>
                        <asp:TextBox ID="txtPrice" ToolTip="What one day of hire costs." runat="server" CssClass="form-control" />
                    </div>
                    <asp:RequiredFieldValidator ID="rfvPrice" runat="server" ControlToValidate="txtPrice"
                                                ValidationGroup="Vehicle" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The daily price is required." />

                    <asp:RangeValidator ID="rvPrice" runat="server" ControlToValidate="txtPrice"
                                        ValidationGroup="Vehicle" Display="Dynamic"
                                        Type="Currency" MinimumValue="0.01" MaximumValue="99999999"
                                        CssClass="text-danger small"
                                        ErrorMessage="The daily price must be greater than zero." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblInclKm" runat="server" AssociatedControlID="txtInclKm"
                               CssClass="form-label">Km included per day <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtInclKm" ToolTip="Kilometres included in each day of hire before the extra rate kicks in." runat="server" CssClass="form-control" Text="200" />
                    <asp:RequiredFieldValidator ID="rfvInclKm" runat="server" ControlToValidate="txtInclKm"
                                                ValidationGroup="Vehicle" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Included km is required." />

                    <asp:RangeValidator ID="rvInclKm" runat="server" ControlToValidate="txtInclKm"
                                        ValidationGroup="Vehicle" Display="Dynamic"
                                        Type="Integer" MinimumValue="1" MaximumValue="100000"
                                        CssClass="text-danger small"
                                        ErrorMessage="Included km must be a whole number of at least one." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblExtraKm" runat="server" AssociatedControlID="txtExtraKm"
                               CssClass="form-label">Rate per extra km <span class="text-danger">*</span></asp:Label>
                    <div class="input-group">
                        <span class="input-group-text">R</span>
                        <asp:TextBox ID="txtExtraKm" ToolTip="What each kilometre over the daily allowance costs." runat="server" CssClass="form-control" Text="0.00" />
                    </div>
                    <asp:RequiredFieldValidator ID="rfvExtraKm" runat="server" ControlToValidate="txtExtraKm"
                                                ValidationGroup="Vehicle" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The extra km rate is required." />

                    <asp:RangeValidator ID="rvExtraKm" runat="server" ControlToValidate="txtExtraKm"
                                        ValidationGroup="Vehicle" Display="Dynamic"
                                        Type="Currency" MinimumValue="0" MaximumValue="99999999"
                                        CssClass="text-danger small"
                                        ErrorMessage="The extra km rate cannot be negative." />
                </div>

                <div class="col-12">
                    <asp:Label ID="lblPhoto" runat="server" AssociatedControlID="fuPhoto"
                               CssClass="form-label">Photo</asp:Label>
                    <asp:Panel ID="pnlCurrentPhoto" runat="server" Visible="false" CssClass="mb-2 d-flex flex-wrap align-items-center gap-3">
                        <asp:Image ID="imgCurrentPhoto" runat="server" CssClass="rounded border"
                                   Style="max-height:120px;max-width:200px;object-fit:cover;"
                                   AlternateText="Current photo" />
                        <div class="form-check">
                            <asp:CheckBox ID="chkRemovePhoto" runat="server" CssClass="form-check-input"
                                          ToolTip="Tick to remove the current photo when you save." />
                            <asp:Label ID="lblRemovePhoto" runat="server" AssociatedControlID="chkRemovePhoto"
                                       CssClass="form-check-label">Remove this photo</asp:Label>
                        </div>
                    </asp:Panel>
                    <asp:FileUpload ID="fuPhoto" runat="server" CssClass="form-control"
                                    accept=".jpg,.jpeg,.png,.gif,.webp"
                                    ToolTip="The photo clients see on the car rental card. Choosing a new photo replaces the current one." />
                    <div class="form-text">JPG, PNG, GIF or WEBP, up to 5 MB.</div>
                </div>

            </div>

            <hr class="my-4" />

            <div class="d-flex justify-content-end gap-2">
                <asp:HyperLink ID="lnkCancel" ToolTip="Goes back to the fleet list without saving." runat="server" CssClass="btn btn-outline-secondary"
                               NavigateUrl="~/Maintain/Vehicles/Default.aspx">Cancel</asp:HyperLink>
                <asp:Button ID="btnSave" ToolTip="Saves the car." runat="server" Text="Save" CssClass="btn btn-primary"
                            ValidationGroup="Vehicle" OnClick="btnSave_Click" />
            </div>
        </div>
    </div>

</asp:Content>
