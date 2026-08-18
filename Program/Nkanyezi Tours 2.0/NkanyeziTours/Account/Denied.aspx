<%@ Page Title="Access denied" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="Denied.aspx.cs" Inherits="NkanyeziTours.Account.Denied" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center">
        <div class="col-md-6 text-center py-5">
            <h1 class="h3">Access denied</h1>
            <p class="text-muted">
                Your role does not have permission to open that page.
                If you think this is wrong, speak to an administrator.
            </p>
            <a class="btn btn-primary" href="<%: ResolveUrl("~/Default.aspx") %>">Back to dashboard</a>
        </div>
    </div>
</asp:Content>
