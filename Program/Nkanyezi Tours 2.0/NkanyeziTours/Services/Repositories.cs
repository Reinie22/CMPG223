using System.Data.SqlClient;
using System.Configuration;
using Nkanyezi.Data;

namespace Nkanyezi.Services;

public static class Repositories
{
    private static readonly Db _db = new Db(ResolveConnectionString());

    public static string? ServerInUse { get; private set; }

    private static string ResolveConnectionString()
    {
        var fallbackServers = new[]
        {
            @"(localdb)\MSSQLLocalDB",
            @".\SQLEXPRESS",
            "localhost",
            "."
        };

        var configured = ReadConfiguredConnectionString();

        var working = TryConnect(configured, 5);
        if (working != null)
            return working;

        StartLocalDbIfNeeded(new SqlConnectionStringBuilder(configured).DataSource);

        working = TryConnect(configured, 20);
        if (working != null)
            return working;

        foreach (var server in fallbackServers)
        {
            StartLocalDbIfNeeded(server);

            working = TryConnect(WithServer(configured, server), 10);
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

    private static string ReadConfiguredConnectionString()
    {
        var setting = ConfigurationManager.ConnectionStrings["NkanyeziDb"];

        if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
            throw new ConfigurationErrorsException(
                "The connection string 'NkanyeziDb' was not found in Web.config. " +
                "Check the <connectionStrings> section near the top of that file.");

        return setting.ConnectionString;
    }

    private static void StartLocalDbIfNeeded(string dataSource)
    {
        if (string.IsNullOrWhiteSpace(dataSource) ||
            !dataSource.StartsWith(@"(localdb)\", StringComparison.OrdinalIgnoreCase))
            return;

        var instance = dataSource.Substring(@"(localdb)\".Length).Trim();

        if (instance.Length == 0)
            return;

        RunSqlLocalDb("start " + instance);
    }

    private static bool RunSqlLocalDb(string arguments)
    {
        try
        {
            var info = new System.Diagnostics.ProcessStartInfo("sqllocaldb", arguments)
            {
                UseShellExecute = false,
                CreateNoWindow  = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true
            };

            using (var process = System.Diagnostics.Process.Start(info))
            {
                if (process == null)
                    return false;

                if (!process.WaitForExit(20000))
                    return false;

                return process.ExitCode == 0;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string WithServer(string connectionString, string server)
    {
        return new SqlConnectionStringBuilder(connectionString)
        {
            DataSource = server
        }.ConnectionString;
    }

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
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }

        ServerInUse = builder.DataSource;

        builder.ConnectTimeout = 15;
        return builder.ConnectionString;
    }

    public static Db Database => _db;

    public static LookupRepository Lookups => new LookupRepository(_db);
    public static UserRepository Users => new UserRepository(_db);
    public static AttractionRepository Attractions => new AttractionRepository(_db);
    public static AccommodationRepository Accommodation => new AccommodationRepository(_db);
    public static RoomRepository Rooms => new RoomRepository(_db);
    public static VehicleRepository Vehicles => new VehicleRepository(_db);
    public static VehicleMaintenanceRepository VehicleMaintenance => new VehicleMaintenanceRepository(_db);
    public static RouteRepository Routes => new RouteRepository(_db);
    public static ExchangeRateRepository ExchangeRates => new ExchangeRateRepository(_db);

    public static ItineraryRepository Itineraries => new ItineraryRepository(_db);
    public static RoomBookingRepository RoomBookings => new RoomBookingRepository(_db);
    public static AttractionBookingRepository AttractionBookings => new AttractionBookingRepository(_db);
    public static VehicleBookingRepository VehicleBookings => new VehicleBookingRepository(_db);
    public static RouteBookingRepository RouteBookings => new RouteBookingRepository(_db);

    public static GanttRepository Gantt => new GanttRepository(_db);

    public static TripLegRepository TripLegs => new TripLegRepository(_db);

    public static TripRequestRepository TripRequests => new TripRequestRepository(_db);

    public static TripExpenseRepository Expenses => new TripExpenseRepository(_db);

    public static AuthService Auth => new AuthService(Users);
}
