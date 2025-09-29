using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class AccesosForm : Form
{
    private readonly ComboBox _cmbPerfil = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false };
    private readonly Button _btnGuardar = new() { Text = "Guardar" };
    private DataTable? _tablaAccesos;

    public AccesosForm()
    {
        Text = "Accesos";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1280, 840);
        MinimumSize = new Size(1120, 720);

        UiTheme.ApplyMinimalStyle(this);

        ConfigurarGrid();
        UiTheme.StyleDataGrid(_grid);
        UiTheme.StyleComboBox(_cmbPerfil);
        UiTheme.StylePrimaryButton(_btnGuardar);
        _btnGuardar.Margin = new Padding(0, 0, 0, 0);

        _grid.Margin = new Padding(0, 24, 0, 0);

        var headerLayout = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0),
            Padding = new Padding(0, 0, 0, 8)
        };
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        headerLayout.Controls.Add(UiTheme.CreateSectionLabel("Perfil"), 0, 0);
        headerLayout.Controls.Add(_cmbPerfil, 1, 0);
        _cmbPerfil.Margin = new Padding(16, 0, 0, 0);

        var panelBotones = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(0, 24, 0, 0),
            WrapContents = false
        };
        panelBotones.Controls.Add(_btnGuardar);

        var card = UiTheme.CreateCardPanel();
        card.Dock = DockStyle.Fill;
        card.Padding = new Padding(32, 32, 32, 24);
        card.AutoScroll = true;
        card.Controls.Add(headerLayout);
        card.Controls.Add(panelBotones);
        card.Controls.Add(_grid);

        Controls.Add(card);

        Load += (_, _) => CargarPerfiles();
        _cmbPerfil.SelectedIndexChanged += (_, _) => CargarAccesos();
        _btnGuardar.Click += (_, _) => GuardarAccesos();
        _grid.CellValueChanged += Grid_CellValueChanged;
        _grid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_grid.IsCurrentCellDirty)
            {
                _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
    }

    private void ConfigurarGrid()
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "IdPantalla", Visible = false });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Código", DataPropertyName = "Codigo", Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Pantalla", DataPropertyName = "NombrePantalla", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Padre", DataPropertyName = "NombrePadre", Width = 150 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Ver", DataPropertyName = "PuedeVer" });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Crear", DataPropertyName = "PuedeCrear" });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Editar", DataPropertyName = "PuedeEditar" });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Eliminar", DataPropertyName = "PuedeEliminar" });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Exportar", DataPropertyName = "PuedeExportar" });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Activo", DataPropertyName = "Activo" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "IdAcceso", DataPropertyName = "IdPerfilPantallaAcceso", Visible = false });
    }

    private void CargarPerfiles()
    {
        try
        {
            var perfiles = Db.GetDataTable("SELECT IdPerfil, NombrePerfil FROM Perfil WHERE Activo = 1 ORDER BY NombrePerfil");
            _cmbPerfil.DisplayMember = "NombrePerfil";
            _cmbPerfil.ValueMember = "IdPerfil";
            _cmbPerfil.DataSource = perfiles;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar perfiles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CargarAccesos()
    {
        if (_cmbPerfil.SelectedValue is not int idPerfil)
        {
            _grid.DataSource = null;
            return;
        }

        try
        {
            using var connection = Db.GetConnection();
            connection.Open();
            using var command = new SqlCommand(@"SELECT p.IdPantalla, p.Codigo, p.NombrePantalla, ISNULL(padre.NombrePantalla, '') AS NombrePadre,
       ISNULL(a.IdPerfilPantallaAcceso, 0) AS IdPerfilPantallaAcceso,
       ISNULL(a.PuedeVer, 0) AS PuedeVer,
       ISNULL(a.PuedeCrear, 0) AS PuedeCrear,
       ISNULL(a.PuedeEditar, 0) AS PuedeEditar,
       ISNULL(a.PuedeEliminar, 0) AS PuedeEliminar,
       ISNULL(a.PuedeExportar, 0) AS PuedeExportar,
       ISNULL(a.Activo, 1) AS Activo
FROM Pantalla p
LEFT JOIN Pantalla padre ON padre.IdPantalla = p.IdPadre
LEFT JOIN PerfilPantallaAcceso a ON a.IdPantalla = p.IdPantalla AND a.IdPerfil = @perfil
ORDER BY ISNULL(p.IdPadre, 0), p.Orden, p.NombrePantalla", connection);
            command.Parameters.AddWithValue("@perfil", idPerfil);
            var adapter = new SqlDataAdapter(command);
            _tablaAccesos = new DataTable();
            adapter.Fill(_tablaAccesos);
            _grid.DataSource = _tablaAccesos;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar accesos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _tablaAccesos == null)
        {
            return;
        }

        var columna = _grid.Columns[e.ColumnIndex];
        if (columna.DataPropertyName == "PuedeVer")
        {
            var fila = _tablaAccesos.Rows[e.RowIndex];
            var ver = fila.Field<bool>("PuedeVer");
            if (!ver)
            {
                fila["PuedeCrear"] = false;
                fila["PuedeEditar"] = false;
                fila["PuedeEliminar"] = false;
                fila["PuedeExportar"] = false;
            }
        }
    }

    private void GuardarAccesos()
    {
        if (_cmbPerfil.SelectedValue is not int idPerfil || _tablaAccesos == null)
        {
            MessageBox.Show("Seleccione un perfil", "Accesos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var connection = Db.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            foreach (DataRow fila in _tablaAccesos.Rows)
            {
                var idPantalla = fila.Field<int>("IdPantalla");
                var idAcceso = fila.Field<int>("IdPerfilPantallaAcceso");
                var puedeVer = fila.Field<bool>("PuedeVer");
                var puedeCrear = fila.Field<bool>("PuedeCrear");
                var puedeEditar = fila.Field<bool>("PuedeEditar");
                var puedeEliminar = fila.Field<bool>("PuedeEliminar");
                var puedeExportar = fila.Field<bool>("PuedeExportar");
                var activo = fila.Field<bool>("Activo");

                if (!puedeVer)
                {
                    puedeCrear = puedeEditar = puedeEliminar = puedeExportar = false;
                }

                if (idAcceso == 0)
                {
                    if (puedeVer || puedeCrear || puedeEditar || puedeEliminar || puedeExportar)
                    {
                        using var insertar = new SqlCommand(@"INSERT INTO PerfilPantallaAcceso(IdPerfil, IdPantalla, PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, Activo)
VALUES(@perfil, @pantalla, @ver, @crear, @editar, @eliminar, @exportar, @activo);
SELECT SCOPE_IDENTITY();", connection, transaction);
                        insertar.Parameters.AddWithValue("@perfil", idPerfil);
                        insertar.Parameters.AddWithValue("@pantalla", idPantalla);
                        insertar.Parameters.AddWithValue("@ver", puedeVer);
                        insertar.Parameters.AddWithValue("@crear", puedeCrear);
                        insertar.Parameters.AddWithValue("@editar", puedeEditar);
                        insertar.Parameters.AddWithValue("@eliminar", puedeEliminar);
                        insertar.Parameters.AddWithValue("@exportar", puedeExportar);
                        insertar.Parameters.AddWithValue("@activo", activo);
                        var nuevoId = Convert.ToInt32(insertar.ExecuteScalar());
                        fila["IdPerfilPantallaAcceso"] = nuevoId;
                    }
                }
                else
                {
                    if (puedeVer || puedeCrear || puedeEditar || puedeEliminar || puedeExportar)
                    {
                        using var actualizar = new SqlCommand(@"UPDATE PerfilPantallaAcceso SET PuedeVer = @ver, PuedeCrear = @crear, PuedeEditar = @editar,
PuedeEliminar = @eliminar, PuedeExportar = @exportar, Activo = @activo, FechaOtorgado = GETDATE()
WHERE IdPerfilPantallaAcceso = @id", connection, transaction);
                        actualizar.Parameters.AddWithValue("@ver", puedeVer);
                        actualizar.Parameters.AddWithValue("@crear", puedeCrear);
                        actualizar.Parameters.AddWithValue("@editar", puedeEditar);
                        actualizar.Parameters.AddWithValue("@eliminar", puedeEliminar);
                        actualizar.Parameters.AddWithValue("@exportar", puedeExportar);
                        actualizar.Parameters.AddWithValue("@activo", activo);
                        actualizar.Parameters.AddWithValue("@id", idAcceso);
                        actualizar.ExecuteNonQuery();
                    }
                    else
                    {
                        using var eliminar = new SqlCommand("DELETE FROM PerfilPantallaAcceso WHERE IdPerfilPantallaAcceso = @id", connection, transaction);
                        eliminar.Parameters.AddWithValue("@id", idAcceso);
                        eliminar.ExecuteNonQuery();
                        fila["IdPerfilPantallaAcceso"] = 0;
                    }
                }
            }

            transaction.Commit();
            MessageBox.Show("Accesos guardados", "Accesos", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar accesos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        CargarAccesos();
    }
}
