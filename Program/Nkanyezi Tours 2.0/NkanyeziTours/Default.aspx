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
        </Columns>
    </asp:GridView>

</asp:Content>
