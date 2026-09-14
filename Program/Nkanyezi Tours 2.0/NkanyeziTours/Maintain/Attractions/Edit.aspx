<%@ Page Title="Attraction" Language="C#" MasterPageFile="~/Maintain/Maintain.master" AutoEventWireup="true"
         CodeBehind="Edit.aspx.cs" Inherits="NkanyeziTours.Maintain.Attractions.Edit" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MaintainContent" runat="server">

    <h1 class="h3 mb-3"><asp:Literal ID="litHeading" runat="server" /></h1>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="Attraction"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <div class="card">
        <div class="card-body">
            <div class="row g-3">

                <div class="col-md-6">
                    <asp:Label ID="lblName" runat="server" AssociatedControlID="txtName"
                               CssClass="form-label">Name <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtName" ToolTip="The name clients see on the activity cards." runat="server" CssClass="form-control" MaxLength="120" />
                    <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName"
                                                ValidationGroup="Attraction" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Name is required." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblCategory" runat="server" AssociatedControlID="ddlCategory"
                               CssClass="form-label">Category <span class="text-danger">*</span></asp:Label>
                    <asp:DropDownList ID="ddlCategory" ToolTip="What kind of activity this is." runat="server" CssClass="form-select" />
                    <asp:RequiredFieldValidator ID="rfvCategory" runat="server" ControlToValidate="ddlCategory"
                                                InitialValue=""
                                                ValidationGroup="Attraction" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Choose a category." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblProvince" runat="server" AssociatedControlID="ddlProvince"
                               CssClass="form-label">Province <span class="text-danger">*</span></asp:Label>
                    <asp:DropDownList ID="ddlProvince" ToolTip="Which province it is in, so it can be matched to a leg." runat="server" CssClass="form-select" />
                    <asp:RequiredFieldValidator ID="rfvProvince" runat="server" ControlToValidate="ddlProvince"
                                                InitialValue=""
                                                ValidationGroup="Attraction" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Choose a province." />
                </div>

                <div class="col-12">
                    <asp:Label ID="lblDescription" runat="server" AssociatedControlID="txtDescription"
                               CssClass="form-label">Description <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtDescription" ToolTip="A short sell. Clients read this when choosing what to do." runat="server" CssClass="form-control"
                                 TextMode="MultiLine" Rows="3" MaxLength="1000" />
                    <asp:RequiredFieldValidator ID="rfvDescription" runat="server" ControlToValidate="txtDescription"
                                                ValidationGroup="Attraction" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Description is required." />
                </div>

                <div class="col-md-6">
                    <asp:Label ID="lblAddress" runat="server" AssociatedControlID="txtAddress"
                               CssClass="form-label">Address <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtAddress" ToolTip="Where clients present themselves on the day." runat="server" CssClass="form-control" MaxLength="200" />
                    <asp:RequiredFieldValidator ID="rfvAddress" runat="server" ControlToValidate="txtAddress"
                                                ValidationGroup="Attraction" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Address is required." />
                </div>

                <div class="col-md-6">
                    <asp:Label ID="lblCity" runat="server" AssociatedControlID="txtCity"
                               CssClass="form-label">City <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtCity" ToolTip="The nearest town or city." runat="server" CssClass="form-control" MaxLength="80" />
                    <asp:RequiredFieldValidator ID="rfvCity" runat="server" ControlToValidate="txtCity"
                                                ValidationGroup="Attraction" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="City is required." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblRate" runat="server" AssociatedControlID="txtRate"
                               CssClass="form-label">Rate <span class="text-danger">*</span></asp:Label>
                    <div class="input-group">
                        <span class="input-group-text">R</span>
                        <asp:TextBox ID="txtRate" ToolTip="What one person pays." runat="server" CssClass="form-control" />
                    </div>
                    <asp:RequiredFieldValidator ID="rfvRate" runat="server" ControlToValidate="txtRate"
                                                ValidationGroup="Attraction" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Rate is required." />

                    <asp:RangeValidator ID="rvRate" runat="server" ControlToValidate="txtRate"
                                        ValidationGroup="Attraction" Display="Dynamic"
                                        Type="Currency" MinimumValue="0.01" MaximumValue="99999999"
                                        CssClass="text-danger small"
                                        ErrorMessage="Rate must be a number greater than zero." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblQuantity" runat="server" AssociatedControlID="txtQuantity"
                               CssClass="form-label">Capacity per day <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtQuantity" ToolTip="How many people the attraction can take on one day." runat="server" CssClass="form-control" />
                    <div class="form-text">How many people the attraction takes in one day</div>
                    <asp:RequiredFieldValidator ID="rfvQuantity" runat="server" ControlToValidate="txtQuantity"
                                                ValidationGroup="Attraction" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Capacity is required." />
                    <asp:RangeValidator ID="rvQuantity" runat="server" ControlToValidate="txtQuantity"
                                        ValidationGroup="Attraction" Display="Dynamic"
                                        Type="Integer" MinimumValue="1" MaximumValue="1000000"
                                        CssClass="text-danger small"
                                        ErrorMessage="Capacity must be a whole number greater than zero." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblUnit" runat="server" AssociatedControlID="txtUnit"
                               CssClass="form-label">Unit of measure</asp:Label>
                    <asp:TextBox ID="txtUnit" ToolTip="What the rate is charged per, for example: person, group, vehicle." runat="server" CssClass="form-control"
                                 MaxLength="30" placeholder="per person" />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblDuration" runat="server" AssociatedControlID="txtDuration"
                               CssClass="form-label">Duration (minutes)</asp:Label>
                    <asp:TextBox ID="txtDuration" ToolTip="How long it takes, in minutes." runat="server" CssClass="form-control" />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblOpens" runat="server" AssociatedControlID="txtOpens"
                               CssClass="form-label">Opens at <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtOpens" ToolTip="Earliest start time, as 24-hour time." runat="server" CssClass="form-control"
                                 TextMode="Time" placeholder="08:00" />
                    <asp:RequiredFieldValidator ID="rfvOpens" runat="server" ControlToValidate="txtOpens"
                                                ValidationGroup="Attraction" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Opening time is required." />
                </div>

                <div class="col-md-3">
                    <asp:Label ID="lblCloses" runat="server" AssociatedControlID="txtCloses"
                               CssClass="form-label">Closes at <span class="text-danger">*</span></asp:Label>
                    <asp:TextBox ID="txtCloses" ToolTip="Latest finish time, as 24-hour time." runat="server" CssClass="form-control"
                                 TextMode="Time" placeholder="17:00" />
                    <asp:RequiredFieldValidator ID="rfvCloses" runat="server" ControlToValidate="txtCloses"
                                                ValidationGroup="Attraction" Display="Dynamic"
                                                CssClass="text-danger small"
                                                ErrorMessage="Closing time is required." />
                </div>

                <div class="col-md-6">
                    <asp:Label ID="lblSeason" runat="server" AssociatedControlID="txtSeason"
                               CssClass="form-label">Best season</asp:Label>
                    <asp:TextBox ID="txtSeason" ToolTip="When it runs, if it is not open all year." runat="server" CssClass="form-control" MaxLength="50" />
                </div>

                <div class="col-12">
                    <asp:Label ID="lblPhoto" runat="server" AssociatedControlID="fuPhoto"
                               CssClass="form-label">Photo</asp:Label>
                    <asp:Panel ID="pnlCurrentPhoto" runat="server" Visible="false" CssClass="mb-2 d-flex flex-wrap align-items-center gap-3">
                        <asp:Image ID="imgCurrentPhoto" runat="server" CssClass="rounded border"
                                   Style="max-height:120px;max-width:200px;object-fit:cover;"
                                   AlternateText="Current photo" />
                        <div class="form-check">
                            <asp:CheckBox ID="chkRemovePhoto" runat="server" CssClass="form-check-input"
                                          ToolTip="Tick to remove the current photo when you save." />
                            <asp:Label ID="lblRemovePhoto" runat="server" AssociatedControlID="chkRemovePhoto"
                                       CssClass="form-check-label">Remove this photo</asp:Label>
                        </div>
                    </asp:Panel>
                    <asp:FileUpload ID="fuPhoto" runat="server" CssClass="form-control"
                                    accept=".jpg,.jpeg,.png,.gif,.webp"
                                    ToolTip="The photo clients see on the activity card. Choosing a new photo replaces the current one." />
                    <div class="form-text">JPG, PNG, GIF or WEBP, up to 5 MB.</div>
                </div>

            </div>

            <hr class="my-4" />

            <div class="d-flex justify-content-end gap-2">
                <asp:HyperLink ID="lnkCancel" ToolTip="Goes back to the list without saving." runat="server" CssClass="btn btn-outline-secondary"
                               NavigateUrl="~/Maintain/Attractions/Default.aspx">Cancel</asp:HyperLink>
                <asp:Button ID="btnSave" ToolTip="Saves the activity." runat="server" Text="Save" CssClass="btn btn-primary"
                            ValidationGroup="Attraction" OnClick="btnSave_Click" />
            </div>
        </div>
    </div>

</asp:Content>
