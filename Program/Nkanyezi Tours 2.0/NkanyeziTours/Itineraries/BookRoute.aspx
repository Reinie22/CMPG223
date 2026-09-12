<%@ Page Title="Book bus or flight" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="BookRoute.aspx.cs" Inherits="NkanyeziTours.Itineraries.BookRoute" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlHeader" runat="server" CssClass="booking-page-header mb-4">
        <div>
            <span class="transport-eyebrow">Scheduled transport</span>
            <h1><asp:Literal ID="litHeading" runat="server" /></h1>
            <p><asp:Literal ID="litTrip" runat="server" /></p>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsRoute" runat="server" ValidationGroup="Route"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <asp:Panel ID="pnlForm" runat="server" CssClass="card booking-card">
        <div class="card-body">
            <div class="booking-section-title">
                <span>1</span>
                <div><strong>Journey details</strong><small>Choose the service, date and number of travellers.</small></div>
            </div>
            <div class="row g-3">
                <div class="col-12">
                    <asp:Label ID="lblRoute" runat="server" AssociatedControlID="ddlRoute"
                               CssClass="form-label"><asp:Literal ID="litRouteLabel" runat="server" /></asp:Label>
                    <asp:DropDownList ID="ddlRoute" runat="server" CssClass="form-select"
                                      AutoPostBack="true" OnSelectedIndexChanged="ddlRoute_SelectedIndexChanged" />
                    <asp:RequiredFieldValidator ID="rfvRoute" runat="server" ControlToValidate="ddlRoute"
                                                InitialValue="" ValidationGroup="Route" Display="Dynamic"
                                                CssClass="text-danger small" ErrorMessage="Choose a route." />
                    <asp:Panel ID="pnlAvailability" runat="server" Visible="false"
                               CssClass="seat-availability mt-3" aria-live="polite">
                        <div class="d-flex justify-content-between align-items-center gap-3">
                            <asp:Literal ID="litAvailability" runat="server" />
                            <asp:Label ID="lblAvailabilityStatus" runat="server" CssClass="badge rounded-pill" />
                        </div>
                        <div class="seat-availability-track mt-2">
                            <asp:Panel ID="pnlAvailabilityFill" runat="server" CssClass="seat-availability-fill" />
                        </div>
                    </asp:Panel>
                </div>

                <div class="col-md-4">
                    <asp:Label ID="lblTravelDate" runat="server" AssociatedControlID="txtTravelDate"
                               CssClass="form-label">Travel date <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtTravelDate" runat="server" CssClass="form-control" TextMode="Date"
                                 AutoPostBack="true" OnTextChanged="txtTravelDate_TextChanged" />
                    <asp:RequiredFieldValidator ID="rfvTravelDate" runat="server" ControlToValidate="txtTravelDate"
                                                ValidationGroup="Route" Display="Dynamic" CssClass="text-danger small"
                                                ErrorMessage="Travel date is required." />
                </div>

                <div class="col-md-4">
                    <asp:Label ID="lblSeats" runat="server" AssociatedControlID="txtSeats"
                               CssClass="form-label">Seats <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtSeats" runat="server" CssClass="form-control" TextMode="Number" Text="1" />
                    <asp:RequiredFieldValidator ID="rfvSeats" runat="server" ControlToValidate="txtSeats"
                                                ValidationGroup="Route" Display="Dynamic" CssClass="text-danger small"
                                                ErrorMessage="Number of seats is required." />
                    <asp:RangeValidator ID="rvSeats" runat="server" ControlToValidate="txtSeats"
                                        ValidationGroup="Route" Display="Dynamic" Type="Integer"
                                        MinimumValue="1" MaximumValue="300" CssClass="text-danger small"
                                        ErrorMessage="Choose a valid number of available seats." />
                </div>

                <div class="col-md-4">
                    <asp:Label ID="lblFare" runat="server" AssociatedControlID="txtFare"
                               CssClass="form-label">Fare per passenger <span class="text-danger">*</span></asp:Label>
                    <div class="input-group">
                        <span class="input-group-text">R</span>
                        <asp:TextBox ID="txtFare" runat="server" CssClass="form-control" />
                    </div>
                    <div class="form-text">Price for one passenger.</div>
                    <asp:RequiredFieldValidator ID="rfvFare" runat="server" ControlToValidate="txtFare"
                                                ValidationGroup="Route" Display="Dynamic" CssClass="text-danger small"
                                                ErrorMessage="Fare per seat is required." />
                    <asp:CustomValidator ID="cvFare" runat="server" ControlToValidate="txtFare"
                                         ValidationGroup="Route" Display="Dynamic"
                                         OnServerValidate="cvFare_ServerValidate"
                                         CssClass="text-danger small"
                                         ErrorMessage="Fare must be a number greater than zero." />
                </div>

                <div class="col-12">
                    <div class="booking-total" aria-live="polite">
                        <span>Total booking amount</span>
                        <asp:Label ID="lblBookingTotal" runat="server" Text="R0,00" />
                    </div>
                </div>
            </div>

            <hr class="my-4" />
            <div class="d-flex justify-content-end gap-2">
                <asp:HyperLink ID="lnkCancel" runat="server" CssClass="btn btn-outline-secondary">Cancel</asp:HyperLink>
                <asp:Button ID="btnBook" runat="server" Text="Book transport" CssClass="btn btn-primary"
                            ValidationGroup="Route" OnClick="btnBook_Click" />
            </div>
        </div>
    </asp:Panel>

    <script>
        (function () {
            var seats = document.getElementById('<%= txtSeats.ClientID %>');
            var fare = document.getElementById('<%= txtFare.ClientID %>');
            var total = document.getElementById('<%= lblBookingTotal.ClientID %>');

            function numberFrom(value) {
                var clean = (value || '').replace(/\s/g, '').replace(/[^0-9,.-]/g, '');
                var comma = clean.lastIndexOf(',');
                var dot = clean.lastIndexOf('.');

                if (comma > -1 && dot > -1) {
                    if (comma > dot)
                        clean = clean.replace(/\./g, '').replace(',', '.');
                    else
                        clean = clean.replace(/,/g, '');
                } else {
                    clean = clean.replace(',', '.');
                }

                var parsed = Number(clean);
                return Number.isFinite(parsed) ? parsed : 0;
            }

            function updateTotal() {
                var count = Math.max(0, parseInt(seats.value, 10) || 0);
                var amount = Math.max(0, numberFrom(fare.value));
                total.textContent = new Intl.NumberFormat('en-ZA', {
                    style: 'currency', currency: 'ZAR'
                }).format(count * amount);
            }

            seats.addEventListener('input', updateTotal);
            fare.addEventListener('input', updateTotal);
            updateTotal();
        }());
    </script>
</asp:Content>
