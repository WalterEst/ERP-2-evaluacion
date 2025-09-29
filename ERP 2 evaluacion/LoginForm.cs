using Microsoft.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class LoginForm : Form
{
    private readonly TextBox _txtUsuario = new() { PlaceholderText = "Usuario o correo" };
    private readonly TextBox _txtClave = new() { UseSystemPasswordChar = true, PlaceholderText = "Contraseña" };
    private readonly Button _btnIngresar = new() { Text = "Ingresar" };
    private readonly Label _lblMensaje = new() { AutoSize = true, ForeColor = Color.DarkRed };

    public LoginForm()
    {
        Text = "Ingreso";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        AcceptButton = _btnIngresar;
        Width = 360;
        Height = 240;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 6,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _btnIngresar.Click += BtnIngresar_Click;

        layout.Controls.Add(new Label { Text = "Usuario o Correo", AutoSize = true }, 0, 0);
        layout.Controls.Add(_txtUsuario, 0, 1);
        layout.Controls.Add(new Label { Text = "Contraseña", AutoSize = true }, 0, 2);
        layout.Controls.Add(_txtClave, 0, 3);
        layout.Controls.Add(_lblMensaje, 0, 4);

        var panelBotones = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        panelBotones.Controls.Add(_btnIngresar);
        layout.Controls.Add(panelBotones, 0, 5);

        Controls.Add(layout);
    }

    private void BtnIngresar_Click(object? sender, EventArgs e)
    {
        _lblMensaje.Text = string.Empty;
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
            using var command = new SqlCommand(@"SELECT TOP 1 IdUsuario, NombreUsuario, Correo, ClaveHash, ClaveSalt, NombreCompleto, Activo 
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

            var salt = (byte[])reader["ClaveSalt"];
            var hash = (byte[])reader["ClaveHash"];
            if (!SeguridadUtil.VerificarPassword(clave, salt, hash))
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
}
