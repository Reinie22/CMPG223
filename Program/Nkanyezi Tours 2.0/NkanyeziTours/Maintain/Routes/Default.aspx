<%@ Page Title="Routes" Language="C#" MasterPageFile="~/Maintain/Maintain.master" AutoEventWireup="true"
         CodeBehind="Default.aspx.cs" Inherits="NkanyeziTours.Maintain.Routes._Default" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MaintainContent" runat="server">

    <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
        <h1 class="h3 mb-0">Routes, flights and shuttles</h1>
        <asp:HyperLink ID="lnkAdd" ToolTip="Capture a new bus or flight route." runat="server" CssClass="btn btn-primary"
                       NavigateUrl="~/Maintain/Routes/Edit.aspx">+ Add a route</asp:HyperLink>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <div class="card mb-3">
        <div class="card-body">
            <div class="row g-3 align-items-end">
                <div class="col-md-3">
                    <asp:Label ID="lblType" runat="server" AssociatedControlID="ddlType"
                               CssClass="form-label">Kind</asp:Label>
                    <asp:DropDownList ID="ddlType" ToolTip="Show only buses, or only flights." runat="server" CssClass="form-select">
                        <asp:ListItem Text="Flights and buses" Value="" />
                        <asp:ListItem Text="Flights only" Value="Flight" />
                        <asp:ListItem Text="Buses only" Value="Shuttle" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <asp:Label ID="lblFrom" runat="server" AssociatedControlID="txtFrom"
                               CssClass="form-label">From</asp:Label>
                    <asp:TextBox ID="txtFrom" ToolTip="Show only routes leaving from here." runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-3">
                    <asp:Label ID="lblTo" runat="server" AssociatedControlID="txtTo"
                               CssClass="form-label">To</asp:Label>
                    <asp:TextBox ID="txtTo" ToolTip="Show only routes arriving here." runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-3 d-flex gap-2">
                    <asp:Button ID="btnSearch" ToolTip="Applies the filters above." runat="server" Text="Search" CssClass="btn btn-outline-primary"
                                OnClick="btnSearch_Click" />
                    <asp:Button ID="btnClear" ToolTip="Clears every filter and shows the whole list again." runat="server" Text="Clear" CssClass="btn btn-outline-secondary"
                                CausesValidation="false" OnClick="btnClear_Click" />
                </div>
            </div>
        </div>
    </div>

    <div class="card">
        <div class="card-body">
            <asp:GridView ID="gvRoutes" runat="server"
                          AutoGenerateColumns="false"
                          DataKeyNames="RouteId"
                          OnRowCommand="gvRoutes_RowCommand"
                          CssClass="table table-striped align-middle"
                          EmptyDataText="No routes match that search."
                          GridLines="None">
                <Columns>
                    <asp:TemplateField HeaderText="Kind">
                        <ItemTemplate>
                            <asp:Label ID="lblKind" runat="server"
                                       CssClass='<%# Convert.ToBoolean(Eval("IsFlight")) ? "badge text-bg-primary" : "badge text-bg-warning text-dark" %>'
                                       Text='<%# Eval("TypeDisplay") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Carrier" HeaderText="Carrier" />
                    <asp:BoundField DataField="RouteCode" HeaderText="Number" />
                    <asp:BoundField DataField="Journey" HeaderText="Journey" />
                    <asp:BoundField DataField="DurationDisplay" HeaderText="Duration" />
                    <asp:BoundField DataField="IndicativeFarePerSeat" HeaderText="Fare"
                                    DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                    <asp:BoundField DataField="SeatCapacity" HeaderText="Seats" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <asp:Label ID="lblStatus" runat="server"
                                       CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge text-bg-success" : "badge text-bg-secondary" %>'
                                       Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Withdrawn" %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <div class="d-flex gap-1">
                                <asp:HyperLink ID="lnkEdit" ToolTip="Change this route’s times, fare or seat count." runat="server"
                                               CssClass="btn btn-sm btn-outline-primary text-nowrap" Text="Edit"
                                               NavigateUrl='<%# "~/Maintain/Routes/Edit.aspx?id=" + Eval("RouteId") %>' />
                                <asp:LinkButton ID="lnkWithdraw" ToolTip="Takes the route off sale. Seats already booked on it are not affected." runat="server"
                                                CssClass="btn btn-sm btn-outline-danger text-nowrap"
                                                CommandName="WithdrawRoute"
                                                CommandArgument='<%# Eval("RouteId") %>'
                                                Visible='<%# Convert.ToBoolean(Eval("IsActive")) %>'
                                                Text="Withdraw"
                                                data-confirm="Withdraw this route? If seats have been booked it is kept and marked withdrawn." />
                                <asp:LinkButton ID="lnkReactivate" ToolTip="Puts the route back on sale." runat="server"
                                                CssClass="btn btn-sm btn-outline-success text-nowrap"
                                                CommandName="ReactivateRoute"
                                                CommandArgument='<%# Eval("RouteId") %>'
                                                Visible='<%# !Convert.ToBoolean(Eval("IsActive")) %>'
                                                Text="Reactivate" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

</asp:Content>
