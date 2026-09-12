<%@ Page Title="Book a trip" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="BookTrip.aspx.cs" Inherits="NkanyeziTours.Trips.BookTrip" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-1">Book a trip</h1>
    <p class="text-muted mb-4">
        Tell us roughly what you have in mind. One of our travel agents picks it
        up from here and builds the rest with you.
    </p>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="Trip"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <div class="card">
        <div class="card-body">
            <div class="row g-3">

                <div class="col-md-8">
                    <asp:Label ID="lblTripName" runat="server" AssociatedControlID="txtTripName"
                               CssClass="form-label">What should we call this trip <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtTripName" runat="server" CssClass="form-control" MaxLength="120"
                                 placeholder="e.g. Cape Town for our anniversary" />
                    <asp:RequiredFieldValidator ID="rfvTripName" runat="server" ControlToValidate="txtTripName"
                                                ValidationGroup="Trip" Display="Dynamic" CssClass="text-danger small"
                                                ErrorMessage="Please give the trip a name." />
                </div>

                <div class="col-md-4">
                    <asp:Label ID="lblBudget" runat="server" AssociatedControlID="txtBudget"
                               CssClass="form-label">Budget <span class="text-danger">*</span></asp:Label>
                    <div class="input-group">
                        <span class="input-group-text">R</span>
                        <asp:TextBox ID="txtBudget" runat="server" CssClass="form-control" placeholder="e.g. 25000" />
                    </div>
                    <asp:RequiredFieldValidator ID="rfvBudget" runat="server" ControlToValidate="txtBudget"
                                                ValidationGroup="Trip" Display="Dynamic" CssClass="text-danger small"
                                                ErrorMessage="Please give us a budget to work to." />
                    <asp:RangeValidator ID="rvBudget" runat="server" ControlToValidate="txtBudget"
                                        ValidationGroup="Trip" Display="Dynamic"
                                        Type="Currency" MinimumValue="0.01" MaximumValue="99999999"
                                        CssClass="text-danger small"
                                        ErrorMessage="The budget must be an amount greater than zero." />
                </div>

                <div class="col-md-4">
                    <asp:Label ID="lblStartDate" runat="server" AssociatedControlID="txtStartDate"
                               CssClass="form-label">Leaving on</asp:Label>
                    <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date" />
                </div>

                <div class="col-md-4">
                    <asp:Label ID="lblEndDate" runat="server" AssociatedControlID="txtEndDate"
                               CssClass="form-label">Coming back on</asp:Label>
                    <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date" />
                </div>

                <div class="col-12">
                    <asp:Label ID="lblComments" runat="server" AssociatedControlID="txtComments"
                               CssClass="form-label">Anything else we should know</asp:Label>
                    <asp:TextBox ID="txtComments" runat="server" CssClass="form-control"
                                 TextMode="MultiLine" Rows="4"
                                 placeholder="Who is travelling, the kind of places you like, anything you would rather avoid" />
                </div>

            </div>

            <hr class="my-4" />

            <div class="d-flex justify-content-end gap-2">
                <asp:HyperLink ID="lnkCancel" runat="server" CssClass="btn btn-outline-secondary"
                               NavigateUrl="~/Default.aspx">Cancel</asp:HyperLink>
                <asp:Button ID="btnSubmit" runat="server" Text="Send my request" CssClass="btn btn-primary"
                            ValidationGroup="Trip" OnClick="btnSubmit_Click" />
            </div>
        </div>
    </div>

</asp:Content>
