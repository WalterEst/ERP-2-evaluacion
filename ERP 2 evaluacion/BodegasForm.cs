using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class BodegasForm : Form
{
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoGenerateColumns = false
    };

    private readonly TextBox _txtCodigo = new() { PlaceholderText = "Código" };
    private readonly TextBox _txtNombre = new() { PlaceholderText = "Nombre" };
    private readonly TextBox _txtUbicacion = new() { PlaceholderText = "Ubicación" };
    private readonly TextBox _txtEncargado = new() { PlaceholderText = "Encargado" };
    private readonly TextBox _txtDescripcion = new() { Multiline = true, Height = 80, PlaceholderText = "Descripción" };
    private readonly CheckBox _chkActivo = new() { Text = "Activo", Checked = true };

    private readonly Button _btnNuevo = new() { Text = "Nuevo" };
    private readonly Button _btnGuardar = new() { Text = "Guardar" };
    private readonly Button _btnActivar = new() { Text = "Activar" };
    private readonly Button _btnDesactivar = new() { Text = "Desactivar" };

    private readonly Label _lblMensaje = new() { AutoSize = true, ForeColor = UiTheme.DangerColor };

    private int? _idSeleccionado;

    public BodegasForm()
    {
        Text = "Bodegas";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1200, 780);

        UiTheme.ApplyMinimalStyle(this);

        ConfigurarGrid();
        UiTheme.StyleDataGrid(_grid);

        UiTheme.StyleTextInput(_txtCodigo);
        UiTheme.StyleTextInput(_txtNombre);
        UiTheme.StyleTextInput(_txtUbicacion);
        UiTheme.StyleTextInput(_txtEncargado);
        UiTheme.StyleTextInput(_txtDescripcion);
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
            CargarBodegas();
            ActualizarAccionesEstado(null);
        };
        _grid.SelectionChanged += (_, _) => CargarSeleccion();
        _btnNuevo.Click += (_, _) => LimpiarFormulario();
        _btnGuardar.Click += (_, _) => GuardarBodega();
        _btnDesactivar.Click += (_, _) => CambiarEstadoBodega(false);
        _btnActivar.Click += (_, _) => CambiarEstadoBodega(true);
    }

    private void ConfigurarGrid()
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "IdBodega", Width = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Código", DataPropertyName = "Codigo", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ubicación", DataPropertyName = "Ubicacion", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Encargado", DataPropertyName = "Encargado", Width = 200 });
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

        layout.Controls.Add(new Label { Text = "Código", AutoSize = true, ForeColor = UiTheme.MutedTextColor }, 0, 0);
        layout.Controls.Add(new Label { Text = "Nombre", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(16, 0, 0, 0) }, 1, 0);
        layout.Controls.Add(_txtCodigo, 0, 1);
        layout.Controls.Add(_txtNombre, 1, 1);

        layout.Controls.Add(new Label { Text = "Ubicación", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(0, 12, 0, 0) }, 0, 2);
        layout.Controls.Add(new Label { Text = "Encargado", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(16, 12, 0, 0) }, 1, 2);
        layout.Controls.Add(_txtUbicacion, 0, 3);
        layout.Controls.Add(_txtEncargado, 1, 3);

        layout.Controls.Add(new Label { Text = "Descripción", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(0, 12, 0, 0) }, 0, 4);
        layout.SetColumnSpan(_txtDescripcion, 2);
        layout.Controls.Add(_txtDescripcion, 0, 5);

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

    private void CargarBodegas()
    {
        try
        {
            _grid.DataSource = Db.GetDataTable("SELECT IdBodega, Codigo, Nombre, Ubicacion, Encargado, Descripcion, Activo FROM Bodega ORDER BY Nombre");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar bodegas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            _idSeleccionado = (int)fila["IdBodega"];
            _txtCodigo.Text = Convert.ToString(fila["Codigo"]) ?? string.Empty;
            _txtNombre.Text = Convert.ToString(fila["Nombre"]) ?? string.Empty;
            _txtUbicacion.Text = Convert.ToString(fila["Ubicacion"]) ?? string.Empty;
            _txtEncargado.Text = Convert.ToString(fila["Encargado"]) ?? string.Empty;
            _txtDescripcion.Text = Convert.ToString(fila["Descripcion"]) ?? string.Empty;
            var activo = fila.Row.Field<bool>("Activo");
            _chkActivo.Checked = activo;
            ActualizarAccionesEstado(activo);
        }
    }

    private void LimpiarFormulario()
    {
        _idSeleccionado = null;
        _txtCodigo.Text = string.Empty;
        _txtNombre.Text = string.Empty;
        _txtUbicacion.Text = string.Empty;
        _txtEncargado.Text = string.Empty;
        _txtDescripcion.Text = string.Empty;
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

    private void GuardarBodega()
    {
        _lblMensaje.Text = string.Empty;
        var codigo = _txtCodigo.Text.Trim();
        var nombre = _txtNombre.Text.Trim();
        var ubicacion = _txtUbicacion.Text.Trim();
        var encargado = _txtEncargado.Text.Trim();
        var descripcion = _txtDescripcion.Text.Trim();
        var activo = _chkActivo.Checked;

        if (codigo.Length == 0 || nombre.Length == 0)
        {
            _lblMensaje.Text = "Código y nombre son obligatorios";
            return;
        }

        try
        {
            using var connection = Db.GetConnection();
            connection.Open();

            SqlCommand command;
            if (_idSeleccionado == null)
            {
                command = new SqlCommand(@"INSERT INTO Bodega (Codigo, Nombre, Ubicacion, Encargado, Descripcion, Activo)
VALUES (@codigo, @nombre, @ubicacion, @encargado, @descripcion, @activo);", connection);
            }
            else
            {
                command = new SqlCommand(@"UPDATE Bodega
SET Codigo = @codigo,
    Nombre = @nombre,
    Ubicacion = @ubicacion,
    Encargado = @encargado,
    Descripcion = @descripcion,
    Activo = @activo
WHERE IdBodega = @id;", connection);
                command.Parameters.AddWithValue("@id", _idSeleccionado.Value);
            }

            command.Parameters.AddWithValue("@codigo", codigo);
            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@ubicacion", string.IsNullOrWhiteSpace(ubicacion) ? DBNull.Value : ubicacion);
            command.Parameters.AddWithValue("@encargado", string.IsNullOrWhiteSpace(encargado) ? DBNull.Value : encargado);
            command.Parameters.AddWithValue("@descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : descripcion);
            command.Parameters.AddWithValue("@activo", activo);

            command.ExecuteNonQuery();

            MessageBox.Show("Bodega guardada correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            CargarBodegas();
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            _lblMensaje.Text = "El código de la bodega ya existe";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar la bodega: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CambiarEstadoBodega(bool activar)
    {
        if (_idSeleccionado == null)
        {
            MessageBox.Show("Seleccione una bodega primero", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var mensaje = activar ? "activar" : "desactivar";
        if (MessageBox.Show($"¿Seguro que desea {mensaje} la bodega seleccionada?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            Db.Execute("UPDATE Bodega SET Activo = @activo WHERE IdBodega = @id", p =>
            {
                p.AddWithValue("@activo", activar);
                p.AddWithValue("@id", _idSeleccionado);
            });

            CargarBodegas();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al actualizar la bodega: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
