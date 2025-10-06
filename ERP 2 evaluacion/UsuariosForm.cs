using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class UsuariosForm : Form
{
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoGenerateColumns = false };
    private readonly TextBox _txtNombreUsuario = new();
    private readonly TextBox _txtCorreo = new();
    private readonly TextBox _txtNombreCompleto = new() { PlaceholderText = "Nombre completo" };
    private readonly TextBox _txtClave = new() { UseSystemPasswordChar = true, PlaceholderText = "Contraseña", MaxLength = 50 };
    private readonly TextBox _txtConfirmarClave = new() { UseSystemPasswordChar = true, PlaceholderText = "Confirmar contraseña", MaxLength = 50 };
    private readonly CheckBox _chkMostrarContrasenas = new() { Text = "Mostrar contraseñas" };
    private readonly CheckBox _chkActivo = new() { Text = "Activo", Checked = true };
    private readonly Button _btnNuevo = new() { Text = "Nuevo" };
    private readonly Button _btnGuardar = new() { Text = "Guardar" };
    private readonly Button _btnEliminar = new() { Text = "Eliminar" };
    private readonly Button _btnReiniciar = new() { Text = "Reiniciar contraseña" };
    private readonly Label _lblMensaje = new() { AutoSize = true, ForeColor = UiTheme.DangerColor, Margin = new Padding(8, 0, 0, 0) };
    private readonly CheckedListBox _lstPerfiles = new() { CheckOnClick = true, IntegralHeight = false, BorderStyle = BorderStyle.FixedSingle };
    private readonly SplitContainer _splitContainer;
    private readonly bool _permitirMostrarContrasenas;
    private readonly bool _puedeGestionarPerfiles;
    private readonly string? _usuarioActual;
    private readonly List<PerfilListItem> _perfiles = new();

    private int? _idSeleccionado;

    public UsuariosForm(bool privilegiosAdministracion = false, string? usuarioActual = null)
    {
        Text = "Usuarios";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1160, 760);

        _permitirMostrarContrasenas = privilegiosAdministracion;
        _puedeGestionarPerfiles = privilegiosAdministracion;
        _usuarioActual = usuarioActual;

        UiTheme.ApplyMinimalStyle(this);

        ConfigurarGrid();
        UiTheme.StyleDataGrid(_grid);

        UiTheme.StyleTextInput(_txtNombreUsuario);
        UiTheme.StyleTextInput(_txtCorreo);
        UiTheme.StyleTextInput(_txtNombreCompleto);
        UiTheme.StyleTextInput(_txtClave);
        UiTheme.StyleTextInput(_txtConfirmarClave);
        UiTheme.StyleCheckBox(_chkMostrarContrasenas);
        UiTheme.StyleCheckBox(_chkActivo);
        UiTheme.StyleSecondaryButton(_btnNuevo);
        UiTheme.StylePrimaryButton(_btnGuardar);
        UiTheme.StyleDangerButton(_btnEliminar);
        UiTheme.StyleSecondaryButton(_btnReiniciar);
        _btnGuardar.Margin = new Padding(0, 0, 0, 0);
        _btnReiniciar.Margin = new Padding(0, 0, 0, 0);

        foreach (var input in new[] { _txtNombreUsuario, _txtCorreo, _txtNombreCompleto, _txtClave, _txtConfirmarClave })
        {
            input.Margin = new Padding(0, 4, 0, 12);
        }

        _chkMostrarContrasenas.Margin = new Padding(0, 8, 0, 0);
        _chkActivo.Margin = new Padding(0, 8, 0, 0);
        _lblMensaje.Margin = new Padding(12, 8, 0, 0);

        _lstPerfiles.BackColor = UiTheme.SurfaceColor;
        _lstPerfiles.ForeColor = UiTheme.TextColor;
        _lstPerfiles.Margin = new Padding(0, 4, 0, 8);

        _chkMostrarContrasenas.Visible = _permitirMostrarContrasenas;
        _chkMostrarContrasenas.Enabled = _permitirMostrarContrasenas;

        var panelEdicion = CrearPanelEdicion();

        _splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            BorderStyle = BorderStyle.None,
            SplitterWidth = 10
        };

        _splitContainer.Panel1.BackColor = Color.Transparent;
        _splitContainer.Panel2.BackColor = Color.Transparent;

        var gridCard = UiTheme.CreateCardPanel();
        gridCard.Padding = new Padding(24, 24, 24, 16);
        gridCard.Margin = new Padding(0, 0, 0, 16);
        gridCard.Controls.Add(_grid);
        _splitContainer.Panel1.Controls.Add(gridCard);
        _splitContainer.Panel2.Controls.Add(panelEdicion);

        Controls.Add(_splitContainer);

        Load += UsuariosForm_Load;
        SizeChanged += UsuariosForm_SizeChanged;
        _grid.SelectionChanged += Grid_SelectionChanged;
        _btnNuevo.Click += (_, _) => LimpiarFormulario();
        _btnGuardar.Click += (_, _) => GuardarUsuario();
        _btnEliminar.Click += (_, _) => EliminarUsuario();
        _btnReiniciar.Click += (_, _) => ReiniciarContrasena();
        _chkMostrarContrasenas.CheckedChanged += (_, _) => ActualizarVisibilidadContrasenas();

        ActualizarVisibilidadContrasenas();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        AjustarSplit();
    }

    private void UsuariosForm_Load(object? sender, EventArgs e)
    {
        BeginInvoke((Action)AjustarSplit);
        if (_puedeGestionarPerfiles)
        {
            CargarPerfiles();
        }
        CargarUsuarios();
    }

    private void UsuariosForm_SizeChanged(object? sender, EventArgs e)
    {
        AjustarSplit();
    }

    private void AjustarSplit()
    {
        var s = _splitContainer;
        if (s.Width <= 0 && s.Height <= 0)
        {
            return;
        }

        s.Panel1MinSize = 150;
        s.Panel2MinSize = 200;

        int total = s.Orientation == Orientation.Vertical ? s.Width : s.Height;
        if (total <= 0)
        {
            return;
        }

        int min = s.Panel1MinSize;
        int max = total - s.Panel2MinSize - s.SplitterWidth;

        if (max < min)
        {
            int fallback = Math.Max(0, Math.Min(min, total - s.Panel2MinSize));
            s.SplitterDistance = fallback;
            return;
        }

        int deseo = s.SplitterDistance > 0 ? s.SplitterDistance : total / 2;
        s.SplitterDistance = Math.Clamp(deseo, min, max);
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
        var contenido = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = _puedeGestionarPerfiles ? 2 : 1,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, _puedeGestionarPerfiles ? 60 : 100));
        if (_puedeGestionarPerfiles)
        {
            contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        }

        var datosLayout = CrearLayoutDatos();
        contenido.Controls.Add(datosLayout, 0, 0);

        if (_puedeGestionarPerfiles)
        {
            var rolesLayout = CrearLayoutPerfiles();
            contenido.Controls.Add(rolesLayout, 1, 0);
        }

        var panelBotones = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(0, 16, 0, 0),
            WrapContents = false,
            Margin = new Padding(0, 12, 0, 0)
        };
        panelBotones.Controls.AddRange(new Control[] { _btnGuardar, _btnNuevo, _btnEliminar, _btnReiniciar });

        var panel = UiTheme.CreateCardPanel();
        panel.AutoScroll = true;
        panel.Padding = new Padding(24, 24, 24, 16);
        panel.Controls.Add(contenido);
        panel.Controls.Add(panelBotones);

        return panel;
    }

    private TableLayoutPanel CrearLayoutDatos()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Margin = new Padding(0),
            Padding = new Padding(0, 0, _puedeGestionarPerfiles ? 16 : 0, 0)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        int row = 0;

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var lblAcceso = UiTheme.CreateSectionLabel("Datos de acceso");
        lblAcceso.Margin = new Padding(0, 0, 0, 6);
        layout.Controls.Add(lblAcceso, 0, row);
        layout.SetColumnSpan(lblAcceso, 2);
        row++;

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(CrearLabelCampo("Usuario"), 0, row);
        layout.Controls.Add(CrearLabelCampo("Correo"), 1, row);
        row++;

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _txtNombreUsuario.Margin = new Padding(0, 4, 12, 12);
        layout.Controls.Add(_txtNombreUsuario, 0, row);
        _txtCorreo.Margin = new Padding(12, 4, 0, 12);
        layout.Controls.Add(_txtCorreo, 1, row);
        row++;

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(CrearLabelCampo("Contraseña"), 0, row);
        layout.Controls.Add(CrearLabelCampo("Confirmar contraseña"), 1, row);
        row++;

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _txtClave.Margin = new Padding(0, 4, 12, 12);
        layout.Controls.Add(_txtClave, 0, row);
        _txtConfirmarClave.Margin = new Padding(12, 4, 0, 12);
        layout.Controls.Add(_txtConfirmarClave, 1, row);
        row++;

        if (_permitirMostrarContrasenas)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(_chkMostrarContrasenas, 0, row);
            layout.SetColumnSpan(_chkMostrarContrasenas, 2);
            row++;
        }

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var lblNota = new Label
        {
            Text = "Deja la contraseña en blanco para mantenerla al editar un usuario existente.",
            ForeColor = UiTheme.MutedTextColor,
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 12),
            MaximumSize = new Size(600, 0)
        };
        layout.Controls.Add(lblNota, 0, row);
        layout.SetColumnSpan(lblNota, 2);
        row++;

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var lblInfo = UiTheme.CreateSectionLabel("Información personal");
        lblInfo.Margin = new Padding(0, 4, 0, 6);
        layout.Controls.Add(lblInfo, 0, row);
        layout.SetColumnSpan(lblInfo, 2);
        row++;

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var lblNombreCompleto = CrearLabelCampo("Nombre completo");
        layout.Controls.Add(lblNombreCompleto, 0, row);
        layout.SetColumnSpan(lblNombreCompleto, 2);
        row++;

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _txtNombreCompleto.Margin = new Padding(0, 4, 0, 12);
        layout.Controls.Add(_txtNombreCompleto, 0, row);
        layout.SetColumnSpan(_txtNombreCompleto, 2);
        row++;

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var estadoLayout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 8, 0, 0)
        };
        _chkActivo.Margin = new Padding(0, 0, 12, 0);
        _lblMensaje.Margin = new Padding(0, 0, 0, 0);
        estadoLayout.Controls.Add(_chkActivo);
        estadoLayout.Controls.Add(_lblMensaje);
        layout.Controls.Add(estadoLayout, 0, row);
        layout.SetColumnSpan(estadoLayout, 2);

        return layout;
    }

    private Control CrearLayoutPerfiles()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Margin = new Padding(16, 0, 0, 0),
            Padding = new Padding(0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var titulo = UiTheme.CreateSectionLabel("Roles asignados");
        titulo.Margin = new Padding(0, 0, 0, 6);
        layout.Controls.Add(titulo, 0, 0);

        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _lstPerfiles.Dock = DockStyle.Fill;
        layout.Controls.Add(_lstPerfiles, 0, 1);

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var descripcion = new Label
        {
            Text = "Selecciona los roles que aplicarán para el usuario seleccionado.",
            ForeColor = UiTheme.MutedTextColor,
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 0),
            MaximumSize = new Size(360, 0)
        };
        layout.Controls.Add(descripcion, 0, 2);

        return layout;
    }

    private static Label CrearLabelCampo(string texto)
    {
        return new Label
        {
            Text = texto,
            AutoSize = true,
            ForeColor = UiTheme.MutedTextColor,
            Margin = new Padding(0, 8, 0, 2)
        };
    }

    private void CargarPerfiles()
    {
        if (!_puedeGestionarPerfiles)
        {
            return;
        }

        _lstPerfiles.BeginUpdate();
        try
        {
            var tabla = Db.GetDataTable("SELECT IdPerfil, NombrePerfil, Codigo, Activo FROM Perfil ORDER BY NombrePerfil");
            _perfiles.Clear();
            foreach (DataRow fila in tabla.Rows)
            {
                _perfiles.Add(new PerfilListItem(
                    Convert.ToInt32(fila["IdPerfil"]),
                    fila["NombrePerfil"].ToString() ?? string.Empty,
                    fila["Codigo"].ToString() ?? string.Empty,
                    fila.Field<bool>("Activo")));
            }

            _lstPerfiles.Items.Clear();
            foreach (var perfil in _perfiles)
            {
                _lstPerfiles.Items.Add(perfil, false);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar roles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _lstPerfiles.EndUpdate();
        }

        if (_idSeleccionado.HasValue)
        {
            CargarPerfilesUsuario(_idSeleccionado.Value);
        }
        else
        {
            LimpiarSeleccionPerfiles();
        }
    }

    private void CargarPerfilesUsuario(int idUsuario)
    {
        if (!_puedeGestionarPerfiles)
        {
            return;
        }

        if (_perfiles.Count == 0)
        {
            LimpiarSeleccionPerfiles();
            return;
        }

        try
        {
            var tabla = Db.GetDataTable("SELECT IdPerfil FROM UsuarioPerfil WHERE IdUsuario = @usuario", p =>
            {
                p.AddWithValue("@usuario", idUsuario);
            });
            var seleccionados = tabla.AsEnumerable().Select(r => r.Field<int>("IdPerfil"));
            AplicarSeleccionPerfiles(seleccionados);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar roles del usuario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AplicarSeleccionPerfiles(IEnumerable<int> perfilesSeleccionados)
    {
        if (!_puedeGestionarPerfiles)
        {
            return;
        }

        var seleccionados = new HashSet<int>(perfilesSeleccionados);
        for (int i = 0; i < _lstPerfiles.Items.Count; i++)
        {
            if (_lstPerfiles.Items[i] is PerfilListItem item)
            {
                _lstPerfiles.SetItemChecked(i, seleccionados.Contains(item.Id));
            }
        }
    }

    private void LimpiarSeleccionPerfiles()
    {
        AplicarSeleccionPerfiles(Array.Empty<int>());
    }

    private List<int> ObtenerPerfilesMarcados()
    {
        var ids = new List<int>();
        foreach (var item in _lstPerfiles.CheckedItems)
        {
            if (item is PerfilListItem perfil)
            {
                ids.Add(perfil.Id);
            }
        }

        return ids;
    }

    private void GuardarAsignacionesPerfiles(SqlConnection connection, SqlTransaction transaction, int idUsuario)
    {
        if (!_puedeGestionarPerfiles || _perfiles.Count == 0)
        {
            return;
        }

        var seleccionados = new HashSet<int>(ObtenerPerfilesMarcados());
        var visibles = _perfiles.Select(p => p.Id).ToList();
        if (visibles.Count == 0)
        {
            return;
        }

        var parametros = string.Join(", ", visibles.Select((_, i) => "@perfil" + i));
        var existentes = new List<int>();

        using (var obtener = new SqlCommand($"SELECT IdPerfil FROM UsuarioPerfil WHERE IdUsuario = @usuario AND IdPerfil IN ({parametros})", connection, transaction))
        {
            obtener.Parameters.AddWithValue("@usuario", idUsuario);
            for (int i = 0; i < visibles.Count; i++)
            {
                obtener.Parameters.AddWithValue("@perfil" + i, visibles[i]);
            }

            using var reader = obtener.ExecuteReader();
            while (reader.Read())
            {
                existentes.Add(reader.GetInt32(0));
            }
        }

        foreach (var idPerfil in seleccionados.Except(existentes))
        {
            using var insertar = new SqlCommand("INSERT INTO UsuarioPerfil (IdUsuario, IdPerfil, AsignadoPor) VALUES (@usuario, @perfil, @asignadoPor)", connection, transaction);
            insertar.Parameters.AddWithValue("@usuario", idUsuario);
            insertar.Parameters.AddWithValue("@perfil", idPerfil);
            insertar.Parameters.AddWithValue("@asignadoPor", (object?)_usuarioActual ?? DBNull.Value);
            insertar.ExecuteNonQuery();
        }

        foreach (var idPerfil in existentes.Except(seleccionados))
        {
            using var eliminar = new SqlCommand("DELETE FROM UsuarioPerfil WHERE IdUsuario = @usuario AND IdPerfil = @perfil", connection, transaction);
            eliminar.Parameters.AddWithValue("@usuario", idUsuario);
            eliminar.Parameters.AddWithValue("@perfil", idPerfil);
            eliminar.ExecuteNonQuery();
        }
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
            _idSeleccionado = null;
            LimpiarSeleccionPerfiles();
            return;
        }

        if (_grid.SelectedRows[0].DataBoundItem is DataRowView fila)
        {
            _idSeleccionado = (int)fila["IdUsuario"];
            _txtNombreUsuario.Text = fila["NombreUsuario"].ToString();
            _txtCorreo.Text = fila["Correo"].ToString();
            _txtNombreCompleto.Text = fila["NombreCompleto"].ToString();
            _chkActivo.Checked = fila.Row.Field<bool>("Activo");
            _txtClave.Text = string.Empty;
            _txtConfirmarClave.Text = string.Empty;

            if (_idSeleccionado.HasValue)
            {
                CargarPerfilesUsuario(_idSeleccionado.Value);
            }
        }
    }

    private void LimpiarFormulario()
    {
        _idSeleccionado = null;
        _txtNombreUsuario.Text = string.Empty;
        _txtCorreo.Text = string.Empty;
        _txtNombreCompleto.Text = string.Empty;
        _txtClave.Text = string.Empty;
        _txtConfirmarClave.Text = string.Empty;
        _chkActivo.Checked = true;
        _lblMensaje.Text = string.Empty;
        _grid.ClearSelection();
        _chkMostrarContrasenas.Checked = false;
        ActualizarVisibilidadContrasenas();
        LimpiarSeleccionPerfiles();
    }

    private void GuardarUsuario()
    {
        _lblMensaje.Text = string.Empty;
        var nombreUsuario = _txtNombreUsuario.Text.Trim();
        var correo = _txtCorreo.Text.Trim();
        var nombreCompleto = _txtNombreCompleto.Text.Trim();
        var activo = _chkActivo.Checked;
        var clave = _txtClave.Text;
        var confirmarClave = _txtConfirmarClave.Text;
        var esNuevo = !_idSeleccionado.HasValue;

        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(nombreCompleto))
        {
            _lblMensaje.Text = "Complete los campos requeridos (usuario, correo y nombre completo)";
            return;
        }

        if (!Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            _lblMensaje.Text = "Correo inválido";
            return;
        }

        if (esNuevo && string.IsNullOrWhiteSpace(clave))
        {
            _lblMensaje.Text = "Defina una contraseña para el nuevo usuario";
            return;
        }

        if (!string.IsNullOrEmpty(clave) || !string.IsNullOrEmpty(confirmarClave))
        {
            if (clave.Length < 8)
            {
                _lblMensaje.Text = "La contraseña debe tener al menos 8 caracteres";
                return;
            }

            if (clave.Length > 50)
            {
                _lblMensaje.Text = "La contraseña no puede exceder 50 caracteres";
                return;
            }

            if (!string.Equals(clave, confirmarClave, StringComparison.Ordinal))
            {
                _lblMensaje.Text = "Las contraseñas no coinciden";
                return;
            }
        }

        string? mensajeExito = null;

        using var connection = Db.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            using (var verificarNombre = new SqlCommand("SELECT COUNT(*) FROM Usuario WHERE NombreUsuario = @nombre AND (@id IS NULL OR IdUsuario <> @id)", connection, transaction))
            {
                verificarNombre.Parameters.AddWithValue("@nombre", nombreUsuario);
                verificarNombre.Parameters.AddWithValue("@id", (object?)_idSeleccionado ?? DBNull.Value);
                var existeNombre = (int)verificarNombre.ExecuteScalar();
                if (existeNombre > 0)
                {
                    _lblMensaje.Text = "Nombre de usuario ya existe";
                    transaction.Rollback();
                    return;
                }
            }

            using (var verificarCorreo = new SqlCommand("SELECT COUNT(*) FROM Usuario WHERE Correo = @correo AND (@id IS NULL OR IdUsuario <> @id)", connection, transaction))
            {
                verificarCorreo.Parameters.AddWithValue("@correo", correo);
                verificarCorreo.Parameters.AddWithValue("@id", (object?)_idSeleccionado ?? DBNull.Value);
                var existeCorreo = (int)verificarCorreo.ExecuteScalar();
                if (existeCorreo > 0)
                {
                    _lblMensaje.Text = "Correo ya existe";
                    transaction.Rollback();
                    return;
                }
            }

            int idUsuario;
            if (_idSeleccionado.HasValue)
            {
                var sql = "UPDATE Usuario SET NombreUsuario = @nombre, Correo = @correo, NombreCompleto = @completo, Activo = @activo";
                if (!string.IsNullOrEmpty(clave))
                {
                    sql += ", Clave = @clave";
                }
                sql += " WHERE IdUsuario = @id";

                using var actualizar = new SqlCommand(sql, connection, transaction);
                actualizar.Parameters.AddWithValue("@nombre", nombreUsuario);
                actualizar.Parameters.AddWithValue("@correo", correo);
                actualizar.Parameters.AddWithValue("@completo", nombreCompleto);
                actualizar.Parameters.AddWithValue("@activo", activo);
                actualizar.Parameters.AddWithValue("@id", _idSeleccionado.Value);
                if (!string.IsNullOrEmpty(clave))
                {
                    actualizar.Parameters.AddWithValue("@clave", clave);
                }
                actualizar.ExecuteNonQuery();

                idUsuario = _idSeleccionado.Value;
                mensajeExito = !string.IsNullOrEmpty(clave)
                    ? "Usuario actualizado y contraseña restablecida"
                    : "Usuario actualizado";
            }
            else
            {
                using var insertar = new SqlCommand(@"INSERT INTO Usuario(NombreUsuario, Correo, Clave, NombreCompleto, Activo)
VALUES(@nombre, @correo, @clave, @completo, @activo);
SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction);
                insertar.Parameters.AddWithValue("@nombre", nombreUsuario);
                insertar.Parameters.AddWithValue("@correo", correo);
                insertar.Parameters.AddWithValue("@completo", nombreCompleto);
                insertar.Parameters.AddWithValue("@activo", activo);
                insertar.Parameters.AddWithValue("@clave", clave);
                idUsuario = Convert.ToInt32(insertar.ExecuteScalar());
                mensajeExito = "Usuario creado correctamente";
            }

            GuardarAsignacionesPerfiles(connection, transaction, idUsuario);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
                // Ignorar errores de rollback
            }

            MessageBox.Show($"Error al guardar usuario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        CargarUsuarios();
        LimpiarFormulario();

        if (!string.IsNullOrEmpty(mensajeExito))
        {
            MessageBox.Show(mensajeExito, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
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
            connection.Open();
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

        try
        {
            using var connection = Db.GetConnection();
            connection.Open();
            using var command = new SqlCommand("UPDATE Usuario SET Clave = @clave WHERE IdUsuario = @id", connection);
            command.Parameters.AddWithValue("@clave", nuevaContrasena);
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

    private sealed class PerfilListItem
    {
        public PerfilListItem(int id, string nombre, string codigo, bool activo)
        {
            Id = id;
            Nombre = nombre;
            Codigo = codigo;
            Activo = activo;
        }

        public int Id { get; }
        public string Nombre { get; }
        public string Codigo { get; }
        public bool Activo { get; }

        public override string ToString()
        {
            var etiqueta = string.IsNullOrWhiteSpace(Codigo) ? Nombre : $"{Nombre} ({Codigo})";
            return Activo ? etiqueta : $"{etiqueta} - Inactivo";
        }
    }

    private void ActualizarVisibilidadContrasenas()
    {
        var mostrar = _permitirMostrarContrasenas && _chkMostrarContrasenas.Checked;
        _txtClave.UseSystemPasswordChar = !mostrar;
        _txtConfirmarClave.UseSystemPasswordChar = !mostrar;
    }
}
