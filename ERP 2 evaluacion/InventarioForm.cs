using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class InventarioForm : Form
{
    private readonly int? _idUsuario;

    private readonly ComboBox _cmbBodega = new();
    private readonly TextBox _txtBuscar = new() { PlaceholderText = "Buscar producto" };
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoGenerateColumns = false
    };

    private readonly Button _btnEntrada = new() { Text = "Registrar entrada" };
    private readonly Button _btnSalida = new() { Text = "Registrar salida" };
    private readonly Button _btnAjuste = new() { Text = "Ajuste de stock" };

    private readonly Label _lblResumen = new() { AutoSize = true, ForeColor = UiTheme.MutedTextColor };
    private readonly Label _lblMensaje = new() { AutoSize = true, ForeColor = UiTheme.DangerColor };

    private DataTable? _inventario;

    // --- NUEVO: referencias para responsividad ---
    private Panel? _card;
    private TableLayoutPanel? _root;
    private FlowLayoutPanel? _botones;

    public InventarioForm(int? idUsuario = null)
    {
        _idUsuario = idUsuario;

        Text = "Inventario";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1280, 840);
        MinimumSize = new Size(900, 600); // evita romper layout en ventanas muy pequeñas

        UiTheme.ApplyMinimalStyle(this);

        UiTheme.StyleComboBox(_cmbBodega);
        _cmbBodega.Dock = DockStyle.Fill;
        UiTheme.StyleTextInput(_txtBuscar);
        UiTheme.StyleDataGrid(_grid);

        UiTheme.StyleSecondaryButton(_btnEntrada);
        UiTheme.StyleSecondaryButton(_btnSalida);
        UiTheme.StyleSecondaryButton(_btnAjuste);
        _btnEntrada.Margin = new Padding(0, 0, 12, 0);
        _btnSalida.Margin = new Padding(0, 0, 12, 0);

        ConfigurarGrid();

        var layout = CrearLayout();
        Controls.Add(layout);

        // Responsividad
        Resize += (_, _) => ApplyResponsivePadding();
        Shown += (_, _) => ApplyResponsivePadding();

        Load += InventarioForm_Load;
        _cmbBodega.SelectedIndexChanged += (_, _) => CargarInventario();
        _txtBuscar.TextChanged += (_, _) => CargarInventario();
        _btnEntrada.Click += (_, _) => RegistrarMovimiento(TipoMovimiento.Entrada);
        _btnSalida.Click += (_, _) => RegistrarMovimiento(TipoMovimiento.Salida);
        _btnAjuste.Click += (_, _) => RegistrarMovimiento(TipoMovimiento.Ajuste);
    }

    private void InventarioForm_Load(object? sender, EventArgs e)
    {
        CargarBodegas();
        CargarInventario();
    }

    private void ConfigurarGrid()
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "IdInventario", Width = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Código", DataPropertyName = "Codigo", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock", DataPropertyName = "StockActual", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Reservado", DataPropertyName = "StockReservado", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock mínimo", DataPropertyName = "StockMinimo", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock máximo", DataPropertyName = "StockMaximo", Width = 120 });
    }

    private Control CrearLayout()
    {
        _root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1
        };
        _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _card = UiTheme.CreateCardPanel();
        _card.Dock = DockStyle.Fill;                 // Ocupa todo
        _card.Padding = new Padding(32, 32, 32, 24); // Se recalcula dinámicamente

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        header.Controls.Add(UiTheme.CreateSectionLabel("Bodega"), 0, 0);
        header.Controls.Add(UiTheme.CreateSectionLabel("Buscar"), 1, 0);
        header.Controls.Add(_cmbBodega, 0, 1);
        header.Controls.Add(_txtBuscar, 1, 1);

        var resumenPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,                 // puede saltar línea en espacios angostos
            Margin = new Padding(0, 16, 0, 0)
        };
        resumenPanel.Controls.Add(_lblResumen);

        _botones = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,                 // responsive: hace wrap cuando no cabe
            Margin = new Padding(0, 16, 0, 0)
        };
        _botones.Controls.AddRange(new Control[] { _btnEntrada, _btnSalida, _btnAjuste });

        _lblMensaje.Margin = new Padding(0, 16, 0, 0);
        _grid.Margin = new Padding(0, 24, 0, 0);
        _grid.Dock = DockStyle.Fill;

        content.Controls.Add(header, 0, 0);
        content.Controls.Add(resumenPanel, 0, 1);
        content.Controls.Add(_botones, 0, 2);
        content.Controls.Add(_lblMensaje, 0, 3);
        content.Controls.Add(_grid, 0, 4);

        _card.Controls.Add(content);
        _root.Controls.Add(_card, 0, 0);

        return _root;
    }

    // --- NUEVO: padding y gaps proporcionales al tamaño de la ventana ---
    private void ApplyResponsivePadding()
    {
        if (_card == null) return;

        // 4% del ancho y 3% del alto como padding, con mínimos razonables
        int padX = Math.Max(24, (int)(ClientSize.Width * 0.04));
        int padY = Math.Max(20, (int)(ClientSize.Height * 0.03));

        _card.Padding = new Padding(padX, padY, padX, Math.Max(16, padY - 8));

        // separaciones verticales adaptativas
        int gapYSmall = Math.Max(10, (int)(ClientSize.Height * 0.015)); // ~1.5%
        int gapYMedium = Math.Max(16, (int)(ClientSize.Height * 0.02));  // ~2%

        _lblMensaje.Margin = new Padding(0, gapYSmall, 0, 0);
        _grid.Margin = new Padding(0, gapYMedium, 0, 0);

        // Botonera: espacio entre botones según ancho
        if (_botones is not null)
        {
            int inter = Math.Max(8, (int)(ClientSize.Width * 0.008)); // ~0.8%
            foreach (Control c in _botones.Controls)
            {
                c.Margin = new Padding(0, 0, inter, 8); // un pequeño bottom para cuando haga wrap
            }
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
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar bodegas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CargarInventario()
    {
        if (_cmbBodega.SelectedValue is not int idBodega)
        {
            _grid.DataSource = null;
            _lblResumen.Text = "Seleccione una bodega para ver el inventario.";
            return;
        }

        var filtro = _txtBuscar.Text.Trim();

        try
        {
            _inventario = Db.GetDataTable(@"SELECT i.IdInventario,
       p.Codigo,
       p.Nombre,
       i.StockActual,
       i.StockReservado,
       i.StockMinimo,
       i.StockMaximo
FROM Inventario i
JOIN Producto p ON p.IdProducto = i.IdProducto
WHERE i.IdBodega = @bodega
  AND (@filtro = '' OR p.Nombre LIKE @patron OR p.Codigo LIKE @patron)
ORDER BY p.Nombre",
                p =>
                {
                    p.AddWithValue("@bodega", idBodega);
                    p.AddWithValue("@filtro", filtro);
                    p.AddWithValue("@patron", $"%{filtro}%");
                });

            _grid.DataSource = _inventario;
            ActualizarResumen();
            _lblMensaje.Text = string.Empty;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar inventario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ActualizarResumen()
    {
        if (_inventario == null || _inventario.Rows.Count == 0)
        {
            _lblResumen.Text = "No hay productos con inventario registrado en la bodega seleccionada.";
            return;
        }

        var totalProductos = _inventario.Rows.Count;
        var totalUnidades = _inventario.AsEnumerable().Sum(row => row.Field<decimal>("StockActual"));
        _lblResumen.Text = $"Productos: {totalProductos} | Unidades disponibles: {totalUnidades.ToString("N2", CultureInfo.CurrentCulture)}";
    }

    private void RegistrarMovimiento(TipoMovimiento tipo)
    {
        if (!TryObtenerSeleccion(out var idInventario, out var producto, out var stockActual))
        {
            return;
        }

        var dialogo = new MovimientoDialog(tipo, producto, stockActual);
        if (dialogo.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            using var connection = Db.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            decimal nuevoStock;
            decimal cantidadMovimiento;
            switch (tipo)
            {
                case TipoMovimiento.Entrada:
                    nuevoStock = stockActual + dialogo.Cantidad;
                    cantidadMovimiento = dialogo.Cantidad;
                    EjecutarActualizacionInventario(connection, transaction, idInventario, nuevoStock);
                    RegistrarMovimientoInventario(connection, transaction, idInventario, "ENTRADA", cantidadMovimiento, dialogo.Motivo);
                    break;
                case TipoMovimiento.Salida:
                    if (dialogo.Cantidad > stockActual)
                    {
                        throw new InvalidOperationException("La cantidad a retirar supera el stock disponible");
                    }
                    nuevoStock = stockActual - dialogo.Cantidad;
                    cantidadMovimiento = dialogo.Cantidad;
                    EjecutarActualizacionInventario(connection, transaction, idInventario, nuevoStock);
                    RegistrarMovimientoInventario(connection, transaction, idInventario, "SALIDA", cantidadMovimiento, dialogo.Motivo);
                    break;
                case TipoMovimiento.Ajuste:
                    nuevoStock = dialogo.NuevoStock;
                    if (Math.Abs(nuevoStock - stockActual) < 0.0001m)
                    {
                        throw new InvalidOperationException("El ajuste no modifica el stock actual");
                    }
                    cantidadMovimiento = Math.Abs(nuevoStock - stockActual);
                    EjecutarActualizacionInventario(connection, transaction, idInventario, nuevoStock);
                    RegistrarMovimientoInventario(connection, transaction, idInventario, "AJUSTE", cantidadMovimiento, dialogo.Motivo + $" (antes: {stockActual}, ahora: {nuevoStock})");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipo));
            }

            transaction.Commit();
            CargarInventario();
            _lblMensaje.ForeColor = UiTheme.AccentColor;
            _lblMensaje.Text = tipo switch
            {
                TipoMovimiento.Entrada => $"Entrada registrada por {dialogo.Cantidad.ToString("N2", CultureInfo.CurrentCulture)} unidades.",
                TipoMovimiento.Salida => $"Salida registrada por {dialogo.Cantidad.ToString("N2", CultureInfo.CurrentCulture)} unidades.",
                _ => "Ajuste de inventario registrado correctamente."
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al registrar movimiento: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void EjecutarActualizacionInventario(SqlConnection connection, SqlTransaction transaction, int idInventario, decimal nuevoStock)
    {
        using var command = new SqlCommand("UPDATE Inventario SET StockActual = @stock, FechaActualizacion = GETDATE() WHERE IdInventario = @id", connection, transaction);
        command.Parameters.AddWithValue("@stock", nuevoStock);
        command.Parameters.AddWithValue("@id", idInventario);
        command.ExecuteNonQuery();
    }

    private void RegistrarMovimientoInventario(SqlConnection connection, SqlTransaction transaction, int idInventario, string tipo, decimal cantidad, string motivo)
    {
        using var command = new SqlCommand(@"INSERT INTO MovimientoInventario (IdInventario, TipoMovimiento, Cantidad, Motivo, Referencia, IdUsuario)
VALUES (@inventario, @tipo, @cantidad, @motivo, @referencia, @usuario);", connection, transaction);
        command.Parameters.AddWithValue("@inventario", idInventario);
        command.Parameters.AddWithValue("@tipo", tipo);
        command.Parameters.AddWithValue("@cantidad", cantidad);
        command.Parameters.AddWithValue("@motivo", string.IsNullOrWhiteSpace(motivo) ? DBNull.Value : motivo.Trim());
        command.Parameters.AddWithValue("@referencia", DBNull.Value);
        command.Parameters.AddWithValue("@usuario", _idUsuario.HasValue ? _idUsuario.Value : DBNull.Value);
        command.ExecuteNonQuery();
    }

    private bool TryObtenerSeleccion(out int idInventario, out string producto, out decimal stockActual)
    {
        idInventario = 0;
        producto = string.Empty;
        stockActual = 0m;

        if (_grid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Seleccione un producto del inventario", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        if (_grid.SelectedRows[0].DataBoundItem is not DataRowView fila)
        {
            return false;
        }

        idInventario = fila.Row.Field<int>("IdInventario");
        producto = fila.Row.Field<string>("Nombre");
        stockActual = fila.Row.Field<decimal>("StockActual");
        return true;
    }

    private enum TipoMovimiento
    {
        Entrada,
        Salida,
        Ajuste
    }

    private sealed class MovimientoDialog : Form
    {
        private readonly NumericUpDown _nudCantidad = new()
        {
            DecimalPlaces = 2,
            Minimum = 0,
            Maximum = 1_000_000,
            Increment = 1,
            Dock = DockStyle.Fill
        };

        private readonly TextBox _txtMotivo = new() { PlaceholderText = "Motivo" };
        private readonly bool _esAjuste;
        private readonly decimal _stockActual;

        private readonly TableLayoutPanel _layout; // para ajustar padding dinámico

        public decimal Cantidad => _nudCantidad.Value;
        public decimal NuevoStock => _esAjuste ? _nudCantidad.Value : _stockActual;
        public string Motivo => _txtMotivo.Text.Trim();

        public MovimientoDialog(TipoMovimiento tipo, string producto, decimal stockActual)
        {
            _esAjuste = tipo == TipoMovimiento.Ajuste;
            _stockActual = stockActual;

            Text = tipo switch
            {
                TipoMovimiento.Entrada => "Registrar entrada",
                TipoMovimiento.Salida => "Registrar salida",
                _ => "Ajustar stock"
            };
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Size = new Size(460, 280);
            MinimumSize = new Size(420, 260);

            UiTheme.ApplyMinimalStyle(this);

            UiTheme.StyleTextInput(_txtMotivo);
            _txtMotivo.Margin = new Padding(0, 12, 0, 12);

            var lblProducto = new Label
            {
                Text = $"Producto: {producto}",
                AutoSize = true,
                ForeColor = UiTheme.MutedTextColor
            };
            var lblActual = new Label
            {
                Text = $"Stock actual: {stockActual.ToString("N2", CultureInfo.CurrentCulture)}",
                AutoSize = true,
                ForeColor = UiTheme.MutedTextColor,
                Margin = new Padding(0, 6, 0, 0)
            };

            var lblCantidad = new Label
            {
                Text = _esAjuste ? "Nuevo stock" : "Cantidad",
                AutoSize = true,
                ForeColor = UiTheme.MutedTextColor,
                Margin = new Padding(0, 12, 0, 0)
            };

            if (_esAjuste)
            {
                _nudCantidad.Value = stockActual;
            }

            var btnAceptar = new Button { Text = "Aceptar", DialogResult = DialogResult.OK };
            var btnCancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel };
            UiTheme.StylePrimaryButton(btnAceptar);
            UiTheme.StyleSecondaryButton(btnCancelar);
            btnAceptar.Margin = new Padding(0, 0, 8, 0);

            var botones = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Margin = new Padding(0, 24, 0, 0)
            };
            botones.Controls.Add(btnAceptar);
            botones.Controls.Add(btnCancelar);

            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7
            };
            for (int i = 0; i < 7; i++)
            {
                _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            _layout.Controls.Add(lblProducto, 0, 0);
            _layout.Controls.Add(lblActual, 0, 1);
            _layout.Controls.Add(lblCantidad, 0, 2);
            _layout.Controls.Add(_nudCantidad, 0, 3);
            _layout.Controls.Add(new Label { Text = "Motivo", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(0, 12, 0, 0) }, 0, 4);
            _layout.Controls.Add(_txtMotivo, 0, 5);
            _layout.Controls.Add(botones, 0, 6);

            Controls.Add(_layout);

            AcceptButton = btnAceptar;
            CancelButton = btnCancelar;

            btnAceptar.Click += (_, _) =>
            {
                if (_esAjuste && _nudCantidad.Value < 0)
                {
                    MessageBox.Show("El stock no puede ser negativo", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                }
                else if (!_esAjuste && _nudCantidad.Value <= 0)
                {
                    MessageBox.Show("Ingrese una cantidad mayor a cero", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                }
            };

            // Responsividad
            Resize += (_, _) => ApplyResponsivePadding();
            Shown += (_, _) => ApplyResponsivePadding();
        }

        private void ApplyResponsivePadding()
        {
            int padX = Math.Max(16, (int)(ClientSize.Width * 0.06)); // 6% del ancho
            int padY = Math.Max(12, (int)(ClientSize.Height * 0.06)); // 6% del alto

            _layout.Padding = new Padding(padX, padY, padX, padY - 4);

            // separación vertical entre bloques
            int gapY = Math.Max(8, (int)(ClientSize.Height * 0.04));
            _txtMotivo.Margin = new Padding(0, gapY, 0, 0);
        }
    }
}
