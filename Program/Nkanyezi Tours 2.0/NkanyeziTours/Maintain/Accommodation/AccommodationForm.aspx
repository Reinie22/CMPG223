<%@ Page Title="Accommodation" Language="C#" MasterPageFile="~/Maintain/Maintain.master" AutoEventWireup="true"
         CodeBehind="AccommodationForm.aspx.cs" Inherits="NkanyeziTours.Maintain.Accommodation.AccommodationForm" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MaintainContent" runat="server">

    <h1 class="h3 mb-3"><asp:Literal ID="litHeading" runat="server" Text="Add Accommodation" /></h1>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:ValidationSummary ID="vsMain" runat="server" ValidationGroup="Accommodation"
                           CssClass="alert alert-danger py-2" DisplayMode="BulletList" />

    <asp:Panel ID="pnlForm" runat="server">
        <div class="card">
            <div class="card-body">
                <div class="row g-3">

                    <div class="col-md-6">
                        <asp:Label ID="lblName" runat="server" AssociatedControlID="txtName"
                                   CssClass="form-label">Name <span class="text-danger">*</span></asp:Label>
                        <asp:TextBox ID="txtName" ToolTip="The name clients see on the resort cards." runat="server" CssClass="form-control" MaxLength="150" />
                        <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName"
                                                    ValidationGroup="Accommodation" Display="Dynamic"
                                                    CssClass="text-danger small"
                                                    ErrorMessage="Name is required." />
                    </div>

                    <div class="col-md-3">
                        <asp:Label ID="lblType" runat="server" AssociatedControlID="ddlType"
                                   CssClass="form-label">Type</asp:Label>
                        <asp:DropDownList ID="ddlType" ToolTip="What kind of place this is: a lodge, a hotel, a campsite and so on." runat="server" CssClass="form-select">
                            <asp:ListItem Text="Hotel" Value="Hotel" />
                            <asp:ListItem Text="Guest House" Value="Guest House" />
                            <asp:ListItem Text="Lodge" Value="Lodge" />
                            <asp:ListItem Text="Resort" Value="Resort" />
                            <asp:ListItem Text="Bed &amp; Breakfast" Value="Bed &amp; Breakfast" />
                        </asp:DropDownList>
                    </div>

                    <div class="col-md-3">
                        <asp:Label ID="lblProvince" runat="server" AssociatedControlID="ddlProvince"
                                   CssClass="form-label">Province</asp:Label>
                        <asp:DropDownList ID="ddlProvince" ToolTip="Which province it is in. This is how cars and legs are matched to it." runat="server" CssClass="form-select" />
                    </div>

                    <div class="col-12">
                        <asp:Label ID="lblDescription" runat="server" AssociatedControlID="txtDescription"
                                   CssClass="form-label">Description</asp:Label>
                        <asp:TextBox ID="txtDescription" ToolTip="A short sell. Clients read this when choosing where to stay." runat="server" CssClass="form-control"
                                     TextMode="MultiLine" Rows="3" MaxLength="1000" />
                    </div>

                    <div class="col-md-6">
                        <asp:Label ID="lblAddress" runat="server" AssociatedControlID="txtAddress"
                                   CssClass="form-label">Address</asp:Label>
                        <asp:TextBox ID="txtAddress" ToolTip="Street address, for the client itinerary." runat="server" CssClass="form-control" MaxLength="200" />
                    </div>

                    <div class="col-md-6">
                        <asp:Label ID="lblCity" runat="server" AssociatedControlID="txtCity"
                                   CssClass="form-label">City</asp:Label>
                        <asp:TextBox ID="txtCity" ToolTip="The nearest town or city." runat="server" CssClass="form-control" MaxLength="80" />
                    </div>

                    <div class="col-md-3">
                        <asp:Label ID="lblStarRating" runat="server" AssociatedControlID="ddlStarRating"
                                   CssClass="form-label">Star Rating</asp:Label>
                        <asp:DropDownList ID="ddlStarRating" ToolTip="The official grading, if it has one." runat="server" CssClass="form-select">
                            <asp:ListItem Text="Not rated" Value="" />
                            <asp:ListItem Text="1" Value="1" />
                            <asp:ListItem Text="2" Value="2" />
                            <asp:ListItem Text="3" Value="3" />
                            <asp:ListItem Text="4" Value="4" />
                            <asp:ListItem Text="5" Value="5" />
                        </asp:DropDownList>
                    </div>

                    <div class="col-12">
                        <div class="alert alert-light border small mb-0">
                            Price and how many people it sleeps are set on each
                            <strong>unit</strong>, not here - use the
                            <strong>Rooms</strong> button on the accommodation list.
                            <asp:Literal ID="litUnitSummary" runat="server" />
                        </div>
                    </div>

                    <div class="col-12">
                        <asp:Label ID="lblAmenities" runat="server" AssociatedControlID="txtAmenities"
                                   CssClass="form-label">Amenities</asp:Label>
                        <asp:TextBox ID="txtAmenities" ToolTip="Separate each one with a comma, for example: pool, wifi, breakfast." runat="server" CssClass="form-control"
                                     TextMode="MultiLine" Rows="2" MaxLength="500"
                                     placeholder="e.g. WiFi, Pool, Breakfast included" />
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
                                        ToolTip="The photo clients see on the accommodation card. Choosing a new photo replaces the current one." />
                        <div class="form-text">JPG, PNG, GIF or WEBP, up to 5 MB.</div>
                    </div>

                </div>

                <hr class="my-4" />

                <div class="d-flex justify-content-end gap-2">
                    <asp:HyperLink ID="lnkCancel" ToolTip="Goes back to the list without saving." runat="server" CssClass="btn btn-outline-secondary"
                                   NavigateUrl="~/Maintain/Accommodation/AccommodationList.aspx">Cancel</asp:HyperLink>
                    <asp:Button ID="btnSave" ToolTip="Saves the resort. Add its units afterwards with the Rooms button." runat="server" Text="Save" CssClass="btn btn-primary"
                                ValidationGroup="Accommodation" OnClick="btnSave_Click" />
                </div>
            </div>
        </div>
    </asp:Panel>

</asp:Content>
