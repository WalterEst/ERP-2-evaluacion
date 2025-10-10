using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class ClientesForm : Form
{
    private readonly int? _idUsuario;
    private SeguridadUtil.PermisosPantalla? _permisosPantalla;
    private bool _puedeAdministrar = true;

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

    public ClientesForm(int? idUsuario = null)
    {
        _idUsuario = idUsuario;

        Text = "Clientes";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1200, 780);

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

        Load += ClientesForm_Load;
        _grid.SelectionChanged += (_, _) => CargarSeleccion();
        _btnNuevo.Click += (_, _) => LimpiarFormulario();
        _btnGuardar.Click += (_, _) => GuardarCliente();
        _btnActivar.Click += (_, _) => CambiarEstado(true);
        _btnDesactivar.Click += (_, _) => CambiarEstado(false);
    }

    private void ClientesForm_Load(object? sender, EventArgs e)
    {
        if (!AplicarPermisos())
        {
            ActualizarAccionesEstado(null);
            return;
        }

        CargarClientes();
        ActualizarAccionesEstado(null);
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
        var puedeAdministrar = _puedeAdministrar;
        _btnActivar.Enabled = puedeAdministrar && haySeleccion && activo == false;
        _btnDesactivar.Enabled = puedeAdministrar && haySeleccion && activo != false;
    }

    private void GuardarCliente()
    {
        if (_permisosPantalla is { TieneAccesoColaboracion: false })
        {
            MessageBox.Show("No cuentas con permisos para crear o editar clientes.", "Acción no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

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
        if (_permisosPantalla is { TieneAccesoAdministracion: false })
        {
            MessageBox.Show("No cuentas con permisos de administración para cambiar el estado de los clientes.", "Acción no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

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

    private bool AplicarPermisos()
    {
        if (!_idUsuario.HasValue)
        {
            _permisosPantalla = null;
            _puedeAdministrar = true;
            EstablecerModoColaboracion(true);
            _lblMensaje.ForeColor = UiTheme.DangerColor;
            _lblMensaje.Text = string.Empty;
            return true;
        }

        try
        {
            _permisosPantalla = SeguridadUtil.ObtenerPermisosPantalla(_idUsuario.Value, "CLIENTES");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No fue posible validar tus permisos: {ex.Message}", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _permisosPantalla = null;
            _puedeAdministrar = true;
            EstablecerModoColaboracion(true);
            return true;
        }

        if (_permisosPantalla is { TieneAccesoLectura: false })
        {
            _puedeAdministrar = false;
            EstablecerModoSinAcceso();
            _lblMensaje.ForeColor = UiTheme.MutedTextColor;
            _lblMensaje.Text = "No cuentas con permisos de lectura sobre Clientes.";
            MessageBox.Show("No cuentas con permisos de lectura para Clientes.", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        var puedeColaborar = _permisosPantalla.TieneAccesoColaboracion;
        _puedeAdministrar = _permisosPantalla.TieneAccesoAdministracion;

        if (!puedeColaborar)
        {
            EstablecerModoLectura();
            _lblMensaje.ForeColor = UiTheme.MutedTextColor;
            _lblMensaje.Text = "Tienes acceso de lectura. Las acciones de guardado y cambio de estado están deshabilitadas.";
        }
        else
        {
            EstablecerModoColaboracion(_puedeAdministrar);
            _lblMensaje.ForeColor = UiTheme.DangerColor;
            _lblMensaje.Text = string.Empty;
        }

        return true;
    }

    private void EstablecerModoSinAcceso()
    {
        _grid.Enabled = false;
        _grid.DataSource = null;
        HabilitarCampos(false);
        EstablecerSoloLecturaCampos(true);
        _btnNuevo.Enabled = false;
        _btnGuardar.Enabled = false;
        _btnActivar.Enabled = false;
        _btnDesactivar.Enabled = false;
    }

    private void EstablecerModoLectura()
    {
        _grid.Enabled = true;
        HabilitarCampos(true);
        EstablecerSoloLecturaCampos(true);
        _btnNuevo.Enabled = false;
        _btnGuardar.Enabled = false;
        _btnActivar.Enabled = false;
        _btnDesactivar.Enabled = false;
    }

    private void EstablecerModoColaboracion(bool puedeAdministrar)
    {
        _grid.Enabled = true;
        HabilitarCampos(true);
        EstablecerSoloLecturaCampos(false);
        _btnNuevo.Enabled = true;
        _btnGuardar.Enabled = true;
        _btnActivar.Enabled = false;
        _btnDesactivar.Enabled = false;
        _puedeAdministrar = puedeAdministrar;
    }

    private void HabilitarCampos(bool habilitar)
    {
        foreach (var control in new Control[] { _txtNombre, _txtIdentificacion, _txtTipoDocumento, _txtCorreo, _txtTelefono, _txtDireccion })
        {
            control.Enabled = habilitar;
        }

        _chkActivo.Enabled = habilitar;
    }

    private void EstablecerSoloLecturaCampos(bool soloLectura)
    {
        foreach (var control in new[] { _txtNombre, _txtIdentificacion, _txtTipoDocumento, _txtCorreo, _txtTelefono, _txtDireccion })
        {
            control.ReadOnly = soloLectura;
        }

        if (_chkActivo.Enabled)
        {
            _chkActivo.Enabled = !soloLectura;
        }
    }
}
