using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class ClientesForm : Form
{
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoGenerateColumns = false
    };

    private readonly TextBox _txtNombre = new() { PlaceholderText = "Nombre completo" };
    private readonly TextBox _txtIdentificacion = new() { PlaceholderText = "Documento" };
    private readonly TextBox _txtTipoDocumento = new() { PlaceholderText = "Tipo de documento" };
    private readonly TextBox _txtCorreo = new() { PlaceholderText = "Correo" };
    private readonly TextBox _txtTelefono = new() { PlaceholderText = "Teléfono" };
    private readonly TextBox _txtDireccion = new() { PlaceholderText = "Dirección", Multiline = true, Height = 80 };
    private readonly CheckBox _chkActivo = new() { Text = "Activo", Checked = true };

    private readonly Button _btnNuevo = new() { Text = "Nuevo" };
    private readonly Button _btnGuardar = new() { Text = "Guardar" };
    private readonly Button _btnActivar = new() { Text = "Activar" };
    private readonly Button _btnDesactivar = new() { Text = "Desactivar" };

    private readonly Label _lblMensaje = new() { AutoSize = true, ForeColor = UiTheme.DangerColor };

    private int? _idSeleccionado;

    public ClientesForm()
    {
        Text = "Clientes";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1200, 780);
        MinimumSize = new Size(1024, 680);

        UiTheme.ApplyMinimalStyle(this);

        ConfigurarGrid();
        UiTheme.StyleDataGrid(_grid);

        UiTheme.StyleTextInput(_txtNombre);
        UiTheme.StyleTextInput(_txtIdentificacion);
        UiTheme.StyleTextInput(_txtTipoDocumento);
        UiTheme.StyleTextInput(_txtCorreo);
        UiTheme.StyleTextInput(_txtTelefono);
        UiTheme.StyleTextInput(_txtDireccion);
        UiTheme.StyleCheckBox(_chkActivo);

        UiTheme.StyleSecondaryButton(_btnNuevo);
        UiTheme.StylePrimaryButton(_btnGuardar);
        UiTheme.StyleSuccessButton(_btnActivar);
        UiTheme.StyleDangerButton(_btnDesactivar);
        _btnGuardar.Margin = new Padding(0);

        var layout = CrearLayout();

        var gridCard = UiTheme.CreateCardPanel();
        gridCard.Padding = new Padding(32, 32, 32, 24);
        gridCard.Controls.Add(_grid);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            BorderStyle = BorderStyle.None,
            SplitterWidth = 12
        };
        split.Panel1.Controls.Add(gridCard);
        split.Panel2.Controls.Add(layout);

        Controls.Add(split);

        Load += (_, _) =>
        {
            CargarClientes();
            ActualizarAccionesEstado(null);
        };
        _grid.SelectionChanged += (_, _) => CargarSeleccion();
        _btnNuevo.Click += (_, _) => LimpiarFormulario();
        _btnGuardar.Click += (_, _) => GuardarCliente();
        _btnActivar.Click += (_, _) => CambiarEstado(true);
        _btnDesactivar.Click += (_, _) => CambiarEstado(false);
    }

    private void ConfigurarGrid()
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "IdCliente", Width = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "NombreCompleto", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Documento", DataPropertyName = "Identificacion", Width = 140 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tipo", DataPropertyName = "TipoDocumento", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Correo", DataPropertyName = "Correo", Width = 200 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Teléfono", DataPropertyName = "Telefono", Width = 140 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Activo", DataPropertyName = "Activo", Width = 80 });
    }

    private Control CrearLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(0, 0, 0, 16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        layout.Controls.Add(new Label { Text = "Nombre", AutoSize = true, ForeColor = UiTheme.MutedTextColor }, 0, 0);
        layout.Controls.Add(new Label { Text = "Documento", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(16, 0, 0, 0) }, 1, 0);
        layout.Controls.Add(_txtNombre, 0, 1);
        layout.Controls.Add(_txtIdentificacion, 1, 1);

        layout.Controls.Add(new Label { Text = "Tipo de documento", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(0, 12, 0, 0) }, 0, 2);
        layout.Controls.Add(new Label { Text = "Correo", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(16, 12, 0, 0) }, 1, 2);
        layout.Controls.Add(_txtTipoDocumento, 0, 3);
        layout.Controls.Add(_txtCorreo, 1, 3);

        layout.Controls.Add(new Label { Text = "Teléfono", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(0, 12, 0, 0) }, 0, 4);
        layout.Controls.Add(new Label { Text = "Dirección", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(16, 12, 0, 0) }, 1, 4);
        layout.Controls.Add(_txtTelefono, 0, 5);
        layout.Controls.Add(_txtDireccion, 1, 5);

        layout.Controls.Add(_chkActivo, 0, 6);
        layout.SetColumnSpan(_chkActivo, 2);
        layout.Controls.Add(_lblMensaje, 0, 7);
        layout.SetColumnSpan(_lblMensaje, 2);

        var botones = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Bottom,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Padding = new Padding(0, 24, 0, 0)
        };
        botones.Controls.AddRange(new Control[] { _btnGuardar, _btnNuevo, _btnActivar, _btnDesactivar });

        var card = UiTheme.CreateCardPanel();
        card.AutoScroll = true;
        card.Padding = new Padding(32, 32, 32, 24);
        card.Controls.Add(layout);
        card.Controls.Add(botones);

        return card;
    }

    private void CargarClientes()
    {
        try
        {
            _grid.DataSource = Db.GetDataTable("SELECT IdCliente, NombreCompleto, Identificacion, TipoDocumento, Correo, Telefono, Direccion, Activo FROM Cliente ORDER BY NombreCompleto");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CargarSeleccion()
    {
        if (_grid.SelectedRows.Count == 0)
        {
            return;
        }

        if (_grid.SelectedRows[0].DataBoundItem is DataRowView fila)
        {
            _idSeleccionado = (int)fila["IdCliente"];
            _txtNombre.Text = Convert.ToString(fila["NombreCompleto"]) ?? string.Empty;
            _txtIdentificacion.Text = Convert.ToString(fila["Identificacion"]) ?? string.Empty;
            _txtTipoDocumento.Text = Convert.ToString(fila["TipoDocumento"]) ?? string.Empty;
            _txtCorreo.Text = Convert.ToString(fila["Correo"]) ?? string.Empty;
            _txtTelefono.Text = Convert.ToString(fila["Telefono"]) ?? string.Empty;
            _txtDireccion.Text = Convert.ToString(fila["Direccion"]) ?? string.Empty;
            var activo = fila.Row.Field<bool>("Activo");
            _chkActivo.Checked = activo;
            ActualizarAccionesEstado(activo);
        }
    }

    private void LimpiarFormulario()
    {
        _idSeleccionado = null;
        _txtNombre.Text = string.Empty;
        _txtIdentificacion.Text = string.Empty;
        _txtTipoDocumento.Text = string.Empty;
        _txtCorreo.Text = string.Empty;
        _txtTelefono.Text = string.Empty;
        _txtDireccion.Text = string.Empty;
        _chkActivo.Checked = true;
        _lblMensaje.Text = string.Empty;
        _grid.ClearSelection();
        ActualizarAccionesEstado(null);
    }

    private void ActualizarAccionesEstado(bool? activo)
    {
        var haySeleccion = _idSeleccionado != null;
        _btnActivar.Enabled = haySeleccion && activo == false;
        _btnDesactivar.Enabled = haySeleccion && activo != false;
    }

    private void GuardarCliente()
    {
        _lblMensaje.Text = string.Empty;

        var nombre = _txtNombre.Text.Trim();
        var identificacion = _txtIdentificacion.Text.Trim();
        var tipoDocumento = _txtTipoDocumento.Text.Trim();
        var correo = _txtCorreo.Text.Trim();
        var telefono = _txtTelefono.Text.Trim();
        var direccion = _txtDireccion.Text.Trim();
        var activo = _chkActivo.Checked;

        if (string.IsNullOrWhiteSpace(nombre))
        {
            _lblMensaje.Text = "El nombre es obligatorio";
            return;
        }

        try
        {
            using var connection = Db.GetConnection();
            connection.Open();

            SqlCommand command;
            if (_idSeleccionado == null)
            {
                command = new SqlCommand(@"INSERT INTO Cliente (NombreCompleto, Identificacion, TipoDocumento, Correo, Telefono, Direccion, Activo)
VALUES (@nombre, @identificacion, @tipo, @correo, @telefono, @direccion, @activo);", connection);
            }
            else
            {
                command = new SqlCommand(@"UPDATE Cliente
SET NombreCompleto = @nombre,
    Identificacion = @identificacion,
    TipoDocumento = @tipo,
    Correo = @correo,
    Telefono = @telefono,
    Direccion = @direccion,
    Activo = @activo
WHERE IdCliente = @id;", connection);
                command.Parameters.AddWithValue("@id", _idSeleccionado.Value);
            }

            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@identificacion", string.IsNullOrWhiteSpace(identificacion) ? DBNull.Value : identificacion);
            command.Parameters.AddWithValue("@tipo", string.IsNullOrWhiteSpace(tipoDocumento) ? DBNull.Value : tipoDocumento);
            command.Parameters.AddWithValue("@correo", string.IsNullOrWhiteSpace(correo) ? DBNull.Value : correo);
            command.Parameters.AddWithValue("@telefono", string.IsNullOrWhiteSpace(telefono) ? DBNull.Value : telefono);
            command.Parameters.AddWithValue("@direccion", string.IsNullOrWhiteSpace(direccion) ? DBNull.Value : direccion);
            command.Parameters.AddWithValue("@activo", activo);

            command.ExecuteNonQuery();

            MessageBox.Show("Cliente guardado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            CargarClientes();
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            _lblMensaje.Text = "Ya existe un cliente con ese documento";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar cliente: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CambiarEstado(bool activar)
    {
        if (_idSeleccionado == null)
        {
            MessageBox.Show("Seleccione un cliente primero", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var accion = activar ? "activar" : "desactivar";
        if (MessageBox.Show($"¿Desea {accion} el cliente seleccionado?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            Db.Execute("UPDATE Cliente SET Activo = @activo WHERE IdCliente = @id", p =>
            {
                p.AddWithValue("@activo", activar);
                p.AddWithValue("@id", _idSeleccionado);
            });

            CargarClientes();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al actualizar cliente: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
