using System.Data.SqlClient;
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
    /// The one connection to the database. Worked out once, at start-up, by
    /// ResolveConnectionString below - which starts from Web.config and falls
    /// back to the other usual SQL Server instances if that one is not there.
    /// </summary>
    private static readonly Db _db = new Db(ResolveConnectionString());

    /// <summary>The server we actually ended up talking to. Handy when debugging.</summary>
    public static string? ServerInUse { get; private set; }

    /// <summary>
    /// Works out which SQL Server to use, once, at start-up.
    ///
    /// WHY THIS IS NOT JUST ConfigurationManager.ConnectionStrings[...]
    /// ===============================================================
    /// Web.config is committed to git. If everybody edits the connection string
    /// to match their own machine, everybody conflicts with everybody on every
    /// merge, and somebody's laptop-specific server name eventually lands on
    /// main and breaks the build for the rest of us.
    ///
    /// So instead: the value in Web.config is a *preference*, not a rule. We try
    /// it first. If it does not answer, we try the other usual suspects and use
    /// whichever one has the NkanyeziTours database on it. Nobody edits
    /// Web.config, nobody conflicts, and the project runs from any folder on any
    /// machine in the group with no setup at all.
    ///
    /// Note that none of this has anything to do with where the project folder
    /// lives. The database is inside SQL Server, not inside our folder - there
    /// is no .mdf file here and no AttachDbFilename. Moving, renaming or copying
    /// the folder has never affected the connection and still does not.
    /// </summary>
    private static string ResolveConnectionString()
    {
        // A LOCAL, not a static field. As a static field this was initialised
        // AFTER _db in textual order, so it was still null while _db's
        // initializer was running this method - the foreach below then threw a
        // NullReferenceException instead of the helpful message at the bottom,
        // but only on a machine where the Web.config server did not answer.
        //
        // LocalDB ships with Visual Studio, SQLEXPRESS is the separate free
        // install, and the last two cover a full SQL Server on the default
        // instance. Between them they cover every machine in the group.
        var fallbackServers = new[]
        {
            @"(localdb)\MSSQLLocalDB",
            @".\SQLEXPRESS",
            "localhost",
            "."
        };

        var configured = ReadConfiguredConnectionString();

        // Try what Web.config asked for first. A generous timeout, because the
        // very first connection of the day has to start the LocalDB service and
        // that genuinely can take ten seconds.
        var working = TryConnect(configured, 15);
        if (working != null)
            return working;

        // It did not answer. Try the others with a short timeout - if a server
        // is not installed we want to find that out immediately, not in three
        // seconds each.
        foreach (var server in fallbackServers)
        {
            working = TryConnect(WithServer(configured, server), 3);
            if (working != null)
                return working;
        }

        throw new ConfigurationErrorsException(
            "Could not reach SQL Server on any of the usual instances.\r\n\r\n" +
            "Tried: " + new SqlConnectionStringBuilder(configured).DataSource +
            " (from Web.config), " + string.Join(", ", fallbackServers) + "\r\n\r\n" +
            "Most likely the NkanyeziTours database has not been created yet on " +
            @"this machine. Run Project\database\setup-database.ps1 and try again.");
    }

    /// <summary>
    /// Reads the connection string out of Web.config.
    ///
    /// Fails loudly rather than quietly returning null, because a missing
    /// connection string is the single most likely problem on a machine that has
    /// just cloned the repository.
    /// </summary>
    private static string ReadConfiguredConnectionString()
    {
        var setting = ConfigurationManager.ConnectionStrings["NkanyeziDb"];

        if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
            throw new ConfigurationErrorsException(
                "The connection string 'NkanyeziDb' was not found in Web.config. " +
                "Check the <connectionStrings> section near the top of that file.");

        return setting.ConnectionString;
    }

    /// <summary>
    /// Copies a connection string but points it at a different server. Everything
    /// else - the database name, Trusted_Connection and so on - is left alone.
    /// </summary>
    private static string WithServer(string connectionString, string server)
    {
        return new SqlConnectionStringBuilder(connectionString)
        {
            DataSource = server
        }.ConnectionString;
    }

    /// <summary>
    /// Opens a real connection to find out whether this server is actually there
    /// and actually has our database on it.
    ///
    /// Returns the connection string to use if it worked, or null if it did not.
    /// The returned string has the timeout put back to something sensible,
    /// because the short probe timeout is only right for probing, not for the
    /// real queries that follow.
    /// </summary>
    private static string? TryConnect(string connectionString, int timeoutSeconds)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            ConnectTimeout = timeoutSeconds
        };

        try
        {
            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
            }
        }
        catch (SqlException)
        {
            return null;    // no such server, or no such database on it
        }
        catch (InvalidOperationException)
        {
            return null;    // LocalDB instance name that does not exist
        }

        ServerInUse = builder.DataSource;

        builder.ConnectTimeout = 15;
        return builder.ConnectionString;
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
    public static VehicleMaintenanceRepository VehicleMaintenance => new VehicleMaintenanceRepository(_db);
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
    /// <summary>What clients have asked for on their own draft trips.</summary>
    public static TripRequestRepository TripRequests => new TripRequestRepository(_db);

    public static TripExpenseRepository Expenses => new TripExpenseRepository(_db);

    // -------------------------------------------------------------------------
    // Services
    // -------------------------------------------------------------------------
    public static AuthService Auth => new AuthService(Users);
}
