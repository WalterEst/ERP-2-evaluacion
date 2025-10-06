using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class ProductosForm : Form
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
    private readonly TextBox _txtDescripcion = new() { PlaceholderText = "Descripción", Multiline = true, Height = 80 };
    private readonly ComboBox _cmbCategoria = new();
    private readonly Button _btnNuevaCategoria = new() { Text = "Nueva categoría" };
    private readonly TextBox _txtPrecioCosto = new() { PlaceholderText = "Precio costo" };
    private readonly TextBox _txtPrecioVenta = new() { PlaceholderText = "Precio venta" };
    private readonly TextBox _txtStockMinimo = new() { PlaceholderText = "Stock mínimo" };
    private readonly TextBox _txtStockMaximo = new() { PlaceholderText = "Stock máximo" };
    private readonly CheckBox _chkActivo = new() { Text = "Activo", Checked = true };

    private readonly Button _btnNuevo = new() { Text = "Nuevo" };
    private readonly Button _btnGuardar = new() { Text = "Guardar" };
    private readonly Button _btnActivar = new() { Text = "Activar" };
    private readonly Button _btnDesactivar = new() { Text = "Desactivar" };

    private readonly Label _lblMensaje = new() { AutoSize = true, ForeColor = UiTheme.DangerColor };

    private DataTable? _categorias;
    private int? _idSeleccionado;

    public ProductosForm()
    {
        Text = "Productos";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1280, 840);

        UiTheme.ApplyMinimalStyle(this);

        ConfigurarGrid();
        UiTheme.StyleDataGrid(_grid);

        UiTheme.StyleTextInput(_txtCodigo);
        UiTheme.StyleTextInput(_txtNombre);
        UiTheme.StyleTextInput(_txtDescripcion);
        UiTheme.StyleComboBox(_cmbCategoria);
        _cmbCategoria.Dock = DockStyle.None;
        _cmbCategoria.Width = 240;
        UiTheme.StyleTextInput(_txtPrecioCosto);
        UiTheme.StyleTextInput(_txtPrecioVenta);
        UiTheme.StyleTextInput(_txtStockMinimo);
        UiTheme.StyleTextInput(_txtStockMaximo);
        UiTheme.StyleCheckBox(_chkActivo);

        UiTheme.StyleSecondaryButton(_btnNuevaCategoria);
        _btnNuevaCategoria.Margin = new Padding(12, 0, 0, 0);
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

        Load += ProductosForm_Load;
        _grid.SelectionChanged += (_, _) => CargarSeleccion();
        _btnNuevo.Click += (_, _) => LimpiarFormulario();
        _btnGuardar.Click += (_, _) => GuardarProducto();
        _btnActivar.Click += (_, _) => CambiarEstado(true);
        _btnDesactivar.Click += (_, _) => CambiarEstado(false);
        _btnNuevaCategoria.Click += (_, _) => CrearCategoria();
    }

    private void ProductosForm_Load(object? sender, EventArgs e)
    {
        CargarCategorias();
        CargarProductos();
        ActualizarAccionesEstado(null);
    }

    private void ConfigurarGrid()
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "IdProducto", Width = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Código", DataPropertyName = "Codigo", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Categoría", DataPropertyName = "Categoria", Width = 180 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Precio venta", DataPropertyName = "PrecioVenta", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock mínimo", DataPropertyName = "StockMinimo", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock máximo", DataPropertyName = "StockMaximo", Width = 120 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Activo", DataPropertyName = "Activo", Width = 80 });
    }

    private Control CrearLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            Padding = new Padding(0, 0, 0, 16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        int row = 0;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = "Código", AutoSize = true, ForeColor = UiTheme.MutedTextColor }, 0, row);
        layout.Controls.Add(new Label { Text = "Nombre", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(16, 0, 0, 0) }, 1, row);
        row++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_txtCodigo, 0, row);
        layout.SetColumnSpan(_txtCodigo, 1);
        layout.Controls.Add(_txtNombre, 1, row);
        layout.SetColumnSpan(_txtNombre, 3);
        row++;

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = "Categoría", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(0, 12, 0, 0) }, 0, row);
        layout.Controls.Add(new Label { Text = "Descripción", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(16, 12, 0, 0) }, 1, row);
        row++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var categoriaPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 0)
        };
        categoriaPanel.Controls.Add(_cmbCategoria);
        categoriaPanel.Controls.Add(_btnNuevaCategoria);
        layout.Controls.Add(categoriaPanel, 0, row);
        layout.SetColumnSpan(categoriaPanel, 1);
        layout.Controls.Add(_txtDescripcion, 1, row);
        layout.SetColumnSpan(_txtDescripcion, 3);
        row++;

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = "Precio costo", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(0, 12, 0, 0) }, 0, row);
        layout.Controls.Add(new Label { Text = "Precio venta", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(16, 12, 0, 0) }, 1, row);
        layout.Controls.Add(new Label { Text = "Stock mínimo", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(16, 12, 0, 0) }, 2, row);
        layout.Controls.Add(new Label { Text = "Stock máximo", AutoSize = true, ForeColor = UiTheme.MutedTextColor, Margin = new Padding(16, 12, 0, 0) }, 3, row);
        row++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_txtPrecioCosto, 0, row);
        layout.Controls.Add(_txtPrecioVenta, 1, row);
        layout.Controls.Add(_txtStockMinimo, 2, row);
        layout.Controls.Add(_txtStockMaximo, 3, row);
        row++;

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_chkActivo, 0, row);
        layout.SetColumnSpan(_chkActivo, 4);
        row++;

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_lblMensaje, 0, row);
        layout.SetColumnSpan(_lblMensaje, 4);

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

    private void CargarCategorias()
    {
        try
        {
            _categorias = Db.GetDataTable("SELECT IdCategoria, Nombre FROM CategoriaProducto WHERE Activo = 1 ORDER BY Nombre");
            _cmbCategoria.DisplayMember = "Nombre";
            _cmbCategoria.ValueMember = "IdCategoria";
            _cmbCategoria.DataSource = _categorias;
            if (_categorias.Rows.Count == 0)
            {
                _cmbCategoria.SelectedIndex = -1;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar categorías: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CargarProductos()
    {
        try
        {
            _grid.DataSource = Db.GetDataTable(@"SELECT p.IdProducto,
       p.Codigo,
       p.Nombre,
       p.Descripcion,
       p.IdCategoria,
       p.PrecioCosto,
       p.PrecioVenta,
       p.StockMinimo,
       p.StockMaximo,
       p.Activo,
       ISNULL(c.Nombre, 'Sin categoría') AS Categoria
FROM Producto p
LEFT JOIN CategoriaProducto c ON c.IdCategoria = p.IdCategoria
ORDER BY p.Nombre");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            _idSeleccionado = (int)fila["IdProducto"];
            _txtCodigo.Text = Convert.ToString(fila["Codigo"]) ?? string.Empty;
            _txtNombre.Text = Convert.ToString(fila["Nombre"]) ?? string.Empty;
            _txtDescripcion.Text = Convert.ToString(fila["Descripcion"]) ?? string.Empty;
            _txtPrecioCosto.Text = Convert.ToString(fila["PrecioCosto"]) ?? string.Empty;
            _txtPrecioVenta.Text = Convert.ToString(fila["PrecioVenta"]) ?? string.Empty;
            _txtStockMinimo.Text = Convert.ToString(fila["StockMinimo"]) ?? string.Empty;
            _txtStockMaximo.Text = Convert.ToString(fila["StockMaximo"]) ?? string.Empty;
            var activo = fila.Row.Field<bool>("Activo");
            _chkActivo.Checked = activo;
            ActualizarAccionesEstado(activo);

            var idCategoria = fila["IdCategoria"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["IdCategoria"]);
            if (idCategoria.HasValue && _categorias != null)
            {
                _cmbCategoria.SelectedValue = idCategoria.Value;
            }
            else if (_categorias != null)
            {
                _cmbCategoria.SelectedIndex = -1;
            }
        }
    }

    private void LimpiarFormulario()
    {
        _idSeleccionado = null;
        _txtCodigo.Text = string.Empty;
        _txtNombre.Text = string.Empty;
        _txtDescripcion.Text = string.Empty;
        _txtPrecioCosto.Text = string.Empty;
        _txtPrecioVenta.Text = string.Empty;
        _txtStockMinimo.Text = string.Empty;
        _txtStockMaximo.Text = string.Empty;
        _chkActivo.Checked = true;
        _lblMensaje.Text = string.Empty;
        if (_categorias != null && _categorias.Rows.Count > 0)
        {
            _cmbCategoria.SelectedIndex = 0;
        }
        else
        {
            _cmbCategoria.SelectedIndex = -1;
        }
        _grid.ClearSelection();
        ActualizarAccionesEstado(null);
    }

    private void ActualizarAccionesEstado(bool? activo)
    {
        var haySeleccion = _idSeleccionado != null;
        _btnActivar.Enabled = haySeleccion && activo == false;
        _btnDesactivar.Enabled = haySeleccion && activo != false;
    }

    private void GuardarProducto()
    {
        _lblMensaje.Text = string.Empty;

        var codigo = _txtCodigo.Text.Trim();
        var nombre = _txtNombre.Text.Trim();
        var descripcion = _txtDescripcion.Text.Trim();
        var precioCostoTexto = _txtPrecioCosto.Text.Trim();
        var precioVentaTexto = _txtPrecioVenta.Text.Trim();
        var stockMinimoTexto = _txtStockMinimo.Text.Trim();
        var stockMaximoTexto = _txtStockMaximo.Text.Trim();
        var activo = _chkActivo.Checked;

        if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre))
        {
            _lblMensaje.Text = "Código y nombre son obligatorios";
            return;
        }

        if (_cmbCategoria.SelectedValue is not int idCategoria)
        {
            _lblMensaje.Text = "Seleccione una categoría";
            return;
        }

        if (!decimal.TryParse(precioCostoTexto, NumberStyles.Number, CultureInfo.CurrentCulture, out var precioCosto) || precioCosto < 0)
        {
            _lblMensaje.Text = "Precio de costo inválido";
            return;
        }

        if (!decimal.TryParse(precioVentaTexto, NumberStyles.Number, CultureInfo.CurrentCulture, out var precioVenta) || precioVenta <= 0)
        {
            _lblMensaje.Text = "Precio de venta inválido";
            return;
        }

        if (!decimal.TryParse(stockMinimoTexto.Length == 0 ? "0" : stockMinimoTexto, NumberStyles.Number, CultureInfo.CurrentCulture, out var stockMinimo) || stockMinimo < 0)
        {
            _lblMensaje.Text = "Stock mínimo inválido";
            return;
        }

        decimal? stockMaximo = null;
        if (stockMaximoTexto.Length > 0)
        {
            if (!decimal.TryParse(stockMaximoTexto, NumberStyles.Number, CultureInfo.CurrentCulture, out var valorMax) || valorMax < 0)
            {
                _lblMensaje.Text = "Stock máximo inválido";
                return;
            }
            stockMaximo = valorMax;
        }

        try
        {
            using var connection = Db.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            int productoId;
            using (var command = new SqlCommand
                   {
                       Connection = connection,
                       Transaction = transaction
                   })
            {
                if (_idSeleccionado == null)
                {
                    command.CommandText = @"INSERT INTO Producto (Codigo, Nombre, Descripcion, IdCategoria, PrecioCosto, PrecioVenta, StockMinimo, StockMaximo, Activo)
VALUES (@codigo, @nombre, @descripcion, @categoria, @precioCosto, @precioVenta, @stockMinimo, @stockMaximo, @activo);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
                }
                else
                {
                    command.CommandText = @"UPDATE Producto
SET Codigo = @codigo,
    Nombre = @nombre,
    Descripcion = @descripcion,
    IdCategoria = @categoria,
    PrecioCosto = @precioCosto,
    PrecioVenta = @precioVenta,
    StockMinimo = @stockMinimo,
    StockMaximo = @stockMaximo,
    Activo = @activo
WHERE IdProducto = @id;
SELECT @id;";
                    command.Parameters.AddWithValue("@id", _idSeleccionado.Value);
                }

                command.Parameters.AddWithValue("@codigo", codigo);
                command.Parameters.AddWithValue("@nombre", nombre);
                command.Parameters.AddWithValue("@descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : descripcion);
                command.Parameters.AddWithValue("@categoria", idCategoria);
                command.Parameters.AddWithValue("@precioCosto", precioCosto);
                command.Parameters.AddWithValue("@precioVenta", precioVenta);
                command.Parameters.AddWithValue("@stockMinimo", stockMinimo);
                command.Parameters.AddWithValue("@stockMaximo", (object?)stockMaximo ?? DBNull.Value);
                command.Parameters.AddWithValue("@activo", activo);

                var resultado = command.ExecuteScalar();
                if (resultado == null)
                {
                    throw new InvalidOperationException("No se pudo obtener el identificador del producto");
                }

                productoId = Convert.ToInt32(resultado);
            }

            AsegurarInventarioParaProducto(connection, transaction, productoId, stockMinimo, stockMaximo);

            transaction.Commit();

            MessageBox.Show("Producto guardado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            CargarProductos();
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            _lblMensaje.Text = "El código del producto ya existe";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void AsegurarInventarioParaProducto(SqlConnection connection, SqlTransaction transaction, int idProducto, decimal stockMinimo, decimal? stockMaximo)
    {
        using var command = new SqlCommand(@"MERGE Inventario AS destino
USING (
    SELECT IdBodega FROM Bodega WHERE Activo = 1
) AS origen
ON destino.IdProducto = @producto AND destino.IdBodega = origen.IdBodega
WHEN MATCHED THEN
    UPDATE SET StockMinimo = @stockMinimo,
               StockMaximo = @stockMaximo,
WHEN NOT MATCHED THEN
    INSERT (IdProducto, IdBodega, StockActual, StockReservado, StockMinimo, StockMaximo)
    VALUES (@producto, origen.IdBodega, 0, 0, @stockMinimo, @stockMaximo);", connection, transaction);
        command.Parameters.AddWithValue("@producto", idProducto);
        command.Parameters.AddWithValue("@stockMinimo", stockMinimo);
        command.Parameters.AddWithValue("@stockMaximo", (object?)stockMaximo ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    private void CambiarEstado(bool activar)
    {
        if (_idSeleccionado == null)
        {
            MessageBox.Show("Seleccione un producto", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var accion = activar ? "activar" : "desactivar";
        if (MessageBox.Show($"¿Desea {accion} el producto seleccionado?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            Db.Execute("UPDATE Producto SET Activo = @activo WHERE IdProducto = @id", p =>
            {
                p.AddWithValue("@activo", activar);
                p.AddWithValue("@id", _idSeleccionado);
            });

            CargarProductos();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al actualizar producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CrearCategoria()
    {
        var nombre = SolicitarTexto("Nueva categoría", "Nombre de la categoría");
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return;
        }

        var descripcion = SolicitarTexto("Descripción opcional", "Descripción", permitirVacío: true);

        try
        {
            using var connection = Db.GetConnection();
            connection.Open();
            using var command = new SqlCommand(@"INSERT INTO CategoriaProducto (Nombre, Descripcion, Activo)
VALUES (@nombre, @descripcion, 1);
SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);
            command.Parameters.AddWithValue("@nombre", nombre.Trim());
            command.Parameters.AddWithValue("@descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : descripcion.Trim());
            var id = command.ExecuteScalar();
            if (id != null)
            {
                CargarCategorias();
                _cmbCategoria.SelectedValue = Convert.ToInt32(id);
            }
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            MessageBox.Show("Ya existe una categoría con ese nombre", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al crear categoría: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string? SolicitarTexto(string titulo, string etiqueta, bool permitirVacío = false)
    {
        using var dialogo = new Form
        {
            Text = titulo,
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(420, 200),
            MinimizeBox = false,
            MaximizeBox = false,
            FormBorderStyle = FormBorderStyle.FixedDialog
        };

        UiTheme.ApplyMinimalStyle(dialogo);
        dialogo.Padding = new Padding(24);

        var label = new Label { Text = etiqueta, AutoSize = true, ForeColor = UiTheme.MutedTextColor };
        var texto = new TextBox { Dock = DockStyle.Top, Margin = new Padding(0, 12, 0, 12) };
        UiTheme.StyleTextInput(texto);

        var btnAceptar = new Button { Text = "Aceptar", DialogResult = DialogResult.OK };
        var btnCancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel };
        UiTheme.StylePrimaryButton(btnAceptar);
        UiTheme.StyleSecondaryButton(btnCancelar);
        btnAceptar.Margin = new Padding(0, 0, 8, 0);

        var panelBotones = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Bottom,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false
        };
        panelBotones.Controls.Add(btnAceptar);
        panelBotones.Controls.Add(btnCancelar);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(label, 0, 0);
        layout.Controls.Add(texto, 0, 1);
        layout.Controls.Add(panelBotones, 0, 2);

        dialogo.Controls.Add(layout);
        dialogo.AcceptButton = btnAceptar;
        dialogo.CancelButton = btnCancelar;

        return dialogo.ShowDialog() == DialogResult.OK
            ? (permitirVacío ? texto.Text : texto.Text.Trim().Length == 0 ? null : texto.Text.Trim())
            : null;
    }
}
