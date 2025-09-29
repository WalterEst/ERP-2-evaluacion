using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ERP_2_evaluacion;

public static class Db
{
    private static readonly Lazy<string> _connectionString = new(() =>
        ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
        ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada en app.config"));

    private static string ConnectionString => _connectionString.Value;

    public static SqlConnection GetConnection()
    {
        var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    public static DataTable GetDataTable(string query, Action<SqlParameterCollection>? parameterize = null)
    {
        using var connection = GetConnection();
        using var command = new SqlCommand(query, connection);
        parameterize?.Invoke(command.Parameters);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static int Execute(string commandText, Action<SqlParameterCollection>? parameterize = null)
    {
        using var connection = GetConnection();
        using var command = new SqlCommand(commandText, connection);
        parameterize?.Invoke(command.Parameters);
        return command.ExecuteNonQuery();
    }

    public static object? Scalar(string commandText, Action<SqlParameterCollection>? parameterize = null)
    {
        using var connection = GetConnection();
        using var command = new SqlCommand(commandText, connection);
        parameterize?.Invoke(command.Parameters);
        return command.ExecuteScalar();
    }
}
