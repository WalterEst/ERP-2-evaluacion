SET NOCOUNT ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'LOGIN')
BEGIN
    INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, Orden, CreadoPor)
    VALUES ('LOGIN', 'Login', 'LoginForm', 0, 'SEED');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'PRINCIPAL')
BEGIN
    INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, Orden, CreadoPor)
    VALUES ('PRINCIPAL', 'Principal', 'PrincipalForm', 1, 'SEED');
END;

DECLARE @IdPantallaPrincipal INT = (SELECT IdPantalla FROM dbo.Pantalla WHERE Codigo = 'PRINCIPAL');

IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'USUARIOS')
BEGIN
    INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
    VALUES ('USUARIOS', 'Usuarios', 'UsuariosForm', @IdPantallaPrincipal, 1, 'SEED');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'PERFILES')
BEGIN
    INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
    VALUES ('PERFILES', 'Perfiles', 'PerfilesForm', @IdPantallaPrincipal, 2, 'SEED');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'ACCESOS')
BEGIN
    INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
    VALUES ('ACCESOS', 'Accesos', 'AccesosForm', @IdPantallaPrincipal, 3, 'SEED');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Perfil WHERE Codigo = 'ADMIN')
BEGIN
    INSERT INTO dbo.Perfil (NombrePerfil, Codigo, Descripcion, Activo)
    VALUES ('ADMIN', 'ADMIN', 'Perfil administrador con gestión completa', 1);
END;
GO
