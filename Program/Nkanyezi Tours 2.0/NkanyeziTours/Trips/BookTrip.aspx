<%@ Page Title="Create a new trip" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="BookTrip.aspx.cs" Inherits="NkanyeziTours.Trips.BookTrip" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-4">Create a new trip</h1>

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
                               CssClass="form-label">Trip name <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtTripName" ToolTip="What you want to call this trip, for example: Honeymoon 2027." runat="server" CssClass="form-control" MaxLength="120"
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
                        <asp:TextBox ID="txtBudget" ToolTip="Roughly what you have to spend. We show you how your choices add up against it." runat="server" CssClass="form-control" placeholder="e.g. 25000" />
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

                <div class="col-12">
                    <asp:Label ID="lblComments" runat="server" AssociatedControlID="txtComments"
                               CssClass="form-label">Anything else we should know</asp:Label>
                    <asp:TextBox ID="txtComments" ToolTip="Anything your agent should know: occasions, dietary needs, must-sees." runat="server" CssClass="form-control"
                                 TextMode="MultiLine" Rows="4"
                                 placeholder="Who is travelling, the kind of places you like, anything you would rather avoid" />
                </div>

            </div>

            <hr class="my-4" />

            <div class="d-flex justify-content-end gap-2">
                <asp:HyperLink ID="lnkCancel" ToolTip="Goes back without sending anything." runat="server" CssClass="btn btn-outline-secondary"
                               NavigateUrl="~/Default.aspx">Cancel</asp:HyperLink>
                <asp:Button ID="btnSubmit" ToolTip="Creates the trip. You then add its legs." runat="server" Text="Create trip" CssClass="btn btn-primary"
                            ValidationGroup="Trip" OnClick="btnSubmit_Click" />
            </div>
        </div>
    </div>

</asp:Content>
