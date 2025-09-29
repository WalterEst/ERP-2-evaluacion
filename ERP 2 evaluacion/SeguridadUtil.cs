using System.Security.Cryptography;
using System.Text;

namespace ERP_2_evaluacion;

public static class SeguridadUtil
{
    public const int Iteraciones = 100000;
    public const int TamanoSalt = 32;
    public const int TamanoHash = 64;
    public const string ContrasenaPorDefecto = "Temporal123!";

    public static (byte[] hash, byte[] salt) CrearPasswordHash(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[TamanoSalt];
        rng.GetBytes(salt);
        return (CrearPasswordHash(password, salt), salt);
    }

    public static byte[] CrearPasswordHash(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iteraciones, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(TamanoHash);
    }

    public static bool VerificarPassword(string password, byte[] salt, byte[] hash)
    {
        var hashComparar = CrearPasswordHash(password, salt);
        return CryptographicOperations.FixedTimeEquals(hash, hashComparar);
    }

    public static string GenerarPasswordTemporal(int largo = 12)
    {
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
}
