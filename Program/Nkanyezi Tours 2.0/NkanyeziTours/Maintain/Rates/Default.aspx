<%@ Page Title="Exchange rates" Language="C#" MasterPageFile="~/Maintain/Maintain.master" AutoEventWireup="true"
         CodeBehind="Default.aspx.cs" Inherits="NkanyeziTours.Maintain.Rates._Default" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MaintainContent" runat="server">

    <h1 class="h3 mb-1">Exchange rates</h1>
    <p class="text-muted mb-4">
        What one unit of each currency is worth in Rand. A new rate does not replace
        the old one - the old one stays, so a trip priced last month still adds up
        the way it did then.
    </p>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="Rate"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <div class="card mb-4">
        <div class="card-body">
            <h2 class="h5 mb-3">Capture a rate</h2>
            <div class="row g-3">

                <div class="col-md-2">
                    <asp:Label ID="lblCode" runat="server" AssociatedControlID="txtCode"
                               CssClass="form-label">Currency <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtCode" ToolTip="The three letter currency code, for example: USD." runat="server" CssClass="form-control text-uppercase"
                                 MaxLength="3" placeholder="USD" />
                    <asp:RequiredFieldValidator ID="rfvCode" runat="server" ControlToValidate="txtCode"
                                                ValidationGroup="Rate" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The currency code is required." />

                    <asp:RegularExpressionValidator ID="revCode" runat="server" ControlToValidate="txtCode"
                                                    ValidationGroup="Rate" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ValidationExpression="^[A-Za-z]{3}$"
                                                    ErrorMessage="The currency code must be exactly three letters, e.g. USD." />
                </div>

                <div class="col-md-4">
                    <asp:Label ID="lblName" runat="server" AssociatedControlID="txtName"
                               CssClass="form-label">Name <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtName" ToolTip="The full name of the currency." runat="server" CssClass="form-control" MaxLength="50"
                                 placeholder="US Dollar" />
                    <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName"
                                                ValidationGroup="Rate" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The currency name is required." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblRate" runat="server" AssociatedControlID="txtRate"
                               CssClass="form-label">Rand per unit <span class="text-danger">*</span></asp:Label>
                    <div class="input-group">
                        <span class="input-group-text">R</span>
                        <asp:TextBox ID="txtRate" ToolTip="How many rand one unit of this currency is worth." runat="server" CssClass="form-control" placeholder="18.45" />
                    </div>
                    <asp:RequiredFieldValidator ID="rfvRate" runat="server" ControlToValidate="txtRate"
                                                ValidationGroup="Rate" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The rate is required." />

                    <asp:RangeValidator ID="rvRate" runat="server" ControlToValidate="txtRate"
                                        ValidationGroup="Rate" Display="Dynamic"
                                        Type="Double" MinimumValue="0.000001" MaximumValue="999999"
                                        CssClass="text-danger small"
                                        ErrorMessage="The rate must be greater than zero." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblFrom" runat="server" AssociatedControlID="txtFrom"
                               CssClass="form-label">Effective from <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtFrom" ToolTip="The date this rate takes effect. Capture a new row rather than editing an old one." runat="server" CssClass="form-control" TextMode="Date" />
                    <asp:RequiredFieldValidator ID="rfvFrom" runat="server" ControlToValidate="txtFrom"
                                                ValidationGroup="Rate" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="The effective date is required." />
                </div>

            </div>

            <hr class="my-4" />

            <asp:Button ID="btnSave" ToolTip="Saves the rate from the date given." runat="server" Text="Capture rate" CssClass="btn btn-primary"
                        ValidationGroup="Rate" OnClick="btnSave_Click" />
        </div>
    </div>

    <div class="card">
        <div class="card-body">
            <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
                <h2 class="h5 mb-0">Captured rates</h2>
                <div class="form-check">
                    <asp:CheckBox ID="chkCurrentOnly" ToolTip="Hide superseded rates and show only what is in force today." runat="server" CssClass="form-check-input"
                                  AutoPostBack="true" Checked="true"
                                  OnCheckedChanged="chkCurrentOnly_Changed" />
                    <asp:Label ID="lblCurrentOnly" runat="server" AssociatedControlID="chkCurrentOnly"
                               CssClass="form-check-label">Latest for each currency only</asp:Label>
                </div>
            </div>

            <asp:GridView ID="gvRates" runat="server"
                          AutoGenerateColumns="false"
                          DataKeyNames="ExchangeRateId"
                          OnRowCommand="gvRates_RowCommand"
                          CssClass="table table-striped align-middle"
                          EmptyDataText="No rates captured yet."
                          GridLines="None">
                <Columns>
                    <asp:BoundField DataField="CurrencyCode" HeaderText="Code" />
                    <asp:BoundField DataField="CurrencyName" HeaderText="Currency" />
                    <asp:BoundField DataField="RateToZar" HeaderText="Rand per unit"
                                    DataFormatString="R{0:N6}" ItemStyle-CssClass="text-end" />
                    <asp:BoundField DataField="EffectiveFrom" HeaderText="Effective from"
                                    DataFormatString="{0:dd/MM/yyyy}" />
                    <asp:BoundField DataField="CapturedByName" HeaderText="Captured by" />
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkDelete" ToolTip="Removes this rate row." runat="server"
                                            CssClass="btn btn-sm btn-outline-danger text-nowrap"
                                            CommandName="DeleteRate"
                                            CommandArgument='<%# Eval("ExchangeRateId") %>'
                                            Text="Delete"
                                            data-confirm="Delete this rate? Anything already converted at it will not change." />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

</asp:Content>
