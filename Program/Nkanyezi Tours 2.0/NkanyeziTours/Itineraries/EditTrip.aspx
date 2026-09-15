<%@ Page Title="Trip" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="EditTrip.aspx.cs" Inherits="NkanyeziTours.Itineraries.EditTrip" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="mb-3">
        <asp:HyperLink ID="lnkBackTop" ToolTip="Go back to the list of itineraries." runat="server" CssClass="btn btn-outline-secondary"
                       NavigateUrl="~/Itineraries/Default.aspx">&#8592; Back to itineraries</asp:HyperLink>
    </div>

    <div class="d-flex justify-content-between align-items-start mb-1">
        <h1 class="h3 mb-0"><asp:Literal ID="litHeading" runat="server" /></h1>
        <asp:Label ID="lblStatus" runat="server" CssClass="badge" />
    </div>
    <p class="text-muted mb-4"><asp:Literal ID="litSubtitle" runat="server" /></p>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="Trip"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <asp:Panel ID="pnlTrip" runat="server">

        <div class="card mb-4">
            <div class="card-body">
                <h2 class="h5 mb-3">Budget</h2>
                <div class="row g-3 mb-3">
                    <div class="col-6 col-md-3">
                        <div class="text-muted small">Budget</div>
                        <div class="fs-5 fw-semibold"><asp:Literal ID="litBudgetTotal" runat="server" /></div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="text-muted small" title="Everything you have approved: accommodation, activities and transport.">Amount</div>
                        <div class="fs-5 fw-semibold"><asp:Literal ID="litBookingsTotal" runat="server" /></div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="text-muted small">Expenses</div>
                        <div class="fs-5 fw-semibold"><asp:Literal ID="litExpensesTotal" runat="server" /></div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="text-muted small"><asp:Literal ID="litRemainingLabel" runat="server" /></div>
                        <div class="fs-5 fw-semibold"><asp:Literal ID="litRemaining" runat="server" /></div>
                    </div>
                </div>

                <div class="progress" style="height: .6rem;">
                    <asp:Panel ID="pnlBudgetBar" runat="server" CssClass="progress-bar" />
                </div>

                <asp:Panel ID="pnlOverBudget" runat="server" Visible="false"
                           CssClass="alert alert-warning py-2 mt-3 mb-0">
                    This trip is over its budget. Either raise the budget below, or drop
                    something from the plan.
                </asp:Panel>
            </div>
        </div>

        <div class="card mb-4">
            <div class="card-body">
                <h2 class="h5 mb-3">Trip details</h2>
                <div class="row g-3">

                    <div class="col-md-8">
                        <asp:Label ID="lblTripName" runat="server" AssociatedControlID="txtTripName"
                                   CssClass="form-label">Trip name <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtTripName" ToolTip="What the client calls this trip. It shows on their dashboard." runat="server" CssClass="form-control" MaxLength="120" />
                        <asp:RequiredFieldValidator ID="rfvTripName" runat="server" ControlToValidate="txtTripName"
                                                    ValidationGroup="Trip" Display="Dynamic" CssClass="text-danger small"
                                                    ErrorMessage="The trip needs a name." />
                    </div>

                    <div class="col-md-4">
                        <asp:Label ID="lblBudget" runat="server" AssociatedControlID="txtBudget"
                                   CssClass="form-label">Budget <span class="text-danger">*</span></asp:Label>
                        <div class="input-group">
                            <span class="input-group-text">R</span>
                            <asp:TextBox ID="txtBudget" ToolTip="What the client has to spend. Costs are measured against this, but nothing is blocked when it is exceeded." runat="server" CssClass="form-control" />
                        </div>
                        <asp:RequiredFieldValidator ID="rfvBudget" runat="server" ControlToValidate="txtBudget"
                                                    ValidationGroup="Trip" Display="Dynamic" CssClass="text-danger small"
                                                    ErrorMessage="The budget is required." />
                        <asp:RangeValidator ID="rvBudget" runat="server" ControlToValidate="txtBudget"
                                            ValidationGroup="Trip" Display="Dynamic"
                                            Type="Currency" MinimumValue="0.01" MaximumValue="99999999"
                                            CssClass="text-danger small"
                                            ErrorMessage="The budget must be greater than zero." />
                    </div>

                    <div class="col-md-4">
                        <asp:Label ID="lblStartDate" runat="server" AssociatedControlID="txtStartDate"
                                   CssClass="form-label">Start date</asp:Label>
                        <asp:TextBox ID="txtStartDate" ToolTip="The first day of the trip." runat="server" CssClass="form-control" TextMode="Date" />
                    </div>

                    <div class="col-md-4">
                        <asp:Label ID="lblEndDate" runat="server" AssociatedControlID="txtEndDate"
                                   CssClass="form-label">End date</asp:Label>
                        <asp:TextBox ID="txtEndDate" ToolTip="The last day of the trip." runat="server" CssClass="form-control" TextMode="Date" />
                    </div>

                    <div class="col-12">
                        <asp:Label ID="lblComments" runat="server" AssociatedControlID="txtComments"
                                   CssClass="form-label">Notes</asp:Label>
                        <asp:TextBox ID="txtComments" ToolTip="Notes for whoever picks this trip up next. The client does not see these." runat="server" CssClass="form-control"
                                     TextMode="MultiLine" Rows="4" />
                    </div>

                </div>

                <hr class="my-4" />

                <div class="d-flex flex-wrap justify-content-between gap-2">
                    <div class="d-flex flex-wrap gap-2">
                        <asp:Button ID="btnSave" ToolTip="Saves your changes. The trip stays in its current state." runat="server" Text="Save changes" CssClass="btn btn-primary"
                                    ValidationGroup="Trip" OnClick="btnSave_Click" />
                        <asp:HyperLink ID="lnkBookTransport" ToolTip="Add a hire car, bus or flight to this trip." runat="server" CssClass="btn btn-outline-primary"
                                       Text="Book transport" />
                        <asp:HyperLink ID="lnkBack" ToolTip="Back to the trip list without saving." runat="server" CssClass="btn btn-outline-secondary"
                                       NavigateUrl="~/Itineraries/Default.aspx">Back to itineraries</asp:HyperLink>
                    </div>

                    <div class="d-flex flex-wrap gap-2">
                        <asp:Button ID="btnConfirm" ToolTip="Confirms the trip with the client. A confirmed trip cannot be put back to draft." runat="server" Text="Confirm trip" CssClass="btn btn-success"
                                    CausesValidation="false" OnClick="btnConfirm_Click" />
                        <asp:Button ID="btnComplete" ToolTip="Marks the trip as finished. Use this once the client is home." runat="server" Text="Mark completed" CssClass="btn btn-outline-success"
                                    CausesValidation="false" OnClick="btnComplete_Click" />
                        <asp:Button ID="btnCancelTrip" ToolTip="Cancels the trip. This cannot be undone, and a cancelled trip can no longer be edited." runat="server" Text="Cancel trip" CssClass="btn btn-outline-danger"
                                    CausesValidation="false" OnClick="btnCancelTrip_Click" />
                    </div>
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlRequests" runat="server" Visible="false" CssClass="card mb-4">
            <div class="card-body">
                <h2 class="h5 mb-3">
                    Asked for by the client
                    <asp:Label ID="lblPendingCount" runat="server" CssClass="badge text-bg-warning ms-1" />
                </h2>

                <asp:Panel ID="pnlResorts" runat="server" Visible="false">
                    <h3 class="h6 text-uppercase text-muted mb-2">Accommodation</h3>
                    <asp:GridView ID="gvResorts" runat="server"
                                  AutoGenerateColumns="false"
                                  DataKeyNames="TripLegId"
                                  OnRowCommand="gvResorts_RowCommand"
                                  CssClass="table table-striped align-middle mb-4"
                                  GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="LegDisplay" HeaderText="Leg" />
                            <asp:BoundField DataField="ResortName" HeaderText="Choice" />
                            <asp:BoundField DataField="UnitDisplay" HeaderText="Rooms" />
                            <asp:BoundField DataField="DateDisplay" HeaderText="When" />
                            <asp:BoundField DataField="Guests" HeaderText="People" />
                            <asp:BoundField DataField="EstimatedResortCost" HeaderText="Cost"
                                            DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <asp:Label ID="lblResortStatus" runat="server"
                                               CssClass='<%# "badge " + Eval("ResortStatusCssClass") %>'
                                               Text='<%# Eval("ResortStatus") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="">
                                <ItemTemplate>
                                    <div class="d-flex gap-1">
                                        <asp:LinkButton ID="lnkApproveResort" ToolTip="Accepts the resort and rooms the client chose for this leg." runat="server"
                                                        CssClass="btn btn-sm btn-success text-nowrap"
                                                        CommandName="ApproveResort"
                                                        CommandArgument='<%# Eval("TripLegId") %>'
                                                        CausesValidation="false"
                                                        Visible='<%# Convert.ToBoolean(Eval("IsResortRequested")) %>'
                                                        Text="Approve" />
                                        <asp:LinkButton ID="lnkDeclineResort" ToolTip="Turns this accommodation down. The client sees it was declined." runat="server"
                                                        CssClass="btn btn-sm btn-outline-danger text-nowrap"
                                                        CommandName="DeclineResort"
                                                        CommandArgument='<%# Eval("TripLegId") %>'
                                                        CausesValidation="false"
                                                        Visible='<%# Convert.ToBoolean(Eval("IsResortRequested")) %>'
                                                        Text="Decline" />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <h3 class="h6 text-uppercase text-muted mb-2">Activities and transport</h3>
                </asp:Panel>

                <asp:GridView ID="gvRequests" runat="server"
                              AutoGenerateColumns="false"
                              DataKeyNames="TripRequestItemId"
                              OnRowCommand="gvRequests_RowCommand"
                              CssClass="table table-striped align-middle"
                              EmptyDataText="The client has not asked for anything."
                              GridLines="None">
                    <Columns>
                        <asp:TemplateField HeaderText="Type">
                            <ItemTemplate>
                                <asp:Label ID="lblKind" runat="server" CssClass="badge text-bg-light"
                                           Text='<%# Eval("TypeDisplay") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ItemName" HeaderText="Choice" />
                        <asp:BoundField DataField="ItemLocation" HeaderText="Where" />
                        <asp:BoundField DataField="DateDisplay" HeaderText="When" />
                        <asp:BoundField DataField="Guests" HeaderText="People" />
                        <asp:BoundField DataField="EstimatedCost" HeaderText="Cost"
                                        DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                        <asp:TemplateField HeaderText="Client note">
                            <ItemTemplate>
                                <span class="text-muted small"><%# Eval("ClientNote") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <asp:Label ID="lblReqStatus" runat="server"
                                           CssClass='<%# "badge " + Eval("StatusCssClass") %>'
                                           Text='<%# Eval("Status") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="">
                            <ItemTemplate>
                                <div class="d-flex gap-1">
                                    <asp:LinkButton ID="lnkApprove" ToolTip="Accepts what the client asked for and adds it to the trip." runat="server"
                                                    CssClass="btn btn-sm btn-success text-nowrap"
                                                    CommandName="ApproveRequest"
                                                    CommandArgument='<%# Eval("TripRequestItemId") %>'
                                                    CausesValidation="false"
                                                    Visible='<%# Convert.ToBoolean(Eval("IsRequested")) %>'
                                                    Text="Approve" />
                                    <asp:LinkButton ID="lnkDecline" ToolTip="Turns this request down. The client sees it was declined." runat="server"
                                                    CssClass="btn btn-sm btn-outline-danger text-nowrap"
                                                    CommandName="DeclineRequest"
                                                    CommandArgument='<%# Eval("TripRequestItemId") %>'
                                                    CausesValidation="false"
                                                    Visible='<%# Convert.ToBoolean(Eval("IsRequested")) %>'
                                                    Text="Decline" />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

                <p class="text-muted small mb-0 mt-2">
                    Approving an item adds its cost to the trip amount above. Declined and waiting
                    items are not counted.
                </p>
            </div>
        </asp:Panel>

    </asp:Panel>

</asp:Content>
