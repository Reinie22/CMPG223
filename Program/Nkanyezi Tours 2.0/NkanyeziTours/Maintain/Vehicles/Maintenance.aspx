<%@ Page Title="Fleet maintenance" Language="C#" MasterPageFile="~/Maintain/Maintain.master" AutoEventWireup="true"
         CodeBehind="Maintenance.aspx.cs" Inherits="NkanyeziTours.Maintain.Vehicles.Maintenance" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MaintainContent" runat="server">

    <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
        <h1 class="h3 mb-0">Fleet maintenance</h1>
        <asp:HyperLink ID="lnkFleet" ToolTip="Back to the list of cars and their details."
                       runat="server" CssClass="btn btn-outline-secondary"
                       NavigateUrl="~/Maintain/Vehicles/Default.aspx">Vehicles</asp:HyperLink>
    </div>

    <p class="text-muted">
        Where the cars are, and which of them are off the road. A car booked in
        here does not appear as available while the work is on.
    </p>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <div class="d-flex flex-wrap justify-content-between align-items-end gap-2 mb-3">
        <div>
            <h2 class="h5 mb-1">Cars by collection branch</h2>
            <p class="text-muted small mb-0">
                Where the fleet stands on <asp:Literal ID="litSummaryDate" runat="server" />.
            </p>
        </div>
        <span class="small text-muted">A branch is short when it has one car or none left.</span>
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
            <div class="card">
                <div class="card-body">
                    <h2 class="h5">Book a car in</h2>
                    <p class="text-muted small">
                        Nobody can hire this car over these dates once it is booked in.
                    </p>

                    <div class="mb-3">
                        <asp:Label ID="lblVehicle" runat="server" AssociatedControlID="ddlVehicle"
                                   CssClass="form-label">Vehicle <span class="text-danger">*</span></asp:Label>
                        <asp:DropDownList ID="ddlVehicle" runat="server" CssClass="form-select"
                                          ToolTip="The car going off the road. Every car is listed, wherever it is based." />
                        <asp:RequiredFieldValidator ID="rfvVehicle" runat="server" ControlToValidate="ddlVehicle"
                                                    InitialValue="" ValidationGroup="Maintenance" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Choose a vehicle." />
                    </div>

                    <div class="mb-3">
                        <asp:Label ID="lblType" runat="server" AssociatedControlID="txtType"
                                   CssClass="form-label">Service or repair <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtType" runat="server" CssClass="form-control" MaxLength="100"
                                     placeholder="For example: annual service"
                                     ToolTip="What is being done to the car, so the next person knows why it is off the road." />
                        <asp:RequiredFieldValidator ID="rfvType" runat="server" ControlToValidate="txtType"
                                                    ValidationGroup="Maintenance" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Say what the work is." />
                    </div>

                    <div class="row g-2 mb-3">
                        <div class="col-6">
                            <asp:Label ID="lblStart" runat="server" AssociatedControlID="txtStartDate"
                                       CssClass="form-label">From <span class="text-danger">*</span></asp:Label>
                            <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date"
                                         ToolTip="The first day the car is off the road." />
                            <asp:RequiredFieldValidator ID="rfvStart" runat="server" ControlToValidate="txtStartDate"
                                                        ValidationGroup="Maintenance" Display="Dynamic"
                                                        CssClass="text-danger small"
                                                        ErrorMessage="Required." />
                        </div>
                        <div class="col-6">
                            <asp:Label ID="lblEnd" runat="server" AssociatedControlID="txtEndDate"
                                       CssClass="form-label">Until <span class="text-danger">*</span></asp:Label>
                            <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date"
                                         ToolTip="The last day the car is off the road. It can be hired again the day after." />
                            <asp:RequiredFieldValidator ID="rfvEnd" runat="server" ControlToValidate="txtEndDate"
                                                        ValidationGroup="Maintenance" Display="Dynamic"
                                                        CssClass="text-danger small"
                                                        ErrorMessage="Required." />
                        </div>
                    </div>

                    <div class="row g-2 mb-3">
                        <div class="col-6">
                            <asp:Label ID="lblOdometer" runat="server" AssociatedControlID="txtOdometer"
                                       CssClass="form-label">Odometer</asp:Label>
                            <asp:TextBox ID="txtOdometer" runat="server" CssClass="form-control" TextMode="Number" min="0"
                                         ToolTip="Kilometres on the clock when the car went in. Leave blank if you do not know." />
                        </div>
                        <div class="col-6">
                            <asp:Label ID="lblCost" runat="server" AssociatedControlID="txtEstimatedCost"
                                       CssClass="form-label">Est. cost (R)</asp:Label>
                            <asp:TextBox ID="txtEstimatedCost" runat="server" CssClass="form-control" inputmode="decimal"
                                         ToolTip="What the workshop quoted. You can record what it actually came to when you close the job." />
                        </div>
                    </div>

                    <div class="mb-3">
                        <asp:Label ID="lblNotes" runat="server" AssociatedControlID="txtNotes"
                                   CssClass="form-label">Notes</asp:Label>
                        <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" TextMode="MultiLine"
                                     Rows="3" MaxLength="500" placeholder="Workshop, parts or safety notes"
                                     ToolTip="Anything the next person should know: which workshop has it, parts on order, safety concerns." />
                    </div>

                    <asp:Button ID="btnSchedule" runat="server" Text="Book the car in"
                                CssClass="btn btn-primary w-100"
                                ValidationGroup="Maintenance" OnClick="btnSchedule_Click"
                                ToolTip="Takes the car off hire for those dates. It is refused if the car is already booked or already in for work then." />
                </div>
            </div>
        </div>

        <div class="col-lg-8">
            <div class="card">
                <div class="card-body">
                    <h2 class="h5 mb-1">What is booked in</h2>
                    <p class="text-muted small mb-3">
                        Work that has run past its end date is marked overdue, so a
                        safety item cannot quietly sit there.
                    </p>

                    <div class="table-responsive">
                        <asp:Repeater ID="rptMaintenance" runat="server" OnItemCommand="rptMaintenance_ItemCommand">
                            <HeaderTemplate>
                                <table class="table align-middle transport-maintenance-table">
                                    <thead>
                                        <tr>
                                            <th>Vehicle</th><th>Work</th><th>Dates</th><th>Status</th><th></th>
                                        </tr>
                                    </thead>
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr class='<%# Convert.ToBoolean(Eval("IsOverdue")) ? "table-danger" : "" %>'>
                                    <td>
                                        <strong><%#: Eval("VehicleDisplay") %></strong><br />
                                        <span class="small text-muted">
                                            <%#: Eval("RegistrationNumber") %> &middot; <%#: Eval("Location") %>
                                        </span>
                                    </td>
                                    <td>
                                        <%#: Eval("MaintenanceType") %>
                                        <asp:PlaceHolder ID="phOdometer" runat="server" Visible='<%# Eval("Odometer") != null %>'>
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
                                        <asp:LinkButton ID="lnkComplete" runat="server" Text="Mark complete"
                                                        CssClass="btn btn-sm btn-outline-success text-nowrap"
                                                        CommandName="Complete"
                                                        CommandArgument='<%# Eval("VehicleMaintenanceId") %>'
                                                        Visible='<%# Eval("CanComplete") %>' CausesValidation="false"
                                                        ToolTip="Closes the job and puts the car back on hire from today." />
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                    </tbody>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>

                        <asp:Panel ID="pnlNoMaintenance" runat="server" Visible="false"
                                   CssClass="text-muted py-4 text-center">
                            No car is booked in for work.
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>

    </div>

</asp:Content>
