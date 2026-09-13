<%@ Page Title="Choose rooms" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="ChooseUnit.aspx.cs" Inherits="NkanyeziTours.Trips.ChooseUnit" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlBody" runat="server">

        <div class="d-flex flex-wrap justify-content-between align-items-center mb-1 gap-2">
            <h1 class="h3 mb-0">Choose your rooms</h1>
            <asp:HyperLink ID="lnkBack" runat="server" CssClass="btn btn-outline-secondary"
                           ToolTip="Back to your trip."
                           Text="Back to my trip" />
        </div>

        <p class="text-muted"><asp:Literal ID="litSubtitle" runat="server" /></p>

        <asp:Panel ID="pnlSelection" runat="server" CssClass="alert py-2 d-flex flex-wrap justify-content-between align-items-center gap-2">
            <span><asp:Literal ID="litSelection" runat="server" /></span>
            <asp:HyperLink ID="lnkDone" runat="server" CssClass="btn btn-sm btn-primary"
                           ToolTip="Save your rooms and go back to your trip."
                           Text="Done" />
        </asp:Panel>

        <div class="d-flex flex-wrap justify-content-between align-items-end gap-2 mb-3">
            <asp:HyperLink ID="lnkChangeResort" runat="server" CssClass="small"
                           ToolTip="Pick a different resort for this leg. Your room choices start again." />
            <span class="small text-muted">
                Prices are per night for the whole room, not per person.
            </span>
        </div>

        <asp:Repeater ID="rptUnits" runat="server" OnItemCommand="rptUnits_ItemCommand">
            <HeaderTemplate>
                <div class="row g-3">
            </HeaderTemplate>
            <ItemTemplate>
                <div class="col-sm-6 col-lg-4">
                    <div class='choice-card d-flex flex-column <%# Convert.ToBoolean(Eval("IsChosen")) ? "is-selected" : "" %>'>
                        <div class="choice-card-photo">
                            <asp:Label ID="lblUnitHint" runat="server" CssClass="placeholder-text"
                                       Text="Photo to come" />
                        </div>
                        <div class="choice-card-body">
                            <div class="choice-card-title">Room <%#: Eval("UnitName") %></div>
                            <div class="choice-card-meta"><%#: Eval("TypeDisplay") %></div>
                            <div class="choice-card-meta mt-1">
                                <%#: Eval("SleepsDisplay") %> &middot; <%#: Eval("BedsDisplay") %>
                            </div>
                            <div class="choice-card-meta"><%#: Eval("BathroomDisplay") %></div>

                            <div class="mt-2 choice-card-price">
                                R<%# string.Format("{0:N2}", Eval("PricePerNight")) %>
                                <span class="text-muted fw-normal small">per night</span>
                            </div>
                            <div class="choice-card-meta"><%#: Eval("TotalDisplay") %></div>
                        </div>
                        <div class="p-3 pt-0">
                            <asp:LinkButton ID="lnkChoose" runat="server"
                                            CssClass='<%# Convert.ToBoolean(Eval("IsChosen")) ? "btn btn-outline-danger w-100" : "btn btn-outline-primary w-100" %>'
                                            Text='<%# Convert.ToBoolean(Eval("IsChosen")) ? "Remove room" : "Add room" %>'
                                            CommandName="ToggleRoom"
                                            CommandArgument='<%# Eval("RoomId") %>'
                                            ToolTip="Adds or removes this room. You can choose as many rooms as you need." />
                        </div>
                    </div>
                </div>
            </ItemTemplate>
            <FooterTemplate>
                </div>
            </FooterTemplate>
        </asp:Repeater>

        <asp:Panel ID="pnlNone" runat="server" Visible="false" CssClass="alert alert-light border">
            <p class="mb-2">
                <strong>This resort has nothing free for your dates.</strong>
            </p>
            <p class="mb-0 small">
                Either every room is taken for those nights, or the resort has no
                rooms captured yet. Try a different resort, or change the dates of
                this leg on your trip page.
            </p>
        </asp:Panel>

    </asp:Panel>

</asp:Content>
