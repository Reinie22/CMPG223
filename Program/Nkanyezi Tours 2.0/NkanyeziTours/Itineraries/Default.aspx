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

            <asp:GridView ID="gvMine" runat="server"
                          AutoGenerateColumns="false"
                          CssClass="table table-striped table-hover align-middle"
                          EmptyDataText="No trips yet."
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
