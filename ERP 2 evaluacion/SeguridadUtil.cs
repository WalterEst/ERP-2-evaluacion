using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace ERP_2_evaluacion;

public static class SeguridadUtil
{
    public const int LongitudMinimaPassword = 8;
    public const int LongitudMaximaPassword = 50;
    public const string ContrasenaPorDefecto = "Temporal123";

    private static readonly string[] CodigosPerfilesPrivilegiados = { "SUPERADMIN", "ADMIN" };
    private static readonly string CodigosPrivilegiadosSql = string.Join(", ",
        CodigosPerfilesPrivilegiados.Select((_, index) => $"@codigoPriv{index}"));

    public static string GenerarPasswordTemporal(int largo = 12)
    {
        if (largo < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(largo), "La longitud debe ser positiva");
        }

        if (largo > LongitudMaximaPassword)
        {
            largo = LongitudMaximaPassword;
        }

        const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
        var bytes = new byte[largo];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var sb = new StringBuilder(largo);
        for (int i = 0; i < largo; i++)
        {
            sb.Append(caracteres[bytes[i] % caracteres.Length]);
        }

        return sb.ToString();
    }

    public static bool EsUsuarioPrivilegiadoPorId(int idUsuario)
    {
        return EjecutarConsultaPrivilegio(connection =>
        {
            var command = new SqlCommand($@"SELECT TOP 1 1
FROM UsuarioPerfil up
JOIN Perfil p ON p.IdPerfil = up.IdPerfil
WHERE up.IdUsuario = @idUsuario
  AND p.Codigo IN ({CodigosPrivilegiadosSql});", connection);
            command.Parameters.AddWithValue("@idUsuario", idUsuario);
            AgregarParametrosPrivilegio(command);
            return command;
        });
    }

    public static bool EsUsuarioPrivilegiadoPorIdentificador(string identificador)
    {
        if (string.IsNullOrWhiteSpace(identificador))
        {
            return false;
        }

        return EjecutarConsultaPrivilegio(connection =>
        {
            var command = new SqlCommand($@"SELECT TOP 1 1
FROM Usuario u
WHERE (u.NombreUsuario = @identificador OR u.Correo = @identificador)
  AND EXISTS (
        SELECT 1
        FROM UsuarioPerfil up
        JOIN Perfil p ON p.IdPerfil = up.IdPerfil
        WHERE up.IdUsuario = u.IdUsuario
          AND p.Codigo IN ({CodigosPrivilegiadosSql})
    );", connection);
            command.Parameters.AddWithValue("@identificador", identificador);
            AgregarParametrosPrivilegio(command);
            return command;
        });
    }

    private static bool EjecutarConsultaPrivilegio(Func<SqlConnection, SqlCommand> comandoFactory)
    {
        using var connection = Db.GetConnection();
        connection.Open();
        using var command = comandoFactory(connection);
        var resultado = command.ExecuteScalar();
        return resultado != null && resultado != DBNull.Value;
    }

    private static void AgregarParametrosPrivilegio(SqlCommand command)
    {
        for (int i = 0; i < CodigosPerfilesPrivilegiados.Length; i++)
        {
            command.Parameters.AddWithValue($"@codigoPriv{i}", CodigosPerfilesPrivilegiados[i]);
        }
    }
}
