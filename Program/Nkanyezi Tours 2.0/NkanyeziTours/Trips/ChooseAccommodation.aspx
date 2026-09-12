<%@ Page Title="Choose a place to stay" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="ChooseAccommodation.aspx.cs" Inherits="NkanyeziTours.Trips.ChooseAccommodation" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-1">Choose a place to stay</h1>
    <p class="text-muted mb-4"><asp:Literal ID="litSubtitle" runat="server" /></p>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="Choose"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <asp:Panel ID="pnlBody" runat="server">

        <%--
            The dates and guests sit ABOVE the list, not on every row. They
            apply to whichever place the client then picks, which keeps this to
            one page instead of a list page plus a confirm page.
        --%>
        <div class="card mb-4">
            <div class="card-body">
                <h2 class="h5 mb-3">Your stay</h2>
                <div class="row g-3">
                    <div class="col-md-3">
                        <asp:Label ID="lblCheckIn" runat="server" AssociatedControlID="txtCheckIn"
                                   CssClass="form-label">Check in <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtCheckIn" runat="server" CssClass="form-control" TextMode="Date" />
                        <asp:RequiredFieldValidator ID="rfvCheckIn" runat="server" ControlToValidate="txtCheckIn"
                                                    ValidationGroup="Choose" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Choose a check-in date." />
                    </div>
                    <div class="col-md-3">
                        <asp:Label ID="lblCheckOut" runat="server" AssociatedControlID="txtCheckOut"
                                   CssClass="form-label">Check out <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtCheckOut" runat="server" CssClass="form-control" TextMode="Date" />
                        <asp:RequiredFieldValidator ID="rfvCheckOut" runat="server" ControlToValidate="txtCheckOut"
                                                    ValidationGroup="Choose" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Choose a check-out date." />
                    </div>
                    <div class="col-md-2">
                        <asp:Label ID="lblGuests" runat="server" AssociatedControlID="txtGuests"
                                   CssClass="form-label">Guests <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtGuests" runat="server" CssClass="form-control" Text="2" />
                        <asp:RangeValidator ID="rvGuests" runat="server" ControlToValidate="txtGuests"
                                            ValidationGroup="Choose" Display="Dynamic"
                                            Type="Integer" MinimumValue="1" MaximumValue="200"
                                            CssClass="text-danger small"
                                            ErrorMessage="Guests must be a whole number of at least one." />
                    </div>
                    <div class="col-md-4">
                        <asp:Label ID="lblNote" runat="server" AssociatedControlID="txtNote"
                                   CssClass="form-label">Anything to mention</asp:Label>
                        <asp:TextBox ID="txtNote" runat="server" CssClass="form-control" MaxLength="500"
                                     placeholder="e.g. a sea facing room if possible" />
                    </div>
                </div>
            </div>
        </div>

        <div class="card">
            <div class="card-body">
                <div class="row g-3 align-items-end mb-3">
                    <div class="col-md-4">
                        <asp:Label ID="lblSearchName" runat="server" AssociatedControlID="txtSearchName"
                                   CssClass="form-label">Name</asp:Label>
                        <asp:TextBox ID="txtSearchName" runat="server" CssClass="form-control"
                                     placeholder="Search by name" />
                    </div>
                    <div class="col-md-3">
                        <asp:Label ID="lblProvince" runat="server" AssociatedControlID="ddlProvince"
                                   CssClass="form-label">Province</asp:Label>
                        <asp:DropDownList ID="ddlProvince" runat="server" CssClass="form-select" />
                    </div>
                    <div class="col-md-3">
                        <asp:Label ID="lblMaxPrice" runat="server" AssociatedControlID="txtMaxPrice"
                                   CssClass="form-label">Up to, per night</asp:Label>
                        <div class="input-group">
                            <span class="input-group-text">R</span>
                            <asp:TextBox ID="txtMaxPrice" runat="server" CssClass="form-control" />
                        </div>
                    </div>
                    <div class="col-md-2 d-flex gap-2">
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-outline-primary"
                                    CausesValidation="false" OnClick="btnSearch_Click" />
                    </div>
                </div>

                <asp:GridView ID="gvOptions" runat="server"
                              AutoGenerateColumns="false"
                              DataKeyNames="AccommodationId"
                              OnRowCommand="gvOptions_RowCommand"
                              CssClass="table table-striped align-middle"
                              EmptyDataText="Nothing matches that search."
                              GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="Name" HeaderText="Place" />
                        <asp:BoundField DataField="Type" HeaderText="Type" />
                        <asp:BoundField DataField="Location" HeaderText="Where" />
                        <asp:BoundField DataField="StarDisplay" HeaderText="Stars" />
                        <asp:BoundField DataField="PricePerNight" HeaderText="Per night"
                                        DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                        <asp:BoundField DataField="Capacity" HeaderText="Sleeps up to" />
                        <asp:TemplateField HeaderText="">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkChoose" runat="server"
                                                CssClass="btn btn-sm btn-primary text-nowrap"
                                                CommandName="ChooseItem"
                                                CommandArgument='<%# Eval("AccommodationId") %>'
                                                ValidationGroup="Choose"
                                                Text="Add to my trip" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

                <div class="mt-3">
                    <asp:HyperLink ID="lnkBack" runat="server" CssClass="btn btn-outline-secondary"
                                   Text="Back to my trip" />
                </div>
            </div>
        </div>

    </asp:Panel>

</asp:Content>
