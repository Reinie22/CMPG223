using System.Configuration;
using Nkanyezi.Data;

namespace Nkanyezi.Services;

/// <summary>
/// Hands out repositories to the pages.
///
/// WHY THIS CLASS EXISTS
/// =====================
/// The old ASP.NET Core project had a built-in dependency injection container.
/// Program.cs listed every repository with builder.Services.AddScoped(...) and
/// a page just asked for one in its constructor.
///
/// Web Forms has no such container. A page is created by ASP.NET itself and you
/// cannot add constructor arguments to it. So this small class takes its place.
///
/// A page uses it like this:
///
///     private readonly AttractionRepository _attractions = Repositories.Attractions;
///
/// That is the whole thing. You never write "new Db(...)" and you never touch a
/// connection string.
///
/// The Db object is created once and shared, because it holds nothing but the
/// connection string - it opens and closes a real connection inside each
/// method. The repositories themselves are tiny wrappers around it, so a fresh
/// one per call costs nothing and keeps them free of shared state.
/// </summary>
public static class Repositories
{
    /// <summary>
    /// The one connection to the database, read from Web.config.
    ///
    /// Fails loudly at start-up rather than quietly returning null, because a
    /// missing connection string is the single most likely problem on a machine
    /// that has just cloned the repository.
    /// </summary>
    private static readonly Db _db = new Db(ReadConnectionString());

    private static string ReadConnectionString()
    {
        var setting = ConfigurationManager.ConnectionStrings["NkanyeziDb"];

        if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
            throw new ConfigurationErrorsException(
                "The connection string 'NkanyeziDb' was not found in Web.config. " +
                "Check the <connectionStrings> section near the top of that file.");

        return setting.ConnectionString;
    }

    /// <summary>The shared database helper, for anything that needs it directly.</summary>
    public static Db Database => _db;

    // -------------------------------------------------------------------------
    // Master data
    // -------------------------------------------------------------------------
    public static LookupRepository Lookups => new LookupRepository(_db);
    public static UserRepository Users => new UserRepository(_db);
    public static AttractionRepository Attractions => new AttractionRepository(_db);
    public static AccommodationRepository Accommodation => new AccommodationRepository(_db);
    public static RoomRepository Rooms => new RoomRepository(_db);
    public static VehicleRepository Vehicles => new VehicleRepository(_db);
    public static RouteRepository Routes => new RouteRepository(_db);
    public static ExchangeRateRepository ExchangeRates => new ExchangeRateRepository(_db);

    // -------------------------------------------------------------------------
    // Trips, bookings and expenses
    // -------------------------------------------------------------------------
    public static ItineraryRepository Itineraries => new ItineraryRepository(_db);
    public static RoomBookingRepository RoomBookings => new RoomBookingRepository(_db);
    public static AttractionBookingRepository AttractionBookings => new AttractionBookingRepository(_db);
    public static VehicleBookingRepository VehicleBookings => new VehicleBookingRepository(_db);
    public static RouteBookingRepository RouteBookings => new RouteBookingRepository(_db);
    public static TripExpenseRepository Expenses => new TripExpenseRepository(_db);

    // -------------------------------------------------------------------------
    // Services
    // -------------------------------------------------------------------------
    public static AuthService Auth => new AuthService(Users);
}
