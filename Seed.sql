SET NOCOUNT ON;
GO

DECLARE @PerfilAdminId INT;
INSERT INTO dbo.Perfil (NombrePerfil, Codigo, Descripcion, Activo)
VALUES ('ADMIN', 'ADMIN', 'Perfil administrador del sistema', 1);
SET @PerfilAdminId = SCOPE_IDENTITY();

DECLARE @PantallaLogin INT;
DECLARE @PantallaPrincipal INT;
DECLARE @PantallaUsuarios INT;
DECLARE @PantallaPerfiles INT;
DECLARE @PantallaAccesos INT;

INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, Orden)
VALUES ('LOGIN', 'Login', 'LoginForm', 0);
SET @PantallaLogin = SCOPE_IDENTITY();

INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, Orden)
VALUES ('PRINCIPAL', 'Principal', 'PrincipalForm', 1);
SET @PantallaPrincipal = SCOPE_IDENTITY();

INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden)
VALUES ('USUARIOS', 'Usuarios', 'UsuariosForm', @PantallaPrincipal, 1);
SET @PantallaUsuarios = SCOPE_IDENTITY();

INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden)
VALUES ('PERFILES', 'Perfiles', 'PerfilesForm', @PantallaPrincipal, 2);
SET @PantallaPerfiles = SCOPE_IDENTITY();

INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden)
VALUES ('ACCESOS', 'Accesos', 'AccesosForm', @PantallaPrincipal, 3);
SET @PantallaAccesos = SCOPE_IDENTITY();

DECLARE @Salt VARBINARY(32) = 0x8F7E7960776C3F2A03C702974CDA994BC15CC6BA8BF71C86B11CAE743D072977;
DECLARE @Hash VARBINARY(64) = 0xB6723BA508058C5D16BE925D330C9A2DCA980E3C89E0718706BF2B6F53CCBCA5A8850467EBAA3098323E304B08D8909F4F31FC0763342E5EC09B196BAFB70B8B;

DECLARE @UsuarioAdminId INT;
INSERT INTO dbo.Usuario (NombreUsuario, Correo, ClaveHash, ClaveSalt, NombreCompleto, Activo)
VALUES ('admin', 'admin@local', @Hash, @Salt, 'Administrador General', 1);
SET @UsuarioAdminId = SCOPE_IDENTITY();

INSERT INTO dbo.UsuarioPerfil (IdUsuario, IdPerfil, AsignadoPor)
VALUES (@UsuarioAdminId, @PerfilAdminId, 'SEED');

INSERT INTO dbo.PerfilPantallaAcceso (IdPerfil, IdPantalla, PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, Activo, OtorgadoPor)
SELECT @PerfilAdminId, p.IdPantalla, 1, 1, 1, 1, 1, 1, 'SEED'
FROM dbo.Pantalla p;
GO
