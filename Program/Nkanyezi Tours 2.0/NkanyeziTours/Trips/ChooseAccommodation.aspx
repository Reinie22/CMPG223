<%@ Page Title="Choose a resort" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="ChooseAccommodation.aspx.cs" Inherits="NkanyeziTours.Trips.ChooseAccommodation" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-1">Choose a resort</h1>
    <p class="text-muted mb-4"><asp:Literal ID="litSubtitle" runat="server" /></p>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlBody" runat="server">

        <div class="card mb-4">
            <div class="card-body">
                <div class="row g-3 align-items-end">
                    <div class="col-md-4">
                        <asp:Label ID="lblSearchName" runat="server" AssociatedControlID="txtSearchName"
                                   CssClass="form-label">Name</asp:Label>
                        <asp:TextBox ID="txtSearchName" ToolTip="Part of the name is enough." runat="server" CssClass="form-control"
                                     placeholder="Search by name" />
                    </div>
                    <div class="col-md-3">
                        <asp:Label ID="lblProvince" runat="server" AssociatedControlID="ddlProvince"
                                   CssClass="form-label">Province</asp:Label>
                        <asp:DropDownList ID="ddlProvince" ToolTip="Show only resorts in one province." runat="server" CssClass="form-select" />
                    </div>
                    <div class="col-md-3">
                        <asp:Label ID="lblMaxPrice" runat="server" AssociatedControlID="txtMaxPrice"
                                   CssClass="form-label">Up to, per night</asp:Label>
                        <div class="input-group">
                            <span class="input-group-text">R</span>
                            <asp:TextBox ID="txtMaxPrice" ToolTip="Hide resorts whose cheapest unit costs more than this per night." runat="server" CssClass="form-control" />
                        </div>
                    </div>
                    <div class="col-md-2 d-flex gap-2">
                        <asp:Button ID="btnSearch" ToolTip="Applies the filters above." runat="server" Text="Search" CssClass="btn btn-outline-primary"
                                    OnClick="btnSearch_Click" />
                        <asp:Button ID="btnClear" ToolTip="Clears the filters and shows everything again." runat="server" Text="Clear" CssClass="btn btn-outline-secondary"
                                    CausesValidation="false" OnClick="btnClear_Click" />
                    </div>
                </div>
            </div>
        </div>

        <asp:Repeater ID="rptOptions" runat="server" OnItemCommand="rptOptions_ItemCommand">
            <HeaderTemplate>
                <div class="row g-3">
            </HeaderTemplate>
            <ItemTemplate>
                <div class="col-sm-6 col-lg-4">
                    <div class="choice-card d-flex flex-column">
                        <div class="choice-card-photo">
                            <asp:Image ID="imgPhoto" runat="server"
                                       Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("ImageUrl"))) %>'
                                       ImageUrl='<%# Eval("ImageUrl") %>'
                                       AlternateText='<%# Eval("Name") %>' />
                            <asp:Label ID="lblPhotoHint" runat="server" CssClass="placeholder-text"
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
                                <asp:LinkButton ID="lnkChoose" ToolTip="Puts this resort on the leg. One resort per leg - choosing another replaces it." runat="server"
                                                CssClass="btn btn-sm btn-primary w-100"
                                                CommandName="ChooseResort"
                                                CommandArgument='<%# Eval("AccommodationId") %>'
                                                Text="Stay here" />
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

        <div class="mt-4">
            <asp:HyperLink ID="lnkBack" ToolTip="Back to your trip without choosing." runat="server" CssClass="btn btn-outline-secondary"
                           Text="Back to my trip" />
        </div>

    </asp:Panel>

</asp:Content>
