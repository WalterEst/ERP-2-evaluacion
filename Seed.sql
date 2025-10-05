SET NOCOUNT ON;
GO

DECLARE @Ahora DATETIME = GETDATE();

-- Pantallas base requeridas por la aplicación
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

-- Perfiles básicos de la aplicación
IF NOT EXISTS (SELECT 1 FROM dbo.Perfil WHERE Codigo = 'SUPERADMIN')
BEGIN
    INSERT INTO dbo.Perfil (NombrePerfil, Codigo, Descripcion, Activo)
    VALUES ('Super Administrador', 'SUPERADMIN', 'Control total del sistema', 1);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Perfil WHERE Codigo = 'ADMIN')
BEGIN
    INSERT INTO dbo.Perfil (NombrePerfil, Codigo, Descripcion, Activo)
    VALUES ('Administrador', 'ADMIN', 'Administrador funcional con permisos ampliados', 1);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Perfil WHERE Codigo = 'BASICO')
BEGIN
    INSERT INTO dbo.Perfil (NombrePerfil, Codigo, Descripcion, Activo)
    VALUES ('Básico', 'BASICO', 'Perfil por defecto para nuevos usuarios', 1);
END;

DECLARE @IdPerfilSuper INT = (SELECT IdPerfil FROM dbo.Perfil WHERE Codigo = 'SUPERADMIN');

-- Crear o actualizar el super usuario por defecto
DECLARE @IdUsuarioAdmin INT;

IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE NombreUsuario = 'admin')
BEGIN
    INSERT INTO dbo.Usuario (NombreUsuario, Correo, Clave, NombreCompleto, Activo, UltimoIngreso, FechaCreacion)
    VALUES ('admin', 'admin@erp.local', 'admin123', 'Super Administrador del sistema', 1, NULL, CAST(@Ahora AS DATE));

    SET @IdUsuarioAdmin = SCOPE_IDENTITY();
END;
ELSE
BEGIN
    SELECT @IdUsuarioAdmin = IdUsuario FROM dbo.Usuario WHERE NombreUsuario = 'admin';

    UPDATE dbo.Usuario
    SET Correo = CASE
                     WHEN Correo = 'admin@erp.local' THEN Correo
                     WHEN NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Correo = 'admin@erp.local' AND IdUsuario <> @IdUsuarioAdmin)
                         THEN 'admin@erp.local'
                     ELSE Correo
                 END,
        Clave = 'admin123',
        NombreCompleto = CASE WHEN NULLIF(LTRIM(RTRIM(NombreCompleto)), '') IS NULL THEN 'Super Administrador del sistema' ELSE NombreCompleto END,
        Activo = 1
    WHERE IdUsuario = @IdUsuarioAdmin;
END;

IF @IdUsuarioAdmin IS NULL
BEGIN
    SELECT @IdUsuarioAdmin = IdUsuario FROM dbo.Usuario WHERE NombreUsuario = 'admin';
END;

-- Asignar el perfil SUPERADMIN al usuario admin
IF NOT EXISTS (SELECT 1 FROM dbo.UsuarioPerfil WHERE IdUsuario = @IdUsuarioAdmin AND IdPerfil = @IdPerfilSuper)
BEGIN
    INSERT INTO dbo.UsuarioPerfil (IdUsuario, IdPerfil, AsignadoPor)
    VALUES (@IdUsuarioAdmin, @IdPerfilSuper, 'SEED');
END;

-- Otorgar permisos completos al perfil SUPERADMIN sobre todas las pantallas
MERGE dbo.PerfilPantallaAcceso AS destino
USING (
    SELECT @IdPerfilSuper AS IdPerfil, IdPantalla
    FROM dbo.Pantalla
) AS origen
ON destino.IdPerfil = origen.IdPerfil AND destino.IdPantalla = origen.IdPantalla
WHEN MATCHED THEN
    UPDATE SET PuedeVer = 1,
               PuedeCrear = 1,
               PuedeEditar = 1,
               PuedeEliminar = 1,
               PuedeExportar = 1,
               Activo = 1,
               FechaOtorgado = @Ahora,
               OtorgadoPor = 'SEED'
WHEN NOT MATCHED THEN
    INSERT (IdPerfil, IdPantalla, PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, Activo, FechaOtorgado, OtorgadoPor)
    VALUES (origen.IdPerfil, origen.IdPantalla, 1, 1, 1, 1, 1, 1, @Ahora, 'SEED');
GO
