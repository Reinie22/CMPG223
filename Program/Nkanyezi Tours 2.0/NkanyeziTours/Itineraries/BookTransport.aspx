<%@ Page Title="Book transport" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="BookTransport.aspx.cs" Inherits="NkanyeziTours.Itineraries.BookTransport" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="transport-hero mb-4">
        <img src="<%: ResolveUrl("~/Content/images/transport-hero.png") %>"
             alt="A car, coach bus and aeroplane travelling through South Africa" />
        <div class="transport-hero-shade"></div>
        <div class="transport-hero-content">
            <span class="transport-eyebrow">Nkanyezi Tours</span>
            <h1>How would you like to travel?</h1>
            <p><asp:Literal ID="litTrip" runat="server" /></p>
        </div>
        <asp:HyperLink ID="lnkDashboard" ToolTip="Back to your dashboard." runat="server" CssClass="btn btn-light transport-back"
                       NavigateUrl="~/Default.aspx">&larr; Dashboard</asp:HyperLink>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlChoices" runat="server">
        <div class="row g-4">
            <div class="col-md-4">
                <div class="card transport-choice transport-choice-car h-100">
                    <div class="card-body d-flex flex-column">
                        <div class="d-flex justify-content-end align-items-start mb-3">
                            <span class="transport-step">01</span>
                        </div>
                        <span class="transport-kicker">Freedom to explore</span>
                        <h2 class="h4">Car hire</h2>
                        <p class="text-muted flex-grow-1">
                            Enjoy the flexibility of your own car, with clear daily pricing and included kilometres.
                        </p>
                        <asp:HyperLink ID="lnkCar" ToolTip="Hire a car for a stretch of days. Priced per day." runat="server" CssClass="btn btn-primary stretched-link">
                            Choose a car
                        </asp:HyperLink>
                    </div>
                </div>
            </div>

            <div class="col-md-4">
                <div class="card transport-choice transport-choice-bus h-100">
                    <div class="card-body d-flex flex-column">
                        <div class="d-flex justify-content-end align-items-start mb-3">
                            <span class="transport-step">02</span>
                        </div>
                        <span class="transport-kicker">Comfort on the road</span>
                        <h2 class="h4">Bus or shuttle</h2>
                        <p class="text-muted flex-grow-1">
                            Relax on a comfortable scheduled journey between your departure and destination cities.
                        </p>
                        <asp:HyperLink ID="lnkBus" ToolTip="Book bus seats on a scheduled route. Priced per seat." runat="server" CssClass="btn btn-primary stretched-link">
                            Choose a bus
                        </asp:HyperLink>
                    </div>
                </div>
            </div>

            <div class="col-md-4">
                <div class="card transport-choice transport-choice-flight h-100">
                    <div class="card-body d-flex flex-column">
                        <div class="d-flex justify-content-end align-items-start mb-3">
                            <span class="transport-step">03</span>
                        </div>
                        <span class="transport-kicker">Travel further, faster</span>
                        <h2 class="h4">Aeroplane</h2>
                        <p class="text-muted flex-grow-1">
                            Reach your destination faster with convenient local and international flights.
                        </p>
                        <asp:HyperLink ID="lnkFlight" ToolTip="Book flight seats on a scheduled route. Priced per seat." runat="server" CssClass="btn btn-primary stretched-link">
                            Choose a flight
                        </asp:HyperLink>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>
</asp:Content>
