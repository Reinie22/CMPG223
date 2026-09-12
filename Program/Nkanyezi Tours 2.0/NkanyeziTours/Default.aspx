<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="Default.aspx.cs" Inherits="NkanyeziTours._Default" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-1">
        Welcome, <asp:Literal ID="litName" runat="server" />
    </h1>
    <p class="text-muted mb-4">
        <asp:Literal ID="litSubtitle" runat="server" />
    </p>

    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert alert-danger">
        <asp:Literal ID="litError" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlRequested" runat="server" Visible="false" CssClass="alert alert-success py-2">
        Thank you - your trip request is in. An agent will pick it up and be in touch.
    </asp:Panel>

    <%-- Clients start their own trips here. Staff have their own screens. --%>
    <asp:Panel ID="pnlBookTrip" runat="server" Visible="false" CssClass="mb-4">
        <asp:HyperLink ID="lnkBookTrip" runat="server" CssClass="btn btn-primary"
                       NavigateUrl="~/Trips/BookTrip.aspx">Book a trip</asp:HyperLink>
    </asp:Panel>

    <%--
        Management's shortcuts. Allocation is the one thing a supervisor does
        that nobody else can, so it belongs on the landing page rather than two
        clicks away.
    --%>
    <asp:Panel ID="pnlManage" runat="server" Visible="false" CssClass="mb-4 d-flex flex-wrap gap-2">
        <asp:HyperLink ID="lnkAssignTrips" runat="server" CssClass="btn btn-primary"
                       NavigateUrl="~/Itineraries/Assign.aspx">Assign trips to agents</asp:HyperLink>
        <asp:HyperLink ID="lnkAllTrips" runat="server" CssClass="btn btn-outline-primary"
                       NavigateUrl="~/Itineraries/Default.aspx">All itineraries</asp:HyperLink>
        <asp:Label ID="lblUnassignedCount" runat="server" Visible="false"
                   CssClass="badge text-bg-warning align-self-center" />
    </asp:Panel>

    <%-- Stat cards. Which ones show depends on the role. --%>
    <div class="row g-3 mb-4">
        <asp:Repeater ID="rptStats" runat="server">
            <ItemTemplate>
                <div class="col-6 col-md-3">
                    <div class="card h-100">
                        <div class="card-body">
                            <div class="text-muted small"><%# Eval("Label") %></div>
                            <div class="fs-4 fw-semibold"><%# Eval("Value") %></div>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <h2 class="h5">Recent trips</h2>
    <asp:GridView ID="gvTrips" runat="server"
                  AutoGenerateColumns="false"
                  CssClass="table table-striped table-hover align-middle"
                  EmptyDataText="No trips yet."
                  GridLines="None">
        <Columns>
            <asp:BoundField DataField="TripName"  HeaderText="Trip" />
            <asp:BoundField DataField="ClientName" HeaderText="Client" />
            <asp:BoundField DataField="AgentName"  HeaderText="Agent" />
            <asp:BoundField DataField="DateRangeDisplay" HeaderText="Dates" />
            <asp:BoundField DataField="TotalBudget" HeaderText="Budget"
                            DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
            <asp:BoundField DataField="Status" HeaderText="Status" />
            <asp:TemplateField HeaderText="">
                <ItemTemplate>
                    <%-- Clients open their own trip to choose places and things to do. --%>
                    <asp:HyperLink ID="lnkMyTrip" runat="server"
                                   CssClass="btn btn-sm btn-primary text-nowrap"
                                   Text="Open"
                                   Visible='<%# !Nkanyezi.Services.AuthService.IsStaff %>'
                                   NavigateUrl='<%# "~/Trips/MyTrip.aspx?id=" + Eval("ItineraryId") %>' />
                    <asp:HyperLink ID="lnkBookTransport" runat="server"
                                   CssClass="btn btn-sm btn-outline-primary text-nowrap"
                                   Text="Book transport"
                                   Visible='<%# Nkanyezi.Services.AuthService.IsStaff && Convert.ToBoolean(Eval("IsEditable")) %>'
                                   NavigateUrl='<%# "~/Itineraries/BookTransport.aspx?itineraryId=" + Eval("ItineraryId") %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>
