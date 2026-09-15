<%@ Page Title="Add transport" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="ChooseTransport.aspx.cs" Inherits="NkanyeziTours.Trips.ChooseTransport" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-1">Add transport</h1>
    <p class="text-muted mb-4"><asp:Literal ID="litSubtitle" runat="server" /></p>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlBody" runat="server">

        <ul class="nav nav-tabs mb-4">
            <li class="nav-item">
                <asp:LinkButton ID="lnkTabCars" ToolTip="Hire a car for the whole leg. Priced per day." runat="server" CssClass="nav-link"
                                CausesValidation="false" OnClick="lnkTabCars_Click"
                                Text="Car Rentals" />
            </li>
            <li class="nav-item">
                <asp:LinkButton ID="lnkTabFlights" ToolTip="Tickets on a scheduled flight, one per person." runat="server" CssClass="nav-link"
                                CausesValidation="false" OnClick="lnkTabFlights_Click"
                                Text="Flights" />
            </li>
            <li class="nav-item">
                <asp:LinkButton ID="lnkTabBuses" ToolTip="Tickets on a scheduled bus, one per person." runat="server" CssClass="nav-link"
                                CausesValidation="false" OnClick="lnkTabBuses_Click"
                                Text="Bus tickets" />
            </li>
        </ul>

        <asp:Panel ID="pnlCars" runat="server">

            <div class="alert alert-light border small">
                <asp:Literal ID="litCarRegionNote" runat="server" />
            </div>

            <asp:Repeater ID="rptCars" runat="server" OnItemCommand="rptCars_ItemCommand">
                <HeaderTemplate>
                    <div class="row g-3">
                </HeaderTemplate>
                <ItemTemplate>
                    <div class="col-sm-6 col-lg-4">
                        <div class="choice-card d-flex flex-column">
                            <div class="choice-card-photo">
                                <asp:Image ID="imgCar" runat="server"
                                           Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("ImageUrl"))) %>'
                                           ImageUrl='<%# Eval("ImageUrl") %>'
                                           AlternateText='<%# Eval("Display") %>' />
                                <asp:Label ID="lblCarPhotoHint" runat="server" CssClass="placeholder-text"
                                           Visible='<%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("ImageUrl"))) %>'
                                           Text="Photo to come" />
                            </div>
                            <div class="choice-card-body d-flex flex-column flex-grow-1">
                                <div class="choice-card-title"><%# Server.HtmlEncode(Convert.ToString(Eval("Display"))) %></div>
                                <div class="choice-card-meta">
                                    <%# Eval("RegistrationNumber") %>
                                    &middot; <%# Eval("Location") %>
                                </div>
                                <div class="choice-card-meta mt-1">
                                    Seats <%# Eval("Capacity") %>
                                    &middot; <%# Eval("IncludedKmPerDay") %>km per day included
                                </div>
                                <div class="mt-2 mb-3 choice-card-price">
                                    R<%# string.Format("{0:N2}", Eval("PricePerDay")) %>
                                    <span class="text-muted fw-normal small">per day</span>
                                </div>
                                <div class="mt-auto">
                                    <asp:LinkButton ID="lnkTakeCar" ToolTip="Asks for this car for the days of this leg." runat="server"
                                                    CssClass="btn btn-sm btn-primary w-100"
                                                    CommandName="TakeCar"
                                                    CommandArgument='<%# Eval("VehicleId") %>'
                                                    Text="Add this car" />
                                </div>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
                <FooterTemplate>
                    </div>
                </FooterTemplate>
            </asp:Repeater>

            <asp:Panel ID="pnlNoCars" runat="server" Visible="false" CssClass="alert alert-light border">
                <asp:Literal ID="litNoCars" runat="server" />
            </asp:Panel>

        </asp:Panel>

        <asp:Panel ID="pnlSeats" runat="server" Visible="false">

            <div class="card mb-4">
                <div class="card-body">
                    <div class="row g-3 align-items-end">
                        <div class="col-md-3">
                            <asp:Label ID="lblTravelDate" runat="server" AssociatedControlID="ddlTravelDate"
                                       CssClass="form-label">Travelling on <span class="text-danger">*</span></asp:Label>
                            <asp:DropDownList ID="ddlTravelDate" ToolTip="Which day of this leg you travel." runat="server" CssClass="form-select" />
                        </div>
                        <div class="col-md-5">
                            <div class="form-label">Tickets</div>
                            <div class="form-control-plaintext fw-semibold" title="One ticket for every person on this leg.">
                                <asp:Literal ID="litTickets" runat="server" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <asp:Repeater ID="rptRoutes" runat="server" OnItemCommand="rptRoutes_ItemCommand">
                <HeaderTemplate>
                    <div class="row g-3">
                </HeaderTemplate>
                <ItemTemplate>
                    <div class="col-sm-6 col-lg-4">
                        <div class="choice-card d-flex flex-column">
                            <div class="choice-card-body d-flex flex-column flex-grow-1">
                                <div class="choice-card-title"><%# Server.HtmlEncode(Convert.ToString(Eval("Journey"))) %></div>
                                <div class="choice-card-meta">
                                    <%# Eval("Carrier") %> <%# Eval("RouteCode") %>
                                </div>
                                <div class="choice-card-meta mt-1">
                                    <%# Eval("TypeDisplay") %>
                                    &middot; <%# Eval("DurationDisplay") %>
                                </div>
                                <div class="mt-2 mb-3 choice-card-price">
                                    R<%# string.Format("{0:N2}", Eval("IndicativeFarePerSeat")) %>
                                    <span class="text-muted fw-normal small">per ticket</span>
                                </div>
                                <div class="mt-auto">
                                    <asp:LinkButton ID="lnkTakeSeats" ToolTip="Asks for one ticket per person on this leg. Your agent confirms them." runat="server"
                                                    CssClass="btn btn-sm btn-primary w-100"
                                                    CommandName="TakeSeats"
                                                    CommandArgument='<%# Eval("RouteId") %>'
                                                    Text="Add tickets" />
                                </div>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
                <FooterTemplate>
                    </div>
                </FooterTemplate>
            </asp:Repeater>

            <asp:Panel ID="pnlNoRoutes" runat="server" Visible="false" CssClass="alert alert-light border">
                <asp:Literal ID="litNoRoutes" runat="server" />
            </asp:Panel>

        </asp:Panel>

        <div class="mt-4">
            <asp:HyperLink ID="lnkBack" ToolTip="Back to your trip without choosing." runat="server" CssClass="btn btn-outline-secondary"
                           Text="Back to my trip" />
        </div>

    </asp:Panel>

</asp:Content>
