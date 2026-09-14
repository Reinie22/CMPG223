using System.Data;
using System.Data.SqlClient;

namespace Nkanyezi.Data;

public class Db
{
    private readonly string _connectionString;

    public Db(string connectionString)
    {
        _connectionString = connectionString;
    }

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

    public T? QuerySingle<T>(string sql, Func<SqlDataReader, T> map,
                             params SqlParameter[] parameters) where T : class
        => Query(sql, map, parameters).FirstOrDefault();

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

    public int Execute(string sql, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = NewCommand(sql, connection, parameters);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int ExecuteInsert(string sql, params SqlParameter[] parameters)
        => Scalar<int>(sql, parameters);

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

    public static SqlParameter P(string name, object? value)
        => new SqlParameter(name, value ?? DBNull.Value);

    public static SqlParameter Like(string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return P(name, null);

        var trimmed = value!.Trim();

        if (trimmed.Length > 200)
            trimmed = trimmed.Substring(0, 200);

        var escaped = trimmed
                           .Replace("\\", "\\\\")
                           .Replace("%",  "\\%")
                           .Replace("_",  "\\_")
                           .Replace("[",  "\\[");

        return new SqlParameter(name, "%" + escaped + "%");
    }

    public static T? GetNullable<T>(SqlDataReader reader, string column) where T : struct
    {
        var value = reader[column];
        return value == DBNull.Value ? null : (T)value;
    }

    public static string? GetNullableString(SqlDataReader reader, string column)
    {
        var value = reader[column];
        return value == DBNull.Value ? null : (string)value;
    }

    public static string? GetOptionalString(SqlDataReader reader, string column)
        => HasColumn(reader, column) ? GetNullableString(reader, column) : null;

    public static bool HasColumn(SqlDataReader reader, string column)
    {
        for (var i = 0; i < reader.FieldCount; i++)
            if (string.Equals(reader.GetName(i), column, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    private static SqlCommand NewCommand(string sql, SqlConnection connection,
                                         SqlParameter[] parameters)
    {
        var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);

        return command;
    }
}
