using System.Data;
using System.Data.SqlClient;

namespace Nkanyezi.Data;

/// <summary>
/// The one place in the whole solution that opens a connection to SQL Server.
///
/// Nobody else creates a SqlConnection. Nobody else writes a connection string.
/// Repositories call the methods below and that is the entire contract.
///
/// ============================================================================
/// READ THIS PART. IT IS THE MOST IMPORTANT COMMENT IN THE PROJECT.
/// ============================================================================
///
/// We write our SQL as text, inside the repository classes. That means YOU are
/// responsible for making it safe. There is exactly one rule:
///
///     NEVER put a value into a SQL string. Always pass it as a parameter.
///
/// WRONG - and this is a real, exploitable bug, not a style opinion:
///
///     var sql = "SELECT * FROM dbo.AppUser WHERE Email = '" + email + "'";
///
/// If somebody types this into the email box:
///
///     x' OR '1'='1
///
/// the database receives:
///
///     SELECT * FROM dbo.AppUser WHERE Email = 'x' OR '1'='1'
///
/// which matches every user in the table, and they are logged in as the first
/// one. Type something worse and they can empty a table:
///
///     x'; DROP TABLE dbo.Itinerary; --
///
/// That is SQL injection. It is the single most common way real systems get
/// broken into, and it is caused entirely by gluing text together.
///
/// RIGHT - the value travels separately from the query:
///
///     var sql = "SELECT * FROM dbo.AppUser WHERE Email = @Email";
///     _db.Query(sql, Map, Db.P("@Email", email));
///
/// Now SQL Server is handed the query and the value as two different things.
/// It compiles the query first and then slots the value in, so whatever the
/// user typed is treated as *text to compare against*, never as commands to
/// run. Typing "x' OR '1'='1" simply looks for a user whose email address is
/// literally x' OR '1'='1, finds nobody, and fails the login. Exactly right.
///
/// This also fixes apostrophes for free. Without parameters, an attraction
/// named "Ludwig's Roses" breaks the query. With parameters it just works.
///
/// The rule in one line:
///     if it came from a user, a form, a query string or a file, it is a
///     PARAMETER. Only column names and table names are ever typed into the
///     SQL string itself, and those never come from the user.
/// ============================================================================
///
/// Written once so that the repetitive part of ADO.NET (open, command, close,
/// dispose) is not copied into seventeen repositories by eight different people.
/// </summary>
public class Db
{
    private readonly string _connectionString;

    public Db(string connectionString)
    {
        _connectionString = connectionString;
    }

    // -------------------------------------------------------------------------
    // Reading
    // -------------------------------------------------------------------------

    /// <summary>
    /// Runs a SELECT and turns each row into an object using the map function
    /// you pass in.
    ///
    /// Example:
    ///     const string sql = @"SELECT * FROM dbo.Vehicle WHERE IsActive = 1;";
    ///     var list = _db.Query(sql, Map);
    /// </summary>
    public List<T> Query<T>(string sql, Func<SqlDataReader, T> map,
                            params SqlParameter[] parameters)
    {
        var results = new List<T>();

        using var connection = new SqlConnection(_connectionString);
        using var command = NewCommand(sql, connection, parameters);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
            results.Add(map(reader));

        return results;
    }

    /// <summary>
    /// Same as Query, but for a SELECT that returns one row at most.
    /// Returns null when nothing was found, which is what "not found" looks
    /// like everywhere in this system.
    /// </summary>
    public T? QuerySingle<T>(string sql, Func<SqlDataReader, T> map,
                             params SqlParameter[] parameters) where T : class
        => Query(sql, map, parameters).FirstOrDefault();

    /// <summary>
    /// Runs a SELECT that returns a single value: a COUNT, a SUM, a MAX.
    /// Returns the default when the query gives back nothing or NULL, so a SUM
    /// over zero rows comes back as 0 rather than blowing up.
    /// </summary>
    public T? Scalar<T>(string sql, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = NewCommand(sql, connection, parameters);

        connection.Open();
        var result = command.ExecuteScalar();

        if (result == null || result == DBNull.Value)
            return default;

        return (T)Convert.ChangeType(result, typeof(T));
    }

    // -------------------------------------------------------------------------
    // Writing
    // -------------------------------------------------------------------------

    /// <summary>
    /// Runs an INSERT, UPDATE or DELETE and gives back how many rows changed.
    ///
    /// The row count is worth checking on an update: zero means the id did not
    /// exist, which usually means somebody else deleted the record while this
    /// user had the page open.
    /// </summary>
    public int Execute(string sql, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = NewCommand(sql, connection, parameters);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    /// <summary>
    /// Runs an INSERT and gives back the id of the new row.
    ///
    /// Your SQL must end with SELECT CAST(SCOPE_IDENTITY() AS INT); which is
    /// what this reads. SCOPE_IDENTITY is the safe one to use: it returns the
    /// id generated by *your* statement, not by a trigger or by whatever
    /// another user happened to insert a millisecond later.
    /// </summary>
    public int ExecuteInsert(string sql, params SqlParameter[] parameters)
        => Scalar<int>(sql, parameters);

    /// <summary>
    /// Runs several statements as one all-or-nothing unit of work.
    ///
    /// Use this when two writes only make sense together, for example marking a
    /// vehicle returned and settling its final cost. If the second statement
    /// fails, the first is undone, so the database is never left half updated.
    ///
    /// Example:
    ///     _db.InTransaction(run =>
    ///     {
    ///         run("UPDATE ...", Db.P("@Id", id));
    ///         run("INSERT ...", Db.P("@Id", id));
    ///     });
    /// </summary>
    public void InTransaction(Action<Func<string, SqlParameter[], int>> work)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            work((sql, parameters) =>
            {
                using var command = NewCommand(sql, connection, parameters);
                command.Transaction = transaction;
                return command.ExecuteNonQuery();
            });

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // -------------------------------------------------------------------------
    // Building parameters
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a parameter, turning a C# null into a database NULL properly.
    ///
    /// Passing null straight into a SqlParameter does NOT work: ADO.NET reads it
    /// as "no value supplied" and the call fails with a confusing error. This is
    /// the single most common ADO.NET mistake, so use this method every time.
    ///
    ///     Db.P("@ProvinceId", provinceId)   // provinceId may be null, that is fine
    /// </summary>
    public static SqlParameter P(string name, object? value)
        => new SqlParameter(name, value ?? DBNull.Value);

    /// <summary>
    /// Builds a parameter for a LIKE search box.
    ///
    /// Two things happen here. First the value is wrapped in % signs so that
    /// searching for "berg" finds "Drakensberg". Second, any character that
    /// LIKE treats as a wildcard is escaped, so a client searching for "50%"
    /// gets rows containing "50%" instead of every row in the table.
    ///
    /// This is not a security fix - the parameter already stops injection - it
    /// is a correctness fix, and searching is where users notice it.
    ///
    /// Pair it with:  WHERE Name LIKE @Name ESCAPE '\'
    /// </summary>
    public static SqlParameter Like(string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return P(name, null);

        // value! - see the note in AuthService: on .NET Framework the
        // IsNullOrWhiteSpace check above does not narrow the type for the compiler.
        var escaped = value!.Trim()
                           .Replace("\\", "\\\\")
                           .Replace("%",  "\\%")
                           .Replace("_",  "\\_")
                           .Replace("[",  "\\[");

        return new SqlParameter(name, "%" + escaped + "%");
    }

    // -------------------------------------------------------------------------
    // Reading columns out of a row
    // -------------------------------------------------------------------------

    /// <summary>Reads a column that is allowed to be NULL.</summary>
    public static T? GetNullable<T>(SqlDataReader reader, string column) where T : struct
    {
        var value = reader[column];
        return value == DBNull.Value ? null : (T)value;
    }

    /// <summary>Reads a text column that is allowed to be NULL.</summary>
    public static string? GetNullableString(SqlDataReader reader, string column)
    {
        var value = reader[column];
        return value == DBNull.Value ? null : (string)value;
    }

    /// <summary>
    /// Reads a column that may not be in the result set at all.
    ///
    /// Useful for the joined-in display columns like ProvinceName: a query that
    /// joins Province has it, a plain SELECT * does not, and the same Map method
    /// serves both.
    /// </summary>
    public static string? GetOptionalString(SqlDataReader reader, string column)
        => HasColumn(reader, column) ? GetNullableString(reader, column) : null;

    /// <summary>True when the result set actually contains this column.</summary>
    public static bool HasColumn(SqlDataReader reader, string column)
    {
        for (var i = 0; i < reader.FieldCount; i++)
            if (string.Equals(reader.GetName(i), column, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds the command. CommandType.Text because we write our own SQL rather
    /// than calling stored procedures.
    /// </summary>
    private static SqlCommand NewCommand(string sql, SqlConnection connection,
                                         SqlParameter[] parameters)
    {
        var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);

        return command;
    }
}
