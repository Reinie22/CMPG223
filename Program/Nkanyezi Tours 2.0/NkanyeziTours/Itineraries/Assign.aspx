<%@ Page Title="Assign trips" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="Assign.aspx.cs" Inherits="NkanyeziTours.Itineraries.Assign" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-1">Assign trips</h1>
    <p class="text-muted mb-4">
        Allocate a trip to an agent, or move it to a different agent.
    </p>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <div class="card mb-4">
        <div class="card-body">
            <h2 class="h5 mb-3">Agents&#39; current workload</h2>
            <div class="row g-3">
                <asp:Repeater ID="rptWorkload" runat="server">
                    <ItemTemplate>
                        <div class="col-6 col-md-3">
                            <div class="border rounded p-3 h-100">
                                <div class="fw-semibold"><%# Eval("AgentName") %></div>
                                <div class="text-muted small">
                                    <%# Eval("LiveCount") %> live, <%# Eval("TotalCount") %> in total
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>

    <div class="card">
        <div class="card-body">
            <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
                <h2 class="h5 mb-0">Trips</h2>
                <div class="d-flex align-items-center gap-2">
                    <asp:Label ID="lblFilter" runat="server" AssociatedControlID="ddlFilter"
                               CssClass="form-label mb-0 text-nowrap">Show</asp:Label>
                    <asp:DropDownList ID="ddlFilter" ToolTip="Narrow the list down to trips in a particular state." runat="server" CssClass="form-select"
                                      AutoPostBack="true" OnSelectedIndexChanged="ddlFilter_Changed">
                        <asp:ListItem Text="Needing an agent" Value="unassigned" />
                        <asp:ListItem Text="Live trips" Value="live" />
                        <asp:ListItem Text="Everything" Value="all" />
                    </asp:DropDownList>
                </div>
            </div>

            <asp:GridView ID="gvTrips" runat="server"
                          AutoGenerateColumns="false"
                          DataKeyNames="ItineraryId"
                          OnRowCommand="gvTrips_RowCommand"
                          OnRowDataBound="gvTrips_RowDataBound"
                          CssClass="table table-striped align-middle"
                          EmptyDataText="No trips match that filter."
                          GridLines="None">
                <Columns>
                    <asp:BoundField DataField="TripName" HeaderText="Trip" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client" />
                    <asp:BoundField DataField="DateRangeDisplay" HeaderText="Dates" />
                    <asp:BoundField DataField="TotalBudget" HeaderText="Budget"
                                    DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                    <asp:TemplateField HeaderText="Agent">
                        <ItemTemplate>
                            <asp:DropDownList ID="ddlAgent" ToolTip="The agent who will take this trip on." runat="server" CssClass="form-select form-select-sm" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkAssign" ToolTip="Gives this trip to the chosen agent. They see it on their dashboard straight away." runat="server"
                                            CssClass="btn btn-sm btn-primary text-nowrap"
                                            CommandName="AssignTrip"
                                            CommandArgument='<%# Eval("ItineraryId") %>'
                                            Text="Save agent" />
                            <asp:HyperLink ID="lnkOpen" ToolTip="Open the trip to see its legs and costs." runat="server"
                                           CssClass="btn btn-sm btn-outline-secondary text-nowrap"
                                           Text="Open"
                                           NavigateUrl='<%# "~/Itineraries/EditTrip.aspx?id=" + Eval("ItineraryId") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

</asp:Content>
