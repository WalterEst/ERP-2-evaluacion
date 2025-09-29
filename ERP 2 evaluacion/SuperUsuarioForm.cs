using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class SuperUsuarioForm : Form
{
    private const string CodigoPerfilSuper = "SUPERADMIN";
    private const string NombrePerfilSuper = "Super Administrador";

    private readonly TextBox _txtNombreCompleto = new() { PlaceholderText = "Nombre completo" };
    private readonly TextBox _txtUsuario = new() { PlaceholderText = "Nombre de usuario" };
    private readonly TextBox _txtCorreo = new() { PlaceholderText = "Correo electrónico" };
    private readonly TextBox _txtClave = new() { UseSystemPasswordChar = true, PlaceholderText = "Contraseña" };
    private readonly TextBox _txtConfirmacion = new() { UseSystemPasswordChar = true, PlaceholderText = "Confirmar contraseña" };
    private readonly Button _btnCrear = new() { Text = "Crear super usuario" };
    private readonly Button _btnCancelar = new() { Text = "Cancelar", DialogResult = DialogResult.Cancel };
    private readonly Label _lblMensaje = new() { AutoSize = true, ForeColor = UiTheme.DangerColor, Margin = new Padding(0, 8, 0, 0) };

    private static readonly Regex CorreoRegex = new(@"^[^\s]+@[^@\s]+\.[^\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private sealed record PantallaSeed(string Codigo, string Nombre, string Ruta, string? CodigoPadre, int Orden, string? Icono = null);

    private static readonly PantallaSeed[] PantallasBase =
    {
        new("LOGIN", "Login", "LoginForm", null, 0),
        new("PRINCIPAL", "Principal", "PrincipalForm", null, 1),
        new("USUARIOS", "Usuarios", "UsuariosForm", "PRINCIPAL", 1),
        new("PERFILES", "Perfiles", "PerfilesForm", "PRINCIPAL", 2),
        new("ACCESOS", "Accesos", "AccesosForm", "PRINCIPAL", 3)
    };

    public SuperUsuarioForm()
    {
        Text = "Crear super usuario";
        StartPosition = FormStartPosition.CenterParent;
        AcceptButton = _btnCrear;
        CancelButton = _btnCancelar;
        Width = 560;
        Height = 520;

        UiTheme.ApplyMinimalStyle(this);
        UiTheme.StyleTextInput(_txtNombreCompleto);
        UiTheme.StyleTextInput(_txtUsuario);
        UiTheme.StyleTextInput(_txtCorreo);
        UiTheme.StyleTextInput(_txtClave);
        UiTheme.StyleTextInput(_txtConfirmacion);
        UiTheme.StylePrimaryButton(_btnCrear);
        UiTheme.StyleSecondaryButton(_btnCancelar);

        _btnCrear.Click += (_, _) => CrearSuperUsuario();

        var titulo = UiTheme.CreateTitleLabel("Super usuario");
        var subtitulo = new Label
        {
            Text = "Este usuario tendrá acceso completo. Solo puede existir uno.",
            AutoSize = true,
            ForeColor = UiTheme.MutedTextColor,
            Margin = new Padding(0, 0, 0, 12)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(titulo, 0, 0);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(subtitulo, 0, 1);

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(CreateFieldLabel("Nombre completo"), 0, 2);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_txtNombreCompleto, 0, 3);

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(CreateFieldLabel("Nombre de usuario"), 0, 4);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_txtUsuario, 0, 5);

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(CreateFieldLabel("Correo electrónico"), 0, 6);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_txtCorreo, 0, 7);

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(CreateFieldLabel("Contraseña"), 0, 8);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_txtClave, 0, 9);

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(CreateFieldLabel("Confirmar contraseña"), 0, 10);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_txtConfirmacion, 0, 11);

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_lblMensaje, 0, 12);

        var panelBotones = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 20, 0, 0)
        };
        panelBotones.Controls.Add(_btnCrear);
        panelBotones.Controls.Add(_btnCancelar);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(panelBotones, 0, 13);

        var card = UiTheme.CreateCardPanel();
        card.AutoSize = true;
        card.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        card.Anchor = AnchorStyles.None;
        card.Controls.Add(layout);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        root.Controls.Add(card, 1, 1);

        Controls.Add(root);
    }

    private static Label CreateFieldLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = UiTheme.MutedTextColor,
        Margin = new Padding(0, 12, 0, 4)
    };

    private void CrearSuperUsuario()
    {
        _lblMensaje.ForeColor = UiTheme.DangerColor;
        _lblMensaje.Text = string.Empty;

        var nombreCompleto = _txtNombreCompleto.Text.Trim();
        var usuario = _txtUsuario.Text.Trim();
        var correo = _txtCorreo.Text.Trim();
        var clave = _txtClave.Text;
        var confirmacion = _txtConfirmacion.Text;

        if (string.IsNullOrWhiteSpace(nombreCompleto) || string.IsNullOrWhiteSpace(usuario) ||
            string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave) ||
            string.IsNullOrWhiteSpace(confirmacion))
        {
            _lblMensaje.Text = "Todos los campos son obligatorios";
            return;
        }

        if (!CorreoRegex.IsMatch(correo))
        {
            _lblMensaje.Text = "Ingrese un correo electrónico válido";
            return;
        }

        if (clave.Length < 10)
        {
            _lblMensaje.Text = "La contraseña debe tener al menos 10 caracteres";
            return;
        }

        if (!string.Equals(clave, confirmacion, StringComparison.Ordinal))
        {
            _lblMensaje.Text = "Las contraseñas no coinciden";
            return;
        }

        using var connection = Db.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            if (ExisteSuperUsuario(connection, transaction))
            {
                _lblMensaje.Text = "Ya existe un super usuario";
                transaction.Rollback();
                return;
            }

            ValidarDuplicados(connection, transaction, usuario, correo);

            var (hash, salt) = SeguridadUtil.CrearPasswordHash(clave);
            var idPerfilSuper = ObtenerOCrearPerfilSuper(connection, transaction);
            var idUsuario = InsertarUsuario(connection, transaction, usuario, correo, hash, salt, nombreCompleto);
            var pantallas = AsegurarPantallasBase(connection, transaction, usuario);
            OtorgarPermisosTotales(connection, transaction, idPerfilSuper, pantallas.Values, usuario);
            AsignarPerfil(connection, transaction, idUsuario, idPerfilSuper, usuario);

            transaction.Commit();

            _lblMensaje.ForeColor = UiTheme.AccentColor;
            _lblMensaje.Text = "Super usuario creado correctamente";
            MessageBox.Show("Super usuario creado. Ahora puedes iniciar sesión.", "Super usuario", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            transaction.Rollback();
            _lblMensaje.Text = "Nombre de usuario o correo ya existe";
        }
        catch (InvalidOperationException ex)
        {
            transaction.Rollback();
            _lblMensaje.Text = ex.Message;
        }
        catch (Exception ex)
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
                // Ignorar errores de rollback
            }

            MessageBox.Show($"No se pudo crear el super usuario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static bool ExisteSuperUsuario(SqlConnection connection, SqlTransaction transaction)
    {
        using var command = new SqlCommand(@"SELECT TOP 1 1
FROM UsuarioPerfil up
JOIN Perfil p ON p.IdPerfil = up.IdPerfil
WHERE p.Codigo = @codigo;", connection, transaction);
        command.Parameters.AddWithValue("@codigo", CodigoPerfilSuper);
        return command.ExecuteScalar() != null;
    }

    private static void ValidarDuplicados(SqlConnection connection, SqlTransaction transaction, string usuario, string correo)
    {
        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Usuario WHERE NombreUsuario = @usuario", connection, transaction))
        {
            cmd.Parameters.AddWithValue("@usuario", usuario);
            if ((int)cmd.ExecuteScalar() > 0)
            {
                throw new InvalidOperationException("El nombre de usuario ya está registrado");
            }
        }

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Usuario WHERE Correo = @correo", connection, transaction))
        {
            cmd.Parameters.AddWithValue("@correo", correo);
            if ((int)cmd.ExecuteScalar() > 0)
            {
                throw new InvalidOperationException("El correo electrónico ya está registrado");
            }
        }
    }

    private static int ObtenerOCrearPerfilSuper(SqlConnection connection, SqlTransaction transaction)
    {
        using var seleccionar = new SqlCommand("SELECT IdPerfil FROM Perfil WHERE Codigo = @codigo", connection, transaction);
        seleccionar.Parameters.AddWithValue("@codigo", CodigoPerfilSuper);
        var existente = seleccionar.ExecuteScalar();
        if (existente != null)
        {
            return Convert.ToInt32(existente);
        }

        using var insertar = new SqlCommand(@"INSERT INTO Perfil (NombrePerfil, Codigo, Descripcion, Activo)
VALUES (@nombre, @codigo, 'Perfil con control total del sistema', 1);
SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction);
        insertar.Parameters.AddWithValue("@nombre", NombrePerfilSuper);
        insertar.Parameters.AddWithValue("@codigo", CodigoPerfilSuper);
        return Convert.ToInt32(insertar.ExecuteScalar());
    }

    private static int InsertarUsuario(SqlConnection connection, SqlTransaction transaction, string usuario, string correo, byte[] hash, byte[] salt, string nombreCompleto)
    {
        using var insertar = new SqlCommand(@"INSERT INTO Usuario (NombreUsuario, Correo, ClaveHash, ClaveSalt, NombreCompleto, Activo)
VALUES (@usuario, @correo, @hash, @salt, @nombre, 1);
SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction);
        insertar.Parameters.AddWithValue("@usuario", usuario);
        insertar.Parameters.AddWithValue("@correo", correo);
        insertar.Parameters.AddWithValue("@nombre", nombreCompleto);
        insertar.Parameters.Add("@hash", SqlDbType.VarBinary, SeguridadUtil.TamanoHash).Value = hash;
        insertar.Parameters.Add("@salt", SqlDbType.VarBinary, SeguridadUtil.TamanoSalt).Value = salt;
        return Convert.ToInt32(insertar.ExecuteScalar());
    }

    private static Dictionary<string, int> AsegurarPantallasBase(SqlConnection connection, SqlTransaction transaction, string creadoPor)
    {
        var resultados = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        using (var existentes = new SqlCommand($"SELECT Codigo, IdPantalla FROM Pantalla WHERE Codigo IN ({string.Join(", ", PantallasBase.Select((_, i) => "@c" + i))})", connection, transaction))
        {
            for (int i = 0; i < PantallasBase.Length; i++)
            {
                existentes.Parameters.AddWithValue("@c" + i, PantallasBase[i].Codigo);
            }

            using var reader = existentes.ExecuteReader();
            while (reader.Read())
            {
                resultados[reader.GetString(0)] = reader.GetInt32(1);
            }
        }

        foreach (var pantalla in PantallasBase)
        {
            if (!resultados.ContainsKey(pantalla.Codigo))
            {
                int? idPadre = null;
                if (!string.IsNullOrWhiteSpace(pantalla.CodigoPadre))
                {
                    if (!resultados.TryGetValue(pantalla.CodigoPadre, out var idPadreExistente))
                    {
                        using var obtenerPadre = new SqlCommand("SELECT IdPantalla FROM Pantalla WHERE Codigo = @codigo", connection, transaction);
                        obtenerPadre.Parameters.AddWithValue("@codigo", pantalla.CodigoPadre);
                        var valorPadre = obtenerPadre.ExecuteScalar();
                        if (valorPadre != null)
                        {
                            idPadreExistente = Convert.ToInt32(valorPadre);
                            resultados[pantalla.CodigoPadre] = idPadreExistente;
                        }
                        else
                        {
                            throw new InvalidOperationException($"No se encontró la pantalla padre '{pantalla.CodigoPadre}'.");
                        }
                    }

                    idPadre = idPadreExistente;
                }

                using var insertar = new SqlCommand(@"INSERT INTO Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Icono, Orden, Activo, CreadoPor)
VALUES (@codigo, @nombre, @ruta, @padre, @icono, @orden, 1, @creadoPor);
SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction);
                insertar.Parameters.AddWithValue("@codigo", pantalla.Codigo);
                insertar.Parameters.AddWithValue("@nombre", pantalla.Nombre);
                insertar.Parameters.AddWithValue("@ruta", pantalla.Ruta);
                insertar.Parameters.AddWithValue("@padre", (object?)idPadre ?? DBNull.Value);
                insertar.Parameters.AddWithValue("@icono", (object?)pantalla.Icono ?? DBNull.Value);
                insertar.Parameters.AddWithValue("@orden", pantalla.Orden);
                insertar.Parameters.AddWithValue("@creadoPor", creadoPor);

                var idNuevo = Convert.ToInt32(insertar.ExecuteScalar());
                resultados[pantalla.Codigo] = idNuevo;
            }
        }

        return resultados;
    }

    private static void OtorgarPermisosTotales(SqlConnection connection, SqlTransaction transaction, int idPerfil, IEnumerable<int> pantallas, string otorgadoPor)
    {
        foreach (var idPantalla in pantallas)
        {
            using var comando = new SqlCommand(@"MERGE PerfilPantallaAcceso AS destino
USING (SELECT @perfil AS IdPerfil, @pantalla AS IdPantalla) AS origen
ON destino.IdPerfil = origen.IdPerfil AND destino.IdPantalla = origen.IdPantalla
WHEN MATCHED THEN
    UPDATE SET PuedeVer = 1, PuedeCrear = 1, PuedeEditar = 1, PuedeEliminar = 1, PuedeExportar = 1, Activo = 1, FechaOtorgado = GETDATE(), OtorgadoPor = @otorgadoPor
WHEN NOT MATCHED THEN
    INSERT (IdPerfil, IdPantalla, PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, Activo, FechaOtorgado, OtorgadoPor)
    VALUES (@perfil, @pantalla, 1, 1, 1, 1, 1, 1, GETDATE(), @otorgadoPor);", connection, transaction);
            comando.Parameters.AddWithValue("@perfil", idPerfil);
            comando.Parameters.AddWithValue("@pantalla", idPantalla);
            comando.Parameters.AddWithValue("@otorgadoPor", otorgadoPor);
            comando.ExecuteNonQuery();
        }
    }

    private static void AsignarPerfil(SqlConnection connection, SqlTransaction transaction, int idUsuario, int idPerfil, string asignadoPor)
    {
        using var insertar = new SqlCommand(@"INSERT INTO UsuarioPerfil (IdUsuario, IdPerfil, AsignadoPor)
VALUES (@usuario, @perfil, @asignadoPor);", connection, transaction);
        insertar.Parameters.AddWithValue("@usuario", idUsuario);
        insertar.Parameters.AddWithValue("@perfil", idPerfil);
        insertar.Parameters.AddWithValue("@asignadoPor", asignadoPor);
        insertar.ExecuteNonQuery();
    }
}
