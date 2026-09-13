<%@ Page Title="Book a car" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="BookVehicle.aspx.cs" Inherits="NkanyeziTours.Itineraries.BookVehicle" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="booking-page-header mb-4">
        <div>
            <span class="transport-eyebrow">Car hire</span>
            <h1>Find the right car</h1>
            <p><asp:Literal ID="litTrip" runat="server" /></p>
        </div>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsCar" runat="server" ValidationGroup="Car"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <asp:Panel ID="pnlForm" runat="server" CssClass="card booking-card">
        <div class="card-body">
            <div class="booking-section-title">
                <span>1</span>
                <div><strong>Rental dates</strong><small>Choose when the client will collect and return the car.</small></div>
            </div>
            <div class="row g-3">
                <div class="col-md-4">
                    <asp:Label ID="lblStartDate" runat="server" AssociatedControlID="txtStartDate"
                               CssClass="form-label">Collection date <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtStartDate" ToolTip="The day the client collects the car." runat="server" CssClass="form-control" TextMode="Date" />
                    <asp:RequiredFieldValidator ID="rfvStartDate" runat="server" ControlToValidate="txtStartDate"
                                                ValidationGroup="Car" Display="Dynamic" CssClass="text-danger small"
                                                ErrorMessage="Collection date is required." />
                </div>
                <div class="col-md-4">
                    <asp:Label ID="lblEndDate" runat="server" AssociatedControlID="txtEndDate"
                               CssClass="form-label">Return date <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtEndDate" ToolTip="The day the client brings the car back. You are charged for this day too." runat="server" CssClass="form-control" TextMode="Date" />
                    <asp:RequiredFieldValidator ID="rfvEndDate" runat="server" ControlToValidate="txtEndDate"
                                                ValidationGroup="Car" Display="Dynamic" CssClass="text-danger small"
                                                ErrorMessage="Return date is required." />
                </div>
                <div class="col-md-4 d-flex align-items-end">
                    <asp:Button ID="btnFind" ToolTip="Lists the cars free for every day in that range." runat="server" Text="Find available cars"
                                CssClass="btn btn-outline-primary w-100" ValidationGroup="Car"
                                OnClick="btnFind_Click" />
                </div>

                <asp:Panel ID="pnlVehicle" runat="server" Visible="false" CssClass="col-12">
                    <hr />
                    <div class="booking-section-title mb-3">
                        <span>2</span>
                        <div><strong>Select a vehicle</strong><small>Only cars available for the full date range are listed.</small></div>
                    </div>

                    <asp:HiddenField ID="hfVehicleId" runat="server" />
                    <div class="row g-3">
                        <asp:Repeater ID="rptVehicles" runat="server" OnItemCommand="rptVehicles_ItemCommand">
                            <ItemTemplate>
                                <div class="col-md-6 col-xl-4">
                                    <article class='car-hire-card h-100 <%# Convert.ToBoolean(Eval("IsSelected")) ? "is-selected" : "" %>'>
                                        <div class="car-hire-card-icon" aria-hidden="true">&#x1F697;</div>
                                        <div class="d-flex justify-content-between gap-2 align-items-start">
                                            <div>
                                                <h3 class="h5 mb-1"><%#: Eval("Display") %></h3>
                                                <div class="small text-muted"><%#: Eval("RegistrationNumber") %></div>
                                            </div>
                                            <span class="badge bg-success">Available</span>
                                        </div>
                                        <div class="car-hire-details">
                                            <span><strong>Branch</strong><%#: Eval("Location") %></span>
                                            <span><strong>Seats</strong><%#: Eval("Capacity") %></span>
                                        </div>
                                        <div class="car-hire-price">
                                            <span>Per day</span>
                                            <strong><%#: Eval("PricePerDay", "R{0:N2}") %></strong>
                                        </div>
                                        <asp:LinkButton ID="lnkSelectCar" runat="server"
                                                        CssClass='<%# Convert.ToBoolean(Eval("IsSelected")) ? "btn btn-success w-100" : "btn btn-outline-primary w-100" %>'
                                                        Text='<%# Convert.ToBoolean(Eval("IsSelected")) ? "Selected" : "Select this car" %>'
                                                        CommandName="SelectCar" CommandArgument='<%# Eval("VehicleId") %>'
                                                        CausesValidation="false"
                                                        ToolTip="Picks this car. Its daily rate and the total for your dates appear below." />
                                    </article>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <asp:Panel ID="pnlRate" runat="server" Visible="false" CssClass="alert alert-light border mt-3 mb-0">
                        <asp:Literal ID="litRate" runat="server" />
                    </asp:Panel>
                </asp:Panel>
            </div>

            <hr class="my-4" />
            <div class="d-flex justify-content-end gap-2">
                <asp:HyperLink ID="lnkCancel" ToolTip="Goes back without booking anything." runat="server" CssClass="btn btn-outline-secondary">Cancel</asp:HyperLink>
                <asp:Button ID="btnBook" ToolTip="Holds the car for those dates and adds the hire cost to the trip." runat="server" Text="Book car" CssClass="btn btn-primary"
                            Visible="false" ValidationGroup="Car" OnClick="btnBook_Click" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>
