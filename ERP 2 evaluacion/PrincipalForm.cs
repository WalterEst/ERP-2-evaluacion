using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ERP_2_evaluacion
{
    public class PrincipalForm : Form
    {
        private readonly int _idUsuario;
        private readonly string _nombreUsuario;
        private readonly bool _usuarioPrivilegiado;

        private readonly TreeView _arbolPantallas = new() { Dock = DockStyle.Fill };
        private readonly Label _lblBienvenida = new() { AutoSize = true };
        private readonly Label _lblHeroSubtitulo = new() { AutoSize = true };
        private readonly TextBox _txtBusqueda = new() { PlaceholderText = "Buscar en el menú..." };
        private readonly Label _lblResumenAccesos = new() { AutoSize = true };
        private readonly Label _lblHeroTotalPantallas = new() { AutoSize = true };
        private readonly Label _lblHeroTotalSecciones = new() { AutoSize = true };
        private readonly Button _btnIrUsuarios = new();
        private readonly Button _btnIrPerfiles = new();
        private readonly Button _btnIrAccesos = new();
        private readonly string _heroSubtitleDefault = "Gestiona tus módulos y accesos desde un solo lugar";
        private List<PantallaNodo> _pantallasDisponibles = new();

        private class PantallaNodo
        {
            public int Id { get; init; }
            public int? IdPadre { get; init; }
            public string Codigo { get; init; } = string.Empty;
            public string Nombre { get; init; } = string.Empty;
            public int Orden { get; init; }
        }

        public PrincipalForm(int idUsuario, string nombreUsuario)
        {
            _idUsuario = idUsuario;
            _nombreUsuario = nombreUsuario;

            try
            {
                _usuarioPrivilegiado = SeguridadUtil.EsUsuarioPrivilegiadoPorId(idUsuario);
            }
            catch (Exception ex)
            {
                _usuarioPrivilegiado = false;
                MessageBox.Show($"No se pudieron validar los privilegios del usuario: {ex.Message}",
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Text = "Principal";
            WindowState = FormWindowState.Maximized;

            UiTheme.ApplyMinimalStyle(this);
            Padding = new Padding(32);

            _lblBienvenida.Text = $"Hola, {_nombreUsuario}";
            _lblBienvenida.Font = UiTheme.TitleFont;
            _lblBienvenida.ForeColor = Color.White;
            _lblBienvenida.Margin = new Padding(0);

            _lblHeroSubtitulo.Text = _heroSubtitleDefault;
            _lblHeroSubtitulo.Font = UiTheme.SectionTitleFont;
            _lblHeroSubtitulo.ForeColor = Color.FromArgb(228, 235, 255);
            _lblHeroSubtitulo.Margin = new Padding(0, 12, 0, 0);
            _lblHeroSubtitulo.MaximumSize = new Size(640, 0);

            _lblHeroTotalPantallas.Text = "0";
            _lblHeroTotalSecciones.Text = "0";

            UiTheme.StyleTreeView(_arbolPantallas);
            _arbolPantallas.Margin = new Padding(0);
            _arbolPantallas.ShowNodeToolTips = true;
            _arbolPantallas.NodeMouseDoubleClick += ArbolPantallas_NodeMouseDoubleClick;

            UiTheme.StyleTextInput(_txtBusqueda);
            _txtBusqueda.Margin = new Padding(0, 6, 0, 0);
            _txtBusqueda.MinimumSize = new Size(0, 38);
            _txtBusqueda.TextChanged += (_, _) => RenderMenu(_txtBusqueda.Text);

            _lblResumenAccesos.Text = "Cargando menú...";
            _lblResumenAccesos.ForeColor = UiTheme.MutedTextColor;
            _lblResumenAccesos.Margin = new Padding(0, 4, 0, 0);
            _lblResumenAccesos.MaximumSize = new Size(360, 0);

            var heroPanel = new HeroPanel
            {
                Dock = DockStyle.Fill,
                Height = 200,
                Margin = new Padding(0, 0, 0, 24),
                Padding = new Padding(32, 28, 32, 28)
            };

            var heroLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            heroLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            heroLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            var heroTextLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 3,
                Margin = new Padding(0)
            };
            heroTextLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            heroTextLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            heroTextLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            heroTextLayout.Controls.Add(_lblBienvenida, 0, 0);
            heroTextLayout.Controls.Add(_lblHeroSubtitulo, 0, 1);

            var heroStats = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 20, 0, 0)
            };
            heroStats.Controls.Add(CrearChipEstadistica("Pantallas activas", _lblHeroTotalPantallas));
            heroStats.Controls.Add(CrearChipEstadistica("Secciones", _lblHeroTotalSecciones));
            heroTextLayout.Controls.Add(heroStats, 0, 2);

            var heroIcon = new Label
            {
                Text = "🗂️",
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 72F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Margin = new Padding(24, 0, 0, 0)
            };

            heroLayout.Controls.Add(heroTextLayout, 0, 0);
            heroLayout.Controls.Add(heroIcon, 1, 0);
            heroPanel.Controls.Add(heroLayout);

            var treeCard = UiTheme.CreateCardPanel();
            treeCard.Padding = new Padding(32, 28, 32, 28);

            var treeLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            treeLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            treeLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            treeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var lblMenu = UiTheme.CreateSectionLabel("Menú principal");
            lblMenu.Margin = new Padding(0, 0, 0, 8);
            treeLayout.Controls.Add(lblMenu, 0, 0);

            var searchLayout = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            searchLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            searchLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            searchLayout.Controls.Add(_txtBusqueda, 0, 0);
            searchLayout.Controls.Add(new Label
            {
                Text = "Escribe el nombre o código de una pantalla para filtrar el menú.",
                AutoSize = true,
                ForeColor = UiTheme.MutedTextColor,
                Margin = new Padding(0, 6, 0, 0)
            }, 0, 1);

            treeLayout.Controls.Add(searchLayout, 0, 1);
            treeLayout.Controls.Add(_arbolPantallas, 0, 2);
            treeCard.Controls.Add(treeLayout);

            var summaryCard = UiTheme.CreateCardPanel();
            summaryCard.Padding = new Padding(32, 28, 32, 28);
            summaryCard.MinimumSize = new Size(280, 0);

            var summaryLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            summaryLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            summaryLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            summaryLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            summaryLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            summaryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var resumenTitulo = UiTheme.CreateSectionLabel("Resumen");
            resumenTitulo.Margin = new Padding(0, 0, 0, 8);
            summaryLayout.Controls.Add(resumenTitulo, 0, 0);
            summaryLayout.Controls.Add(_lblResumenAccesos, 0, 1);

            var tipsLabel = new Label
            {
                Text = "• Haz doble clic sobre una pantalla para abrirla.\n• Usa el buscador para encontrar módulos rápidamente.\n• Ajusta los accesos desde los botones disponibles.",
                AutoSize = true,
                ForeColor = UiTheme.MutedTextColor,
                Margin = new Padding(0, 8, 0, 12),
                MaximumSize = new Size(360, 0)
            };
            summaryLayout.Controls.Add(tipsLabel, 0, 2);

            var accionesTitulo = UiTheme.CreateSectionLabel("Accesos rápidos");
            accionesTitulo.Margin = new Padding(0, 8, 0, 8);
            summaryLayout.Controls.Add(accionesTitulo, 0, 3);
            summaryLayout.Controls.Add(CrearPanelAccionesRapidas(), 0, 4);

            summaryCard.Controls.Add(summaryLayout);

            var contentLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            contentLayout.Controls.Add(treeCard, 0, 0);
            contentLayout.Controls.Add(summaryCard, 1, 0);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.Controls.Add(heroPanel, 0, 0);
            mainLayout.Controls.Add(contentLayout, 0, 1);

            Controls.Add(mainLayout);

            Load += (_, _) => CargarMenu();
        }

        private void CargarMenu()
        {
            _pantallasDisponibles = new List<PantallaNodo>();

            try
            {
                using var connection = Db.GetConnection();
                connection.Open();

                using var command = new SqlCommand(@"
SELECT p.IdPantalla, p.Codigo, p.NombrePantalla, p.IdPadre, p.Orden
FROM Pantalla p
WHERE p.Activo = 1
  AND EXISTS (
        SELECT 1
        FROM UsuarioPerfil up
        JOIN PerfilPantallaAcceso pa ON pa.IdPerfil = up.IdPerfil
        WHERE up.IdUsuario = @IdUsuario
          AND pa.IdPantalla = p.IdPantalla
          AND pa.PuedeVer = 1
          AND pa.Activo = 1
  )
ORDER BY CASE WHEN p.IdPadre IS NULL THEN 0 ELSE 1 END,
       p.IdPadre, p.Orden, p.NombrePantalla;", connection);
                command.Parameters.AddWithValue("@IdUsuario", _idUsuario);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    _pantallasDisponibles.Add(new PantallaNodo
                    {
                        Id = reader.GetInt32(0),
                        Codigo = reader.GetString(1),
                        Nombre = reader.GetString(2),
                        IdPadre = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                        Orden = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No fue posible cargar el menú: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_pantallasDisponibles.Count == 0)
            {
                MessageBox.Show("Este usuario no tiene pantallas visibles. Asigne permisos en Accesos.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RenderMenu(null);
                return;
            }

            RenderMenu(_txtBusqueda.Text);
        }

        private void RenderMenu(string? filtro)
        {
            var filter = string.IsNullOrWhiteSpace(filtro) ? null : filtro.Trim();

            _arbolPantallas.BeginUpdate();
            _arbolPantallas.Nodes.Clear();

            if (_pantallasDisponibles.Count == 0)
            {
                _arbolPantallas.Nodes.Add(new TreeNode("Sin pantallas disponibles")
                {
                    ForeColor = UiTheme.MutedTextColor,
                    NodeFont = new Font(UiTheme.BaseFont, FontStyle.Italic)
                });
                _arbolPantallas.Enabled = false;
                _txtBusqueda.Enabled = false;
                _lblResumenAccesos.Text = "No hay pantallas visibles para este usuario.";
                _lblHeroSubtitulo.Text = $"{_heroSubtitleDefault}\nSolicita permisos para comenzar.";
                _lblHeroTotalPantallas.Text = "0";
                _lblHeroTotalSecciones.Text = "0";
                ActualizarEstadoAcciones();
                _arbolPantallas.EndUpdate();
                return;
            }

            _txtBusqueda.Enabled = true;
            _arbolPantallas.Enabled = true;

            var lookup = _pantallasDisponibles
                .Where(p => p.IdPadre.HasValue)
                .GroupBy(p => p.IdPadre.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.Orden).ThenBy(x => x.Nombre).ToList());

            var raices = _pantallasDisponibles
                .Where(p => !p.IdPadre.HasValue)
                .OrderBy(x => x.Orden)
                .ThenBy(x => x.Nombre)
                .ToList();

            foreach (var pantalla in raices)
            {
                var nodo = CrearNodoFiltrado(pantalla, lookup, filter);
                if (nodo != null)
                {
                    _arbolPantallas.Nodes.Add(nodo);
                }
            }

            if (_arbolPantallas.Nodes.Count == 0)
            {
                _arbolPantallas.Nodes.Add(new TreeNode("No se encontraron pantallas con el filtro aplicado")
                {
                    ForeColor = UiTheme.MutedTextColor,
                    NodeFont = new Font(UiTheme.BaseFont, FontStyle.Italic)
                });
                _arbolPantallas.Enabled = false;
                _lblResumenAccesos.Text = $"No se encontraron pantallas que coincidan con \"{filter}\".";
                _lblHeroSubtitulo.Text = $"Mostrando resultados para \"{filter}\"";
            }
            else
            {
                _arbolPantallas.Enabled = true;
                _arbolPantallas.ExpandAll();
                _arbolPantallas.SelectedNode = _arbolPantallas.Nodes[0];

                var totalPantallas = _pantallasDisponibles.Count;
                var totalSecciones = _pantallasDisponibles.Count(p => !p.IdPadre.HasValue);
                _lblHeroTotalPantallas.Text = totalPantallas.ToString();
                _lblHeroTotalSecciones.Text = totalSecciones.ToString();

                if (filter == null)
                {
                    _lblResumenAccesos.Text = totalPantallas == 1
                        ? "Tienes acceso a 1 pantalla disponible."
                        : $"Tienes acceso a {totalPantallas} pantallas distribuidas en {totalSecciones} secciones.";
                    _lblHeroSubtitulo.Text = $"{_heroSubtitleDefault}\nGestionas {totalPantallas} pantallas en {totalSecciones} secciones.";
                }
                else
                {
                    var visibles = ContarNodos(_arbolPantallas.Nodes);
                    _lblResumenAccesos.Text = visibles == 1
                        ? "Se muestra 1 pantalla con el filtro aplicado."
                        : $"Se muestran {visibles} pantallas que coinciden con \"{filter}\".";
                    _lblHeroSubtitulo.Text = $"Mostrando resultados para \"{filter}\"";
                }
            }

            ActualizarEstadoAcciones();
            _arbolPantallas.EndUpdate();
        }

        private TreeNode? CrearNodoFiltrado(PantallaNodo pantalla, Dictionary<int, List<PantallaNodo>> lookup, string? filtro)
        {
            var hijos = lookup.TryGetValue(pantalla.Id, out var listaHijos) ? listaHijos : new List<PantallaNodo>();
            var nodosHijo = new List<TreeNode>();

            foreach (var hijo in hijos)
            {
                var nodoHijo = CrearNodoFiltrado(hijo, lookup, filtro);
                if (nodoHijo != null)
                {
                    nodosHijo.Add(nodoHijo);
                }
            }

            var coincide = filtro == null
                || pantalla.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase)
                || pantalla.Codigo.Contains(filtro, StringComparison.OrdinalIgnoreCase);

            if (!coincide && nodosHijo.Count == 0)
            {
                return null;
            }

            var nodo = new TreeNode(pantalla.Nombre)
            {
                Tag = pantalla,
                ToolTipText = pantalla.Codigo
            };

            foreach (var hijo in nodosHijo)
            {
                nodo.Nodes.Add(hijo);
            }

            return nodo;
        }

        private int ContarNodos(TreeNodeCollection nodes)
        {
            var total = 0;
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is PantallaNodo)
                {
                    total++;
                }
                total += ContarNodos(node.Nodes);
            }
            return total;
        }

        private FlowLayoutPanel CrearPanelAccionesRapidas()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            ConfigurarBotonAccion(_btnIrUsuarios, "Usuarios", "Gestiona cuentas y credenciales", "USUARIOS");
            ConfigurarBotonAccion(_btnIrPerfiles, "Perfiles", "Administra roles y perfiles", "PERFILES");
            ConfigurarBotonAccion(_btnIrAccesos, "Accesos", "Configura permisos por pantalla", "ACCESOS");

            panel.Controls.Add(_btnIrUsuarios);
            panel.Controls.Add(_btnIrPerfiles);
            panel.Controls.Add(_btnIrAccesos);

            return panel;
        }

        private void ConfigurarBotonAccion(Button boton, string titulo, string descripcion, string codigoPantalla)
        {
            boton.Tag = codigoPantalla;
            boton.Text = $"{titulo}\n{descripcion}";
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.Margin = new Padding(0, 0, 0, 12);
            boton.AutoSize = false;
            boton.Size = new Size(260, 72);
            boton.Padding = new Padding(16, 12, 16, 12);
            UiTheme.StyleSecondaryButton(boton);
            boton.AutoSize = false;
            boton.Size = new Size(260, 72);
            boton.Font = UiTheme.SectionTitleFont;
            boton.ImageAlign = ContentAlignment.MiddleRight;
            boton.Click -= BotonAccion_Click;
            boton.Click += BotonAccion_Click;
        }

        private void BotonAccion_Click(object? sender, EventArgs e)
        {
            if (sender is Button boton && boton.Tag is string codigo)
            {
                AbrirPantallaDesdeCodigo(codigo);
            }
        }

        private void AbrirPantallaDesdeCodigo(string codigoPantalla)
        {
            Form? formulario = codigoPantalla switch
            {
                "USUARIOS" => new UsuariosForm(_usuarioPrivilegiado),
                "PERFILES" => new PerfilesForm(),
                "ACCESOS" => new AccesosForm(),
                _ => null
            };

            if (formulario != null)
            {
                using (formulario)
                {
                    formulario.StartPosition = FormStartPosition.CenterParent;
                    formulario.ShowDialog(this);
                }
                CargarMenu();
            }
        }

        private void ActualizarEstadoAcciones()
        {
            bool tieneUsuarios = _pantallasDisponibles.Any(p => string.Equals(p.Codigo, "USUARIOS", StringComparison.OrdinalIgnoreCase));
            bool tienePerfiles = _pantallasDisponibles.Any(p => string.Equals(p.Codigo, "PERFILES", StringComparison.OrdinalIgnoreCase));
            bool tieneAccesos = _pantallasDisponibles.Any(p => string.Equals(p.Codigo, "ACCESOS", StringComparison.OrdinalIgnoreCase));

            _btnIrUsuarios.Enabled = tieneUsuarios;
            _btnIrPerfiles.Enabled = tienePerfiles;
            _btnIrAccesos.Enabled = tieneAccesos;
        }

        private Panel CrearChipEstadistica(string titulo, Label valorLabel)
        {
            valorLabel.Font = new Font(UiTheme.TitleFont.FontFamily, 28F, FontStyle.Bold, GraphicsUnit.Point);
            valorLabel.ForeColor = Color.White;
            valorLabel.AutoSize = true;

            var tituloLabel = new Label
            {
                Text = titulo,
                AutoSize = true,
                ForeColor = Color.FromArgb(220, 230, 255),
                Margin = new Padding(0, 4, 0, 0)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(valorLabel, 0, 0);
            layout.Controls.Add(tituloLabel, 0, 1);

            var panel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(20, 12, 20, 12),
                Margin = new Padding(0, 0, 12, 0),
                BackColor = Color.FromArgb(72, 116, 255)
            };
            panel.Controls.Add(layout);

            return panel;
        }

        private void ArbolPantallas_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node?.Tag is not PantallaNodo pantalla)
            {
                return;
            }

            AbrirPantallaDesdeCodigo(pantalla.Codigo);
        }

        private sealed class HeroPanel : Panel
        {
            public HeroPanel()
            {
                DoubleBuffered = true;
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                base.OnPaintBackground(e);

                using var brush = new LinearGradientBrush(ClientRectangle,
                    Color.FromArgb(46, 100, 255),
                    Color.FromArgb(86, 132, 255),
                    LinearGradientMode.ForwardDiagonal);
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using var pen = new Pen(Color.FromArgb(120, 160, 255), 1);
                e.Graphics.DrawRectangle(pen, rect);
            }
        }
    }
}
