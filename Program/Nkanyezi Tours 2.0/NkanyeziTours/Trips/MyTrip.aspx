<%@ Page Title="My trip" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="MyTrip.aspx.cs" Inherits="NkanyeziTours.Trips.MyTrip" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="d-flex justify-content-between align-items-start mb-1">
        <h1 class="h3 mb-0"><asp:Literal ID="litHeading" runat="server" /></h1>
        <asp:Label ID="lblStatus" runat="server" CssClass="badge" />
    </div>
    <p class="text-muted mb-4"><asp:Literal ID="litSubtitle" runat="server" /></p>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlTrip" runat="server">

        <%-- BUDGET, against the guide prices of what they have chosen --%>
        <div class="card mb-4">
            <div class="card-body">
                <div class="row g-3">
                    <div class="col-6 col-md-3">
                        <div class="text-muted small">Your budget</div>
                        <div class="fs-5 fw-semibold"><asp:Literal ID="litBudget" runat="server" /></div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="text-muted small">Chosen so far (guide)</div>
                        <div class="fs-5 fw-semibold"><asp:Literal ID="litEstimate" runat="server" /></div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="text-muted small">Already booked</div>
                        <div class="fs-5 fw-semibold"><asp:Literal ID="litBooked" runat="server" /></div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="text-muted small">Dates</div>
                        <div class="fs-5 fw-semibold"><asp:Literal ID="litDates" runat="server" /></div>
                    </div>
                </div>
                <p class="text-muted small mb-0 mt-3">
                    Guide prices only. Your agent confirms the real cost when they book.
                </p>
            </div>
        </div>

        <%-- WHAT THE CLIENT HAS CHOSEN --%>
        <div class="card mb-4">
            <div class="card-body">
                <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
                    <h2 class="h5 mb-0">What you have chosen</h2>
                    <asp:Panel ID="pnlAddButtons" runat="server" CssClass="d-flex flex-wrap gap-2">
                        <asp:HyperLink ID="lnkAddAccommodation" runat="server" CssClass="btn btn-primary btn-sm"
                                       Text="Add a place to stay" />
                        <asp:HyperLink ID="lnkAddAttraction" runat="server" CssClass="btn btn-primary btn-sm"
                                       Text="Add something to do" />
                    </asp:Panel>
                </div>

                <asp:GridView ID="gvRequests" runat="server"
                              AutoGenerateColumns="false"
                              DataKeyNames="TripRequestItemId"
                              OnRowCommand="gvRequests_RowCommand"
                              CssClass="table table-striped align-middle"
                              EmptyDataText="Nothing chosen yet. Use the buttons above to pick places to stay and things to do."
                              GridLines="None">
                    <Columns>
                        <asp:TemplateField HeaderText="Type">
                            <ItemTemplate>
                                <asp:Label ID="lblKind" runat="server" CssClass="badge text-bg-light"
                                           Text='<%# Eval("ItemType") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ItemName" HeaderText="Choice" />
                        <asp:BoundField DataField="ItemLocation" HeaderText="Where" />
                        <asp:BoundField DataField="DateDisplay" HeaderText="When" />
                        <asp:BoundField DataField="Guests" HeaderText="Guests" />
                        <asp:BoundField DataField="EstimatedCost" HeaderText="Guide cost"
                                        DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <asp:Label ID="lblStatusCell" runat="server"
                                           CssClass='<%# "badge " + Eval("StatusCssClass") %>'
                                           Text='<%# Eval("Status") %>' />
                                <div class="text-muted small"><%# Eval("AgentNote") %></div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="">
                            <ItemTemplate>
                                <%-- Only while the agent has not decided yet. --%>
                                <asp:LinkButton ID="lnkRemove" runat="server"
                                                CssClass="btn btn-sm btn-outline-danger text-nowrap"
                                                CommandName="RemoveRequest"
                                                CommandArgument='<%# Eval("TripRequestItemId") %>'
                                                Visible='<%# Convert.ToBoolean(Eval("IsRequested")) %>'
                                                Text="Remove" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <%-- WHAT THE AGENT HAS ACTUALLY BOOKED --%>
        <div class="card">
            <div class="card-body">
                <h2 class="h5 mb-3">Confirmed by your agent</h2>
                <asp:GridView ID="gvBookings" runat="server"
                              AutoGenerateColumns="false"
                              CssClass="table table-striped align-middle"
                              EmptyDataText="Your agent has not booked anything yet."
                              GridLines="None">
                    <Columns>
                        <asp:TemplateField HeaderText="Type">
                            <ItemTemplate>
                                <asp:Label ID="lblType" runat="server"
                                           CssClass='<%# "badge " + Eval("TypeCssClass") %>'
                                           Text='<%# Eval("BookingType") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Description" HeaderText="Detail" />
                        <asp:BoundField DataField="Date" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" />
                        <asp:BoundField DataField="TotalCost" HeaderText="Cost"
                                        DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>

    </asp:Panel>

</asp:Content>
