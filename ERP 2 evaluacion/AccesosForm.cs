using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class AccesosForm : Form
{
    private readonly ComboBox _cmbPerfil = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false };
    private readonly Button _btnGuardar = new() { Text = "Guardar" };
    private readonly DataGridViewComboBoxColumn _colNivel;
    private bool _actualizandoNivel;
    private DataTable? _tablaAccesos;

    private readonly IReadOnlyList<NivelPermisoDefinition> _nivelesPermiso = new List<NivelPermisoDefinition>
    {
        new("Sin acceso", false, false, false, false, false,
            "Deshabilita por completo la pantalla para el perfil seleccionado."),
        new("Lectura", true, false, false, false, false,
            "Permite consultar la información sin realizar modificaciones."),
        new("Colaboración", true, true, true, false, true,
            "Habilita crear, editar y exportar datos sin permitir eliminaciones."),
        new("Administración", true, true, true, true, true,
            "Otorga todos los permisos disponibles, incluyendo eliminar registros.")
    };

    public AccesosForm()
    {
        Text = "Accesos";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1280, 840);
        MinimumSize = new Size(1120, 720);

        UiTheme.ApplyMinimalStyle(this);

        _colNivel = new DataGridViewComboBoxColumn
        {
            HeaderText = "Nivel",
            DataPropertyName = "NivelPermiso",
            DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
            FlatStyle = FlatStyle.Flat,
            Width = 150,
            DisplayStyleForCurrentCellOnly = true
        };
        foreach (var nivel in _nivelesPermiso)
        {
            _colNivel.Items.Add(nivel.Nombre);
        }
        _colNivel.DefaultCellStyle.NullValue = "Personalizado";
        _colNivel.ToolTipText = "Selecciona un nivel para aplicar automáticamente los permisos asociados.";

        ConfigurarGrid();
        _grid.EditMode = DataGridViewEditMode.EditOnEnter;
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

        var legendPanel = CrearPanelLeyenda();

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
        card.Controls.Add(legendPanel);
        card.Controls.Add(panelBotones);
        card.Controls.Add(_grid);

        Controls.Add(card);

        Load += (_, _) => CargarPerfiles();
        _cmbPerfil.SelectedIndexChanged += (_, _) => CargarAccesos();
        _btnGuardar.Click += (_, _) => GuardarAccesos();
        _grid.CellValueChanged += Grid_CellValueChanged;
        _grid.CellFormatting += Grid_CellFormatting;
        _grid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_grid.IsCurrentCellDirty)
            {
                _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
        _grid.DataError += (_, e) => e.Cancel = true;
    }

    private void ConfigurarGrid()
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "IdPantalla", Visible = false, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Código", DataPropertyName = "Codigo", Width = 150, ToolTipText = "Identificador único de la pantalla.", ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Pantalla", DataPropertyName = "NombrePantalla", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Padre", DataPropertyName = "NombrePadre", Width = 150, ToolTipText = "Módulo al que pertenece la pantalla.", ReadOnly = true });
        _colNivel.ReadOnly = false;
        _grid.Columns.Add(_colNivel);
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Ver", DataPropertyName = "PuedeVer", Width = 70, ToolTipText = "Permite visualizar la pantalla.", ReadOnly = false });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Crear", DataPropertyName = "PuedeCrear", Width = 70, ToolTipText = "Autoriza la creación de nuevos registros.", ReadOnly = false });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Editar", DataPropertyName = "PuedeEditar", Width = 70, ToolTipText = "Permite modificar registros existentes.", ReadOnly = false });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Eliminar", DataPropertyName = "PuedeEliminar", Width = 80, ToolTipText = "Permite eliminar registros.", ReadOnly = false });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Exportar", DataPropertyName = "PuedeExportar", Width = 85, ToolTipText = "Permite exportar información.", ReadOnly = false });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Activo", DataPropertyName = "Activo", Width = 70, ToolTipText = "Indica si el permiso se encuentra vigente.", ReadOnly = false });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "IdAcceso", DataPropertyName = "IdPerfilPantallaAcceso", Visible = false, ReadOnly = true });
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
            if (!_tablaAccesos.Columns.Contains("NivelPermiso"))
            {
                _tablaAccesos.Columns.Add("NivelPermiso", typeof(string));
            }
            foreach (var columnaEditable in new[] { "PuedeVer", "PuedeCrear", "PuedeEditar", "PuedeEliminar", "PuedeExportar", "Activo", "NivelPermiso", "IdPerfilPantallaAcceso" })
            {
                if (_tablaAccesos.Columns.Contains(columnaEditable))
                {
                    _tablaAccesos.Columns[columnaEditable].ReadOnly = false;
                }
            }
            foreach (DataRow fila in _tablaAccesos.Rows)
            {
                NormalizarPermisos(fila);
            }
            _grid.DataSource = _tablaAccesos;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar accesos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _tablaAccesos == null || _actualizandoNivel)
        {
            return;
        }

        var columna = _grid.Columns[e.ColumnIndex];
        if (string.IsNullOrEmpty(columna.DataPropertyName))
        {
            return;
        }

        var fila = _tablaAccesos.Rows[e.RowIndex];

        try
        {
            _actualizandoNivel = true;

            if (columna.DataPropertyName == "NivelPermiso")
            {
                if (fila["NivelPermiso"] is string nivel && !string.IsNullOrWhiteSpace(nivel))
                {
                    AplicarNivelPermiso(fila, nivel);
                }
            }
            else if (EsColumnaPermiso(columna.DataPropertyName))
            {
                if (!fila.Field<bool>("PuedeVer"))
                {
                    fila["PuedeCrear"] = false;
                    fila["PuedeEditar"] = false;
                    fila["PuedeEliminar"] = false;
                    fila["PuedeExportar"] = false;
                    fila["Activo"] = false;
                }

                ActualizarNivelEnFila(fila);
            }
        }
        finally
        {
            _actualizandoNivel = false;
        }
    }

    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || _tablaAccesos == null)
        {
            return;
        }

        var columna = _grid.Columns[e.ColumnIndex];
        if (columna == _colNivel)
        {
            var texto = e.Value as string;
            if (string.IsNullOrWhiteSpace(texto))
            {
                texto = "Personalizado";
                e.Value = texto;
                e.FormattingApplied = true;
            }

            var (backColor, foreColor) = ObtenerColoresNivel(texto);
            e.CellStyle.BackColor = backColor;
            e.CellStyle.ForeColor = foreColor;
        }
        else if (columna.DataPropertyName == "NombrePantalla")
        {
            var fila = _tablaAccesos.Rows[e.RowIndex];
            var padre = fila.Field<string>("NombrePadre");
            e.CellStyle.Padding = string.IsNullOrWhiteSpace(padre)
                ? new Padding(12, 6, 4, 6)
                : new Padding(32, 6, 4, 6);
            if (string.IsNullOrWhiteSpace(padre))
            {
                e.CellStyle.Font = UiTheme.SectionTitleFont;
            }
        }
        else if (columna.DataPropertyName == "NombrePadre")
        {
            e.CellStyle.ForeColor = UiTheme.MutedTextColor;
        }
    }

    private Control CrearPanelLeyenda()
    {
        var legendPanel = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 8, 0, 16),
            Padding = new Padding(0, 8, 0, 8)
        };
        legendPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        legendPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var titulo = UiTheme.CreateSectionLabel("Niveles de permiso");
        titulo.Margin = new Padding(0, 0, 0, 16);
        legendPanel.Controls.Add(titulo, 0, 0);
        legendPanel.SetColumnSpan(titulo, 2);

        var filaIndice = 1;
        foreach (var nivel in _nivelesPermiso)
        {
            var badge = CrearBadgeNivel(nivel.Nombre);
            legendPanel.Controls.Add(badge, 0, filaIndice);

            var descripcion = new Label
            {
                Text = nivel.Descripcion,
                AutoSize = true,
                ForeColor = UiTheme.MutedTextColor,
                Margin = new Padding(16, 4, 0, 12),
                MaximumSize = new Size(780, 0)
            };
            legendPanel.Controls.Add(descripcion, 1, filaIndice);
            filaIndice++;
        }

        var badgePersonalizado = CrearBadgeNivel("Personalizado");
        legendPanel.Controls.Add(badgePersonalizado, 0, filaIndice);
        var descripcionPersonalizado = new Label
        {
            Text = "Ajusta manualmente las columnas de permisos cuando necesites una combinación distinta a los niveles propuestos.",
            AutoSize = true,
            ForeColor = UiTheme.MutedTextColor,
            Margin = new Padding(16, 4, 0, 0),
            MaximumSize = new Size(780, 0)
        };
        legendPanel.Controls.Add(descripcionPersonalizado, 1, filaIndice);

        return legendPanel;
    }

    private static Label CrearBadgeNivel(string nivel)
    {
        var (backColor, foreColor) = ObtenerColoresNivel(nivel);
        return new Label
        {
            Text = nivel,
            AutoSize = true,
            BackColor = backColor,
            ForeColor = foreColor,
            Padding = new Padding(14, 6, 14, 6),
            Margin = new Padding(0, 4, 0, 8),
            Font = UiTheme.HeaderFont
        };
    }

    private static (Color BackColor, Color ForeColor) ObtenerColoresNivel(string? nivel)
    {
        return nivel switch
        {
            "Sin acceso" => (Color.FromArgb(255, 239, 239), UiTheme.DangerColor),
            "Lectura" => (Color.FromArgb(232, 244, 253), Color.FromArgb(30, 111, 170)),
            "Colaboración" => (Color.FromArgb(232, 246, 243), Color.FromArgb(16, 117, 97)),
            "Administración" => (Color.FromArgb(244, 236, 252), Color.FromArgb(112, 48, 160)),
            _ => (Color.FromArgb(234, 238, 246), UiTheme.TextColor)
        };
    }

    private void NormalizarPermisos(DataRow fila)
    {
        if (!fila.Field<bool>("PuedeVer"))
        {
            fila["PuedeCrear"] = false;
            fila["PuedeEditar"] = false;
            fila["PuedeEliminar"] = false;
            fila["PuedeExportar"] = false;
            fila["Activo"] = false;
        }

        ActualizarNivelEnFila(fila);
    }

    private void ActualizarNivelEnFila(DataRow fila)
    {
        var nivel = DeterminarNivelPermiso(fila);
        fila["NivelPermiso"] = nivel is null ? DBNull.Value : nivel;
    }

    private static bool EsColumnaPermiso(string nombreColumna)
    {
        return nombreColumna is "PuedeVer" or "PuedeCrear" or "PuedeEditar" or "PuedeEliminar" or "PuedeExportar";
    }

    private void AplicarNivelPermiso(DataRow fila, string nivel)
    {
        foreach (var definicion in _nivelesPermiso)
        {
            if (string.Equals(definicion.Nombre, nivel, StringComparison.OrdinalIgnoreCase))
            {
                fila["PuedeVer"] = definicion.PuedeVer;
                fila["PuedeCrear"] = definicion.PuedeCrear;
                fila["PuedeEditar"] = definicion.PuedeEditar;
                fila["PuedeEliminar"] = definicion.PuedeEliminar;
                fila["PuedeExportar"] = definicion.PuedeExportar;
                fila["Activo"] = definicion.PuedeVer;
                NormalizarPermisos(fila);
                return;
            }
        }
    }

    private string? DeterminarNivelPermiso(DataRow fila)
    {
        var puedeVer = fila.Field<bool>("PuedeVer");
        var puedeCrear = fila.Field<bool>("PuedeCrear");
        var puedeEditar = fila.Field<bool>("PuedeEditar");
        var puedeEliminar = fila.Field<bool>("PuedeEliminar");
        var puedeExportar = fila.Field<bool>("PuedeExportar");

        if (!puedeVer)
        {
            return _nivelesPermiso[0].Nombre;
        }

        foreach (var definicion in _nivelesPermiso)
        {
            if (definicion.PuedeVer == puedeVer &&
                definicion.PuedeCrear == puedeCrear &&
                definicion.PuedeEditar == puedeEditar &&
                definicion.PuedeEliminar == puedeEliminar &&
                definicion.PuedeExportar == puedeExportar)
            {
                return definicion.Nombre;
            }
        }

        return null;
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

    private sealed record NivelPermisoDefinition(string Nombre, bool PuedeVer, bool PuedeCrear, bool PuedeEditar, bool PuedeEliminar, bool PuedeExportar, string Descripcion);
}
