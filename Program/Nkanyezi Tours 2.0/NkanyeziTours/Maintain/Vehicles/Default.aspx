<%@ Page Title="Vehicles" Language="C#" MasterPageFile="~/Maintain/Maintain.master" AutoEventWireup="true"
         CodeBehind="Default.aspx.cs" Inherits="NkanyeziTours.Maintain.Vehicles._Default" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MaintainContent" runat="server">

    <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
        <h1 class="h3 mb-0">Vehicles</h1>
        <asp:HyperLink ID="lnkAdd" ToolTip="Add a car to the hire fleet." runat="server" CssClass="btn btn-primary"
                       NavigateUrl="~/Maintain/Vehicles/Edit.aspx">+ Add a vehicle</asp:HyperLink>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <div class="card mb-3">
        <div class="card-body">
            <div class="row g-3 align-items-end">
                <div class="col-md-4">
                    <asp:Label ID="lblSearchText" runat="server" AssociatedControlID="txtSearchText"
                               CssClass="form-label">Make, model or registration</asp:Label>
                    <asp:TextBox ID="txtSearchText" ToolTip="Search on registration, make or model." runat="server" CssClass="form-control"
                                 placeholder="e.g. Corolla or CA 123-456" />
                </div>
                <div class="col-md-3">
                    <asp:Label ID="lblLocation" runat="server" AssociatedControlID="txtLocation"
                               CssClass="form-label">Depot</asp:Label>
                    <asp:TextBox ID="txtLocation" ToolTip="Show only cars based at this depot." runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-2">
                    <asp:Label ID="lblMinSeats" runat="server" AssociatedControlID="txtMinSeats"
                               CssClass="form-label">Seats from</asp:Label>
                    <asp:TextBox ID="txtMinSeats" ToolTip="Hide cars that seat fewer than this." runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-3 d-flex gap-2">
                    <asp:Button ID="btnSearch" ToolTip="Applies the filters above." runat="server" Text="Search" CssClass="btn btn-outline-primary"
                                OnClick="btnSearch_Click" />
                    <asp:Button ID="btnClear" ToolTip="Clears every filter and shows the whole fleet again." runat="server" Text="Clear" CssClass="btn btn-outline-secondary"
                                CausesValidation="false" OnClick="btnClear_Click" />
                </div>
            </div>
        </div>
    </div>

    <div class="card">
        <div class="card-body">
            <asp:GridView ID="gvVehicles" runat="server"
                          AutoGenerateColumns="false"
                          DataKeyNames="VehicleId"
                          OnRowCommand="gvVehicles_RowCommand"
                          CssClass="table table-striped align-middle"
                          EmptyDataText="No vehicles match that search."
                          GridLines="None">
                <Columns>
                    <asp:BoundField DataField="Display" HeaderText="Vehicle" />
                    <asp:BoundField DataField="RegistrationNumber" HeaderText="Registration" />
                    <asp:BoundField DataField="Location" HeaderText="Depot" />
                    <asp:BoundField DataField="ProvinceName" HeaderText="Region" />
                    <asp:BoundField DataField="Capacity" HeaderText="Seats" />
                    <asp:BoundField DataField="PricePerDay" HeaderText="Per day"
                                    DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                    <asp:BoundField DataField="IncludedKmPerDay" HeaderText="Km/day" />
                    <asp:BoundField DataField="RatePerExtraKm" HeaderText="Extra km"
                                    DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <asp:Label ID="lblStatus" runat="server"
                                       CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge text-bg-success" : "badge text-bg-secondary" %>'
                                       Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Retired" %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <div class="d-flex gap-1">
                                <asp:HyperLink ID="lnkEdit" ToolTip="Change this car’s details, depot or rate." runat="server"
                                               CssClass="btn btn-sm btn-outline-primary text-nowrap" Text="Edit"
                                               NavigateUrl='<%# "~/Maintain/Vehicles/Edit.aspx?id=" + Eval("VehicleId") %>' />
                                <asp:LinkButton ID="lnkRetire" ToolTip="Takes the car off hire. Bookings already on it are not affected." runat="server"
                                                CssClass="btn btn-sm btn-outline-danger text-nowrap"
                                                CommandName="RetireVehicle"
                                                CommandArgument='<%# Eval("VehicleId") %>'
                                                Visible='<%# Convert.ToBoolean(Eval("IsActive")) %>'
                                                Text="Retire"
                                                data-confirm="Retire this vehicle? If it has bookings, is on a client's trip or has maintenance history, it is kept and marked retired." />
                                <asp:LinkButton ID="lnkReactivate" ToolTip="Puts the car back on hire." runat="server"
                                                CssClass="btn btn-sm btn-outline-success text-nowrap"
                                                CommandName="ReactivateVehicle"
                                                CommandArgument='<%# Eval("VehicleId") %>'
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
