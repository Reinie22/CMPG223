<%@ Page Title="Choose something to do" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="ChooseAttraction.aspx.cs" Inherits="NkanyeziTours.Trips.ChooseAttraction" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-1">Choose something to do</h1>
    <p class="text-muted mb-4"><asp:Literal ID="litSubtitle" runat="server" /></p>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="Choose"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <asp:Panel ID="pnlBody" runat="server">

        <%--
            The visit date and how many people apply to whichever attraction the
            client then picks, so they are captured once above the list.

            The client is not asked to pick a time slot. Slots have their own
            capacity per day and the agent settles that when booking.
        --%>
        <div class="card mb-4">
            <div class="card-body">
                <h2 class="h5 mb-3">Your visit</h2>
                <div class="row g-3">
                    <div class="col-md-3">
                        <asp:Label ID="lblVisitDate" runat="server" AssociatedControlID="txtVisitDate"
                                   CssClass="form-label">Date <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtVisitDate" runat="server" CssClass="form-control" TextMode="Date" />
                        <asp:RequiredFieldValidator ID="rfvVisitDate" runat="server" ControlToValidate="txtVisitDate"
                                                    ValidationGroup="Choose" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Choose a date for the visit." />
                    </div>
                    <div class="col-md-2">
                        <asp:Label ID="lblPeople" runat="server" AssociatedControlID="txtPeople"
                                   CssClass="form-label">People <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtPeople" runat="server" CssClass="form-control" Text="2" />
                        <asp:RangeValidator ID="rvPeople" runat="server" ControlToValidate="txtPeople"
                                            ValidationGroup="Choose" Display="Dynamic"
                                            Type="Integer" MinimumValue="1" MaximumValue="200"
                                            CssClass="text-danger small"
                                            ErrorMessage="People must be a whole number of at least one." />
                    </div>
                    <div class="col-md-7">
                        <asp:Label ID="lblNote" runat="server" AssociatedControlID="txtNote"
                                   CssClass="form-label">Anything to mention</asp:Label>
                        <asp:TextBox ID="txtNote" runat="server" CssClass="form-control" MaxLength="500"
                                     placeholder="e.g. morning would suit us better" />
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
                        <asp:Label ID="lblCategory" runat="server" AssociatedControlID="ddlCategory"
                                   CssClass="form-label">Kind</asp:Label>
                        <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" />
                    </div>
                    <div class="col-md-3">
                        <asp:Label ID="lblProvince" runat="server" AssociatedControlID="ddlProvince"
                                   CssClass="form-label">Province</asp:Label>
                        <asp:DropDownList ID="ddlProvince" runat="server" CssClass="form-select" />
                    </div>
                    <div class="col-md-2 d-flex gap-2">
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-outline-primary"
                                    CausesValidation="false" OnClick="btnSearch_Click" />
                    </div>
                </div>

                <asp:GridView ID="gvOptions" runat="server"
                              AutoGenerateColumns="false"
                              DataKeyNames="AttractionId"
                              OnRowCommand="gvOptions_RowCommand"
                              CssClass="table table-striped align-middle"
                              EmptyDataText="Nothing matches that search."
                              GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="Name" HeaderText="Attraction" />
                        <asp:BoundField DataField="CategoryName" HeaderText="Kind" />
                        <asp:BoundField DataField="Location" HeaderText="Where" />
                        <asp:BoundField DataField="Rate" HeaderText="Per person"
                                        DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                        <asp:TemplateField HeaderText="">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkChoose" runat="server"
                                                CssClass="btn btn-sm btn-primary text-nowrap"
                                                CommandName="ChooseItem"
                                                CommandArgument='<%# Eval("AttractionId") %>'
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
