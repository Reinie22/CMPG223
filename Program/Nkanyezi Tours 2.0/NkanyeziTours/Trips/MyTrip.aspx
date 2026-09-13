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

        <div class="card mb-4">
            <div class="card-body">
                <div class="row g-3">
                    <div class="col-6 col-md-3">
                        <div class="text-muted small">Your budget</div>
                        <div class="fs-5 fw-semibold"><asp:Literal ID="litBudget" runat="server" /></div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="text-muted small">Planned so far</div>
                        <div class="fs-5 fw-semibold"><asp:Literal ID="litEstimate" runat="server" /></div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="text-muted small"><asp:Literal ID="litBalanceLabel" runat="server" /></div>
                        <div class="fs-5 fw-semibold">
                            <asp:Label ID="lblBalance" runat="server" />
                        </div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="text-muted small">Confirmed by your agent</div>
                        <div class="fs-5 fw-semibold"><asp:Literal ID="litBooked" runat="server" /></div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="text-muted small">Start and end</div>
                        <div class="fs-5 fw-semibold"><asp:Literal ID="litDates" runat="server" /></div>
                    </div>
                </div>
                <p class="text-muted small mb-0 mt-3">
                    Your dates come from your legs - the first leg&#39;s start to the last
                    leg&#39;s end. Prices are guides; your agent confirms the real cost.
                </p>
            </div>
        </div>

        <h2 class="h5 mb-3">
            Trip legs
            <asp:Label ID="lblLegCount" runat="server" CssClass="badge text-bg-secondary ms-1" />
        </h2>

        <asp:Repeater ID="rptLegs" runat="server" OnItemCommand="rptLegs_ItemCommand">
            <ItemTemplate>
                <div class='<%# "card mb-3 leg-card" + (Convert.ToBoolean(Eval("HasResort")) ? "" : " leg-card-empty") %>'>
                    <div class="card-body">

                        <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-3">
                            <div>
                                <div class="d-flex align-items-center gap-2">
                                    <span class="leg-number"><%# Eval("LegNumber") %></span>
                                    <span class="h6 mb-0"><%# Server.HtmlEncode(Convert.ToString(Eval("Title"))) %></span>
                                </div>
                                <div class="text-muted small mt-1">
                                    <%# Server.HtmlEncode(Convert.ToString(Eval("RegionDisplay"))) %>
                                    &middot; <%# Eval("DateDisplay") %>
                                    &middot; <%# Eval("Guests") %> guests
                                </div>
                                <div class="small mt-1">
                                    <span class="text-muted">Expenses on this leg</span>
                                    <span class="fw-semibold">R<%# string.Format("{0:N2}", Eval("LegExpenses")) %></span>
                                </div>
                            </div>
                            <div class="d-flex flex-wrap gap-1">
                                <asp:LinkButton ID="lnkRemoveLeg" ToolTip="Removes this leg. The legs after it shift back to close the gap." runat="server"
                                                CssClass="btn btn-sm btn-outline-danger text-nowrap"
                                                CommandName="RemoveLeg"
                                                CommandArgument='<%# Eval("TripLegId") %>'
                                                CausesValidation="false" Text="Remove leg"
                                                data-confirm="Remove this leg, all changes will be lost." />
                            </div>
                        </div>

                        <div class="row g-3">

                            <div class="col-md-5">
                                <div class="text-uppercase text-muted small fw-semibold mb-2">Resort</div>

                                <asp:Panel ID="pnlResort" runat="server" Visible='<%# Convert.ToBoolean(Eval("HasResort")) %>'>
                                    <div class="choice-card">
                                        <div class="choice-card-photo">
                                            <asp:Image ID="imgResort" runat="server"
                                                       Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("ResortImageUrl"))) %>'
                                                       ImageUrl='<%# Eval("ResortImageUrl") %>'
                                                       AlternateText='<%# Eval("ResortName") %>' />
                                            <asp:Label ID="lblResortPhotoHint" runat="server"
                                                       CssClass="placeholder-text"
                                                       Visible='<%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("ResortImageUrl"))) %>'
                                                       Text="Photo to come" />
                                        </div>
                                        <div class="choice-card-body">
                                            <div class="choice-card-title"><%# Server.HtmlEncode(Convert.ToString(Eval("ResortName"))) %></div>
                                            <div class="choice-card-meta">
                                                <%# Eval("ResortType") %> &middot; <%# Eval("ResortLocation") %>
                                            </div>
                                            <div class="choice-card-meta mt-1">
                                                <strong><%#: Eval("UnitDisplay") %></strong>
                                            </div>

                                            <asp:Panel ID="pnlUnitTooSmall" runat="server"
                                                       Visible='<%# Convert.ToBoolean(Eval("UnitTooSmall")) %>'
                                                       CssClass="alert alert-danger py-1 px-2 small mt-2 mb-0">
                                                The rooms chosen hold fewer people than the guests on this leg.
                                            </asp:Panel>

                                            <div class="mt-2 choice-card-price">
                                                R<%# string.Format("{0:N2}", Eval("EstimatedResortCost")) %>
                                                <span class="text-muted fw-normal small">
                                                    for <%# Eval("Nights") %> night(s)
                                                </span>
                                            </div>
                                        </div>
                                    </div>

                                    <asp:HyperLink ID="lnkChooseUnit" runat="server"
                                                   CssClass="btn btn-sm btn-outline-primary mt-2"
                                                   ToolTip="Pick the rooms you stay in at this resort. You can choose more than one."
                                                   Text='<%# Convert.ToBoolean(Eval("HasUnit")) ? "Change rooms" : "Choose rooms" %>'
                                                   NavigateUrl='<%# "~/Trips/ChooseUnit.aspx?tripLegId=" + Eval("TripLegId") %>' />
                                </asp:Panel>

                                <asp:Panel ID="pnlNoResort" runat="server" Visible='<%# !Convert.ToBoolean(Eval("HasResort")) %>'>
                                    <div class="choice-card">
                                        <div class="choice-card-photo">
                                            <span class="placeholder-text">Nothing chosen</span>
                                        </div>
                                        <div class="choice-card-body text-muted small">
                                            You have not picked a resort for this leg yet.
                                        </div>
                                    </div>
                                </asp:Panel>

                                <asp:HyperLink ID="lnkChooseResort" ToolTip="Pick where you stay on this leg. One resort per leg." runat="server"
                                               CssClass="btn btn-sm btn-primary mt-2"
                                               Text='<%# Convert.ToBoolean(Eval("HasResort")) ? "Change resort" : "Choose a resort" %>'
                                               NavigateUrl='<%# "~/Trips/ChooseAccommodation.aspx?tripLegId=" + Eval("TripLegId") %>' />
                            </div>

                            <div class="col-md-7">
                                <div class="text-uppercase text-muted small fw-semibold mb-2">
                                    Activities
                                    <span class="badge text-bg-light"><%# Eval("ActivityCount") %></span>
                                </div>

                                <div class="leg-scroll">
                                <asp:Repeater ID="rptLegActivities" runat="server">
                                    <HeaderTemplate>
                                        <div class="row g-2">
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <div class="col-sm-6">
                                            <div class="choice-card">
                                                <div class="choice-card-body py-2">
                                                    <div class="d-flex justify-content-between align-items-start gap-2">
                                                        <div>
                                                            <div class="choice-card-title" style="font-size:.9rem;">
                                                                <%# Server.HtmlEncode(Convert.ToString(Eval("ItemName"))) %>
                                                            </div>
                                                            <div class="choice-card-meta"><%# Eval("DateDisplay") %></div>
                                                        </div>
                                                        <asp:Label ID="lblActStatus" runat="server"
                                                                   CssClass='<%# "badge " + Eval("StatusCssClass") %>'
                                                                   Text='<%# Eval("Status") %>' />
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </div>
                                    </FooterTemplate>
                                </asp:Repeater>

                                </div>

                                <asp:Panel ID="pnlNoActivities" runat="server" CssClass="text-muted small"
                                           Visible='<%# Convert.ToInt32(Eval("ActivityCount")) == 0 %>'>
                                    Nothing planned for this leg yet.
                                </asp:Panel>

                                <asp:HyperLink ID="lnkAddActivities" ToolTip="Pick things to do on this leg. You can add as many as you like." runat="server"
                                               CssClass="btn btn-sm btn-outline-primary mt-2"
                                               Text="Add activities"
                                               NavigateUrl='<%# "~/Trips/ChooseAttraction.aspx?tripLegId=" + Eval("TripLegId") %>' />

                                <div class="text-uppercase text-muted small fw-semibold mb-2 mt-3">
                                    Transport
                                    <span class="badge text-bg-light"><%# Eval("TransportCount") %></span>
                                </div>

                                <div class="leg-scroll">
                                <asp:Repeater ID="rptLegTransport" runat="server">
                                    <HeaderTemplate>
                                        <div class="row g-2">
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <div class="col-sm-6">
                                            <div class="choice-card">
                                                <div class="choice-card-body py-2">
                                                    <div class="d-flex justify-content-between align-items-start gap-2">
                                                        <div>
                                                            <div class="choice-card-title" style="font-size:.9rem;">
                                                                <asp:Label ID="lblTKind" runat="server"
                                                                           CssClass='<%# "badge me-1 " + Eval("TypeCssClass") %>'
                                                                           Text='<%# Eval("ItemType") %>' />
                                                                <%# Server.HtmlEncode(Convert.ToString(Eval("ItemName"))) %>
                                                            </div>
                                                            <div class="choice-card-meta"><%# Eval("DateDisplay") %></div>
                                                        </div>
                                                        <asp:Label ID="lblTStatus" runat="server"
                                                                   CssClass='<%# "badge " + Eval("StatusCssClass") %>'
                                                                   Text='<%# Eval("Status") %>' />
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </div>
                                    </FooterTemplate>
                                </asp:Repeater>

                                </div>

                                <asp:Panel ID="pnlNoTransport" runat="server" CssClass="text-muted small"
                                           Visible='<%# Convert.ToInt32(Eval("TransportCount")) == 0 %>'>
                                    No transport on this leg yet.
                                </asp:Panel>

                                <asp:HyperLink ID="lnkAddTransport" ToolTip="Pick how you get around on this leg." runat="server"
                                               CssClass="btn btn-sm btn-outline-primary mt-2"
                                               Text="Add transport"
                                               NavigateUrl='<%# "~/Trips/ChooseTransport.aspx?tripLegId=" + Eval("TripLegId") %>' />
                            </div>

                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Panel ID="pnlNoLegs" runat="server" Visible="false" CssClass="alert alert-light border">
            Your trip has no legs yet. A leg is one stop - where you are staying and
            for how long. Add the first one below.
        </asp:Panel>

        <asp:Panel ID="pnlAddLeg" runat="server" CssClass="card mb-4">
            <div class="card-body">
                <h2 class="h5 mb-3"><asp:Literal ID="litAddLegHeading" runat="server" Text="Add the next leg" /></h2>

                <div class="row g-3 align-items-end">
                    <div class="col-md-3">
                        <asp:Label ID="lblRegion" runat="server" AssociatedControlID="ddlRegion"
                                   CssClass="form-label">Region <span class="text-danger">*</span></asp:Label>
                        <asp:DropDownList ID="ddlRegion" ToolTip="Which province this leg is in. It decides which resorts, activities and cars you are offered." runat="server" CssClass="form-select" />
                        <asp:RequiredFieldValidator ID="rfvRegion" runat="server" ControlToValidate="ddlRegion"
                                                    InitialValue="" ValidationGroup="Leg" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Choose a region." />
                    </div>

                    <asp:Panel ID="pnlFirstLegStart" runat="server" CssClass="col-md-3" Visible="false">
                        <asp:Label ID="lblStart" runat="server" AssociatedControlID="txtStart"
                                   CssClass="form-label">Start date <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtStart" ToolTip="When this leg begins. Each leg starts the day the one before it ends." runat="server" CssClass="form-control" TextMode="Date" />
                        <asp:RequiredFieldValidator ID="rfvStart" runat="server" ControlToValidate="txtStart"
                                                    ValidationGroup="Leg" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Choose a start date." />
                    </asp:Panel>

                    <div class="col-md-2">
                        <asp:Label ID="lblNights" runat="server" AssociatedControlID="txtNights"
                                   CssClass="form-label">Nights <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtNights" ToolTip="How many nights you stay on this leg." runat="server" CssClass="form-control" Text="3" />
                        <asp:RequiredFieldValidator ID="rfvNights" runat="server" ControlToValidate="txtNights"
                                                    ValidationGroup="Leg" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Enter the nights." />
                        <asp:RangeValidator ID="rvNights" runat="server" ControlToValidate="txtNights"
                                            ValidationGroup="Leg" Display="Dynamic"
                                            Type="Integer" MinimumValue="1" MaximumValue="365"
                                            CssClass="text-danger small"
                                            ErrorMessage="At least 1 night." />
                    </div>

                    <div class="col-md-2">
                        <asp:Label ID="lblGuests" runat="server" AssociatedControlID="txtGuests"
                                   CssClass="form-label">Guests <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtGuests" ToolTip="How many people are travelling on this leg." runat="server" CssClass="form-control" Text="2" />
                        <asp:RequiredFieldValidator ID="rfvGuests" runat="server" ControlToValidate="txtGuests"
                                                    ValidationGroup="Leg" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Enter the guests." />
                        <asp:RangeValidator ID="rvGuests" runat="server" ControlToValidate="txtGuests"
                                            ValidationGroup="Leg" Display="Dynamic"
                                            Type="Integer" MinimumValue="1" MaximumValue="200"
                                            CssClass="text-danger small"
                                            ErrorMessage="At least 1 guest." />
                    </div>

                    <div class="col-md-1">
                        <asp:Button ID="btnAddLeg" ToolTip="Adds the leg to the end of your trip and moves your end date out." runat="server" Text="Add" CssClass="btn btn-primary w-100"
                                    ValidationGroup="Leg" OnClick="btnAddLeg_Click" />
                    </div>
                </div>

                <p class="text-muted small mb-0 mt-3">
                    <asp:Literal ID="litChainHint" runat="server" />
                </p>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlConfirm" runat="server" Visible="false" CssClass="card mb-4">
            <div class="card-body d-flex flex-wrap justify-content-between align-items-center gap-3">
                <div>
                    <h2 class="h5 mb-1">Happy with your trip?</h2>
                    <p class="text-muted small mb-0">
                        After confirming the trip, you will need to contact the agent to
                        make any change to your booking.
                    </p>
                </div>
                <div class="d-flex flex-wrap gap-2">
                    <asp:Button ID="btnCancelBooking" ToolTip="Cancels the whole trip. This cannot be undone." runat="server" Text="Cancel booking"
                                CssClass="btn btn-outline-danger" CausesValidation="false"
                                OnClick="btnCancelBooking_Click"
                                data-confirm="Are you sure? This cancels the whole trip." />
                    <asp:Button ID="btnConfirmBooking" ToolTip="Sends the trip to an agent to book. After this you cannot change it yourself." runat="server" Text="Confirm booking"
                                CssClass="btn btn-success" CausesValidation="false"
                                OnClick="btnConfirmBooking_Click"
                                data-confirm="After confirming the trip, you will need to contact the agent to make any change to your booking." />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlClosed" runat="server" Visible="false" CssClass="alert alert-secondary">
            <asp:Literal ID="litClosed" runat="server" />
        </asp:Panel>

        <asp:Panel ID="pnlUnplaced" runat="server" Visible="false" CssClass="card mb-4">
            <div class="card-body">
                <h2 class="h5 mb-1">Not on a leg yet</h2>
                <p class="text-muted small mb-3">
                    These were chosen before your trip was split into legs, or the leg they
                    were on was removed. Your agent can still see them.
                </p>
                <asp:GridView ID="gvUnplaced" runat="server"
                              AutoGenerateColumns="false"
                              DataKeyNames="TripRequestItemId"
                              OnRowCommand="gvUnplaced_RowCommand"
                              CssClass="table table-sm table-striped align-middle mb-0"
                              GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="ItemType" HeaderText="Type" />
                        <asp:BoundField DataField="ItemName" HeaderText="Choice" />
                        <asp:BoundField DataField="DateDisplay" HeaderText="When" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <asp:Label ID="lblSt" runat="server"
                                           CssClass='<%# "badge " + Eval("StatusCssClass") %>'
                                           Text='<%# Eval("Status") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkRemove" ToolTip="Takes this off the leg." runat="server"
                                                CssClass="btn btn-sm btn-outline-danger text-nowrap"
                                                CommandName="RemoveRequest"
                                                CommandArgument='<%# Eval("TripRequestItemId") %>'
                                                CausesValidation="false"
                                                Visible='<%# Convert.ToBoolean(Eval("IsRequested")) %>'
                                                Text="Remove" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </asp:Panel>

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
                        <asp:BoundField DataField="Date" HeaderText="Date" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="TotalCost" HeaderText="Cost"
                                        DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>

    </asp:Panel>

</asp:Content>
