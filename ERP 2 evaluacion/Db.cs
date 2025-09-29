using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace ERP_2_evaluacion;

public static class Db
{
    private static string ConnectionString =>
        ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
        ?? throw new InvalidOperationException("No se encontró 'DefaultConnection' en app.config");

    public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);

    public static DataTable GetDataTable(string sql, Action<SqlParameterCollection>? parameterize = null)
    {
        using var cn = GetConnection();
        using var cmd = new SqlCommand(sql, cn);
        parameterize?.Invoke(cmd.Parameters);
        using var da = new SqlDataAdapter(cmd);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static int Execute(string sql, Action<SqlParameterCollection>? parameterize = null)
    {
        using var cn = GetConnection();
        using var cmd = new SqlCommand(sql, cn);
        parameterize?.Invoke(cmd.Parameters);
        cn.Open();
        return cmd.ExecuteNonQuery();
    }

    public static object? Scalar(string sql, Action<SqlParameterCollection>? parameterize = null)
    {
        using var cn = GetConnection();
        using var cmd = new SqlCommand(sql, cn);
        parameterize?.Invoke(cmd.Parameters);
        cn.Open();
        return cmd.ExecuteScalar();
    }
}
