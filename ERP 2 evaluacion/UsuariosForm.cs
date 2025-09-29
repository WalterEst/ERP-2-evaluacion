using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class UsuariosForm : Form
{
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoGenerateColumns = false };
    private readonly TextBox _txtNombreUsuario = new();
    private readonly TextBox _txtCorreo = new();
    private readonly TextBox _txtNombreCompleto = new();
    private readonly CheckBox _chkActivo = new() { Text = "Activo", Checked = true };
    private readonly Button _btnNuevo = new() { Text = "Nuevo" };
    private readonly Button _btnGuardar = new() { Text = "Guardar" };
    private readonly Button _btnEliminar = new() { Text = "Eliminar" };
    private readonly Button _btnReiniciar = new() { Text = "Reiniciar contraseña" };
    private readonly Label _lblMensaje = new() { AutoSize = true, ForeColor = Color.DarkRed };

    private int? _idSeleccionado;

    public UsuariosForm()
    {
        Text = "Usuarios";
        Width = 900;
        Height = 600;

        ConfigurarGrid();

        var panelEdicion = CrearPanelEdicion();

        var contenedor = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 320 };
        contenedor.Panel1.Controls.Add(_grid);
        contenedor.Panel2.Controls.Add(panelEdicion);

        Controls.Add(contenedor);

        Load += (_, _) => CargarUsuarios();
        _grid.SelectionChanged += Grid_SelectionChanged;
        _btnNuevo.Click += (_, _) => LimpiarFormulario();
        _btnGuardar.Click += (_, _) => GuardarUsuario();
        _btnEliminar.Click += (_, _) => EliminarUsuario();
        _btnReiniciar.Click += (_, _) => ReiniciarContrasena();
    }

    private void ConfigurarGrid()
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "IdUsuario", Width = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Usuario", DataPropertyName = "NombreUsuario", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Correo", DataPropertyName = "Correo", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre Completo", DataPropertyName = "NombreCompleto", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Activo", DataPropertyName = "Activo" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Último ingreso", DataPropertyName = "UltimoIngreso", Width = 150 });
    }

    private Control CrearPanelEdicion()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 5,
            Padding = new Padding(10)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(new Label { Text = "Usuario", AutoSize = true }, 0, 0);
        layout.Controls.Add(_txtNombreUsuario, 0, 1);
        layout.SetColumnSpan(_txtNombreUsuario, 2);

        layout.Controls.Add(new Label { Text = "Correo", AutoSize = true }, 2, 0);
        layout.Controls.Add(_txtCorreo, 2, 1);
        layout.SetColumnSpan(_txtCorreo, 2);

        layout.Controls.Add(new Label { Text = "Nombre completo", AutoSize = true }, 0, 2);
        layout.Controls.Add(_txtNombreCompleto, 0, 3);
        layout.SetColumnSpan(_txtNombreCompleto, 4);

        layout.Controls.Add(_chkActivo, 0, 4);
        layout.Controls.Add(_lblMensaje, 1, 4);
        layout.SetColumnSpan(_lblMensaje, 3);

        var panelBotones = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
        panelBotones.Controls.AddRange(new Control[] { _btnReiniciar, _btnEliminar, _btnGuardar, _btnNuevo });

        var panel = new Panel { Dock = DockStyle.Fill };
        panel.Controls.Add(layout);
        panel.Controls.Add(panelBotones);

        return panel;
    }

    private void CargarUsuarios()
    {
        try
        {
            var tabla = Db.GetDataTable("SELECT IdUsuario, NombreUsuario, Correo, NombreCompleto, Activo, UltimoIngreso FROM Usuario ORDER BY NombreUsuario");
            _grid.DataSource = tabla;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar usuarios: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_grid.SelectedRows.Count == 0)
        {
            return;
        }

        if (_grid.SelectedRows[0].DataBoundItem is DataRowView fila)
        {
            _idSeleccionado = (int)fila["IdUsuario"];
            _txtNombreUsuario.Text = fila["NombreUsuario"].ToString();
            _txtCorreo.Text = fila["Correo"].ToString();
            _txtNombreCompleto.Text = fila["NombreCompleto"].ToString();
            _chkActivo.Checked = fila.Row.Field<bool>("Activo");
        }
    }

    private void LimpiarFormulario()
    {
        _idSeleccionado = null;
        _txtNombreUsuario.Text = string.Empty;
        _txtCorreo.Text = string.Empty;
        _txtNombreCompleto.Text = string.Empty;
        _chkActivo.Checked = true;
        _lblMensaje.Text = string.Empty;
        _grid.ClearSelection();
    }

    private void GuardarUsuario()
    {
        _lblMensaje.Text = string.Empty;
        var nombreUsuario = _txtNombreUsuario.Text.Trim();
        var correo = _txtCorreo.Text.Trim();
        var nombreCompleto = _txtNombreCompleto.Text.Trim();
        var activo = _chkActivo.Checked;

        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(nombreCompleto))
        {
            _lblMensaje.Text = "Complete los campos requeridos";
            return;
        }

        if (!Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            _lblMensaje.Text = "Correo inválido";
            return;
        }

        try
        {
            using var connection = Db.GetConnection();
            using var verificarNombre = new SqlCommand("SELECT COUNT(*) FROM Usuario WHERE NombreUsuario = @nombre AND (@id IS NULL OR IdUsuario <> @id)", connection);
            verificarNombre.Parameters.AddWithValue("@nombre", nombreUsuario);
            verificarNombre.Parameters.AddWithValue("@id", (object?)_idSeleccionado ?? DBNull.Value);
            var existeNombre = (int)verificarNombre.ExecuteScalar();
            if (existeNombre > 0)
            {
                _lblMensaje.Text = "Nombre de usuario ya existe";
                return;
            }

            using var verificarCorreo = new SqlCommand("SELECT COUNT(*) FROM Usuario WHERE Correo = @correo AND (@id IS NULL OR IdUsuario <> @id)", connection);
            verificarCorreo.Parameters.AddWithValue("@correo", correo);
            verificarCorreo.Parameters.AddWithValue("@id", (object?)_idSeleccionado ?? DBNull.Value);
            var existeCorreo = (int)verificarCorreo.ExecuteScalar();
            if (existeCorreo > 0)
            {
                _lblMensaje.Text = "Correo ya existe";
                return;
            }

            if (_idSeleccionado.HasValue)
            {
                using var actualizar = new SqlCommand("UPDATE Usuario SET NombreUsuario = @nombre, Correo = @correo, NombreCompleto = @completo, Activo = @activo WHERE IdUsuario = @id", connection);
                actualizar.Parameters.AddWithValue("@nombre", nombreUsuario);
                actualizar.Parameters.AddWithValue("@correo", correo);
                actualizar.Parameters.AddWithValue("@completo", nombreCompleto);
                actualizar.Parameters.AddWithValue("@activo", activo);
                actualizar.Parameters.AddWithValue("@id", _idSeleccionado.Value);
                actualizar.ExecuteNonQuery();
                MessageBox.Show("Usuario actualizado", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var contrasena = SeguridadUtil.GenerarPasswordTemporal();
                var (hash, salt) = SeguridadUtil.CrearPasswordHash(contrasena);
                using var insertar = new SqlCommand(@"INSERT INTO Usuario(NombreUsuario, Correo, ClaveHash, ClaveSalt, NombreCompleto, Activo)
VALUES(@nombre, @correo, @hash, @salt, @completo, @activo)", connection);
                insertar.Parameters.AddWithValue("@nombre", nombreUsuario);
                insertar.Parameters.AddWithValue("@correo", correo);
                insertar.Parameters.Add("@hash", SqlDbType.VarBinary, SeguridadUtil.TamanoHash).Value = hash;
                insertar.Parameters.Add("@salt", SqlDbType.VarBinary, SeguridadUtil.TamanoSalt).Value = salt;
                insertar.Parameters.AddWithValue("@completo", nombreCompleto);
                insertar.Parameters.AddWithValue("@activo", activo);
                insertar.ExecuteNonQuery();
                MessageBox.Show($"Usuario creado. Contraseña temporal: {contrasena}", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar usuario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        CargarUsuarios();
        LimpiarFormulario();
    }

    private void EliminarUsuario()
    {
        if (!_idSeleccionado.HasValue)
        {
            MessageBox.Show("Seleccione un usuario", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show("¿Eliminar usuario seleccionado?", "Usuarios", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            using var connection = Db.GetConnection();
            using var command = new SqlCommand("DELETE FROM Usuario WHERE IdUsuario = @id", connection);
            command.Parameters.AddWithValue("@id", _idSeleccionado.Value);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al eliminar usuario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        CargarUsuarios();
        LimpiarFormulario();
    }

    private void ReiniciarContrasena()
    {
        if (!_idSeleccionado.HasValue)
        {
            MessageBox.Show("Seleccione un usuario", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var nuevaContrasena = SeguridadUtil.GenerarPasswordTemporal();
        var (hash, salt) = SeguridadUtil.CrearPasswordHash(nuevaContrasena);

        try
        {
            using var connection = Db.GetConnection();
            using var command = new SqlCommand("UPDATE Usuario SET ClaveHash = @hash, ClaveSalt = @salt WHERE IdUsuario = @id", connection);
            command.Parameters.Add("@hash", SqlDbType.VarBinary, SeguridadUtil.TamanoHash).Value = hash;
            command.Parameters.Add("@salt", SqlDbType.VarBinary, SeguridadUtil.TamanoSalt).Value = salt;
            command.Parameters.AddWithValue("@id", _idSeleccionado.Value);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al reiniciar contraseña: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        MessageBox.Show($"Contraseña temporal: {nuevaContrasena}", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
