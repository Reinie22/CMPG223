using Microsoft.AspNetCore.Authentication.Cookies;
using Nkanyezi.Data;
using Nkanyezi.Services;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// Razor Pages
//
// Every page requires a logged in user by default. That is safer than the other
// way round: a page somebody forgets to protect is protected anyway, instead of
// being wide open. The handful of public pages say [AllowAnonymous].
// -----------------------------------------------------------------------------
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToFolder("/Account");
    options.Conventions.AllowAnonymousToFolder("/Portal");   // public browsing
});

// -----------------------------------------------------------------------------
// Database
//
// The connection string lives in appsettings.json so that nobody has to type a
// server name into their code. If SQL Server is installed under a different
// instance name on your machine, change it there and nowhere else.
// -----------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("NkanyeziDb")
    ?? throw new InvalidOperationException(
        "Connection string 'NkanyeziDb' was not found in appsettings.json.");

builder.Services.AddSingleton(new Db(connectionString));

// -----------------------------------------------------------------------------
// Login
//
// Cookie authentication: once somebody signs in, the browser holds a cookie
// that identifies them on every later request. SlidingExpiration means the
// clock restarts while they keep working, so an agent is not thrown out in the
// middle of capturing a booking.
// -----------------------------------------------------------------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;   // JavaScript cannot read it
        options.Cookie.Name = "NkanyeziAuth";
    });

builder.Services.AddAuthorization();

// AuthService needs this to reach the current request.
builder.Services.AddHttpContextAccessor();

// -----------------------------------------------------------------------------
// Repositories and services
// Add a line here for every new one you write, otherwise pages asking for it
// fail at startup with "unable to resolve service".
// -----------------------------------------------------------------------------
// Master data
builder.Services.AddScoped<LookupRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AttractionRepository>();
builder.Services.AddScoped<AccommodationRepository>();
builder.Services.AddScoped<RoomRepository>();
builder.Services.AddScoped<VehicleRepository>();
builder.Services.AddScoped<RouteRepository>();
builder.Services.AddScoped<ExchangeRateRepository>();

// Trips, bookings and expenses
builder.Services.AddScoped<ItineraryRepository>();
builder.Services.AddScoped<RoomBookingRepository>();
builder.Services.AddScoped<AttractionBookingRepository>();
builder.Services.AddScoped<VehicleBookingRepository>();
builder.Services.AddScoped<RouteBookingRepository>();
builder.Services.AddScoped<TripExpenseRepository>();

// Services (business rules)
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Order matters. Authentication works out who you are, authorisation then
// decides what you may open. Swapping these two breaks logins in a way that is
// very hard to diagnose.
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
