<%@ Page Title="Add activities" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="ChooseAttraction.aspx.cs" Inherits="NkanyeziTours.Trips.ChooseAttraction" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-1">Add activities</h1>
    <p class="text-muted mb-1"><asp:Literal ID="litSubtitle" runat="server" /></p>
    <p class="text-muted small mb-4"><asp:Literal ID="litRegionNote" runat="server" /></p>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="Choose"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <asp:Panel ID="pnlBody" runat="server">

        <div class="card mb-4">
            <div class="card-body">
                <div class="row g-3 align-items-end">
                    <div class="col-md-3">
                        <asp:Label ID="lblVisitDate" runat="server" AssociatedControlID="ddlVisitDate"
                                   CssClass="form-label">Date <span class="text-danger">*</span></asp:Label>

                        <asp:DropDownList ID="ddlVisitDate" ToolTip="Which day of this leg you want to do it on." runat="server" CssClass="form-select" />
                    </div>
                    <div class="col-md-2">
                        <asp:Label ID="lblPeople" runat="server" AssociatedControlID="txtPeople"
                                   CssClass="form-label">People <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtPeople" ToolTip="How many people are going." runat="server" CssClass="form-control" />
                        <asp:RangeValidator ID="rvPeople" runat="server" ControlToValidate="txtPeople"
                                            ValidationGroup="Choose" Display="Dynamic"
                                            Type="Integer" MinimumValue="1" MaximumValue="200"
                                            CssClass="text-danger small"
                                            ErrorMessage="People must be a whole number of at least one." />
                    </div>
                    <div class="col-md-4">
                        <asp:Label ID="lblSearchName" runat="server" AssociatedControlID="txtSearchName"
                                   CssClass="form-label">Name</asp:Label>
                        <asp:TextBox ID="txtSearchName" ToolTip="Part of the name is enough." runat="server" CssClass="form-control"
                                     placeholder="Search by name" />
                    </div>
                    <div class="col-md-3">
                        <asp:Label ID="lblCategory" runat="server" AssociatedControlID="ddlCategory"
                                   CssClass="form-label">Activity type</asp:Label>
                        <asp:DropDownList ID="ddlCategory" ToolTip="Show only one kind of activity." runat="server" CssClass="form-select" />
                    </div>
                    <div class="col-12 d-flex gap-2">
                        <asp:Button ID="btnSearch" ToolTip="Applies the filters above." runat="server" Text="Search" CssClass="btn btn-outline-primary"
                                    CausesValidation="false" OnClick="btnSearch_Click" />
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
                                <%# Eval("CategoryName") %> &middot; <%# Eval("Location") %>
                            </div>
                            <div class="mt-2 mb-3 choice-card-price">
                                R<%# string.Format("{0:N2}", Eval("Rate")) %>
                                <span class="text-muted fw-normal small">per person</span>
                            </div>
                            <div class="mt-auto">
                                <asp:LinkButton ID="lnkChoose" ToolTip="Adds this to the leg. You can add as many activities to a leg as you like." runat="server"
                                                CssClass="btn btn-sm btn-primary w-100"
                                                CommandName="ChooseItem"
                                                CommandArgument='<%# Eval("AttractionId") %>'
                                                ValidationGroup="Choose"
                                                Text="Add to this leg" />
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
