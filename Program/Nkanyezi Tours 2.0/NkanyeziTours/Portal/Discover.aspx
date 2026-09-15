<%@ Page Title="Discover" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="Discover.aspx.cs" Inherits="NkanyeziTours.Portal.Discover" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-1">Discover South Africa</h1>
    <p class="text-muted mb-4">
        Accommodations, activities and transport across the country.
    </p>

    <ul class="nav nav-tabs mb-3">
        <li class="nav-item">
            <asp:LinkButton ID="lnkTabStay" ToolTip="Resorts, lodges, hotels and campsites." runat="server" CssClass="nav-link"
                            CausesValidation="false" OnClick="lnkTabStay_Click"
                            Text="Accommodations" />
        </li>
        <li class="nav-item">
            <asp:LinkButton ID="lnkTabDo" ToolTip="Activities you can add to a trip." runat="server" CssClass="nav-link"
                            CausesValidation="false" OnClick="lnkTabDo_Click"
                            Text="Activities and leisure" />
        </li>
        <li class="nav-item">
            <asp:LinkButton ID="lnkTabGo" ToolTip="Car rentals, flights and bus tickets." runat="server" CssClass="nav-link"
                            CausesValidation="false" OnClick="lnkTabGo_Click"
                            Text="Transport" />
        </li>
    </ul>

    <asp:Panel ID="pnlGoKinds" runat="server" CssClass="nav nav-pills mb-4 gap-2">
        <asp:LinkButton ID="lnkGoCars" ToolTip="Cars you can hire, per day." runat="server" CssClass="nav-link"
                        CausesValidation="false" OnClick="lnkGoCars_Click" Text="Car rentals" />
        <asp:LinkButton ID="lnkGoFlights" ToolTip="Scheduled flights, per seat." runat="server" CssClass="nav-link"
                        CausesValidation="false" OnClick="lnkGoFlights_Click" Text="Flights" />
        <asp:LinkButton ID="lnkGoBuses" ToolTip="Scheduled buses, per seat." runat="server" CssClass="nav-link"
                        CausesValidation="false" OnClick="lnkGoBuses_Click" Text="Bus tickets" />
    </asp:Panel>

    <asp:Panel ID="pnlFilters" runat="server" CssClass="card mb-4">
        <div class="card-body">
            <div class="row g-3 align-items-end">
                <div class="col-md-4">
                    <asp:Label ID="lblSearch" runat="server" AssociatedControlID="txtSearch"
                               CssClass="form-label">Search</asp:Label>
                    <asp:TextBox ID="txtSearch" ToolTip="Part of the name is enough." runat="server" CssClass="form-control"
                                 placeholder="by name" />
                </div>
                <asp:Panel ID="pnlProvinceFilter" runat="server" CssClass="col-md-3">
                    <asp:Label ID="lblProvince" runat="server" AssociatedControlID="ddlProvince"
                               CssClass="form-label">Province</asp:Label>
                    <asp:DropDownList ID="ddlProvince" ToolTip="Show only one province, or leave it on Anywhere." runat="server" CssClass="form-select" />
                </asp:Panel>
                <asp:Panel ID="pnlTypeFilter" runat="server" CssClass="col-md-3">
                    <asp:Label ID="lblType" runat="server" AssociatedControlID="ddlType"
                               CssClass="form-label">Activity type</asp:Label>
                    <asp:DropDownList ID="ddlType" ToolTip="Show only one type of activity." runat="server" CssClass="form-select" />
                </asp:Panel>
                <div class="col-md-2 d-flex gap-2">
                    <asp:Button ID="btnSearch" ToolTip="Applies the search above." runat="server" Text="Search"
                                CssClass="btn btn-outline-primary"
                                CausesValidation="false" OnClick="btnSearch_Click" />
                    <asp:Button ID="btnClear" ToolTip="Clears the search and shows everything again." runat="server" Text="Clear"
                                CssClass="btn btn-outline-secondary"
                                CausesValidation="false" OnClick="btnClear_Click" />
                </div>
            </div>
        </div>
    </asp:Panel>

    <asp:Repeater ID="rptStay" runat="server" OnItemCommand="rptStay_ItemCommand">
        <HeaderTemplate>
            <div class="row g-3">
        </HeaderTemplate>
        <ItemTemplate>
            <div class="col-sm-6 col-lg-4">
                <div class="choice-card d-flex flex-column h-100">
                    <div class="choice-card-photo">
                        <asp:Image ID="imgStay" runat="server"
                                   Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("ImageUrl"))) %>'
                                   ImageUrl='<%# Eval("ImageUrl") %>'
                                   AlternateText='<%# Eval("Name") %>' />
                        <asp:Label ID="lblStayHint" runat="server" CssClass="placeholder-text"
                                   Visible='<%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("ImageUrl"))) %>'
                                   Text="Photo to come" />
                    </div>
                    <div class="choice-card-body d-flex flex-column flex-grow-1">
                        <div class="choice-card-title"><%# Server.HtmlEncode(Convert.ToString(Eval("Name"))) %></div>
                        <div class="choice-card-meta">
                            <%# Eval("Type") %> &middot; <%# Eval("Location") %>
                        </div>
                        <div class="mt-2 mb-3 choice-card-price">
                            from R<%# string.Format("{0:N2}", Eval("FromPricePerNight")) %>
                            <span class="text-muted fw-normal small">per night</span>
                        </div>
                        <div class="mt-auto">
                            <asp:LinkButton ID="lnkViewRooms" runat="server"
                                            CssClass="btn btn-sm btn-outline-primary w-100"
                                            CommandName="ViewRooms"
                                            CommandArgument='<%# Eval("AccommodationId") %>'
                                            CausesValidation="false"
                                            ToolTip="See the rooms at this accommodation."
                                            Text="View rooms" />
                        </div>
                    </div>
                </div>
            </div>
        </ItemTemplate>
        <FooterTemplate>
            </div>
        </FooterTemplate>
    </asp:Repeater>

    <asp:Panel ID="pnlRooms" runat="server" Visible="false">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-3">
            <div>
                <h2 class="h4 mb-0"><asp:Literal ID="litRoomsHeading" runat="server" /></h2>
                <div class="text-muted"><asp:Literal ID="litRoomsMeta" runat="server" /></div>
            </div>
            <asp:Button ID="btnBackToResorts" runat="server" Text="Back to accommodations"
                        CssClass="btn btn-outline-secondary" CausesValidation="false"
                        OnClick="btnBackToResorts_Click"
                        ToolTip="Back to the list of accommodations." />
        </div>

        <asp:Repeater ID="rptRooms" runat="server">
            <HeaderTemplate>
                <div class="row g-3">
            </HeaderTemplate>
            <ItemTemplate>
                <div class="col-sm-6 col-lg-4">
                    <div class="choice-card d-flex flex-column h-100">
                        <div class="choice-card-body">
                            <div class="choice-card-title">Room <%#: Eval("RoomNumber") %></div>
                            <div class="choice-card-meta">
                                <%#: string.IsNullOrWhiteSpace(Convert.ToString(Eval("RoomType"))) ? "Room" : Convert.ToString(Eval("RoomType")) %>
                            </div>
                            <div class="choice-card-meta mt-1">
                                Capacity <%# Eval("Capacity") %> &middot;
                                <%# Eval("Beds") %> <%# Convert.ToInt32(Eval("Beds")) == 1 ? "bed" : "beds" %>
                            </div>
                            <div class="choice-card-meta">
                                <%# Convert.ToBoolean(Eval("Bathroom")) ? "Private bathroom" : "Shared bathroom" %>
                            </div>
                            <div class="mt-2 choice-card-price">
                                R<%# string.Format("{0:N2}", Eval("PricePerNight")) %>
                                <span class="text-muted fw-normal small">per night</span>
                            </div>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
            <FooterTemplate>
                </div>
            </FooterTemplate>
        </asp:Repeater>

        <asp:Panel ID="pnlNoRooms" runat="server" Visible="false" CssClass="alert alert-light border">
            This accommodation has no rooms available yet.
        </asp:Panel>
    </asp:Panel>

    <asp:Repeater ID="rptDo" runat="server">
        <HeaderTemplate>
            <div class="row g-3">
        </HeaderTemplate>
        <ItemTemplate>
            <div class="col-sm-6 col-lg-4">
                <div class="choice-card d-flex flex-column">
                    <div class="choice-card-photo">
                        <asp:Image ID="imgDo" runat="server"
                                   Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("ImageUrl"))) %>'
                                   ImageUrl='<%# Eval("ImageUrl") %>'
                                   AlternateText='<%# Eval("Name") %>' />
                        <asp:Label ID="lblDoHint" runat="server" CssClass="placeholder-text"
                                   Visible='<%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("ImageUrl"))) %>'
                                   Text="Photo to come" />
                    </div>
                    <div class="choice-card-body">
                        <div class="choice-card-title"><%# Server.HtmlEncode(Convert.ToString(Eval("Name"))) %></div>
                        <div class="choice-card-meta">
                            <%# Eval("CategoryName") %> &middot; <%# Eval("Location") %>
                        </div>
                        <div class="mt-2 choice-card-price">
                            R<%# string.Format("{0:N2}", Eval("Rate")) %>
                            <span class="text-muted fw-normal small">per person</span>
                        </div>
                    </div>
                </div>
            </div>
        </ItemTemplate>
        <FooterTemplate>
            </div>
        </FooterTemplate>
    </asp:Repeater>

    <asp:Repeater ID="rptCars" runat="server">
        <HeaderTemplate>
            <div class="row g-3">
        </HeaderTemplate>
        <ItemTemplate>
            <div class="col-sm-6 col-lg-4">
                <div class="choice-card d-flex flex-column">
                    <div class="choice-card-photo">
                        <asp:Image ID="imgCar" runat="server"
                                   Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("ImageUrl"))) %>'
                                   ImageUrl='<%# Eval("ImageUrl") %>'
                                   AlternateText='<%# Eval("Display") %>' />
                        <asp:Label ID="lblCarHint" runat="server" CssClass="placeholder-text"
                                   Visible='<%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("ImageUrl"))) %>'
                                   Text="Photo to come" />
                    </div>
                    <div class="choice-card-body">
                        <div class="choice-card-title"><%# Server.HtmlEncode(Convert.ToString(Eval("Display"))) %></div>
                        <div class="choice-card-meta">
                            <%#: Eval("Location") %><%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("ProvinceName"))) ? "" : ", " + Server.HtmlEncode(Convert.ToString(Eval("ProvinceName"))) %>
                        </div>
                        <div class="choice-card-meta mt-1">
                            Seats <%# Eval("Capacity") %> &middot; <%# Eval("IncludedKmPerDay") %>km per day included
                        </div>
                        <div class="mt-2 choice-card-price">
                            R<%# string.Format("{0:N2}", Eval("PricePerDay")) %>
                            <span class="text-muted fw-normal small">per day</span>
                        </div>
                    </div>
                </div>
            </div>
        </ItemTemplate>
        <FooterTemplate>
            </div>
        </FooterTemplate>
    </asp:Repeater>

    <asp:Repeater ID="rptGo" runat="server">
        <HeaderTemplate>
            <div class="row g-3">
        </HeaderTemplate>
        <ItemTemplate>
            <div class="col-sm-6 col-lg-4">
                <div class="choice-card d-flex flex-column">
                    <div class="choice-card-body">
                        <div class="choice-card-title"><%# Server.HtmlEncode(Convert.ToString(Eval("Journey"))) %></div>
                        <div class="choice-card-meta">
                            <%# Eval("Carrier") %> <%# Eval("RouteCode") %>
                        </div>
                        <div class="choice-card-meta mt-1">
                            <%# Eval("TypeDisplay") %> &middot; <%# Eval("DurationDisplay") %>
                        </div>
                        <div class="mt-2 choice-card-price">
                            from R<%# string.Format("{0:N2}", Eval("IndicativeFarePerSeat")) %>
                            <span class="text-muted fw-normal small">per seat</span>
                        </div>
                    </div>
                </div>
            </div>
        </ItemTemplate>
        <FooterTemplate>
            </div>
        </FooterTemplate>
    </asp:Repeater>

    <asp:Panel ID="pnlNone" runat="server" Visible="false" CssClass="alert alert-light border">
        Nothing matches that search.
    </asp:Panel>

</asp:Content>
