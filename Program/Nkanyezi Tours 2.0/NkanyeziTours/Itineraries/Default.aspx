<%@ Page Title="Itineraries" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="Default.aspx.cs" Inherits="NkanyeziTours.Itineraries._Default" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-1">Itineraries</h1>
    <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-4">
        <p class="text-muted mb-0"><asp:Literal ID="litSubtitle" runat="server" /></p>
        <asp:HyperLink ID="lnkAssign" runat="server" Visible="false" CssClass="btn btn-outline-primary"
                       NavigateUrl="~/Itineraries/Assign.aspx">Assign trips</asp:HyperLink>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <%--
        THE QUEUE
        =========
        Trips a client started through the portal that no agent has taken yet.
        Claiming one puts the agent's name on it and takes it out of this list.
    --%>
    <div class="card mb-4">
        <div class="card-body">
            <h2 class="h5 mb-3">
                Waiting to be picked up
                <asp:Label ID="lblQueueCount" runat="server" CssClass="badge text-bg-secondary ms-1" />
            </h2>

            <asp:GridView ID="gvUnassigned" runat="server"
                          AutoGenerateColumns="false"
                          DataKeyNames="ItineraryId"
                          OnRowCommand="gvUnassigned_RowCommand"
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
                                    DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkClaim" runat="server"
                                            CssClass="btn btn-sm btn-primary text-nowrap"
                                            CommandName="ClaimTrip"
                                            CommandArgument='<%# Eval("ItineraryId") %>'
                                            Visible='<%# CanClaim %>'
                                            Text="Claim this trip" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

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
                            <asp:HyperLink ID="lnkOpen" runat="server"
                                           CssClass="btn btn-sm btn-primary text-nowrap"
                                           Text="Open"
                                           NavigateUrl='<%# "~/Itineraries/EditTrip.aspx?id=" + Eval("ItineraryId") %>' />
                            <asp:HyperLink ID="lnkBookTransport" runat="server"
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
