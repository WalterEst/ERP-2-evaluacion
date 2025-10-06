using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class VentasForm : Form
{
    private readonly int _idUsuario;

    private readonly ComboBox _cmbCliente = new();
    private readonly ComboBox _cmbBodega = new();
    private readonly DateTimePicker _dtpFecha = new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd/MM/yyyy HH:mm",
        Value = DateTime.Now,
        Dock = DockStyle.Fill
    };
    private readonly TextBox _txtObservaciones = new() { PlaceholderText = "Observaciones", Multiline = true, Height = 80 };

    private readonly ComboBox _cmbProducto = new();
    private readonly NumericUpDown _nudCantidad = new()
    {
        DecimalPlaces = 2,
        Minimum = 0.01M,
        Maximum = 1_000_000,
        Increment = 1,
        Value = 1
    };
    private readonly NumericUpDown _nudPrecio = new()
    {
        DecimalPlaces = 2,
        Minimum = 0.01M,
        Maximum = 1_000_000,
        Increment = 1
    };
    private readonly NumericUpDown _nudDescuento = new()
    {
        DecimalPlaces = 2,
        Minimum = 0M,
        Maximum = 1_000_000,
        Increment = 1
    };

    private readonly Button _btnAgregar = new() { Text = "Agregar" };
    private readonly Button _btnQuitar = new() { Text = "Quitar" };
    private readonly Button _btnGuardar = new() { Text = "Registrar venta" };

    private readonly DataGridView _gridDetalles = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoGenerateColumns = false
    };

    private readonly Label _lblTotales = new() { AutoSize = true, ForeColor = UiTheme.TextColor, Font = UiTheme.SectionTitleFont };
    private readonly Label _lblMensaje = new() { AutoSize = true, ForeColor = UiTheme.DangerColor };

    private DataTable? _productos;
    private DataTable _detalles = new();

    public VentasForm(int idUsuario)
    {
        _idUsuario = idUsuario;

        Text = "Ventas";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1320, 880);
        MinimumSize = new Size(1180, 760);

        UiTheme.ApplyMinimalStyle(this);

        UiTheme.StyleComboBox(_cmbCliente);
        UiTheme.StyleComboBox(_cmbBodega);
        UiTheme.StyleComboBox(_cmbProducto);
        _cmbProducto.Dock = DockStyle.Fill;
        UiTheme.StyleTextInput(_txtObservaciones);
        StyleNumericUpDown(_nudCantidad);
        StyleNumericUpDown(_nudPrecio);
        StyleNumericUpDown(_nudDescuento);

        UiTheme.StyleSecondaryButton(_btnAgregar);
        UiTheme.StyleDangerButton(_btnQuitar);
        UiTheme.StylePrimaryButton(_btnGuardar);
        _btnAgregar.Margin = new Padding(12, 0, 0, 0);
        _btnQuitar.Margin = new Padding(12, 0, 0, 0);
        _btnGuardar.Margin = new Padding(0, 24, 0, 0);

        ConfigurarGrid();
        InicializarDetalles();

        var layout = CrearLayout();
        Controls.Add(layout);

        Load += VentasForm_Load;
        _cmbProducto.SelectedIndexChanged += (_, _) => ActualizarPrecioProducto();
        _btnAgregar.Click += (_, _) => AgregarDetalle();
        _btnQuitar.Click += (_, _) => QuitarDetalle();
        _btnGuardar.Click += (_, _) => GuardarVenta();
        _gridDetalles.SelectionChanged += (_, _) => _btnQuitar.Enabled = _gridDetalles.SelectedRows.Count > 0;
        _btnQuitar.Enabled = false;
    }

    private void VentasForm_Load(object? sender, EventArgs e)
    {
        CargarClientes();
        CargarBodegas();
        CargarProductos();
        ActualizarTotales();
    }

    private void ConfigurarGrid()
    {
        _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "IdProducto", Width = 60 });
        _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Código", DataPropertyName = "Codigo", Width = 120 });
        _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", DataPropertyName = "Producto", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cantidad", DataPropertyName = "Cantidad", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Precio", DataPropertyName = "Precio", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
        _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Descuento", DataPropertyName = "Descuento", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
        _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Total", Width = 140, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
    }

    private void InicializarDetalles()
    {
        if (_detalles != null)
        {
            _detalles.RowChanged -= Detalles_RowChanged;
            _detalles.RowDeleted -= Detalles_RowChanged;
        }

        _detalles = new DataTable();
        _detalles.Columns.Add("IdProducto", typeof(int));
        _detalles.Columns.Add("Codigo", typeof(string));
        _detalles.Columns.Add("Producto", typeof(string));
        _detalles.Columns.Add("Cantidad", typeof(decimal));
        _detalles.Columns.Add("Precio", typeof(decimal));
        _detalles.Columns.Add("Descuento", typeof(decimal));
        _detalles.Columns.Add("Total", typeof(decimal));
        _detalles.RowChanged += Detalles_RowChanged;
        _detalles.RowDeleted += Detalles_RowChanged;
        _gridDetalles.DataSource = _detalles;
    }

    private void Detalles_RowChanged(object? sender, DataRowChangeEventArgs e) => ActualizarTotales();

    private void StyleNumericUpDown(NumericUpDown control)
    {
        control.BorderStyle = BorderStyle.FixedSingle;
        control.Font = UiTheme.BaseFont;
        control.Margin = new Padding(0, 6, 0, 16);
        control.MinimumSize = new Size(160, 36);
        control.MaximumSize = new Size(260, 44);
        control.Dock = DockStyle.Fill;
    }

    private Control CrearLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var card = UiTheme.CreateCardPanel();
        card.Padding = new Padding(32, 32, 32, 24);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var encabezado = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 3,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

        encabezado.Controls.Add(UiTheme.CreateSectionLabel("Bodega"), 0, 0);
        encabezado.Controls.Add(UiTheme.CreateSectionLabel("Cliente"), 1, 0);
        encabezado.Controls.Add(UiTheme.CreateSectionLabel("Fecha"), 2, 0);
        encabezado.Controls.Add(_cmbBodega, 0, 1);
        encabezado.Controls.Add(_cmbCliente, 1, 1);
        encabezado.Controls.Add(_dtpFecha, 2, 1);

        var observacionesLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        observacionesLayout.Controls.Add(new Label { Text = "Observaciones", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(0, 16, 0, 0) }, 0, 0);
        observacionesLayout.Controls.Add(_txtObservaciones, 0, 1);

        var agregarLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 5,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 16, 0, 0)
        };
        agregarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        agregarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        agregarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        agregarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
        agregarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));

        agregarLayout.Controls.Add(new Label { Text = "Producto", AutoSize = true, ForeColor = UiTheme.MutedTextColor }, 0, 0);
        agregarLayout.Controls.Add(new Label { Text = "Cantidad", AutoSize = true, ForeColor = UiTheme.MutedTextColor }, 1, 0);
        agregarLayout.Controls.Add(new Label { Text = "Precio", AutoSize = true, ForeColor = UiTheme.MutedTextColor }, 2, 0);
        agregarLayout.Controls.Add(new Label { Text = "Descuento", AutoSize = true, ForeColor = UiTheme.MutedTextColor }, 3, 0);
        agregarLayout.Controls.Add(new Label(), 4, 0);

        agregarLayout.Controls.Add(_cmbProducto, 0, 1);
        agregarLayout.Controls.Add(_nudCantidad, 1, 1);
        agregarLayout.Controls.Add(_nudPrecio, 2, 1);
        agregarLayout.Controls.Add(_nudDescuento, 3, 1);

        var accionesDetalle = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Margin = new Padding(0, 8, 0, 0)
        };
        accionesDetalle.Controls.Add(_btnAgregar);
        accionesDetalle.Controls.Add(_btnQuitar);
        agregarLayout.Controls.Add(accionesDetalle, 4, 1);

        _lblMensaje.Margin = new Padding(0, 16, 0, 0);
        _gridDetalles.Margin = new Padding(0, 16, 0, 0);
        _lblTotales.Margin = new Padding(0, 16, 0, 0);

        content.Controls.Add(encabezado, 0, 0);
        content.Controls.Add(observacionesLayout, 0, 1);
        content.Controls.Add(agregarLayout, 0, 2);
        content.Controls.Add(_gridDetalles, 0, 3);
        content.Controls.Add(_lblTotales, 0, 4);
        content.Controls.Add(_lblMensaje, 0, 5);

        card.Controls.Add(content);

        var footer = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Bottom,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Margin = new Padding(0, 24, 0, 0)
        };
        footer.Controls.Add(_btnGuardar);
        content.Controls.Add(footer, 0, 6);

        root.Controls.Add(card, 0, 0);

        return root;
    }

    private void CargarClientes()
    {
        try
        {
            var clientes = Db.GetDataTable("SELECT IdCliente, NombreCompleto FROM Cliente WHERE Activo = 1 ORDER BY NombreCompleto");
            _cmbCliente.DisplayMember = "NombreCompleto";
            _cmbCliente.ValueMember = "IdCliente";
            _cmbCliente.DataSource = clientes;
            _cmbCliente.SelectedIndex = clientes.Rows.Count > 0 ? 0 : -1;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CargarBodegas()
    {
        try
        {
            var bodegas = Db.GetDataTable("SELECT IdBodega, Nombre FROM Bodega WHERE Activo = 1 ORDER BY Nombre");
            _cmbBodega.DisplayMember = "Nombre";
            _cmbBodega.ValueMember = "IdBodega";
            _cmbBodega.DataSource = bodegas;
            _cmbBodega.SelectedIndex = bodegas.Rows.Count > 0 ? 0 : -1;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar bodegas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CargarProductos()
    {
        try
        {
            _productos = Db.GetDataTable("SELECT IdProducto, Codigo, Nombre, PrecioVenta FROM Producto WHERE Activo = 1 ORDER BY Nombre");
            _cmbProducto.DisplayMember = "Nombre";
            _cmbProducto.ValueMember = "IdProducto";
            _cmbProducto.DataSource = _productos;
            if (_productos.Rows.Count > 0)
            {
                _cmbProducto.SelectedIndex = 0;
                ActualizarPrecioProducto();
            }
            else
            {
                _cmbProducto.SelectedIndex = -1;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ActualizarPrecioProducto()
    {
        if (_cmbProducto.SelectedValue is not int idProducto || _productos == null)
        {
            return;
        }

        var fila = _productos.AsEnumerable().FirstOrDefault(r => r.Field<int>("IdProducto") == idProducto);
        if (fila != null)
        {
            var precioVenta = fila.Field<decimal>("PrecioVenta");
            _nudPrecio.Value = precioVenta > _nudPrecio.Maximum ? _nudPrecio.Maximum : precioVenta;
        }
    }

    private void AgregarDetalle()
    {
        _lblMensaje.ForeColor = UiTheme.DangerColor;
        _lblMensaje.Text = string.Empty;

        if (_cmbProducto.SelectedValue is not int idProducto)
        {
            _lblMensaje.Text = "Seleccione un producto";
            return;
        }
        if (_cmbBodega.SelectedValue is not int idBodega)
        {
            _lblMensaje.Text = "Seleccione una bodega";
            return;
        }

        var cantidad = _nudCantidad.Value;
        var precio = _nudPrecio.Value;
        var descuento = _nudDescuento.Value;

        if (cantidad <= 0)
        {
            _lblMensaje.Text = "La cantidad debe ser mayor que cero";
            return;
        }

        if (precio <= 0)
        {
            _lblMensaje.Text = "El precio debe ser mayor que cero";
            return;
        }

        try
        {
            var stockDisponible = ObtenerStockDisponible(idProducto, idBodega);
            var cantidadExistente = _detalles.AsEnumerable()
                .Where(r => r.Field<int>("IdProducto") == idProducto)
                .Sum(r => r.Field<decimal>("Cantidad"));

            if (cantidad + cantidadExistente > stockDisponible)
            {
                _lblMensaje.Text = "No hay suficiente stock disponible";
                return;
            }

            var filaExistente = _detalles.AsEnumerable().FirstOrDefault(r => r.Field<int>("IdProducto") == idProducto);
            if (filaExistente != null)
            {
                var nuevaCantidad = filaExistente.Field<decimal>("Cantidad") + cantidad;
                var nuevoDescuento = filaExistente.Field<decimal>("Descuento") + descuento;
                var totalActualizado = CalcularTotalLinea(nuevaCantidad, precio, nuevoDescuento);
                if (totalActualizado < 0)
                {
                    _lblMensaje.Text = "El descuento supera el total de la línea";
                    return;
                }

                filaExistente["Cantidad"] = nuevaCantidad;
                filaExistente["Descuento"] = nuevoDescuento;
                filaExistente["Precio"] = precio;
                filaExistente["Total"] = totalActualizado;
            }
            else
            {
                var codigo = _productos?.AsEnumerable().First(r => r.Field<int>("IdProducto") == idProducto).Field<string>("Codigo");
                var nombre = _cmbProducto.Text;
                var total = CalcularTotalLinea(cantidad, precio, descuento);
                if (total < 0)
                {
                    _lblMensaje.Text = "El descuento supera el total de la línea";
                    return;
                }

                _detalles.Rows.Add(idProducto, codigo, nombre, cantidad, precio, descuento, total);
            }

            _lblMensaje.ForeColor = UiTheme.AccentColor;
            _lblMensaje.Text = "Producto agregado";
            _nudCantidad.Value = 1;
            _nudDescuento.Value = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al agregar producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void QuitarDetalle()
    {
        _lblMensaje.Text = string.Empty;
        if (_gridDetalles.SelectedRows.Count == 0)
        {
            return;
        }

        if (_gridDetalles.SelectedRows[0].DataBoundItem is DataRowView fila)
        {
            _detalles.Rows.Remove(fila.Row);
        }
    }

    private static decimal CalcularTotalLinea(decimal cantidad, decimal precio, decimal descuento) => (cantidad * precio) - descuento;

    private decimal ObtenerStockDisponible(int idProducto, int idBodega)
    {
        var resultado = Db.Scalar("SELECT ISNULL(StockActual, 0) FROM Inventario WHERE IdProducto = @producto AND IdBodega = @bodega", p =>
        {
            p.AddWithValue("@producto", idProducto);
            p.AddWithValue("@bodega", idBodega);
        });
        return resultado == null ? 0 : Convert.ToDecimal(resultado);
    }

    private void ActualizarTotales()
    {
        if (_detalles.Rows.Count == 0)
        {
            _lblTotales.Text = "Sin productos agregados";
            return;
        }

        var subtotal = _detalles.AsEnumerable().Sum(r => CalcularTotalLinea(r.Field<decimal>("Cantidad"), r.Field<decimal>("Precio"), r.Field<decimal>("Descuento")));
        var impuestos = 0m;
        var total = subtotal + impuestos;
        _lblTotales.Text = $"Subtotal: {subtotal.ToString("C2", CultureInfo.CurrentCulture)}    Total: {total.ToString("C2", CultureInfo.CurrentCulture)}";
    }

    private void GuardarVenta()
    {
        _lblMensaje.ForeColor = UiTheme.DangerColor;
        _lblMensaje.Text = string.Empty;

        if (_cmbBodega.SelectedValue is not int idBodega)
        {
            _lblMensaje.Text = "Seleccione una bodega";
            return;
        }

        if (_detalles.Rows.Count == 0)
        {
            _lblMensaje.Text = "Agregue al menos un producto";
            return;
        }

        var clienteId = _cmbCliente.SelectedValue as int?;
        var fecha = _dtpFecha.Value;
        var observaciones = _txtObservaciones.Text.Trim();

        var subtotal = _detalles.AsEnumerable().Sum(r => CalcularTotalLinea(r.Field<decimal>("Cantidad"), r.Field<decimal>("Precio"), r.Field<decimal>("Descuento")));
        var impuestos = 0m;
        var total = subtotal + impuestos;

        try
        {
            using var connection = Db.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            var numero = GenerarNumeroVenta();
            int idVenta;
            using (var cmdVenta = new SqlCommand(@"INSERT INTO Venta (Numero, Fecha, IdUsuario, IdCliente, IdBodega, Subtotal, Impuestos, Total, Observaciones)
VALUES (@numero, @fecha, @usuario, @cliente, @bodega, @subtotal, @impuestos, @total, @observaciones);
SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction))
            {
                cmdVenta.Parameters.AddWithValue("@numero", numero);
                cmdVenta.Parameters.AddWithValue("@fecha", fecha);
                cmdVenta.Parameters.AddWithValue("@usuario", _idUsuario);
                cmdVenta.Parameters.AddWithValue("@cliente", clienteId.HasValue ? clienteId.Value : DBNull.Value);
                cmdVenta.Parameters.AddWithValue("@bodega", idBodega);
                cmdVenta.Parameters.AddWithValue("@subtotal", subtotal);
                cmdVenta.Parameters.AddWithValue("@impuestos", impuestos);
                cmdVenta.Parameters.AddWithValue("@total", total);
                cmdVenta.Parameters.AddWithValue("@observaciones", string.IsNullOrWhiteSpace(observaciones) ? DBNull.Value : observaciones);

                var resultado = cmdVenta.ExecuteScalar();
                if (resultado == null)
                {
                    throw new InvalidOperationException("No se pudo crear la venta");
                }

                idVenta = Convert.ToInt32(resultado);
            }

            foreach (DataRow row in _detalles.Rows)
            {
                var idProducto = row.Field<int>("IdProducto");
                var cantidad = row.Field<decimal>("Cantidad");
                var precio = row.Field<decimal>("Precio");
                var descuento = row.Field<decimal>("Descuento");
                var totalLinea = CalcularTotalLinea(cantidad, precio, descuento);

                int idInventario;
                decimal stockActual;
                using (var cmdInventario = new SqlCommand("SELECT IdInventario, StockActual FROM Inventario WHERE IdProducto = @producto AND IdBodega = @bodega", connection, transaction))
                {
                    cmdInventario.Parameters.AddWithValue("@producto", idProducto);
                    cmdInventario.Parameters.AddWithValue("@bodega", idBodega);
                    using var reader = cmdInventario.ExecuteReader();
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("No existe inventario para el producto seleccionado en la bodega");
                    }
                    idInventario = reader.GetInt32(0);
                    stockActual = reader.GetDecimal(1);
                }

                if (stockActual < cantidad)
                {
                    throw new InvalidOperationException($"Stock insuficiente para el producto {row["Producto"]}");
                }

                using (var cmdActualizar = new SqlCommand("UPDATE Inventario SET StockActual = StockActual - @cantidad, FechaActualizacion = GETDATE() WHERE IdInventario = @inventario", connection, transaction))
                {
                    cmdActualizar.Parameters.AddWithValue("@cantidad", cantidad);
                    cmdActualizar.Parameters.AddWithValue("@inventario", idInventario);
                    cmdActualizar.ExecuteNonQuery();
                }

                using (var cmdMovimiento = new SqlCommand(@"INSERT INTO MovimientoInventario (IdInventario, TipoMovimiento, Cantidad, Motivo, Referencia, IdUsuario)
VALUES (@inventario, 'VENTA', @cantidad, @motivo, @referencia, @usuario);", connection, transaction))
                {
                    cmdMovimiento.Parameters.AddWithValue("@inventario", idInventario);
                    cmdMovimiento.Parameters.AddWithValue("@cantidad", cantidad);
                    cmdMovimiento.Parameters.AddWithValue("@motivo", $"Venta {numero}");
                    cmdMovimiento.Parameters.AddWithValue("@referencia", numero);
                    cmdMovimiento.Parameters.AddWithValue("@usuario", _idUsuario);
                    cmdMovimiento.ExecuteNonQuery();
                }

                using (var cmdDetalle = new SqlCommand(@"INSERT INTO VentaDetalle (IdVenta, IdProducto, Cantidad, PrecioUnitario, Descuento, Total)
VALUES (@venta, @producto, @cantidad, @precio, @descuento, @total);", connection, transaction))
                {
                    cmdDetalle.Parameters.AddWithValue("@venta", idVenta);
                    cmdDetalle.Parameters.AddWithValue("@producto", idProducto);
                    cmdDetalle.Parameters.AddWithValue("@cantidad", cantidad);
                    cmdDetalle.Parameters.AddWithValue("@precio", precio);
                    cmdDetalle.Parameters.AddWithValue("@descuento", descuento);
                    cmdDetalle.Parameters.AddWithValue("@total", totalLinea);
                    cmdDetalle.ExecuteNonQuery();
                }
            }

            transaction.Commit();
            MessageBox.Show("Venta registrada correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            InicializarDetalles();
            ActualizarTotales();
            _txtObservaciones.Clear();
            _lblMensaje.ForeColor = UiTheme.AccentColor;
            _lblMensaje.Text = $"Venta {numero} registrada correctamente.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al registrar la venta: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string GenerarNumeroVenta() => $"V{DateTime.Now:yyyyMMddHHmmssfff}";
}
