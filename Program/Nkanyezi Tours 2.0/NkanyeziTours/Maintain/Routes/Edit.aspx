<%@ Page Title="Route" Language="C#" MasterPageFile="~/Maintain/Maintain.master" AutoEventWireup="true"
         CodeBehind="Edit.aspx.cs" Inherits="NkanyeziTours.Maintain.Routes.Edit" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MaintainContent" runat="server">

    <h1 class="h3 mb-4"><asp:Literal ID="litHeading" runat="server" Text="Add a route" /></h1>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="Route"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <div class="card">
        <div class="card-body">
            <div class="row g-3">

                <div class="col-md-3">
                    <asp:Label ID="lblType" runat="server" AssociatedControlID="ddlType"
                               CssClass="form-label">Kind <span class="text-danger">*</span></asp:Label>
                    <asp:DropDownList ID="ddlType" ToolTip="Whether this is a bus or a flight." runat="server" CssClass="form-select">
                        <asp:ListItem Text="Flight" Value="Flight" />
                        <asp:ListItem Text="Bus" Value="Shuttle" />
                    </asp:DropDownList>
                </div>

                <div class="col-md-5">
                    <asp:Label ID="lblCarrier" runat="server" AssociatedControlID="txtCarrier"
                               CssClass="form-label">Carrier <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtCarrier" ToolTip="Who operates it, for example: Intercape." runat="server" CssClass="form-control" MaxLength="80"
                                 placeholder="airline or bus company" />
                    <asp:RequiredFieldValidator ID="rfvCarrier" runat="server" ControlToValidate="txtCarrier"
                                                ValidationGroup="Route" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The carrier is required." />
                </div>

                <div class="col-md-4">
                    <asp:Label ID="lblCode" runat="server" AssociatedControlID="txtCode"
                               CssClass="form-label">Flight or service number</asp:Label>
                    <asp:TextBox ID="txtCode" ToolTip="The operator’s own code for this service." runat="server" CssClass="form-control" MaxLength="20"
                                 placeholder="e.g. SA 322" />
                </div>

                <div class="col-md-6">
                    <asp:Label ID="lblFrom" runat="server" AssociatedControlID="txtFrom"
                               CssClass="form-label">Departure city <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtFrom" ToolTip="Town or city it leaves from." runat="server" CssClass="form-control" MaxLength="80" />
                    <asp:RequiredFieldValidator ID="rfvFrom" runat="server" ControlToValidate="txtFrom"
                                                ValidationGroup="Route" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The departure city is required." />
                </div>

                <div class="col-md-6">
                    <asp:Label ID="lblTo" runat="server" AssociatedControlID="txtTo"
                               CssClass="form-label">Arrival city <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtTo" ToolTip="Town or city it arrives at." runat="server" CssClass="form-control" MaxLength="80" />
                    <asp:RequiredFieldValidator ID="rfvTo" runat="server" ControlToValidate="txtTo"
                                                ValidationGroup="Route" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The arrival city is required." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblDeparts" runat="server" AssociatedControlID="txtDeparts"
                               CssClass="form-label">Departs</asp:Label>
                    <asp:TextBox ID="txtDeparts" ToolTip="Departure time, as 24-hour time." runat="server" CssClass="form-control" TextMode="Time" />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblArrives" runat="server" AssociatedControlID="txtArrives"
                               CssClass="form-label">Arrives</asp:Label>
                    <asp:TextBox ID="txtArrives" ToolTip="Arrival time, as 24-hour time." runat="server" CssClass="form-control" TextMode="Time" />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblDuration" runat="server" AssociatedControlID="txtDuration"
                               CssClass="form-label">Duration (minutes)</asp:Label>
                    <asp:TextBox ID="txtDuration" ToolTip="How long the journey takes, in minutes." runat="server" CssClass="form-control" />
                    <asp:RangeValidator ID="rvDuration" runat="server" ControlToValidate="txtDuration"
                                        ValidationGroup="Route" Display="Dynamic"
                                        Type="Integer" MinimumValue="1" MaximumValue="10080"
                                        CssClass="text-danger small"
                                        ErrorMessage="Duration must be a whole number of minutes." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblSeats" runat="server" AssociatedControlID="txtSeats"
                               CssClass="form-label">Seats <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtSeats" ToolTip="How many seats there are to sell on each running of this route." runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator ID="rfvSeats" runat="server" ControlToValidate="txtSeats"
                                                ValidationGroup="Route" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The seat count is required." />

                    <asp:RangeValidator ID="rvSeats" runat="server" ControlToValidate="txtSeats"
                                        ValidationGroup="Route" Display="Dynamic"
                                        Type="Integer" MinimumValue="1" MaximumValue="1000"
                                        CssClass="text-danger small"
                                        ErrorMessage="Seats must be a whole number of at least one." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblFare" runat="server" AssociatedControlID="txtFare"
                               CssClass="form-label">Indicative fare per seat <span class="text-danger">*</span></asp:Label>
                    <div class="input-group">
                        <span class="input-group-text">R</span>
                        <asp:TextBox ID="txtFare" ToolTip="The published price of one seat." runat="server" CssClass="form-control" />
                    </div>
                    <asp:RequiredFieldValidator ID="rfvFare" runat="server" ControlToValidate="txtFare"
                                                ValidationGroup="Route" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The fare is required." />

                    <asp:RangeValidator ID="rvFare" runat="server" ControlToValidate="txtFare"
                                        ValidationGroup="Route" Display="Dynamic"
                                        Type="Currency" MinimumValue="0.01" MaximumValue="99999999"
                                        CssClass="text-danger small"
                                        ErrorMessage="The fare must be greater than zero." />
                </div>

            </div>

            <p class="text-muted small mt-3 mb-0">
                The fare is a guide price for the portal. The agent captures what the
                client actually pays on the booking itself.
            </p>

            <hr class="my-4" />

            <div class="d-flex justify-content-end gap-2">
                <asp:HyperLink ID="lnkCancel" ToolTip="Goes back to the list without saving." runat="server" CssClass="btn btn-outline-secondary"
                               NavigateUrl="~/Maintain/Routes/Default.aspx">Cancel</asp:HyperLink>
                <asp:Button ID="btnSave" ToolTip="Saves the route." runat="server" Text="Save" CssClass="btn btn-primary"
                            ValidationGroup="Route" OnClick="btnSave_Click" />
            </div>
        </div>
    </div>

</asp:Content>
