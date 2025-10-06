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

IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'BODEGAS')
BEGIN
    INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
    VALUES ('BODEGAS', 'Bodegas', 'BodegasForm', @IdPantallaPrincipal, 4, 'SEED');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'PRODUCTOS')
BEGIN
    INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
    VALUES ('PRODUCTOS', 'Productos', 'ProductosForm', @IdPantallaPrincipal, 5, 'SEED');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'INVENTARIO')
BEGIN
    INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
    VALUES ('INVENTARIO', 'Inventario', 'InventarioForm', @IdPantallaPrincipal, 6, 'SEED');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'CLIENTES')
BEGIN
    INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
    VALUES ('CLIENTES', 'Clientes', 'ClientesForm', @IdPantallaPrincipal, 7, 'SEED');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'VENTAS')
BEGIN
    INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
    VALUES ('VENTAS', 'Ventas', 'VentasForm', @IdPantallaPrincipal, 8, 'SEED');
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

IF NOT EXISTS (SELECT 1 FROM dbo.Perfil WHERE Codigo = 'BODEGUERO')
BEGIN
    INSERT INTO dbo.Perfil (NombrePerfil, Codigo, Descripcion, Activo)
    VALUES ('Encargado de Bodega', 'BODEGUERO', 'Gestiona bodegas, productos e inventario', 1);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Perfil WHERE Codigo = 'VENDEDOR')
BEGIN
    INSERT INTO dbo.Perfil (NombrePerfil, Codigo, Descripcion, Activo)
    VALUES ('Vendedor', 'VENDEDOR', 'Registra ventas y gestiona clientes', 1);
END;

DECLARE @IdPerfilSuper INT = (SELECT IdPerfil FROM dbo.Perfil WHERE Codigo = 'SUPERADMIN');
DECLARE @IdPerfilAdmin INT = (SELECT IdPerfil FROM dbo.Perfil WHERE Codigo = 'ADMIN');
DECLARE @IdPerfilBodeguero INT = (SELECT IdPerfil FROM dbo.Perfil WHERE Codigo = 'BODEGUERO');
DECLARE @IdPerfilVendedor INT = (SELECT IdPerfil FROM dbo.Perfil WHERE Codigo = 'VENDEDOR');

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

IF @IdPerfilAdmin IS NOT NULL
BEGIN
    DECLARE @PermisosAdmin TABLE (Codigo NVARCHAR(50), PuedeVer BIT, PuedeCrear BIT, PuedeEditar BIT, PuedeEliminar BIT, PuedeExportar BIT);
    INSERT INTO @PermisosAdmin VALUES
        ('PRINCIPAL', 1, 0, 0, 0, 0),
        ('USUARIOS', 1, 1, 1, 1, 1),
        ('PERFILES', 1, 1, 1, 1, 1),
        ('ACCESOS', 1, 1, 1, 1, 1),
        ('BODEGAS', 1, 1, 1, 1, 1),
        ('PRODUCTOS', 1, 1, 1, 1, 1),
        ('INVENTARIO', 1, 1, 1, 1, 1),
        ('CLIENTES', 1, 1, 1, 1, 1),
        ('VENTAS', 1, 1, 1, 1, 1);

    MERGE dbo.PerfilPantallaAcceso AS destino
    USING (
        SELECT @IdPerfilAdmin AS IdPerfil,
               p.IdPantalla,
               r.PuedeVer,
               r.PuedeCrear,
               r.PuedeEditar,
               r.PuedeEliminar,
               r.PuedeExportar
        FROM @PermisosAdmin r
        JOIN dbo.Pantalla p ON p.Codigo = r.Codigo
    ) AS origen
    ON destino.IdPerfil = origen.IdPerfil AND destino.IdPantalla = origen.IdPantalla
    WHEN MATCHED THEN
        UPDATE SET PuedeVer = origen.PuedeVer,
                   PuedeCrear = origen.PuedeCrear,
                   PuedeEditar = origen.PuedeEditar,
                   PuedeEliminar = origen.PuedeEliminar,
                   PuedeExportar = origen.PuedeExportar,
                   Activo = 1,
                   FechaOtorgado = @Ahora,
                   OtorgadoPor = 'SEED'
    WHEN NOT MATCHED THEN
        INSERT (IdPerfil, IdPantalla, PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, Activo, FechaOtorgado, OtorgadoPor)
        VALUES (origen.IdPerfil, origen.IdPantalla, origen.PuedeVer, origen.PuedeCrear, origen.PuedeEditar, origen.PuedeEliminar, origen.PuedeExportar, 1, @Ahora, 'SEED');
END;

IF @IdPerfilBodeguero IS NOT NULL
BEGIN
    DECLARE @PermisosBodeguero TABLE (Codigo NVARCHAR(50), PuedeVer BIT, PuedeCrear BIT, PuedeEditar BIT, PuedeEliminar BIT, PuedeExportar BIT);
    INSERT INTO @PermisosBodeguero VALUES
        ('PRINCIPAL', 1, 0, 0, 0, 0),
        ('BODEGAS', 1, 1, 1, 1, 1),
        ('PRODUCTOS', 1, 1, 1, 1, 1),
        ('INVENTARIO', 1, 1, 1, 1, 1);

    MERGE dbo.PerfilPantallaAcceso AS destino
    USING (
        SELECT @IdPerfilBodeguero AS IdPerfil,
               p.IdPantalla,
               r.PuedeVer,
               r.PuedeCrear,
               r.PuedeEditar,
               r.PuedeEliminar,
               r.PuedeExportar
        FROM @PermisosBodeguero r
        JOIN dbo.Pantalla p ON p.Codigo = r.Codigo
    ) AS origen
    ON destino.IdPerfil = origen.IdPerfil AND destino.IdPantalla = origen.IdPantalla
    WHEN MATCHED THEN
        UPDATE SET PuedeVer = origen.PuedeVer,
                   PuedeCrear = origen.PuedeCrear,
                   PuedeEditar = origen.PuedeEditar,
                   PuedeEliminar = origen.PuedeEliminar,
                   PuedeExportar = origen.PuedeExportar,
                   Activo = 1,
                   FechaOtorgado = @Ahora,
                   OtorgadoPor = 'SEED'
    WHEN NOT MATCHED THEN
        INSERT (IdPerfil, IdPantalla, PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, Activo, FechaOtorgado, OtorgadoPor)
        VALUES (origen.IdPerfil, origen.IdPantalla, origen.PuedeVer, origen.PuedeCrear, origen.PuedeEditar, origen.PuedeEliminar, origen.PuedeExportar, 1, @Ahora, 'SEED');
END;

IF @IdPerfilVendedor IS NOT NULL
BEGIN
    DECLARE @PermisosVendedor TABLE (Codigo NVARCHAR(50), PuedeVer BIT, PuedeCrear BIT, PuedeEditar BIT, PuedeEliminar BIT, PuedeExportar BIT);
    INSERT INTO @PermisosVendedor VALUES
        ('PRINCIPAL', 1, 0, 0, 0, 0),
        ('CLIENTES', 1, 1, 1, 0, 1),
        ('VENTAS', 1, 1, 0, 0, 1);

    MERGE dbo.PerfilPantallaAcceso AS destino
    USING (
        SELECT @IdPerfilVendedor AS IdPerfil,
               p.IdPantalla,
               r.PuedeVer,
               r.PuedeCrear,
               r.PuedeEditar,
               r.PuedeEliminar,
               r.PuedeExportar
        FROM @PermisosVendedor r
        JOIN dbo.Pantalla p ON p.Codigo = r.Codigo
    ) AS origen
    ON destino.IdPerfil = origen.IdPerfil AND destino.IdPantalla = origen.IdPantalla
    WHEN MATCHED THEN
        UPDATE SET PuedeVer = origen.PuedeVer,
                   PuedeCrear = origen.PuedeCrear,
                   PuedeEditar = origen.PuedeEditar,
                   PuedeEliminar = origen.PuedeEliminar,
                   PuedeExportar = origen.PuedeExportar,
                   Activo = 1,
                   FechaOtorgado = @Ahora,
                   OtorgadoPor = 'SEED'
    WHEN NOT MATCHED THEN
        INSERT (IdPerfil, IdPantalla, PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, Activo, FechaOtorgado, OtorgadoPor)
        VALUES (origen.IdPerfil, origen.IdPantalla, origen.PuedeVer, origen.PuedeCrear, origen.PuedeEditar, origen.PuedeEliminar, origen.PuedeExportar, 1, @Ahora, 'SEED');
END;

IF EXISTS (SELECT 1 FROM dbo.Perfil WHERE Codigo = 'BASICO')
BEGIN
    UPDATE dbo.Perfil SET Activo = 0 WHERE Codigo = 'BASICO';
END;

-- Datos base para catálogos comerciales
IF NOT EXISTS (SELECT 1 FROM dbo.Bodega WHERE Codigo = 'BOD-PRINC')
BEGIN
    INSERT INTO dbo.Bodega (Codigo, Nombre, Ubicacion, Encargado, Descripcion)
    VALUES ('BOD-PRINC', 'Bodega Principal', 'Casa matriz', 'Encargado General', 'Bodega principal del negocio');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Bodega WHERE Codigo = 'BOD-SEC')
BEGIN
    INSERT INTO dbo.Bodega (Codigo, Nombre, Ubicacion, Encargado, Descripcion)
    VALUES ('BOD-SEC', 'Bodega Secundaria', 'Sucursal centro', 'Encargado Sucursal', 'Bodega para despacho regional');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.CategoriaProducto WHERE Nombre = 'General')
BEGIN
    INSERT INTO dbo.CategoriaProducto (Nombre, Descripcion)
    VALUES ('General', 'Productos generales de comercio');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.CategoriaProducto WHERE Nombre = 'Tecnología')
BEGIN
    INSERT INTO dbo.CategoriaProducto (Nombre, Descripcion)
    VALUES ('Tecnología', 'Equipos y dispositivos electrónicos');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE Codigo = 'P-0001')
BEGIN
    DECLARE @IdCategoriaGeneral INT = (SELECT IdCategoria FROM dbo.CategoriaProducto WHERE Nombre = 'General');
    INSERT INTO dbo.Producto (Codigo, Nombre, Descripcion, IdCategoria, PrecioCosto, PrecioVenta, StockMinimo, StockMaximo)
    VALUES ('P-0001', 'Producto Genérico', 'Producto de ejemplo para demostración', @IdCategoriaGeneral, 10.00, 18.00, 5, 500);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE Codigo = 'P-0002')
BEGIN
    DECLARE @IdCategoriaTec INT = (SELECT IdCategoria FROM dbo.CategoriaProducto WHERE Nombre = 'Tecnología');
    INSERT INTO dbo.Producto (Codigo, Nombre, Descripcion, IdCategoria, PrecioCosto, PrecioVenta, StockMinimo, StockMaximo)
    VALUES ('P-0002', 'Dispositivo Inteligente', 'Equipo tecnológico para ventas minoristas', @IdCategoriaTec, 180.00, 250.00, 2, 120);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE Identificacion = 'CLI-0001')
BEGIN
    INSERT INTO dbo.Cliente (NombreCompleto, Identificacion, TipoDocumento, Correo, Telefono, Direccion)
    VALUES ('Cliente Mostrador', 'CLI-0001', 'RNC', 'cliente@comercio.local', '809-555-1000', 'Dirección comercial principal');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE Identificacion = 'CLI-0002')
BEGIN
    INSERT INTO dbo.Cliente (NombreCompleto, Identificacion, TipoDocumento, Correo, Telefono, Direccion)
    VALUES ('Compañía Corporativa', 'CLI-0002', 'RNC', 'ventas@clienteempresarial.local', '809-555-2000', 'Parque industrial');
END;

-- Asegurar inventario base para productos de ejemplo
DECLARE @BodegaPrincipal INT = (SELECT IdBodega FROM dbo.Bodega WHERE Codigo = 'BOD-PRINC');
DECLARE @BodegaSecundaria INT = (SELECT IdBodega FROM dbo.Bodega WHERE Codigo = 'BOD-SEC');

IF @BodegaPrincipal IS NOT NULL
BEGIN
    MERGE dbo.Inventario AS destino
    USING (
        SELECT p.IdProducto, @BodegaPrincipal AS IdBodega, Cantidad = CASE p.Codigo WHEN 'P-0001' THEN 150 WHEN 'P-0002' THEN 25 ELSE 0 END
        FROM dbo.Producto p
    ) AS origen
    ON destino.IdProducto = origen.IdProducto AND destino.IdBodega = origen.IdBodega
    WHEN MATCHED THEN
        UPDATE SET StockActual = CASE WHEN origen.Cantidad > 0 THEN origen.Cantidad ELSE destino.StockActual END,
                   FechaActualizacion = @Ahora
    WHEN NOT MATCHED THEN
        INSERT (IdProducto, IdBodega, StockActual, StockMinimo, StockMaximo)
        VALUES (origen.IdProducto, origen.IdBodega, origen.Cantidad, 0, NULL);
END;

IF @BodegaSecundaria IS NOT NULL
BEGIN
    MERGE dbo.Inventario AS destino
    USING (
        SELECT p.IdProducto, @BodegaSecundaria AS IdBodega, Cantidad = CASE p.Codigo WHEN 'P-0001' THEN 80 WHEN 'P-0002' THEN 10 ELSE 0 END
        FROM dbo.Producto p
    ) AS origen
    ON destino.IdProducto = origen.IdProducto AND destino.IdBodega = origen.IdBodega
    WHEN MATCHED THEN
        UPDATE SET StockActual = CASE WHEN origen.Cantidad > 0 THEN origen.Cantidad ELSE destino.StockActual END,
                   FechaActualizacion = @Ahora
    WHEN NOT MATCHED THEN
        INSERT (IdProducto, IdBodega, StockActual, StockMinimo, StockMaximo)
        VALUES (origen.IdProducto, origen.IdBodega, origen.Cantidad, 0, NULL);
END;
