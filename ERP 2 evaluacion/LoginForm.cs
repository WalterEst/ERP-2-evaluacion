using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class LoginForm : Form
{
    private readonly Label _lblTitulo = UiTheme.CreateTitleLabel("Bienvenido");
    private readonly Label _lblSubtitulo = new()
    {
        Text = "Ingresa con tu cuenta para continuar",
        AutoSize = true,
        ForeColor = UiTheme.MutedTextColor,
        Margin = new Padding(0, 0, 0, 12)
    };
    private readonly TextBox _txtUsuario = new() { PlaceholderText = "Usuario o correo" };
    private readonly TextBox _txtClave = new() { UseSystemPasswordChar = true, PlaceholderText = "Contraseña", MaxLength = 50 };
    private readonly CheckBox _chkMostrarClave = new() { Text = "Mostrar contraseña" };
    private readonly Button _btnIngresar = new() { Text = "Ingresar" };
    private readonly Button _btnRegistrarse = new() { Text = "Crear cuenta" };
    private readonly Button _btnSuperUsuario = new() { Text = "Crear super usuario" };
    private readonly Label _lblMensaje = new() { AutoSize = true, ForeColor = UiTheme.DangerColor, Margin = new Padding(0, 8, 0, 0) };

    private string? _ultimoUsuarioConsultado;
    private bool? _ultimoUsuarioPrivilegiado;

    public LoginForm()
    {
        Text = "Ingreso";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        AcceptButton = _btnIngresar;
        Size = new Size(720, 540);
        MinimumSize = new Size(560, 480);

        UiTheme.ApplyMinimalStyle(this);

        UiTheme.StyleTextInput(_txtUsuario);
        UiTheme.StyleTextInput(_txtClave);
        UiTheme.StyleCheckBox(_chkMostrarClave);
        UiTheme.StylePrimaryButton(_btnIngresar);
        UiTheme.StyleSecondaryButton(_btnRegistrarse);
        UiTheme.StyleSecondaryButton(_btnSuperUsuario);
        _btnSuperUsuario.Visible = false;

        _btnIngresar.Click += BtnIngresar_Click;
        _btnRegistrarse.Click += BtnRegistrarse_Click;
        _btnSuperUsuario.Click += BtnSuperUsuario_Click;
        Load += LoginForm_Load;
        _chkMostrarClave.CheckedChanged += (_, _) => AlternarVisibilidadClave();
        _txtUsuario.TextChanged += (_, _) => ReiniciarVerificacionPrivilegios();

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

        layout.Controls.Add(_lblTitulo, 0, 0);
        layout.Controls.Add(_lblSubtitulo, 0, 1);
        layout.Controls.Add(new Label { Text = "Usuario o correo", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(0, 16, 0, 6) }, 0, 2);
        layout.Controls.Add(_txtUsuario, 0, 3);
        layout.Controls.Add(new Label { Text = "Contraseña", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(0, 6, 0, 6) }, 0, 4);
        layout.Controls.Add(_txtClave, 0, 5);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_chkMostrarClave, 0, 6);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_lblMensaje, 0, 7);

        var panelBotones = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 24, 0, 0),
            WrapContents = false
        };
        panelBotones.Controls.Add(_btnIngresar);
        panelBotones.Controls.Add(_btnRegistrarse);
        panelBotones.Controls.Add(_btnSuperUsuario);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(panelBotones, 0, 8);

        var card = UiTheme.CreateCardPanel();
        card.AutoSize = true;
        card.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        card.Anchor = AnchorStyles.None;
        card.Padding = new Padding(40, 40, 40, 32);
        card.MaximumSize = new Size(560, 0);
        card.MinimumSize = new Size(520, 0);
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

    private void LoginForm_Load(object? sender, EventArgs e)
    {
        try
        {
            _btnSuperUsuario.Visible = !SuperUsuarioExiste();
        }
        catch (Exception ex)
        {
            _btnSuperUsuario.Visible = false;
            MessageBox.Show($"No se pudo verificar el super usuario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnIngresar_Click(object? sender, EventArgs e)
    {
        _lblMensaje.Text = string.Empty;
        _lblMensaje.ForeColor = UiTheme.DangerColor;
        var usuario = _txtUsuario.Text.Trim();
        var clave = _txtClave.Text;

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
        {
            _lblMensaje.Text = "Ingrese usuario y contraseña";
            return;
        }

        try
        {
            using var connection = Db.GetConnection();
            connection.Open();
            using var command = new SqlCommand(@"SELECT TOP 1 IdUsuario, NombreUsuario, Correo, Clave, NombreCompleto, Activo
FROM Usuario WHERE NombreUsuario = @usuario OR Correo = @usuario", connection);
            command.Parameters.AddWithValue("@usuario", usuario);

            using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
            if (!reader.Read())
            {
                _lblMensaje.Text = "Usuario no encontrado";
                return;
            }

            var activo = reader.GetBoolean(reader.GetOrdinal("Activo"));
            if (!activo)
            {
                _lblMensaje.Text = "Usuario inactivo";
                return;
            }

            var claveAlmacenada = reader.GetString(reader.GetOrdinal("Clave"));
            if (!string.Equals(claveAlmacenada, clave, StringComparison.Ordinal))
            {
                _lblMensaje.Text = "Contraseña incorrecta";
                return;
            }

            var idUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario"));
            var nombreCompleto = reader.GetString(reader.GetOrdinal("NombreCompleto"));
            reader.Close();

            using var update = new SqlCommand("UPDATE Usuario SET UltimoIngreso = @fecha WHERE IdUsuario = @id", connection);
            update.Parameters.AddWithValue("@fecha", DateTime.Now);
            update.Parameters.AddWithValue("@id", idUsuario);
            update.ExecuteNonQuery();

            Hide();
            using var principal = new PrincipalForm(idUsuario, nombreCompleto);
            principal.FormClosed += (_, _) => Close();
            principal.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al iniciar sesión: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnRegistrarse_Click(object? sender, EventArgs e)
    {
        using var registro = new RegistroForm();
        if (registro.ShowDialog(this) == DialogResult.OK)
        {
            _lblMensaje.ForeColor = UiTheme.AccentColor;
            _lblMensaje.Text = "Cuenta creada con éxito. Ahora puedes iniciar sesión.";
        }
    }

    private void BtnSuperUsuario_Click(object? sender, EventArgs e)
    {
        using var super = new SuperUsuarioForm();
        if (super.ShowDialog(this) == DialogResult.OK)
        {
            _btnSuperUsuario.Visible = false;
            _lblMensaje.ForeColor = UiTheme.AccentColor;
            _lblMensaje.Text = "Super usuario creado. Inicia sesión con esas credenciales.";
        }
    }

    private void AlternarVisibilidadClave()
    {
        if (!_chkMostrarClave.Checked)
        {
            _txtClave.UseSystemPasswordChar = true;
            return;
        }

        var identificador = _txtUsuario.Text.Trim();

        if (string.IsNullOrWhiteSpace(identificador))
        {
            _chkMostrarClave.Checked = false;
            MessageBox.Show("Ingresa el usuario o correo antes de mostrar la contraseña.",
                "Mostrar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            bool esPrivilegiado;
            if (_ultimoUsuarioPrivilegiado.HasValue &&
                string.Equals(_ultimoUsuarioConsultado, identificador, StringComparison.OrdinalIgnoreCase))
            {
                esPrivilegiado = _ultimoUsuarioPrivilegiado.Value;
            }
            else
            {
                esPrivilegiado = SeguridadUtil.EsUsuarioPrivilegiadoPorIdentificador(identificador);
                _ultimoUsuarioConsultado = identificador;
                _ultimoUsuarioPrivilegiado = esPrivilegiado;
            }

            if (!esPrivilegiado)
            {
                _chkMostrarClave.Checked = false;
                MessageBox.Show("Solo los usuarios privilegiados pueden mostrar la contraseña.",
                    "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        catch (Exception ex)
        {
            _chkMostrarClave.Checked = false;
            MessageBox.Show($"No se pudo validar el nivel de acceso: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _txtClave.UseSystemPasswordChar = false;
    }

    private void ReiniciarVerificacionPrivilegios()
    {
        _ultimoUsuarioConsultado = null;
        _ultimoUsuarioPrivilegiado = null;
        if (_chkMostrarClave.Checked)
        {
            _chkMostrarClave.Checked = false;
        }
        else
        {
            _txtClave.UseSystemPasswordChar = true;
        }
    }

    private static bool SuperUsuarioExiste()
    {
        var resultado = Db.Scalar(@"SELECT TOP 1 1
FROM UsuarioPerfil up
JOIN Perfil p ON p.IdPerfil = up.IdPerfil
WHERE p.Codigo = @codigo;", parametros => parametros.AddWithValue("@codigo", "SUPERADMIN"));

        return resultado != null && resultado != DBNull.Value;
    }
}
