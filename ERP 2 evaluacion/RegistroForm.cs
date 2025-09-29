using System;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class RegistroForm : Form
{
    internal const string CodigoPerfilPorDefecto = "BASICO";

    private readonly Label _lblTitulo = UiTheme.CreateTitleLabel("Crear una cuenta");
    private readonly Label _lblSubtitulo = new()
    {
        Text = "Completa la información para registrarte",
        AutoSize = true,
        ForeColor = UiTheme.MutedTextColor,
        Margin = new Padding(0, 0, 0, 12)
    };

    private readonly TextBox _txtNombreCompleto = new() { PlaceholderText = "Nombre completo" };
    private readonly TextBox _txtUsuario = new() { PlaceholderText = "Nombre de usuario" };
    private readonly TextBox _txtCorreo = new() { PlaceholderText = "Correo electrónico" };
    private readonly TextBox _txtClave = new() { UseSystemPasswordChar = true, PlaceholderText = "Contraseña" };
    private readonly TextBox _txtConfirmacion = new() { UseSystemPasswordChar = true, PlaceholderText = "Confirmar contraseña" };
    private readonly Button _btnRegistrar = new() { Text = "Registrar" };
    private readonly Button _btnCancelar = new() { Text = "Cancelar", DialogResult = DialogResult.Cancel };
    private readonly Label _lblMensaje = new() { AutoSize = true, ForeColor = UiTheme.DangerColor, Margin = new Padding(0, 8, 0, 0) };

    private static readonly Regex CorreoRegex = new(@"^[^\s]+@[^@\s]+\.[^\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public RegistroForm()
    {
        Text = "Registro";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        AcceptButton = _btnRegistrar;
        CancelButton = _btnCancelar;
        Width = 560;
        Height = 520;

        UiTheme.ApplyMinimalStyle(this);

        UiTheme.StyleTextInput(_txtNombreCompleto);
        UiTheme.StyleTextInput(_txtUsuario);
        UiTheme.StyleTextInput(_txtCorreo);
        UiTheme.StyleTextInput(_txtClave);
        UiTheme.StyleTextInput(_txtConfirmacion);
        UiTheme.StylePrimaryButton(_btnRegistrar);
        UiTheme.StyleSecondaryButton(_btnCancelar);

        _btnRegistrar.Click += BtnRegistrar_Click;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(_lblTitulo, 0, 0);
        layout.Controls.Add(_lblSubtitulo, 0, 1);
        layout.Controls.Add(CreateFieldLabel("Nombre completo"), 0, 2);
        layout.Controls.Add(_txtNombreCompleto, 0, 3);
        layout.Controls.Add(CreateFieldLabel("Nombre de usuario"), 0, 4);
        layout.Controls.Add(_txtUsuario, 0, 5);
        layout.Controls.Add(CreateFieldLabel("Correo electrónico"), 0, 6);
        layout.Controls.Add(_txtCorreo, 0, 7);
        layout.Controls.Add(CreateFieldLabel("Contraseña"), 0, 8);
        layout.Controls.Add(_txtClave, 0, 9);
        layout.Controls.Add(CreateFieldLabel("Confirmar contraseña"), 0, 10);
        layout.Controls.Add(_txtConfirmacion, 0, 11);
        layout.Controls.Add(_lblMensaje, 0, 12);

        var panelBotones = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 20, 0, 0)
        };
        panelBotones.Controls.Add(_btnRegistrar);
        panelBotones.Controls.Add(_btnCancelar);
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

    private void BtnRegistrar_Click(object? sender, EventArgs e)
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

        if (clave.Length < 8)
        {
            _lblMensaje.Text = "La contraseña debe tener al menos 8 caracteres";
            return;
        }

        if (!string.Equals(clave, confirmacion, StringComparison.Ordinal))
        {
            _lblMensaje.Text = "Las contraseñas no coinciden";
            return;
        }

        try
        {
            var (hash, salt) = SeguridadUtil.CrearPasswordHash(clave);

            using var connection = Db.GetConnection();
            connection.Open();
            using var command = new SqlCommand(@"INSERT INTO Usuario (NombreUsuario, Correo, ClaveHash, ClaveSalt, NombreCompleto, Activo)
VALUES (@usuario, @correo, @hash, @salt, @nombre, 1);
SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);

            command.Parameters.AddWithValue("@usuario", usuario);
            command.Parameters.AddWithValue("@correo", correo);
            command.Parameters.AddWithValue("@nombre", nombreCompleto);
            command.Parameters.Add("@hash", SqlDbType.VarBinary, SeguridadUtil.TamanoHash).Value = hash;
            command.Parameters.Add("@salt", SqlDbType.VarBinary, SeguridadUtil.TamanoSalt).Value = salt;

            var id = command.ExecuteScalar();
            if (id == null)
            {
                throw new InvalidOperationException("No se pudo crear el usuario");
            }

            var idUsuario = Convert.ToInt32(id);

            try
            {
                AsignarPerfilPorDefecto(idUsuario, CodigoPerfilPorDefecto, "app");
            }
            catch (PerfilAsignacionException)
            {
                return;
            }

            MessageBox.Show("Usuario registrado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            if (ex.Message.Contains("Correo", StringComparison.OrdinalIgnoreCase))
            {
                _lblMensaje.Text = "El correo ya está registrado";
            }
            else
            {
                _lblMensaje.Text = "El nombre de usuario ya está registrado";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al registrar usuario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void AsignarPerfilPorDefecto(int idUsuario, string codigoPerfil, string usuarioActual)
    {
        using var connection = Db.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            int idPerfil;
            using (var obtenerPerfil = new SqlCommand(
                       "SELECT IdPerfil FROM Perfil WHERE Codigo = @codigoPerfil AND Activo = 1;",
                       connection, transaction))
            {
                obtenerPerfil.Parameters.AddWithValue("@codigoPerfil", codigoPerfil);
                var resultado = obtenerPerfil.ExecuteScalar();

                if (resultado == null)
                {
                    using var crearPerfil = new SqlCommand(
                        "INSERT INTO Perfil (NombrePerfil, Codigo, Descripcion, Activo) VALUES ('Básico', @codigoPerfil, 'Perfil por defecto', 1);" +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction);
                    crearPerfil.Parameters.AddWithValue("@codigoPerfil", codigoPerfil);
                    var nuevoId = crearPerfil.ExecuteScalar();
                    if (nuevoId == null)
                    {
                        throw new InvalidOperationException("No se pudo obtener el identificador del perfil por defecto.");
                    }

                    idPerfil = Convert.ToInt32(nuevoId);
                }
                else
                {
                    idPerfil = Convert.ToInt32(resultado);
                }
            }

            int idPantallaPrincipal;
            using (var obtenerPantalla = new SqlCommand(
                       "SELECT IdPantalla FROM Pantalla WHERE Codigo = 'PRINCIPAL' AND Activo = 1;",
                       connection, transaction))
            {
                var resultadoPantalla = obtenerPantalla.ExecuteScalar();
                if (resultadoPantalla == null)
                {
                    throw new InvalidOperationException("Falta pantalla PRINCIPAL. Ejecute el seed de pantallas.");
                }

                idPantallaPrincipal = Convert.ToInt32(resultadoPantalla);
            }

            using (var merge = new SqlCommand(
                       "MERGE PerfilPantallaAcceso AS tgt " +
                       "USING (SELECT @IdPerfil AS IdPerfil, @IdPantalla AS IdPantalla) AS src " +
                       "ON (tgt.IdPerfil = src.IdPerfil AND tgt.IdPantalla = src.IdPantalla) " +
                       "WHEN NOT MATCHED THEN " +
                       "  INSERT (IdPerfil, IdPantalla, PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, Activo, FechaOtorgado, OtorgadoPor) " +
                       "  VALUES (src.IdPerfil, src.IdPantalla, 1, 0, 0, 0, 0, 1, GETDATE(), @usuarioActual) " +
                       "WHEN MATCHED THEN " +
                       "  UPDATE SET PuedeVer = 1, PuedeCrear = 0, PuedeEditar = 0, PuedeEliminar = 0, PuedeExportar = 0, Activo = 1, FechaOtorgado = GETDATE(), OtorgadoPor = @usuarioActual;",
                       connection, transaction))
            {
                merge.Parameters.AddWithValue("@IdPerfil", idPerfil);
                merge.Parameters.AddWithValue("@IdPantalla", idPantallaPrincipal);
                merge.Parameters.AddWithValue("@usuarioActual", usuarioActual);
                merge.ExecuteNonQuery();
            }

            using (var asignarPerfil = new SqlCommand(
                       "INSERT INTO UsuarioPerfil (IdUsuario, IdPerfil, AsignadoPor) " +
                       "SELECT @IdUsuario, @IdPerfil, @usuarioActual " +
                       "WHERE NOT EXISTS (SELECT 1 FROM UsuarioPerfil WHERE IdUsuario = @IdUsuario AND IdPerfil = @IdPerfil);",
                       connection, transaction))
            {
                asignarPerfil.Parameters.AddWithValue("@IdUsuario", idUsuario);
                asignarPerfil.Parameters.AddWithValue("@IdPerfil", idPerfil);
                asignarPerfil.Parameters.AddWithValue("@usuarioActual", usuarioActual);
                asignarPerfil.ExecuteNonQuery();
            }

            transaction.Commit();
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

            MessageBox.Show($"No se pudo asignar el perfil por defecto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            throw new PerfilAsignacionException("Error al asignar el perfil por defecto", ex);
        }
    }

    private sealed class PerfilAsignacionException : Exception
    {
        public PerfilAsignacionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
