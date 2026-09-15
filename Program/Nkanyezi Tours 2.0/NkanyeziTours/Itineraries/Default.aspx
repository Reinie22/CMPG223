<%@ Page Title="Itineraries" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="Default.aspx.cs" Inherits="NkanyeziTours.Itineraries._Default" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-1">Itineraries</h1>
    <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-4">
        <p class="text-muted mb-0"><asp:Literal ID="litSubtitle" runat="server" /></p>
        <div class="d-flex flex-wrap gap-2">
            <asp:HyperLink ID="lnkGantt" ToolTip="Month view of what is booked, so you can spot a free unit or a double booking." runat="server" CssClass="btn btn-outline-primary"
                           NavigateUrl="~/Itineraries/Gantt.aspx">Bookings chart</asp:HyperLink>
            <asp:HyperLink ID="lnkAssign" ToolTip="Give a waiting trip to one of your agents." runat="server" Visible="false" CssClass="btn btn-outline-primary"
                           NavigateUrl="~/Itineraries/Assign.aspx">Assign trips</asp:HyperLink>
        </div>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlQueue" runat="server" Visible="false" CssClass="card mb-4">
        <div class="card-body">
            <h2 class="h5 mb-1">
                Pending assignment
                <asp:Label ID="lblQueueCount" runat="server" CssClass="badge text-bg-secondary ms-1" />
            </h2>


            <asp:GridView ID="gvUnassigned" runat="server"
                          AutoGenerateColumns="false"
                          DataKeyNames="ItineraryId"
                          CssClass="table table-striped table-hover align-middle"
                          EmptyDataText="Nothing waiting - every trip has an agent."
                          GridLines="None">
                <Columns>
                    <asp:BoundField DataField="TripName" HeaderText="Trip" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client" />
                    <asp:BoundField DataField="DateRangeDisplay" HeaderText="Dates" />
                    <asp:BoundField DataField="TotalBudget" HeaderText="Budget"
                                    DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                    <asp:BoundField DataField="CreatedDate" HeaderText="Requested"
                                    DataFormatString="{0:dd/MM/yyyy}" />
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <asp:HyperLink ID="lnkAssignOne" runat="server"
                                           CssClass="btn btn-sm btn-primary text-nowrap"
                                           ToolTip="Choose which agent takes this trip."
                                           NavigateUrl="~/Itineraries/Assign.aspx"
                                           Text="Assign to an agent" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>

    <div class="card">
        <div class="card-body">
            <h2 class="h5 mb-3"><asp:Literal ID="litMineHeading" runat="server" /></h2>

            <asp:Panel ID="pnlFilters" runat="server" DefaultButton="btnFilter" CssClass="row g-2 align-items-end mb-3">
                <div class="col-md-5">
                    <asp:Label ID="lblSearch" runat="server" AssociatedControlID="txtSearch" CssClass="form-label small mb-1">Trip or client</asp:Label>
                    <asp:TextBox ID="txtSearch" ToolTip="Type part of a trip name or a client's name." runat="server" CssClass="form-control"
                                 MaxLength="100" placeholder="Search by trip or client name" />
                </div>
                <div class="col-md-3">
                    <asp:Label ID="lblStatusFilter" runat="server" AssociatedControlID="ddlStatus" CssClass="form-label small mb-1">Status</asp:Label>
                    <asp:DropDownList ID="ddlStatus" ToolTip="Show only trips with this status." runat="server" CssClass="form-select">
                        <asp:ListItem Text="All statuses" Value="" />
                        <asp:ListItem Text="Draft" Value="Draft" />
                        <asp:ListItem Text="Confirmed" Value="Confirmed" />
                        <asp:ListItem Text="Completed" Value="Completed" />
                        <asp:ListItem Text="Cancelled" Value="Cancelled" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-4 d-flex gap-2">
                    <asp:Button ID="btnFilter" ToolTip="Show the trips that match." runat="server" Text="Filter"
                                CssClass="btn btn-outline-primary" CausesValidation="false" OnClick="btnFilter_Click" />
                    <asp:Button ID="btnClearFilter" ToolTip="Clear the filters and show every trip again." runat="server" Text="Clear"
                                CssClass="btn btn-outline-secondary" CausesValidation="false" OnClick="btnClearFilter_Click" />
                </div>
            </asp:Panel>

            <asp:GridView ID="gvMine" runat="server"
                          AutoGenerateColumns="false"
                          CssClass="table table-striped table-hover align-middle"
                          EmptyDataText="No trips match."
                          GridLines="None">
                <Columns>
                    <asp:BoundField DataField="TripName" HeaderText="Trip" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client" />
                    <asp:BoundField DataField="AgentName" HeaderText="Agent" />
                    <asp:BoundField DataField="DateRangeDisplay" HeaderText="Dates" />
                    <asp:BoundField DataField="TotalBudget" HeaderText="Budget"
                                    DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <asp:HyperLink ID="lnkOpen" ToolTip="Open the trip to edit it, approve requests or confirm it." runat="server"
                                           CssClass="btn btn-sm btn-primary text-nowrap"
                                           Text="Open"
                                           NavigateUrl='<%# "~/Itineraries/EditTrip.aspx?id=" + Eval("ItineraryId") %>' />
                            <asp:HyperLink ID="lnkBookTransport" ToolTip="Add a hire car, bus or flight to this trip." runat="server"
                                           CssClass="btn btn-sm btn-outline-primary text-nowrap"
                                           Text="Book transport"
                                           Visible='<%# Convert.ToBoolean(Eval("IsEditable")) %>'
                                           NavigateUrl='<%# "~/Itineraries/BookTransport.aspx?itineraryId=" + Eval("ItineraryId") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

</asp:Content>
