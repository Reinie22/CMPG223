<%@ Page Title="Trip" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="EditTrip.aspx.cs" Inherits="NkanyeziTours.Itineraries.EditTrip" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

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

        <%-- WHAT THE TRIP COSTS SO FAR --%>
        <div class="card mb-4">
            <div class="card-body">
                <h2 class="h5 mb-3">Budget</h2>
                <div class="row g-3 mb-3">
                    <div class="col-6 col-md-3">
                        <div class="text-muted small">Budget</div>
                        <div class="fs-5 fw-semibold"><asp:Literal ID="litBudgetTotal" runat="server" /></div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="text-muted small">Bookings</div>
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

        <%-- THE DETAILS THE AGENT MAY CHANGE --%>
        <div class="card mb-4">
            <div class="card-body">
                <h2 class="h5 mb-3">Trip details</h2>
                <div class="row g-3">

                    <div class="col-md-8">
                        <asp:Label ID="lblTripName" runat="server" AssociatedControlID="txtTripName"
                                   CssClass="form-label">Trip name <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtTripName" runat="server" CssClass="form-control" MaxLength="120" />
                        <asp:RequiredFieldValidator ID="rfvTripName" runat="server" ControlToValidate="txtTripName"
                                                    ValidationGroup="Trip" Display="Dynamic" CssClass="text-danger small"
                                                    ErrorMessage="The trip needs a name." />
                    </div>

                    <div class="col-md-4">
                        <asp:Label ID="lblBudget" runat="server" AssociatedControlID="txtBudget"
                                   CssClass="form-label">Budget <span class="text-danger">*</span></asp:Label>
                        <div class="input-group">
                            <span class="input-group-text">R</span>
                            <asp:TextBox ID="txtBudget" runat="server" CssClass="form-control" />
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
                        <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>

                    <div class="col-md-4">
                        <asp:Label ID="lblEndDate" runat="server" AssociatedControlID="txtEndDate"
                                   CssClass="form-label">End date</asp:Label>
                        <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>

                    <div class="col-12">
                        <asp:Label ID="lblComments" runat="server" AssociatedControlID="txtComments"
                                   CssClass="form-label">Notes</asp:Label>
                        <asp:TextBox ID="txtComments" runat="server" CssClass="form-control"
                                     TextMode="MultiLine" Rows="4" />
                    </div>

                </div>

                <hr class="my-4" />

                <div class="d-flex flex-wrap justify-content-between gap-2">
                    <div class="d-flex flex-wrap gap-2">
                        <asp:Button ID="btnSave" runat="server" Text="Save changes" CssClass="btn btn-primary"
                                    ValidationGroup="Trip" OnClick="btnSave_Click" />
                        <asp:HyperLink ID="lnkBookTransport" runat="server" CssClass="btn btn-outline-primary"
                                       Text="Book transport" />
                        <asp:HyperLink ID="lnkBack" runat="server" CssClass="btn btn-outline-secondary"
                                       NavigateUrl="~/Itineraries/Default.aspx">Back to itineraries</asp:HyperLink>
                    </div>

                    <div class="d-flex flex-wrap gap-2">
                        <asp:Button ID="btnConfirm" runat="server" Text="Confirm trip" CssClass="btn btn-success"
                                    CausesValidation="false" OnClick="btnConfirm_Click" />
                        <asp:Button ID="btnComplete" runat="server" Text="Mark completed" CssClass="btn btn-outline-success"
                                    CausesValidation="false" OnClick="btnComplete_Click" />
                        <asp:Button ID="btnCancelTrip" runat="server" Text="Cancel trip" CssClass="btn btn-outline-danger"
                                    CausesValidation="false" OnClick="btnCancelTrip_Click" />
                    </div>
                </div>
            </div>
        </div>

        <%--
            WHAT THE CLIENT HAS ASKED FOR
            =============================
            TripRequestItem rows the client put on their own draft. Approving
            one does not itself book anything - it records the agent's decision.
            The real booking is still made on the booking screens, which is
            where capacity and the real price are settled.
        --%>
        <asp:Panel ID="pnlRequests" runat="server" Visible="false" CssClass="card mb-4">
            <div class="card-body">
                <h2 class="h5 mb-3">
                    Asked for by the client
                    <asp:Label ID="lblPendingCount" runat="server" CssClass="badge text-bg-warning ms-1" />
                </h2>

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
                                           Text='<%# Eval("ItemType") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ItemName" HeaderText="Choice" />
                        <asp:BoundField DataField="ItemLocation" HeaderText="Where" />
                        <asp:BoundField DataField="DateDisplay" HeaderText="When" />
                        <asp:BoundField DataField="Guests" HeaderText="People" />
                        <asp:BoundField DataField="EstimatedCost" HeaderText="Guide cost"
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
                                    <asp:LinkButton ID="lnkApprove" runat="server"
                                                    CssClass="btn btn-sm btn-success text-nowrap"
                                                    CommandName="ApproveRequest"
                                                    CommandArgument='<%# Eval("TripRequestItemId") %>'
                                                    CausesValidation="false"
                                                    Visible='<%# Convert.ToBoolean(Eval("IsRequested")) %>'
                                                    Text="Approve" />
                                    <asp:LinkButton ID="lnkDecline" runat="server"
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
                    Approving records the decision. Make the actual booking on the
                    booking screens, where the room or slot and the real price are captured.
                </p>
            </div>
        </asp:Panel>

        <%-- EVERYTHING BOOKED ON THE TRIP --%>
        <div class="card">
            <div class="card-body">
                <h2 class="h5 mb-3">What is booked</h2>
                <asp:GridView ID="gvBookings" runat="server"
                              AutoGenerateColumns="false"
                              CssClass="table table-striped align-middle"
                              EmptyDataText="Nothing booked on this trip yet."
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
