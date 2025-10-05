using System;
using System.Security.Cryptography;
using System.Text;

namespace ERP_2_evaluacion;

public static class SeguridadUtil
{
    public const int LongitudMinimaPassword = 8;
    public const int LongitudMaximaPassword = 50;
    public const string ContrasenaPorDefecto = "Temporal123";

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
}
