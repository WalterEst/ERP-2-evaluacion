using Microsoft.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class PerfilesForm : Form
{
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoGenerateColumns = false };
    private readonly TextBox _txtNombre = new();
    private readonly TextBox _txtCodigo = new();
    private readonly TextBox _txtDescripcion = new() { Multiline = true, Height = 80 };
    private readonly CheckBox _chkActivo = new() { Text = "Activo", Checked = true };
    private readonly Button _btnNuevo = new() { Text = "Nuevo" };
    private readonly Button _btnGuardar = new() { Text = "Guardar" };
    private readonly Button _btnEliminar = new() { Text = "Eliminar" };
    private readonly Label _lblMensaje = new() { AutoSize = true, ForeColor = Color.DarkRed };

    private int? _idSeleccionado;

    public PerfilesForm()
    {
        Text = "Perfiles";
        Width = 800;
        Height = 550;

        ConfigurarGrid();
        var panelEdicion = CrearPanelEdicion();

        var contenedor = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 300 };
        contenedor.Panel1.Controls.Add(_grid);
        contenedor.Panel2.Controls.Add(panelEdicion);

        Controls.Add(contenedor);

        Load += (_, _) => CargarPerfiles();
        _grid.SelectionChanged += Grid_SelectionChanged;
        _btnNuevo.Click += (_, _) => LimpiarFormulario();
        _btnGuardar.Click += (_, _) => GuardarPerfil();
        _btnEliminar.Click += (_, _) => EliminarPerfil();
    }

    private void ConfigurarGrid()
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "IdPerfil", Width = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "NombrePerfil", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Código", DataPropertyName = "Codigo", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Descripción", DataPropertyName = "Descripcion", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Activo", DataPropertyName = "Activo" });
    }

    private Control CrearPanelEdicion()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(10)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

        layout.Controls.Add(new Label { Text = "Nombre", AutoSize = true }, 0, 0);
        layout.Controls.Add(_txtNombre, 1, 0);
        layout.Controls.Add(new Label { Text = "Código", AutoSize = true }, 0, 1);
        layout.Controls.Add(_txtCodigo, 1, 1);
        layout.Controls.Add(new Label { Text = "Descripción", AutoSize = true }, 0, 2);
        layout.Controls.Add(_txtDescripcion, 1, 2);
        layout.Controls.Add(_chkActivo, 1, 3);
        layout.Controls.Add(_lblMensaje, 1, 4);

        var panelBotones = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
        panelBotones.Controls.AddRange(new Control[] { _btnEliminar, _btnGuardar, _btnNuevo });

        var panel = new Panel { Dock = DockStyle.Fill };
        panel.Controls.Add(layout);
        panel.Controls.Add(panelBotones);
        return panel;
    }

    private void CargarPerfiles()
    {
        try
        {
            _grid.DataSource = Db.GetDataTable("SELECT IdPerfil, NombrePerfil, Codigo, Descripcion, Activo FROM Perfil ORDER BY NombrePerfil");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar perfiles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            _idSeleccionado = (int)fila["IdPerfil"];
            _txtNombre.Text = fila["NombrePerfil"].ToString();
            _txtCodigo.Text = fila["Codigo"].ToString();
            _txtDescripcion.Text = fila["Descripcion"].ToString();
            _chkActivo.Checked = fila.Row.Field<bool>("Activo");
        }
    }

    private void LimpiarFormulario()
    {
        _idSeleccionado = null;
        _txtNombre.Text = string.Empty;
        _txtCodigo.Text = string.Empty;
        _txtDescripcion.Text = string.Empty;
        _chkActivo.Checked = true;
        _lblMensaje.Text = string.Empty;
        _grid.ClearSelection();
    }

    private void GuardarPerfil()
    {
        _lblMensaje.Text = string.Empty;
        var nombre = _txtNombre.Text.Trim();
        var codigo = _txtCodigo.Text.Trim();
        var descripcion = _txtDescripcion.Text.Trim();
        var activo = _chkActivo.Checked;

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(codigo))
        {
            _lblMensaje.Text = "Complete nombre y código";
            return;
        }

        try
        {
            using var connection = Db.GetConnection();
            connection.Open();
            using var verificarNombre = new SqlCommand("SELECT COUNT(*) FROM Perfil WHERE NombrePerfil = @nombre AND (@id IS NULL OR IdPerfil <> @id)", connection);
            verificarNombre.Parameters.AddWithValue("@nombre", nombre);
            verificarNombre.Parameters.AddWithValue("@id", (object?)_idSeleccionado ?? DBNull.Value);
            if ((int)verificarNombre.ExecuteScalar() > 0)
            {
                _lblMensaje.Text = "Nombre ya existe";
                return;
            }

            using var verificarCodigo = new SqlCommand("SELECT COUNT(*) FROM Perfil WHERE Codigo = @codigo AND (@id IS NULL OR IdPerfil <> @id)", connection);
            verificarCodigo.Parameters.AddWithValue("@codigo", codigo);
            verificarCodigo.Parameters.AddWithValue("@id", (object?)_idSeleccionado ?? DBNull.Value);
            if ((int)verificarCodigo.ExecuteScalar() > 0)
            {
                _lblMensaje.Text = "Código ya existe";
                return;
            }

            if (_idSeleccionado.HasValue)
            {
                using var actualizar = new SqlCommand("UPDATE Perfil SET NombrePerfil = @nombre, Codigo = @codigo, Descripcion = @descripcion, Activo = @activo WHERE IdPerfil = @id", connection);
                actualizar.Parameters.AddWithValue("@nombre", nombre);
                actualizar.Parameters.AddWithValue("@codigo", codigo);
                actualizar.Parameters.AddWithValue("@descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : descripcion);
                actualizar.Parameters.AddWithValue("@activo", activo);
                actualizar.Parameters.AddWithValue("@id", _idSeleccionado.Value);
                actualizar.ExecuteNonQuery();
                MessageBox.Show("Perfil actualizado", "Perfiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                using var insertar = new SqlCommand("INSERT INTO Perfil(NombrePerfil, Codigo, Descripcion, Activo) VALUES(@nombre, @codigo, @descripcion, @activo)", connection);
                insertar.Parameters.AddWithValue("@nombre", nombre);
                insertar.Parameters.AddWithValue("@codigo", codigo);
                insertar.Parameters.AddWithValue("@descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : descripcion);
                insertar.Parameters.AddWithValue("@activo", activo);
                insertar.ExecuteNonQuery();
                MessageBox.Show("Perfil creado", "Perfiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar perfil: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        CargarPerfiles();
        LimpiarFormulario();
    }

    private void EliminarPerfil()
    {
        if (!_idSeleccionado.HasValue)
        {
            MessageBox.Show("Seleccione un perfil", "Perfiles", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var connection = Db.GetConnection();
            connection.Open();
            using var validar = new SqlCommand("SELECT COUNT(*) FROM UsuarioPerfil WHERE IdPerfil = @id", connection);
            validar.Parameters.AddWithValue("@id", _idSeleccionado.Value);
            if ((int)validar.ExecuteScalar() > 0)
            {
                MessageBox.Show("No es posible eliminar: hay usuarios asignados", "Perfiles", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("¿Eliminar perfil seleccionado?", "Perfiles", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            using var eliminar = new SqlCommand("DELETE FROM Perfil WHERE IdPerfil = @id", connection);
            eliminar.Parameters.AddWithValue("@id", _idSeleccionado.Value);
            eliminar.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al eliminar perfil: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        CargarPerfiles();
        LimpiarFormulario();
    }
}
