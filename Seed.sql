/* ==========================================================
   Seed ERP - Pantallas, Roles, Usuario admin, Permisos,
   Catálogos base e Inventario inicial (sin MERGE/CTE)
   Ejecutar completo en un solo batch
   ========================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @Ahora DATETIME2 = SYSDATETIME();

    /*--------------------------------------------------------
      1) Pantallas base
    --------------------------------------------------------*/
    IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'LOGIN')
    BEGIN
        INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, Orden, CreadoPor)
        VALUES ('LOGIN', N'Login', N'LoginForm', 0, N'SEED');
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'PRINCIPAL')
    BEGIN
        INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, Orden, CreadoPor)
        VALUES ('PRINCIPAL', N'Principal', N'PrincipalForm', 1, N'SEED');
    END;

    DECLARE @IdPantallaPrincipal INT = (SELECT TOP (1) IdPantalla FROM dbo.Pantalla WHERE Codigo = 'PRINCIPAL');

    IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'USUARIOS')
    BEGIN
        INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
        VALUES ('USUARIOS', N'Usuarios', N'UsuariosForm', @IdPantallaPrincipal, 1, N'SEED');
    END;

    IF EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'PERFILES')
       AND NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'ROLES')
    BEGIN
        UPDATE dbo.Pantalla
           SET Codigo = 'ROLES',
               NombrePantalla = N'Roles',
               Ruta = 'RolesForm'
         WHERE Codigo = 'PERFILES';
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'ROLES')
    BEGIN
        INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
        VALUES ('ROLES', N'Roles', N'RolesForm', @IdPantallaPrincipal, 2, N'SEED');
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'ACCESOS')
    BEGIN
        INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
        VALUES ('ACCESOS', N'Accesos', N'AccesosForm', @IdPantallaPrincipal, 3, N'SEED');
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'BODEGAS')
    BEGIN
        INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
        VALUES ('BODEGAS', N'Bodegas', N'BodegasForm', @IdPantallaPrincipal, 4, N'SEED');
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'PRODUCTOS')
    BEGIN
        INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
        VALUES ('PRODUCTOS', N'Productos', N'ProductosForm', @IdPantallaPrincipal, 5, N'SEED');
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'INVENTARIO')
    BEGIN
        INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
        VALUES ('INVENTARIO', N'Inventario', N'InventarioForm', @IdPantallaPrincipal, 6, N'SEED');
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'CLIENTES')
    BEGIN
        INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
        VALUES ('CLIENTES', N'Clientes', N'ClientesForm', @IdPantallaPrincipal, 7, N'SEED');
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Pantalla WHERE Codigo = 'VENTAS')
    BEGIN
        INSERT INTO dbo.Pantalla (Codigo, NombrePantalla, Ruta, IdPadre, Orden, CreadoPor)
        VALUES ('VENTAS', N'Ventas', N'VentasForm', @IdPantallaPrincipal, 8, N'SEED');
    END;

    /*--------------------------------------------------------
      2) Roles
    --------------------------------------------------------*/
    IF NOT EXISTS (SELECT 1 FROM dbo.Perfil WHERE Codigo = 'SUPERADMIN')
    BEGIN
        INSERT INTO dbo.Perfil (NombrePerfil, Codigo, Descripcion, Activo)
        VALUES (N'Super Administrador', 'SUPERADMIN', N'Control total del sistema', 1);
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Perfil WHERE Codigo = 'ADMIN')
    BEGIN
        INSERT INTO dbo.Perfil (NombrePerfil, Codigo, Descripcion, Activo)
        VALUES (N'Administrador', 'ADMIN', N'Administrador funcional con permisos ampliados', 1);
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Perfil WHERE Codigo = 'BODEGUERO')
    BEGIN
        INSERT INTO dbo.Perfil (NombrePerfil, Codigo, Descripcion, Activo)
        VALUES (N'Encargado de Bodega', 'BODEGUERO', N'Gestiona bodegas, productos e inventario', 1);
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Perfil WHERE Codigo = 'VENDEDOR')
    BEGIN
        INSERT INTO dbo.Perfil (NombrePerfil, Codigo, Descripcion, Activo)
        VALUES (N'Vendedor', 'VENDEDOR', N'Registra ventas y gestiona clientes', 1);
    END;

    DECLARE @IdPerfilSuper     INT = (SELECT TOP(1) IdPerfil FROM dbo.Perfil WHERE Codigo = 'SUPERADMIN');
    DECLARE @IdPerfilAdmin     INT = (SELECT TOP(1) IdPerfil FROM dbo.Perfil WHERE Codigo = 'ADMIN');
    DECLARE @IdPerfilBodeguero INT = (SELECT TOP(1) IdPerfil FROM dbo.Perfil WHERE Codigo = 'BODEGUERO');
    DECLARE @IdPerfilVendedor  INT = (SELECT TOP(1) IdPerfil FROM dbo.Perfil WHERE Codigo = 'VENDEDOR');

    /*--------------------------------------------------------
      3) Usuario admin (crear/actualizar) + asignar SUPERADMIN
    --------------------------------------------------------*/
    DECLARE @IdUsuarioAdmin INT;

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE NombreUsuario = 'admin')
    BEGIN
        INSERT INTO dbo.Usuario (NombreUsuario, Correo, Clave, NombreCompleto, Activo, UltimoIngreso, FechaCreacion)
        VALUES ('admin', 'admin@erp.local', 'admin123', N'Super Administrador del sistema', 1, NULL, CAST(@Ahora AS DATE));

        SET @IdUsuarioAdmin = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        SELECT TOP(1) @IdUsuarioAdmin = IdUsuario
        FROM dbo.Usuario
        WHERE NombreUsuario = 'admin';

        UPDATE u
        SET  Correo = CASE
                        WHEN Correo = 'admin@erp.local' THEN Correo
                        WHEN NOT EXISTS (SELECT 1 FROM dbo.Usuario x WHERE x.Correo = 'admin@erp.local' AND x.IdUsuario <> u.IdUsuario)
                             THEN 'admin@erp.local'
                        ELSE Correo
                      END,
             Clave = 'admin123',
             NombreCompleto = CASE WHEN NULLIF(LTRIM(RTRIM(NombreCompleto)), '') IS NULL
                                   THEN N'Super Administrador del sistema' ELSE NombreCompleto END,
             Activo = 1
        FROM dbo.Usuario u
        WHERE u.IdUsuario = @IdUsuarioAdmin;
    END;

    IF @IdUsuarioAdmin IS NULL
    BEGIN
        SELECT TOP(1) @IdUsuarioAdmin = IdUsuario FROM dbo.Usuario WHERE NombreUsuario = 'admin';
    END;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.UsuarioPerfil WHERE IdUsuario = @IdUsuarioAdmin AND IdPerfil = @IdPerfilSuper
    )
    BEGIN
        INSERT INTO dbo.UsuarioPerfil (IdUsuario, IdPerfil, AsignadoPor)
        VALUES (@IdUsuarioAdmin, @IdPerfilSuper, N'SEED');
    END;

    /*--------------------------------------------------------
      4) Permisos SUPERADMIN: full sobre todas las pantallas
         (UPSERT sin MERGE)
    --------------------------------------------------------*/
    -- UPDATE existentes
    UPDATE d
       SET d.PuedeVer = 1,
           d.PuedeCrear = 1,
           d.PuedeEditar = 1,
           d.PuedeEliminar = 1,
           d.PuedeExportar = 1,
           d.Activo = 1,
           d.FechaOtorgado = @Ahora,
           d.OtorgadoPor = N'SEED'
      FROM dbo.PerfilPantallaAcceso d
      JOIN dbo.Pantalla p ON p.IdPantalla = d.IdPantalla
     WHERE d.IdPerfil = @IdPerfilSuper;

    -- INSERT faltantes
    INSERT INTO dbo.PerfilPantallaAcceso
        (IdPerfil, IdPantalla, PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, Activo, FechaOtorgado, OtorgadoPor)
    SELECT @IdPerfilSuper, p.IdPantalla, 1, 1, 1, 1, 1, 1, @Ahora, N'SEED'
      FROM dbo.Pantalla p
      WHERE NOT EXISTS (
            SELECT 1
              FROM dbo.PerfilPantallaAcceso a
             WHERE a.IdPerfil = @IdPerfilSuper
               AND a.IdPantalla = p.IdPantalla
      );

    /*--------------------------------------------------------
      5) Permisos ADMIN
    --------------------------------------------------------*/
    IF @IdPerfilAdmin IS NOT NULL
    BEGIN
        DECLARE @PermisosAdmin TABLE (Codigo NVARCHAR(50), PuedeVer BIT, PuedeCrear BIT, PuedeEditar BIT, PuedeEliminar BIT, PuedeExportar BIT);
        INSERT INTO @PermisosAdmin VALUES
            (N'PRINCIPAL', 1, 0, 0, 0, 0),
            (N'USUARIOS',  1, 1, 1, 1, 1),
            (N'ROLES',     1, 1, 1, 1, 1),
            (N'ACCESOS',   1, 1, 1, 1, 1),
            (N'BODEGAS',   1, 1, 1, 1, 1),
            (N'PRODUCTOS', 1, 1, 1, 1, 1),
            (N'INVENTARIO',1, 1, 1, 1, 1),
            (N'CLIENTES',  1, 1, 1, 1, 1),
            (N'VENTAS',    1, 1, 1, 1, 1);

        -- UPDATE existentes
        UPDATE d
           SET d.PuedeVer = r.PuedeVer,
               d.PuedeCrear = r.PuedeCrear,
               d.PuedeEditar = r.PuedeEditar,
               d.PuedeEliminar = r.PuedeEliminar,
               d.PuedeExportar = r.PuedeExportar,
               d.Activo = 1,
               d.FechaOtorgado = @Ahora,
               d.OtorgadoPor = N'SEED'
          FROM dbo.PerfilPantallaAcceso d
          JOIN dbo.Pantalla p ON p.IdPantalla = d.IdPantalla
          JOIN @PermisosAdmin r ON r.Codigo = p.Codigo
         WHERE d.IdPerfil = @IdPerfilAdmin;

        -- INSERT faltantes
        INSERT INTO dbo.PerfilPantallaAcceso
            (IdPerfil, IdPantalla, PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, Activo, FechaOtorgado, OtorgadoPor)
        SELECT @IdPerfilAdmin, p.IdPantalla, r.PuedeVer, r.PuedeCrear, r.PuedeEditar, r.PuedeEliminar, r.PuedeExportar, 1, @Ahora, N'SEED'
          FROM @PermisosAdmin r
          JOIN dbo.Pantalla p ON p.Codigo = r.Codigo
         WHERE NOT EXISTS (
                SELECT 1
                  FROM dbo.PerfilPantallaAcceso a
                 WHERE a.IdPerfil = @IdPerfilAdmin
                   AND a.IdPantalla = p.IdPantalla
         );
    END;

    /*--------------------------------------------------------
      6) Permisos BODEGUERO
    --------------------------------------------------------*/
    IF @IdPerfilBodeguero IS NOT NULL
    BEGIN
        DECLARE @PermisosBodeguero TABLE (Codigo NVARCHAR(50), PuedeVer BIT, PuedeCrear BIT, PuedeEditar BIT, PuedeEliminar BIT, PuedeExportar BIT);
        INSERT INTO @PermisosBodeguero VALUES
            (N'PRINCIPAL', 1, 0, 0, 0, 0),
            (N'BODEGAS',   1, 1, 1, 1, 1),
            (N'PRODUCTOS', 1, 1, 1, 1, 1),
            (N'INVENTARIO',1, 1, 1, 1, 1);

        -- UPDATE existentes
        UPDATE d
           SET d.PuedeVer = r.PuedeVer,
               d.PuedeCrear = r.PuedeCrear,
               d.PuedeEditar = r.PuedeEditar,
               d.PuedeEliminar = r.PuedeEliminar,
               d.PuedeExportar = r.PuedeExportar,
               d.Activo = 1,
               d.FechaOtorgado = @Ahora,
               d.OtorgadoPor = N'SEED'
          FROM dbo.PerfilPantallaAcceso d
          JOIN dbo.Pantalla p ON p.IdPantalla = d.IdPantalla
          JOIN @PermisosBodeguero r ON r.Codigo = p.Codigo
         WHERE d.IdPerfil = @IdPerfilBodeguero;

        -- INSERT faltantes
        INSERT INTO dbo.PerfilPantallaAcceso
            (IdPerfil, IdPantalla, PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, Activo, FechaOtorgado, OtorgadoPor)
        SELECT @IdPerfilBodeguero, p.IdPantalla, r.PuedeVer, r.PuedeCrear, r.PuedeEditar, r.PuedeEliminar, r.PuedeExportar, 1, @Ahora, N'SEED'
          FROM @PermisosBodeguero r
          JOIN dbo.Pantalla p ON p.Codigo = r.Codigo
         WHERE NOT EXISTS (
                SELECT 1
                  FROM dbo.PerfilPantallaAcceso a
                 WHERE a.IdPerfil = @IdPerfilBodeguero
                   AND a.IdPantalla = p.IdPantalla
         );
    END;

    /*--------------------------------------------------------
      7) Permisos VENDEDOR
    --------------------------------------------------------*/
    IF @IdPerfilVendedor IS NOT NULL
    BEGIN
        DECLARE @PermisosVendedor TABLE (Codigo NVARCHAR(50), PuedeVer BIT, PuedeCrear BIT, PuedeEditar BIT, PuedeEliminar BIT, PuedeExportar BIT);
        INSERT INTO @PermisosVendedor VALUES
            (N'PRINCIPAL', 1, 0, 0, 0, 0),
            (N'CLIENTES',  1, 1, 1, 0, 1),
            (N'VENTAS',    1, 1, 0, 0, 1);

        -- UPDATE existentes
        UPDATE d
           SET d.PuedeVer = r.PuedeVer,
               d.PuedeCrear = r.PuedeCrear,
               d.PuedeEditar = r.PuedeEditar,
               d.PuedeEliminar = r.PuedeEliminar,
               d.PuedeExportar = r.PuedeExportar,
               d.Activo = 1,
               d.FechaOtorgado = @Ahora,
               d.OtorgadoPor = N'SEED'
          FROM dbo.PerfilPantallaAcceso d
          JOIN dbo.Pantalla p ON p.IdPantalla = d.IdPantalla
          JOIN @PermisosVendedor r ON r.Codigo = p.Codigo
         WHERE d.IdPerfil = @IdPerfilVendedor;

        -- INSERT faltantes
        INSERT INTO dbo.PerfilPantallaAcceso
            (IdPerfil, IdPantalla, PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, Activo, FechaOtorgado, OtorgadoPor)
        SELECT @IdPerfilVendedor, p.IdPantalla, r.PuedeVer, r.PuedeCrear, r.PuedeEditar, r.PuedeEliminar, r.PuedeExportar, 1, @Ahora, N'SEED'
          FROM @PermisosVendedor r
          JOIN dbo.Pantalla p ON p.Codigo = r.Codigo
         WHERE NOT EXISTS (
                SELECT 1
                  FROM dbo.PerfilPantallaAcceso a
                 WHERE a.IdPerfil = @IdPerfilVendedor
                   AND a.IdPantalla = p.IdPantalla
         );
    END;

    /*--------------------------------------------------------
      8) Desactivar perfil BASICO si existe
    --------------------------------------------------------*/
    IF EXISTS (SELECT 1 FROM dbo.Perfil WHERE Codigo = 'BASICO')
    BEGIN
        UPDATE dbo.Perfil SET Activo = 0 WHERE Codigo = 'BASICO';
    END;

    /*--------------------------------------------------------
      9) Catálogos base: Bodegas, Categorías, Productos, Clientes
    --------------------------------------------------------*/
    IF NOT EXISTS (SELECT 1 FROM dbo.Bodega WHERE Codigo = 'BOD-PRINC')
    BEGIN
        INSERT INTO dbo.Bodega (Codigo, Nombre, Ubicacion, Encargado, Descripcion)
        VALUES ('BOD-PRINC', N'Bodega Principal', N'Casa matriz', N'Encargado General', N'Bodega principal del negocio');
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Bodega WHERE Codigo = 'BOD-SEC')
    BEGIN
        INSERT INTO dbo.Bodega (Codigo, Nombre, Ubicacion, Encargado, Descripcion)
        VALUES ('BOD-SEC', N'Bodega Secundaria', N'Sucursal centro', N'Encargado Sucursal', N'Bodega para despacho regional');
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.CategoriaProducto WHERE Nombre = N'General')
    BEGIN
        INSERT INTO dbo.CategoriaProducto (Nombre, Descripcion)
        VALUES (N'General', N'Productos generales de comercio');
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.CategoriaProducto WHERE Nombre = N'Tecnología')
    BEGIN
        INSERT INTO dbo.CategoriaProducto (Nombre, Descripcion)
        VALUES (N'Tecnología', N'Equipos y dispositivos electrónicos');
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE Codigo = 'P-0001')
    BEGIN
        DECLARE @IdCategoriaGeneral INT = (SELECT TOP(1) IdCategoria FROM dbo.CategoriaProducto WHERE Nombre = N'General');
        INSERT INTO dbo.Producto (Codigo, Nombre, Descripcion, IdCategoria, PrecioCosto, PrecioVenta, StockMinimo, StockMaximo)
        VALUES ('P-0001', N'Producto Genérico', N'Producto de ejemplo para demostración', @IdCategoriaGeneral, 10.00, 18.00, 5, 500);
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE Codigo = 'P-0002')
    BEGIN
        DECLARE @IdCategoriaTec INT = (SELECT TOP(1) IdCategoria FROM dbo.CategoriaProducto WHERE Nombre = N'Tecnología');
        INSERT INTO dbo.Producto (Codigo, Nombre, Descripcion, IdCategoria, PrecioCosto, PrecioVenta, StockMinimo, StockMaximo)
        VALUES ('P-0002', N'Dispositivo Inteligente', N'Equipo tecnológico para ventas minoristas', @IdCategoriaTec, 180.00, 250.00, 2, 120);
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE Identificacion = 'CLI-0001')
    BEGIN
        INSERT INTO dbo.Cliente (NombreCompleto, Identificacion, TipoDocumento, Correo, Telefono, Direccion)
        VALUES (N'Cliente Mostrador', 'CLI-0001', 'RNC', 'cliente@comercio.local', '809-555-1000', N'Dirección comercial principal');
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE Identificacion = 'CLI-0002')
    BEGIN
        INSERT INTO dbo.Cliente (NombreCompleto, Identificacion, TipoDocumento, Correo, Telefono, Direccion)
        VALUES (N'Compañía Corporativa', 'CLI-0002', 'RNC', 'ventas@clienteempresarial.local', '809-555-2000', N'Parque industrial');
    END;

    /*--------------------------------------------------------
      10) Inventario base (solo P-0001 y P-0002) sin CTE
    --------------------------------------------------------*/
    DECLARE @BodegaPrincipal INT = (SELECT TOP(1) IdBodega FROM dbo.Bodega WHERE Codigo = 'BOD-PRINC');
    DECLARE @BodegaSecundaria INT = (SELECT TOP(1) IdBodega FROM dbo.Bodega WHERE Codigo = 'BOD-SEC');

    -- Bodega Principal
    IF @BodegaPrincipal IS NOT NULL
    BEGIN
        DECLARE @OrigenPrincipal TABLE (IdProducto INT, IdBodega INT, Cantidad INT);

        INSERT INTO @OrigenPrincipal (IdProducto, IdBodega, Cantidad)
        SELECT p.IdProducto, @BodegaPrincipal, CASE p.Codigo WHEN 'P-0001' THEN 150 WHEN 'P-0002' THEN 25 END
        FROM dbo.Producto p
        WHERE p.Codigo IN ('P-0001','P-0002');

        -- UPDATE existentes
        UPDATE inv
           SET inv.StockActual = orig.Cantidad,
               inv.FechaActualizacion = @Ahora
          FROM dbo.Inventario inv
          JOIN @OrigenPrincipal orig
            ON orig.IdProducto = inv.IdProducto
           AND orig.IdBodega  = inv.IdBodega;

        -- INSERT faltantes
        INSERT INTO dbo.Inventario (IdProducto, IdBodega, StockActual, StockMinimo, StockMaximo)
        SELECT orig.IdProducto, orig.IdBodega, orig.Cantidad, 0, NULL
          FROM @OrigenPrincipal orig
         WHERE NOT EXISTS (
                SELECT 1
                  FROM dbo.Inventario i
                 WHERE i.IdProducto = orig.IdProducto
                   AND i.IdBodega  = orig.IdBodega
         );
    END;

    -- Bodega Secundaria
    IF @BodegaSecundaria IS NOT NULL
    BEGIN
        DECLARE @OrigenSecundaria TABLE (IdProducto INT, IdBodega INT, Cantidad INT);

        INSERT INTO @OrigenSecundaria (IdProducto, IdBodega, Cantidad)
        SELECT p.IdProducto, @BodegaSecundaria, CASE p.Codigo WHEN 'P-0001' THEN 80 WHEN 'P-0002' THEN 10 END
        FROM dbo.Producto p
        WHERE p.Codigo IN ('P-0001','P-0002');

        -- UPDATE existentes
        UPDATE inv
           SET inv.StockActual = orig.Cantidad,
               inv.FechaActualizacion = @Ahora
          FROM dbo.Inventario inv
          JOIN @OrigenSecundaria orig
            ON orig.IdProducto = inv.IdProducto
           AND orig.IdBodega  = inv.IdBodega;

        -- INSERT faltantes
        INSERT INTO dbo.Inventario (IdProducto, IdBodega, StockActual, StockMinimo, StockMaximo)
        SELECT orig.IdProducto, orig.IdBodega, orig.Cantidad, 0, NULL
          FROM @OrigenSecundaria orig
         WHERE NOT EXISTS (
                SELECT 1
                  FROM dbo.Inventario i
                 WHERE i.IdProducto = orig.IdProducto
                   AND i.IdBodega  = orig.IdBodega
         );
    END;

    COMMIT;
    PRINT 'Seed completado correctamente.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK;
    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
            @ErrNum INT = ERROR_NUMBER(),
            @ErrSev INT = ERROR_SEVERITY(),
            @ErrState INT = ERROR_STATE(),
            @ErrLine INT = ERROR_LINE();
    RAISERROR(N'Error %d (sev %d, state %d) en línea %d: %s',
              @ErrSev, @ErrState, 1, @ErrNum, @ErrSev, @ErrState, @ErrLine, @ErrMsg);
END CATCH;
