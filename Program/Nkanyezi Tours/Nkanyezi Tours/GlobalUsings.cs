// =============================================================================
// Global using directives
//
// Anything listed here applies to EVERY file in the project, so nobody has to
// remember to add it at the top of their page.
//
// Keep this file short. A global using that nobody expects makes code harder to
// read, not easier, because the reader cannot see where a name came from.
// =============================================================================

// -----------------------------------------------------------------------------
// Why this alias exists
//
// Our flights-and-shuttles model class is called Route. ASP.NET Core also has a
// class called Route (Microsoft.AspNetCore.Routing.Route), and because this
// project has ImplicitUsings turned on, that namespace is already imported into
// every file. Writing "Route" on its own is therefore ambiguous, and the
// compiler refuses with:
//
//     error CS0104: 'Route' is an ambiguous reference between
//     'Nkanyezi.Models.Route' and 'Microsoft.AspNetCore.Routing.Route'
//
// This line says that in this project, Route always means OUR Route. We never
// use the ASP.NET one directly - routing is configured by MapRazorPages() and
// nothing in this system touches that class by name.
//
// If you ever see CS0104 on some other name, this is the place to fix it.
// -----------------------------------------------------------------------------
global using Route = Nkanyezi.Models.Route;
