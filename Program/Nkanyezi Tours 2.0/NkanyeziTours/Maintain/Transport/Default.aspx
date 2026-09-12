<%@ Page Title="Transport operations" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="Default.aspx.cs" Inherits="NkanyeziTours.Maintain.Transport.Default" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
    <section class="transport-ops-header mb-4">
        <span class="transport-ops-kicker">Transport operations</span>
        <h1>Fleet locations and maintenance</h1>
        <p>See where rental cars are available and keep vehicles out of bookings while they are being serviced.</p>
    </section>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <div class="d-flex flex-wrap justify-content-between align-items-end gap-2 mb-3">
        <div>
            <h2 class="h4 mb-1">Cars by collection branch</h2>
            <p class="text-muted mb-0">Live position for <asp:Literal ID="litSummaryDate" runat="server" />.</p>
        </div>
        <span class="small text-muted">A branch is low when it has one or no cars available.</span>
    </div>

    <div class="row g-3 mb-4">
        <asp:Repeater ID="rptLocations" runat="server">
            <ItemTemplate>
                <div class="col-md-6 col-xl-4">
                    <article class="fleet-location-card h-100">
                        <div class="d-flex justify-content-between align-items-start gap-2">
                            <div>
                                <div class="text-muted small">Collection branch</div>
                                <h3 class="h5 mb-0"><%#: Eval("Location") %></h3>
                            </div>
                            <span class='badge bg-<%# Eval("StockCss") %>'><%#: Eval("StockLevel") %></span>
                        </div>
                        <div class="fleet-location-stats">
                            <div><strong><%#: Eval("AvailableVehicles") %></strong><span>Available</span></div>
                            <div><strong><%#: Eval("RentedVehicles") %></strong><span>Rented</span></div>
                            <div><strong><%#: Eval("MaintenanceVehicles") %></strong><span>Service</span></div>
                            <div><strong><%#: Eval("TotalVehicles") %></strong><span>Total</span></div>
                        </div>
                    </article>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <div class="row g-4 align-items-start">
        <div class="col-lg-4">
            <section class="card booking-card">
                <div class="card-body">
                    <h2 class="h4">Schedule maintenance</h2>
                    <p class="text-muted small">The selected car will not appear as available during these dates.</p>

                    <div class="mb-3">
                        <label class="form-label" for="<%= ddlVehicle.ClientID %>">Vehicle</label>
                        <asp:DropDownList ID="ddlVehicle" runat="server" CssClass="form-select" />
                        <asp:RequiredFieldValidator ID="valVehicle" runat="server" ControlToValidate="ddlVehicle"
                            InitialValue="" ErrorMessage="Choose a vehicle." CssClass="text-danger small" Display="Dynamic" />
                    </div>

                    <div class="mb-3">
                        <label class="form-label" for="<%= txtType.ClientID %>">Service or repair</label>
                        <asp:TextBox ID="txtType" runat="server" CssClass="form-control" MaxLength="100"
                                     placeholder="Example: Annual service" />
                        <asp:RequiredFieldValidator ID="valType" runat="server" ControlToValidate="txtType"
                            ErrorMessage="Enter the maintenance type." CssClass="text-danger small" Display="Dynamic" />
                    </div>

                    <div class="row g-2 mb-3">
                        <div class="col-6">
                            <label class="form-label" for="<%= txtStartDate.ClientID %>">From</label>
                            <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date" />
                            <asp:RequiredFieldValidator ID="valStart" runat="server" ControlToValidate="txtStartDate"
                                ErrorMessage="Required." CssClass="text-danger small" Display="Dynamic" />
                        </div>
                        <div class="col-6">
                            <label class="form-label" for="<%= txtEndDate.ClientID %>">Until</label>
                            <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date" />
                            <asp:RequiredFieldValidator ID="valEnd" runat="server" ControlToValidate="txtEndDate"
                                ErrorMessage="Required." CssClass="text-danger small" Display="Dynamic" />
                        </div>
                    </div>

                    <div class="row g-2 mb-3">
                        <div class="col-6">
                            <label class="form-label" for="<%= txtOdometer.ClientID %>">Odometer</label>
                            <asp:TextBox ID="txtOdometer" runat="server" CssClass="form-control" TextMode="Number" min="0" />
                        </div>
                        <div class="col-6">
                            <label class="form-label" for="<%= txtEstimatedCost.ClientID %>">Est. cost (R)</label>
                            <asp:TextBox ID="txtEstimatedCost" runat="server" CssClass="form-control" inputmode="decimal" />
                        </div>
                    </div>

                    <div class="mb-3">
                        <label class="form-label" for="<%= txtNotes.ClientID %>">Notes</label>
                        <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" TextMode="MultiLine"
                                     Rows="3" MaxLength="500" placeholder="Workshop, parts or safety notes" />
                    </div>

                    <asp:Button ID="btnSchedule" runat="server" Text="Schedule maintenance"
                                CssClass="btn btn-primary w-100" OnClick="btnSchedule_Click" />
                </div>
            </section>
        </div>

        <div class="col-lg-8">
            <section class="card booking-card">
                <div class="card-body">
                    <h2 class="h4 mb-1">Maintenance schedule</h2>
                    <p class="text-muted small mb-3">Overdue work is highlighted so safety items are not missed.</p>

                    <div class="table-responsive">
                        <asp:Repeater ID="rptMaintenance" runat="server" OnItemCommand="rptMaintenance_ItemCommand">
                            <HeaderTemplate>
                                <table class="table align-middle transport-maintenance-table">
                                    <thead><tr><th>Vehicle</th><th>Work</th><th>Dates</th><th>Status</th><th></th></tr></thead>
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr class='<%# Convert.ToBoolean(Eval("IsOverdue")) ? "table-danger" : "" %>'>
                                    <td>
                                        <strong><%#: Eval("VehicleDisplay") %></strong><br />
                                        <span class="small text-muted"><%#: Eval("RegistrationNumber") %> · <%#: Eval("Location") %></span>
                                    </td>
                                    <td>
                                        <%#: Eval("MaintenanceType") %>
                                        <asp:PlaceHolder runat="server" Visible='<%# Eval("Odometer") != null %>'>
                                            <br /><span class="small text-muted"><%#: Eval("Odometer", "{0:N0} km") %></span>
                                        </asp:PlaceHolder>
                                    </td>
                                    <td class="text-nowrap"><%#: Eval("DateRangeDisplay") %></td>
                                    <td>
                                        <span class='badge <%# Convert.ToBoolean(Eval("IsOverdue")) ? "bg-danger" : "bg-secondary" %>'>
                                            <%#: Convert.ToBoolean(Eval("IsOverdue")) ? "Overdue" : Eval("Status") %>
                                        </span>
                                    </td>
                                    <td class="text-end">
                                        <asp:LinkButton ID="btnComplete" runat="server" Text="Mark complete"
                                            CssClass="btn btn-sm btn-outline-success text-nowrap"
                                            CommandName="Complete" CommandArgument='<%# Eval("VehicleMaintenanceId") %>'
                                            Visible='<%# Eval("CanComplete") %>' CausesValidation="false" />
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                    </tbody>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                        <asp:Panel ID="pnlNoMaintenance" runat="server" Visible="false" CssClass="text-muted py-4 text-center">
                            No maintenance has been scheduled.
                        </asp:Panel>
                    </div>
                </div>
            </section>
        </div>
    </div>
</asp:Content>
