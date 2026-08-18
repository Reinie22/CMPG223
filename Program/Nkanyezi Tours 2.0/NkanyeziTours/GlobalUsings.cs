// =============================================================================
// Global using directives
//
// Anything listed here applies to EVERY .cs file in the project.
//
// WHY THIS FILE EXISTS - read this before deleting a line from it.
//
// The old project (ASP.NET Core) had a setting called ImplicitUsings, which
// silently added "using System;", "using System.Linq;" and a few others to
// every file for you. Web Forms projects use the old style .csproj, which does
// NOT support that setting.
//
// Without this file, every model and repository we carried over would fail to
// compile with dozens of "The name 'DateTime' does not exist" and
// "'List<T>' could not be found" errors - not because the code is wrong, but
// because the automatic usings disappeared with the project type.
//
// Listing them here once means the 21 files we ported across needed NO changes
// at all. If you add a using that most files need, add it here rather than to
// forty files.
//
// This needs LangVersion 'latest' in the .csproj. Global usings are a C# 10
// feature and .NET Framework projects default to a much older C# version.
// =============================================================================

global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;

// -----------------------------------------------------------------------------
// Our Route model versus ASP.NET's Route class.
//
// Our flights-and-shuttles model is called Route. System.Web.Routing also has a
// class called Route. Any file that imports both would fail with:
//
//     error CS0104: 'Route' is an ambiguous reference
//
// This line says that in this project, Route always means OUR Route. Nothing in
// this system uses ASP.NET's routing classes by name.
// -----------------------------------------------------------------------------
global using Route = Nkanyezi.Models.Route;

// -----------------------------------------------------------------------------
// Our own namespaces, so page code-behind files do not each have to import them.
// -----------------------------------------------------------------------------
global using Nkanyezi.Models;
global using Nkanyezi.Data;
global using Nkanyezi.Services;
