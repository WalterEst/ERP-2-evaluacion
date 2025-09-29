using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public class PrincipalForm : Form
{
    private readonly int _idUsuario;
    private readonly string _nombreUsuario;
    private readonly TreeView _arbolPantallas = new() { Dock = DockStyle.Fill };
    private readonly Label _lblBienvenida = new() { Dock = DockStyle.Top, Padding = new Padding(10), Font = new Font("Segoe UI", 10, FontStyle.Bold) };

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
        Text = "Principal";
        WindowState = FormWindowState.Maximized;

        _lblBienvenida.Text = $"Bienvenido(a), {_nombreUsuario}";
        _arbolPantallas.NodeMouseDoubleClick += ArbolPantallas_NodeMouseDoubleClick;

        Controls.Add(_arbolPantallas);
        Controls.Add(_lblBienvenida);

        Load += (_, _) => CargarMenu();
    }

    private void CargarMenu()
    {
        _arbolPantallas.Nodes.Clear();
        var pantallas = new List<PantallaNodo>();

        try
        {
            using var connection = Db.GetConnection();
            using var command = new SqlCommand(@"SELECT DISTINCT p.IdPantalla, p.Codigo, p.NombrePantalla, p.IdPadre, p.Orden
FROM Pantalla p
INNER JOIN PerfilPantallaAcceso a ON a.IdPantalla = p.IdPantalla AND a.PuedeVer = 1 AND a.Activo = 1
INNER JOIN UsuarioPerfil up ON up.IdPerfil = a.IdPerfil
WHERE up.IdUsuario = @idUsuario
ORDER BY ISNULL(p.IdPadre, 0), p.Orden, p.NombrePantalla", connection);
            command.Parameters.AddWithValue("@idUsuario", _idUsuario);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                pantallas.Add(new PantallaNodo
                {
                    Id = reader.GetInt32(0),
                    Codigo = reader.GetString(1),
                    Nombre = reader.GetString(2),
                    IdPadre = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    Orden = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No fue posible cargar el menú: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var nodosPorId = pantallas.ToDictionary(p => p.Id, p =>
        {
            var nodo = new TreeNode(p.Nombre) { Tag = p };
            return nodo;
        });

        foreach (var pantalla in pantallas.OrderBy(p => p.Orden))
        {
            var nodo = nodosPorId[pantalla.Id];
            if (pantalla.IdPadre.HasValue && nodosPorId.TryGetValue(pantalla.IdPadre.Value, out var padre))
            {
                padre.Nodes.Add(nodo);
            }
            else
            {
                _arbolPantallas.Nodes.Add(nodo);
            }
        }

        _arbolPantallas.ExpandAll();
    }

    private void ArbolPantallas_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Node?.Tag is not PantallaNodo pantalla)
        {
            return;
        }

        Form? formulario = pantalla.Codigo switch
        {
            "USUARIOS" => new UsuariosForm(),
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
}
