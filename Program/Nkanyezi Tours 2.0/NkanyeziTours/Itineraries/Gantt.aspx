<%@ Page Title="Bookings chart" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="Gantt.aspx.cs" Inherits="NkanyeziTours.Itineraries.Gantt" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-1">Bookings chart</h1>
    <p class="text-muted mb-4">
        What is occupied, one month at a time. Confirmed bookings and the requests
        clients are still waiting on, side by side.
    </p>

    <div class="card mb-4">
        <div class="card-body d-flex flex-wrap justify-content-between align-items-center gap-3">
            <div class="d-flex align-items-center gap-2">
                <asp:Button ID="btnPrev" ToolTip="Show the month before this one." runat="server" Text="&#8592; Previous"
                            CssClass="btn btn-outline-secondary btn-sm"
                            CausesValidation="false" OnClick="btnPrev_Click" />
                <span class="h5 mb-0 mx-2"><asp:Literal ID="litMonth" runat="server" /></span>
                <asp:Button ID="btnNext" ToolTip="Show the month after this one." runat="server" Text="Next &#8594;"
                            CssClass="btn btn-outline-secondary btn-sm"
                            CausesValidation="false" OnClick="btnNext_Click" />
                <asp:Button ID="btnToday" ToolTip="Jump back to the current month." runat="server" Text="This month"
                            CssClass="btn btn-outline-primary btn-sm ms-2"
                            CausesValidation="false" OnClick="btnToday_Click" />
            </div>

            <div class="d-flex flex-wrap align-items-center gap-3 small">
                <span><span class="gantt-key gantt-booked"></span> Confirmed</span>
                <span><span class="gantt-key gantt-draft"></span> Requested, not booked</span>
                <span><span class="gantt-key gantt-weekend"></span> Weekend</span>
            </div>
        </div>
    </div>

    <ul class="nav nav-tabs mb-3">
        <li class="nav-item">
            <asp:LinkButton ID="lnkTabResorts" ToolTip="Rooms and units, grouped by resort." runat="server" CssClass="nav-link"
                            CausesValidation="false" OnClick="lnkTabResorts_Click"
                            Text="Resorts and units" />
        </li>
        <li class="nav-item">
            <asp:LinkButton ID="lnkTabCars" ToolTip="The hire fleet, grouped by the region each car is based in." runat="server" CssClass="nav-link"
                            CausesValidation="false" OnClick="lnkTabCars_Click"
                            Text="Vehicles" />
        </li>
    </ul>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <div class="card">
        <div class="card-body">
            <div class="gantt-scroll">
                <asp:Literal ID="litChart" runat="server" />
            </div>
        </div>
    </div>

</asp:Content>
