<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="NkanyeziTours.ErrorPage" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Something went wrong - Nkanyezi Tours</title>
    <link rel="stylesheet" href="<%: ResolveUrl("~/Content/bootstrap/bootstrap.min.css") %>" />
    <link rel="stylesheet" href="<%: ResolveUrl("~/Content/site.css") %>" />
</head>
<body>
    <nav class="navbar navbar-dark bg-dark">
        <div class="container">
            <a class="navbar-brand" href="<%: ResolveUrl("~/Portal/Discover.aspx") %>">Nkanyezi Tours</a>
        </div>
    </nav>

    <main class="container py-5">
        <div class="card mx-auto" style="max-width: 36rem;">
            <div class="card-body p-4 text-center">
                <h1 class="h3 mb-3"><asp:Literal ID="litHeading" runat="server" /></h1>
                <p class="text-muted mb-4"><asp:Literal ID="litMessage" runat="server" /></p>
                <div class="d-flex justify-content-center gap-2">
                    <a class="btn btn-outline-secondary" href="javascript:history.back()" title="Return to the page you were on.">Go back</a>
                    <a class="btn btn-primary" href="<%: ResolveUrl("~/Default.aspx") %>" title="Open your home page.">Go to home page</a>
                </div>
            </div>
        </div>
    </main>

    <footer class="border-top py-3 mt-4">
        <div class="container text-muted small">
            Group 6 &middot; CMPG223 &middot; Nkanyezi Tours
        </div>
    </footer>
</body>
</html>
