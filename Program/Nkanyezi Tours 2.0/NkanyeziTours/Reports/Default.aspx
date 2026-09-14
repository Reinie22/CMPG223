<%@ Page Title="Reports" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="Default.aspx.cs" Inherits="NkanyeziTours.Reports._Default" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div id="report-top"></div>

    <h1 class="h3 mb-1">Reports</h1>
    <p class="text-muted mb-4">
        Choose a report and a period, then select Show report. Any report can be saved as an Excel or PDF file.
    </p>

    <div class="card mb-4 report-options">
        <div class="card-body">
            <div class="row g-3 align-items-end">
                <div class="col-12 col-md-4">
                    <asp:Label ID="lblReport" runat="server" AssociatedControlID="ddlReport" CssClass="form-label fw-semibold" Text="Report" />
                    <asp:DropDownList ID="ddlReport" ToolTip="Choose which report to create." runat="server" CssClass="form-select"
                                      AutoPostBack="true" OnSelectedIndexChanged="ddlReport_SelectedIndexChanged">
                        <asp:ListItem Value="Revenue" Text="Revenue" />
                        <asp:ListItem Value="Seasonal" Text="Seasonal trends and hotspots" />
                        <asp:ListItem Value="TripCost" Text="Trip cost" />
                    </asp:DropDownList>
                </div>
                <div class="col-6 col-md-2">
                    <asp:Label ID="lblFrom" runat="server" AssociatedControlID="txtFrom" CssClass="form-label fw-semibold" Text="From" />
                    <asp:TextBox ID="txtFrom" ToolTip="The first day of the period. Trips that start on or after this day are included." runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-6 col-md-2">
                    <asp:Label ID="lblTo" runat="server" AssociatedControlID="txtTo" CssClass="form-label fw-semibold" Text="To" />
                    <asp:TextBox ID="txtTo" ToolTip="The last day of the period. Trips that start on or before this day are included." runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <asp:Panel ID="pnlClient" runat="server" CssClass="col-12 col-md-4" Visible="false">
                    <asp:Label ID="lblClient" runat="server" AssociatedControlID="ddlClient" CssClass="form-label fw-semibold" Text="Client" />
                    <asp:DropDownList ID="ddlClient" ToolTip="Show one client's trips only, or leave on All clients." runat="server" CssClass="form-select" />
                </asp:Panel>
            </div>

            <p class="text-muted small mt-2 mb-3">
                <asp:Literal ID="litReportHint" runat="server" />
            </p>

            <div class="d-flex flex-wrap gap-2">
                <asp:Button ID="btnShow" ToolTip="Create the report on screen." runat="server" Text="Show report"
                            CssClass="btn btn-primary" OnClick="btnShow_Click" />
                <asp:Button ID="btnExcel" ToolTip="Save this report as an Excel workbook." runat="server" Text="Save as Excel"
                            CssClass="btn btn-outline-success" OnClick="btnExcel_Click" />
                <asp:Button ID="btnPdf" ToolTip="Save this report as a PDF, ready to print or email." runat="server" Text="Save as PDF"
                            CssClass="btn btn-outline-danger" OnClick="btnPdf_Click" />
            </div>
        </div>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-warning">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <asp:HiddenField ID="hfSection" runat="server" />

    <asp:Panel ID="pnlReport" runat="server" Visible="false">

        <div class="report-header mb-3">
            <h2 class="h4 mb-1"><asp:Literal ID="litTitle" runat="server" /></h2>
            <p class="mb-2"><asp:Literal ID="litDescription" runat="server" /></p>
            <div class="report-meta small text-muted">
                <asp:Repeater ID="rptFilters" runat="server">
                    <ItemTemplate>
                        <span class="me-3"><strong><%#: Eval("Key") %>:</strong> <%#: Eval("Value") %></span>
                    </ItemTemplate>
                </asp:Repeater>
                <span><asp:Literal ID="litGenerated" runat="server" /></span>
            </div>
        </div>

        <div class="row g-3 mb-3">
            <asp:Repeater ID="rptStats" runat="server">
                <ItemTemplate>
                    <div class="col-6 col-lg-3">
                        <div class="card h-100 report-stat">
                            <div class="card-body">
                                <div class="text-muted small"><%#: Eval("Label") %></div>
                                <div class="fs-5 fw-semibold"><%#: Eval("Value") %></div>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <ul class="nav nav-tabs report-nav" role="tablist">
            <asp:Repeater ID="rptNav" runat="server">
                <ItemTemplate>
                    <li class="nav-item" role="presentation">
                        <button type="button" role="tab" data-report-tab="<%#: Eval("Key") %>"
                                class="nav-link <%# IsSelected((string)Eval("Key")) ? "active" : "" %>"
                                title="Show only this part of the report."><%#: Eval("Heading") %></button>
                    </li>
                </ItemTemplate>
            </asp:Repeater>
            <li class="nav-item" role="presentation">
                <button type="button" role="tab" data-report-tab="<%: LegendKey %>"
                        class="nav-link <%: IsSelected(LegendKey) ? "active" : "" %>"
                        title="Show what the terms and short forms in this report mean.">Legend</button>
            </li>
        </ul>

        <div class="card border-top-0 rounded-top-0 mb-3">
            <asp:Repeater ID="rptSections" runat="server" OnItemDataBound="rptSections_ItemDataBound">
                <ItemTemplate>
                    <div class="card-body report-section" data-report-section="<%#: Eval("Key") %>" <%# IsSelected((string)Eval("Key")) ? "" : "hidden" %>>
                        <h3 class="h5 mb-1"><%#: Eval("Heading") %></h3>
                        <p class="text-muted small mb-2"><%#: Eval("Description") %></p>
                        <div class="table-responsive">
                            <asp:GridView ID="gvSection" runat="server"
                                          AutoGenerateColumns="true"
                                          CssClass="table table-sm table-striped align-middle mb-0 report-table"
                                          GridLines="None" />
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <div class="card-body report-section" data-report-section="<%: LegendKey %>" <%: IsSelected(LegendKey) ? "" : "hidden" %>>
                <h3 class="h5 mb-1">Legend</h3>
                <p class="text-muted small mb-2">What the terms and short forms in this report mean.</p>
                <dl class="row mb-0 small">
                    <asp:Repeater ID="rptLegend" runat="server">
                        <ItemTemplate>
                            <dt class="col-sm-3"><%#: Eval("Term") %></dt>
                            <dd class="col-sm-9"><%#: Eval("Meaning") %></dd>
                        </ItemTemplate>
                    </asp:Repeater>
                </dl>
            </div>
        </div>

        <p class="text-muted small">
            Save as Excel or Save as PDF saves the part that is showing. On the Legend tab they save the whole report.
        </p>

    </asp:Panel>

    <script>
        document.addEventListener('click', function (e) {
            var tab = e.target.closest('[data-report-tab]');
            if (!tab) return;
            var key = tab.getAttribute('data-report-tab');
            document.querySelectorAll('[data-report-tab]').forEach(function (t) {
                t.classList.toggle('active', t === tab);
            });
            document.querySelectorAll('[data-report-section]').forEach(function (s) {
                s.hidden = s.getAttribute('data-report-section') !== key;
            });
            document.getElementById('<%= hfSection.ClientID %>').value = key;
        });
    </script>

</asp:Content>
