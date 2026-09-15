<%@ Page Title="Rooms" Language="C#" MasterPageFile="~/Maintain/Maintain.master" AutoEventWireup="true"
         CodeBehind="Rooms.aspx.cs" Inherits="NkanyeziTours.Maintain.Accommodation.Rooms" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MaintainContent" runat="server">

    <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-1">
        <h1 class="h3 mb-0">Rooms</h1>
        <asp:HyperLink ID="lnkBack" ToolTip="Back to the resort list." runat="server" CssClass="btn btn-outline-secondary btn-sm"
                       NavigateUrl="~/Maintain/Accommodation/AccommodationList.aspx">Back to accommodation</asp:HyperLink>
    </div>
    <p class="text-muted mb-4"><asp:Literal ID="litSubtitle" runat="server" /></p>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="Room"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <asp:Panel ID="pnlBody" runat="server">

        <div class="card mb-4">
            <div class="card-body">
                <h2 class="h5 mb-3"><asp:Literal ID="litFormHeading" runat="server" Text="Add a room" /></h2>

                <div class="row g-3">
                    <div class="col-md-2">
                        <asp:Label ID="lblNumber" runat="server" AssociatedControlID="txtNumber"
                                   CssClass="form-label">Number <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtNumber" ToolTip="What this unit is called or numbered, for example: Chalet 4." runat="server" CssClass="form-control" MaxLength="20" />
                        <asp:RequiredFieldValidator ID="rfvNumber" runat="server" ControlToValidate="txtNumber"
                                                    ValidationGroup="Room" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="The room number is required." />
                    </div>

                    <div class="col-md-3">
                        <asp:Label ID="lblType" runat="server" AssociatedControlID="ddlType"
                                   CssClass="form-label">Type</asp:Label>
                        <asp:DropDownList ID="ddlType" ToolTip="What kind of unit it is." runat="server" CssClass="form-select">
                            <asp:ListItem Text="Single" Value="Single" />
                            <asp:ListItem Text="Double" Value="Double" />
                            <asp:ListItem Text="Twin" Value="Twin" />
                            <asp:ListItem Text="Family" Value="Family" />
                            <asp:ListItem Text="Suite" Value="Suite" />
                            <asp:ListItem Text="Dormitory" Value="Dormitory" />
                        </asp:DropDownList>
                    </div>

                    <div class="col-md-2">
                        <asp:Label ID="lblBeds" runat="server" AssociatedControlID="txtBeds"
                                   CssClass="form-label">Beds <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtBeds" ToolTip="How many beds are in it." runat="server" CssClass="form-control" Text="2" />
                        <asp:RequiredFieldValidator ID="rfvBeds" runat="server" ControlToValidate="txtBeds"
                                                    ValidationGroup="Room" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Beds is required." />

                        <asp:RangeValidator ID="rvBeds" runat="server" ControlToValidate="txtBeds"
                                            ValidationGroup="Room" Display="Dynamic"
                                            Type="Integer" MinimumValue="1" MaximumValue="60"
                                            CssClass="text-danger small"
                                            ErrorMessage="Beds must be a whole number of at least one." />
                    </div>

                    <div class="col-md-2">
                        <asp:Label ID="lblSize" runat="server" AssociatedControlID="txtSize"
                                   CssClass="form-label">Size</asp:Label>
                        <asp:TextBox ID="txtSize" ToolTip="Floor area in square metres, if you know it." runat="server" CssClass="form-control" MaxLength="30"
                                     placeholder="e.g. 28 m2" />
                    </div>

                    <div class="col-md-2">
                        <asp:Label ID="lblPrice" runat="server" AssociatedControlID="txtPrice"
                                   CssClass="form-label">Per night <span class="text-danger">*</span></asp:Label>
                        <div class="input-group">
                            <span class="input-group-text">R</span>
                            <asp:TextBox ID="txtPrice" ToolTip="Price per night for this unit. This is what the client is quoted." runat="server" CssClass="form-control" />
                        </div>
                        <asp:RequiredFieldValidator ID="rfvPrice" runat="server" ControlToValidate="txtPrice"
                                                    ValidationGroup="Room" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="The nightly price is required." />

                        <asp:RangeValidator ID="rvPrice" runat="server" ControlToValidate="txtPrice"
                                            ValidationGroup="Room" Display="Dynamic"
                                            Type="Currency" MinimumValue="0.01" MaximumValue="99999999"
                                            CssClass="text-danger small"
                                            ErrorMessage="The nightly price must be greater than zero." />
                    </div>

                    <div class="col-md-2">
                        <asp:Label ID="lblCapacity" runat="server" AssociatedControlID="txtCapacity"
                                   CssClass="form-label">Capacity <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtCapacity" ToolTip="The most people who may sleep in this unit." runat="server" CssClass="form-control" Text="2" />
                        <asp:RequiredFieldValidator ID="rfvCapacity" runat="server" ControlToValidate="txtCapacity"
                                                    ValidationGroup="Room" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Capacity is required." />

                        <asp:RangeValidator ID="rvCapacity" runat="server" ControlToValidate="txtCapacity"
                                            ValidationGroup="Room" Display="Dynamic"
                                            Type="Integer" MinimumValue="1" MaximumValue="200"
                                            CssClass="text-danger small"
                                            ErrorMessage="Capacity must be at least 1." />
                    </div>

                    <div class="col-md-3 d-flex flex-column justify-content-end">
                        <div class="form-check">
                            <asp:CheckBox ID="chkBathroom" ToolTip="Tick if the unit has its own bathroom rather than a shared one." runat="server" CssClass="form-check-input" Checked="true" />
                            <asp:Label ID="lblBathroom" runat="server" AssociatedControlID="chkBathroom"
                                       CssClass="form-check-label">Private bathroom</asp:Label>
                        </div>
                        <div class="form-check">
                            <asp:CheckBox ID="chkAvailable" ToolTip="Untick to take this unit out of service without deleting it or losing its bookings." runat="server" CssClass="form-check-input" Checked="true" />
                            <asp:Label ID="lblAvailable" runat="server" AssociatedControlID="chkAvailable"
                                       CssClass="form-check-label">Available to book</asp:Label>
                        </div>
                    </div>
                </div>

                <hr class="my-4" />

                <div class="d-flex flex-wrap gap-2">
                    <asp:Button ID="btnSave" ToolTip="Saves the unit and adds it to the list below." runat="server" Text="Add room" CssClass="btn btn-primary"
                                ValidationGroup="Room" OnClick="btnSave_Click" />
                    <asp:Button ID="btnCancelEdit" ToolTip="Stops editing and clears the form." runat="server" Text="Cancel" CssClass="btn btn-outline-secondary"
                                Visible="false" CausesValidation="false" OnClick="btnCancelEdit_Click" />
                </div>
            </div>
        </div>

        <div class="card">
            <div class="card-body">
                <h2 class="h5 mb-3">
                    Rooms at this property
                    <asp:Label ID="lblCount" runat="server" CssClass="badge text-bg-secondary ms-1" />
                </h2>

                <asp:GridView ID="gvRooms" runat="server"
                              AutoGenerateColumns="false"
                              DataKeyNames="RoomId"
                              OnRowCommand="gvRooms_RowCommand"
                              CssClass="table table-striped align-middle"
                              EmptyDataText="No rooms captured yet. Add the first one above."
                              GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="RoomNumber" HeaderText="Number" />
                        <asp:BoundField DataField="RoomType" HeaderText="Type" />
                        <asp:BoundField DataField="Beds" HeaderText="Beds" />
                        <asp:BoundField DataField="Size" HeaderText="Size" />
                        <asp:BoundField DataField="Capacity" HeaderText="Capacity" />
                        <asp:BoundField DataField="PricePerNight" HeaderText="Per night"
                                        DataFormatString="R{0:N2}" ItemStyle-CssClass="text-end" />
                        <asp:TemplateField HeaderText="Bathroom">
                            <ItemTemplate>
                                <%# Convert.ToBoolean(Eval("Bathroom")) ? "Private" : "Shared" %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <asp:Label ID="lblStatus" runat="server"
                                           CssClass='<%# Convert.ToBoolean(Eval("AvailabilityStatus"))
                                                          ? "badge text-bg-success" : "badge text-bg-secondary" %>'
                                           Text='<%# Convert.ToBoolean(Eval("AvailabilityStatus"))
                                                     ? "Available" : "Out of service" %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="">
                            <ItemTemplate>
                                <div class="d-flex gap-1">
                                    <asp:LinkButton ID="lnkEdit" ToolTip="Load this unit into the form above to change it." runat="server"
                                                    CssClass="btn btn-sm btn-outline-primary text-nowrap"
                                                    CommandName="EditRoom"
                                                    CommandArgument='<%# Eval("RoomId") %>'
                                                    CausesValidation="false" Text="Edit" />
                                    <asp:LinkButton ID="lnkDelete" ToolTip="Removes this unit. A unit that has been booked is taken out of service instead." runat="server"
                                                    CssClass="btn btn-sm btn-outline-danger text-nowrap"
                                                    CommandName="DeleteRoom"
                                                    CommandArgument='<%# Eval("RoomId") %>'
                                                    CausesValidation="false" Text="Deactivate"
                                                    data-confirm="Delete this room? If it has bookings it will be taken out of service instead." />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

    </asp:Panel>

</asp:Content>
