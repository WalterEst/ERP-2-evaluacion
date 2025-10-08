SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.VentaDetalle', 'U') IS NOT NULL DROP TABLE dbo.VentaDetalle;
IF OBJECT_ID('dbo.Venta', 'U') IS NOT NULL DROP TABLE dbo.Venta;
IF OBJECT_ID('dbo.MovimientoInventario', 'U') IS NOT NULL DROP TABLE dbo.MovimientoInventario;
IF OBJECT_ID('dbo.Inventario', 'U') IS NOT NULL DROP TABLE dbo.Inventario;
IF OBJECT_ID('dbo.Producto', 'U') IS NOT NULL DROP TABLE dbo.Producto;
IF OBJECT_ID('dbo.CategoriaProducto', 'U') IS NOT NULL DROP TABLE dbo.CategoriaProducto;
IF OBJECT_ID('dbo.Cliente', 'U') IS NOT NULL DROP TABLE dbo.Cliente;
IF OBJECT_ID('dbo.Bodega', 'U') IS NOT NULL DROP TABLE dbo.Bodega;
IF OBJECT_ID('dbo.vw_UsuarioPermisoPantalla', 'V') IS NOT NULL DROP VIEW dbo.vw_UsuarioPermisoPantalla;
IF OBJECT_ID('dbo.TR_PerfilPantallaAcceso_Normalizar', 'TR') IS NOT NULL DROP TRIGGER dbo.TR_PerfilPantallaAcceso_Normalizar;
IF OBJECT_ID('dbo.PerfilPantallaAcceso', 'U') IS NOT NULL DROP TABLE dbo.PerfilPantallaAcceso;
IF OBJECT_ID('dbo.NivelPermiso', 'U') IS NOT NULL DROP TABLE dbo.NivelPermiso;
IF OBJECT_ID('dbo.UsuarioPerfil', 'U') IS NOT NULL DROP TABLE dbo.UsuarioPerfil;
IF OBJECT_ID('dbo.Pantalla', 'U') IS NOT NULL DROP TABLE dbo.Pantalla;
IF OBJECT_ID('dbo.Perfil', 'U') IS NOT NULL DROP TABLE dbo.Perfil;
IF OBJECT_ID('dbo.Usuario', 'U') IS NOT NULL DROP TABLE dbo.Usuario;
GO

CREATE TABLE dbo.Usuario
(
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    NombreUsuario NVARCHAR(40) NOT NULL UNIQUE,
    Correo NVARCHAR(80) NOT NULL UNIQUE,
    Clave VARCHAR(50) NOT NULL,
    NombreCompleto NVARCHAR(80) NOT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Usuario_Activo DEFAULT(1),
    UltimoIngreso DATETIME NULL,
    FechaCreacion DATE NOT NULL CONSTRAINT DF_Usuario_FechaCreacion DEFAULT (GETDATE())
);
GO

ALTER TABLE dbo.Usuario
ADD CONSTRAINT CK_Usuario_ClaveLength CHECK (LEN(Clave) >= 8);
GO

CREATE TABLE dbo.Perfil
(
    IdPerfil INT IDENTITY(1,1) PRIMARY KEY,
    NombrePerfil VARCHAR(30) NOT NULL UNIQUE,
    Codigo NVARCHAR(50) NOT NULL UNIQUE,
    Descripcion TEXT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Perfil_Activo DEFAULT(1),
    FechaCreacion DATETIME NOT NULL CONSTRAINT DF_Perfil_FechaCreacion DEFAULT (GETDATE())
);
GO

CREATE TABLE dbo.Pantalla
(
    IdPantalla INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(50) NOT NULL UNIQUE,
    NombrePantalla NVARCHAR(80) NOT NULL,
    Ruta NVARCHAR(200) NOT NULL,
    IdPadre INT NULL,
    Icono NVARCHAR(60) NULL,
    Orden INT NOT NULL CONSTRAINT DF_Pantalla_Orden DEFAULT(0),
    Activo BIT NOT NULL CONSTRAINT DF_Pantalla_Activo DEFAULT(1),
    FechaCreacion DATETIME NOT NULL CONSTRAINT DF_Pantalla_FechaCreacion DEFAULT(GETDATE()),
    CreadoPor NVARCHAR(100) NULL,
    CONSTRAINT UQ_Pantalla_Ruta UNIQUE (Ruta),
    CONSTRAINT FK_Pantalla_Padre FOREIGN KEY(IdPadre) REFERENCES dbo.Pantalla(IdPantalla) ON DELETE SET NULL
);
GO

CREATE TABLE dbo.NivelPermiso
(
    IdNivelPermiso INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(40) NOT NULL UNIQUE,
    Nombre NVARCHAR(120) NOT NULL,
    Descripcion NVARCHAR(400) NULL,
    PuedeVer BIT NOT NULL,
    PuedeCrear BIT NOT NULL,
    PuedeEditar BIT NOT NULL,
    PuedeEliminar BIT NOT NULL,
    PuedeExportar BIT NOT NULL,
    EsPersonalizado BIT NOT NULL CONSTRAINT DF_NivelPermiso_Personalizado DEFAULT(0),
    FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_NivelPermiso_FechaCreacion DEFAULT(SYSDATETIME()),
    CONSTRAINT CK_NivelPermiso_PermisosValidos
        CHECK (CASE WHEN PuedeVer = 0 THEN
                        CASE WHEN PuedeCrear = 0 AND PuedeEditar = 0 AND PuedeEliminar = 0 AND PuedeExportar = 0 THEN 1 ELSE 0 END
                    ELSE 1 END = 1)
);
GO

ALTER TABLE dbo.NivelPermiso
ADD CONSTRAINT UQ_NivelPermiso_Combinacion UNIQUE (PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar);
GO

CREATE TABLE dbo.UsuarioPerfil
(
    IdUsuarioPerfil INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL,
    IdPerfil INT NOT NULL,
    FechaAsignacion DATETIME NOT NULL CONSTRAINT DF_UsuarioPerfil_Fecha DEFAULT (GETDATE()),
    AsignadoPor NVARCHAR(100) NULL,
    CONSTRAINT UQ_UsuarioPerfil UNIQUE(IdUsuario, IdPerfil),
    CONSTRAINT FK_UsuarioPerfil_Usuario FOREIGN KEY(IdUsuario) REFERENCES dbo.Usuario(IdUsuario) ON DELETE CASCADE,
    CONSTRAINT FK_UsuarioPerfil_Perfil FOREIGN KEY(IdPerfil) REFERENCES dbo.Perfil(IdPerfil) ON DELETE CASCADE
);
GO

CREATE INDEX IX_UsuarioPerfil_Usuario
    ON dbo.UsuarioPerfil(IdUsuario);
GO

CREATE INDEX IX_UsuarioPerfil_Perfil
    ON dbo.UsuarioPerfil(IdPerfil);
GO

CREATE TABLE dbo.PerfilPantallaAcceso
(
    IdPerfilPantallaAcceso INT IDENTITY(1,1) PRIMARY KEY,
    IdPerfil INT NOT NULL,
    IdPantalla INT NOT NULL,
    IdNivelPermiso INT NULL,
    PuedeVer BIT NOT NULL CONSTRAINT DF_PPA_Ver DEFAULT(0),
    PuedeCrear BIT NOT NULL CONSTRAINT DF_PPA_Crear DEFAULT(0),
    PuedeEditar BIT NOT NULL CONSTRAINT DF_PPA_Editar DEFAULT(0),
    PuedeEliminar BIT NOT NULL CONSTRAINT DF_PPA_Eliminar DEFAULT(0),
    PuedeExportar BIT NOT NULL CONSTRAINT DF_PPA_Exportar DEFAULT(0),
    Activo BIT NOT NULL CONSTRAINT DF_PPA_Activo DEFAULT(1),
    FechaOtorgado DATETIME2 NOT NULL CONSTRAINT DF_PPA_Fecha DEFAULT (SYSDATETIME()),
    OtorgadoPor NVARCHAR(40) NULL,
    CONSTRAINT UQ_PerfilPantalla UNIQUE(IdPerfil, IdPantalla),
    CONSTRAINT FK_PPA_Perfil FOREIGN KEY(IdPerfil) REFERENCES dbo.Perfil(IdPerfil) ON DELETE CASCADE,
    CONSTRAINT FK_PPA_Pantalla FOREIGN KEY(IdPantalla) REFERENCES dbo.Pantalla(IdPantalla) ON DELETE CASCADE,
    CONSTRAINT FK_PPA_Nivel FOREIGN KEY(IdNivelPermiso) REFERENCES dbo.NivelPermiso(IdNivelPermiso),
    CONSTRAINT CK_PPA_PermisosCoherentes
        CHECK (CASE WHEN PuedeVer = 0 THEN
                        CASE WHEN PuedeCrear = 0 AND PuedeEditar = 0 AND PuedeEliminar = 0 AND PuedeExportar = 0 AND Activo = 0
                             THEN 1 ELSE 0 END
                    ELSE 1 END = 1)
);
GO

ALTER TABLE dbo.PerfilPantallaAcceso
ADD CodigoNivelCalculado AS (
    CASE
        WHEN PuedeVer = 0 THEN 'SIN_ACCESO'
        WHEN PuedeVer = 1 AND PuedeCrear = 0 AND PuedeEditar = 0 AND PuedeEliminar = 0 AND PuedeExportar = 0 THEN 'LECTURA'
        WHEN PuedeVer = 1 AND PuedeCrear = 1 AND PuedeEditar = 1 AND PuedeEliminar = 1 AND PuedeExportar = 1 THEN 'ADMINISTRACION'
        WHEN PuedeVer = 1 AND PuedeCrear = 1 AND PuedeEditar = 1 AND PuedeEliminar = 0 AND PuedeExportar = 1 THEN 'COLABORACION'
        WHEN PuedeVer = 1 AND PuedeCrear = 1 AND PuedeEditar = 0 AND PuedeEliminar = 0 AND PuedeExportar = 1 THEN 'OPERACION'
        ELSE 'PERSONALIZADO'
    END
) PERSISTED;
GO

CREATE INDEX IX_PerfilPantallaAcceso_PerfilPantalla
    ON dbo.PerfilPantallaAcceso(IdPerfil, IdPantalla)
    INCLUDE (PuedeVer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, Activo);
GO

CREATE INDEX IX_PerfilPantallaAcceso_Pantalla
    ON dbo.PerfilPantallaAcceso(IdPantalla)
    INCLUDE (IdPerfil, PuedeVer, Activo);
GO

CREATE TABLE dbo.Bodega
(
    IdBodega INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(30) NOT NULL UNIQUE,
    Nombre NVARCHAR(120) NOT NULL,
    Ubicacion NVARCHAR(160) NULL,
    Encargado NVARCHAR(120) NULL,
    Descripcion NVARCHAR(300) NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Bodega_Activo DEFAULT(1),
    FechaCreacion DATETIME NOT NULL CONSTRAINT DF_Bodega_FechaCreacion DEFAULT(GETDATE())
);
GO

CREATE TABLE dbo.CategoriaProducto
(
    IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(120) NOT NULL UNIQUE,
    Descripcion NVARCHAR(300) NULL,
    Activo BIT NOT NULL CONSTRAINT DF_CategoriaProducto_Activo DEFAULT(1),
    FechaCreacion DATETIME NOT NULL CONSTRAINT DF_CategoriaProducto_FechaCreacion DEFAULT(GETDATE())
);
GO

CREATE TABLE dbo.Producto
(
    IdProducto INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(50) NOT NULL UNIQUE,
    Nombre NVARCHAR(160) NOT NULL,
    Descripcion NVARCHAR(400) NULL,
    IdCategoria INT NULL,
    PrecioCosto DECIMAL(18,2) NOT NULL,
    PrecioVenta DECIMAL(18,2) NOT NULL,
    StockMinimo DECIMAL(18,2) NOT NULL CONSTRAINT DF_Producto_StockMinimo DEFAULT(0),
    StockMaximo DECIMAL(18,2) NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Producto_Activo DEFAULT(1),
    FechaCreacion DATETIME NOT NULL CONSTRAINT DF_Producto_FechaCreacion DEFAULT(GETDATE()),
    CONSTRAINT FK_Producto_Categoria FOREIGN KEY(IdCategoria) REFERENCES dbo.CategoriaProducto(IdCategoria)
);
GO

CREATE TABLE dbo.Cliente
(
    IdCliente INT IDENTITY(1,1) PRIMARY KEY,
    NombreCompleto NVARCHAR(160) NOT NULL,
    Identificacion NVARCHAR(40) NULL,
    TipoDocumento NVARCHAR(30) NULL,
    Correo NVARCHAR(120) NULL,
    Telefono NVARCHAR(40) NULL,
    Direccion NVARCHAR(300) NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Cliente_Activo DEFAULT(1),
    FechaCreacion DATETIME NOT NULL CONSTRAINT DF_Cliente_FechaCreacion DEFAULT(GETDATE())
);
GO

CREATE TABLE dbo.Inventario
(
    IdInventario INT IDENTITY(1,1) PRIMARY KEY,
    IdProducto INT NOT NULL,
    IdBodega INT NOT NULL,
    StockActual DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inventario_StockActual DEFAULT(0),
    StockReservado DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inventario_StockReservado DEFAULT(0),
    StockMinimo DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inventario_StockMinimo DEFAULT(0),
    StockMaximo DECIMAL(18,2) NULL,
    FechaActualizacion DATETIME NOT NULL CONSTRAINT DF_Inventario_FechaActualizacion DEFAULT(GETDATE()),
    CONSTRAINT UQ_Inventario_ProductoBodega UNIQUE(IdProducto, IdBodega),
    CONSTRAINT FK_Inventario_Producto FOREIGN KEY(IdProducto) REFERENCES dbo.Producto(IdProducto),
    CONSTRAINT FK_Inventario_Bodega FOREIGN KEY(IdBodega) REFERENCES dbo.Bodega(IdBodega)
);
GO

CREATE TABLE dbo.MovimientoInventario
(
    IdMovimiento INT IDENTITY(1,1) PRIMARY KEY,
    IdInventario INT NOT NULL,
    TipoMovimiento NVARCHAR(30) NOT NULL,
    Cantidad DECIMAL(18,2) NOT NULL,
    FechaMovimiento DATETIME NOT NULL CONSTRAINT DF_MovimientoInventario_Fecha DEFAULT(GETDATE()),
    Motivo NVARCHAR(300) NULL,
    Referencia NVARCHAR(80) NULL,
    IdUsuario INT NULL,
    CONSTRAINT FK_MovimientoInventario_Inventario FOREIGN KEY(IdInventario) REFERENCES dbo.Inventario(IdInventario),
    CONSTRAINT FK_MovimientoInventario_Usuario FOREIGN KEY(IdUsuario) REFERENCES dbo.Usuario(IdUsuario)
);
GO

CREATE TABLE dbo.Venta
(
    IdVenta INT IDENTITY(1,1) PRIMARY KEY,
    Numero NVARCHAR(40) NOT NULL UNIQUE,
    Fecha DATETIME NOT NULL CONSTRAINT DF_Venta_Fecha DEFAULT(GETDATE()),
    IdUsuario INT NOT NULL,
    IdCliente INT NULL,
    IdBodega INT NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    Impuestos DECIMAL(18,2) NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    Observaciones NVARCHAR(400) NULL,
    Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_Venta_Estado DEFAULT('COMPLETADA'),
    CONSTRAINT FK_Venta_Usuario FOREIGN KEY(IdUsuario) REFERENCES dbo.Usuario(IdUsuario),
    CONSTRAINT FK_Venta_Cliente FOREIGN KEY(IdCliente) REFERENCES dbo.Cliente(IdCliente),
    CONSTRAINT FK_Venta_Bodega FOREIGN KEY(IdBodega) REFERENCES dbo.Bodega(IdBodega)
);
GO

CREATE TABLE dbo.VentaDetalle
(
    IdVentaDetalle INT IDENTITY(1,1) PRIMARY KEY,
    IdVenta INT NOT NULL,
    IdProducto INT NOT NULL,
    Cantidad DECIMAL(18,2) NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Descuento DECIMAL(18,2) NOT NULL CONSTRAINT DF_VentaDetalle_Descuento DEFAULT(0),
    Total DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_VentaDetalle_Venta FOREIGN KEY(IdVenta) REFERENCES dbo.Venta(IdVenta) ON DELETE CASCADE,
    CONSTRAINT FK_VentaDetalle_Producto FOREIGN KEY(IdProducto) REFERENCES dbo.Producto(IdProducto)
);
GO

CREATE VIEW dbo.vw_UsuarioPermisoPantalla
AS
SELECT
    up.IdUsuario,
    pa.IdPantalla,
    CONVERT(BIT, MAX(CAST(pa.PuedeVer AS TINYINT))) AS PuedeVer,
    CONVERT(BIT, MAX(CAST(pa.PuedeCrear AS TINYINT))) AS PuedeCrear,
    CONVERT(BIT, MAX(CAST(pa.PuedeEditar AS TINYINT))) AS PuedeEditar,
    CONVERT(BIT, MAX(CAST(pa.PuedeEliminar AS TINYINT))) AS PuedeEliminar,
    CONVERT(BIT, MAX(CAST(pa.PuedeExportar AS TINYINT))) AS PuedeExportar,
    CONVERT(BIT, MAX(CAST(pa.Activo AS TINYINT))) AS Activo,
    MAX(pa.IdNivelPermiso) AS IdNivelPermiso,
    MAX(pa.CodigoNivelCalculado) AS CodigoNivelCalculado,
    MAX(pa.FechaOtorgado) AS UltimaActualizacion,
    MAX(pa.OtorgadoPor) AS OtorgadoPor
FROM dbo.UsuarioPerfil up
JOIN dbo.PerfilPantallaAcceso pa
    ON pa.IdPerfil = up.IdPerfil
WHERE pa.Activo = 1
GROUP BY up.IdUsuario, pa.IdPantalla;
GO

CREATE OR ALTER TRIGGER dbo.TR_PerfilPantallaAcceso_Normalizar
ON dbo.PerfilPantallaAcceso
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET Activo = CASE WHEN p.PuedeVer = 0 THEN 0 ELSE p.Activo END,
        IdNivelPermiso = n.IdNivelPermiso
    FROM dbo.PerfilPantallaAcceso p
    INNER JOIN inserted i ON i.IdPerfilPantallaAcceso = p.IdPerfilPantallaAcceso
    OUTER APPLY (
        SELECT TOP(1) np.IdNivelPermiso
        FROM dbo.NivelPermiso np
        WHERE np.PuedeVer = p.PuedeVer
          AND np.PuedeCrear = p.PuedeCrear
          AND np.PuedeEditar = p.PuedeEditar
          AND np.PuedeEliminar = p.PuedeEliminar
          AND np.PuedeExportar = p.PuedeExportar
        ORDER BY np.EsPersonalizado, np.IdNivelPermiso
    ) n
    WHERE (p.PuedeVer = 0 AND p.Activo <> 0)
       OR (p.PuedeVer = 1 AND (
                (p.IdNivelPermiso IS NULL AND n.IdNivelPermiso IS NOT NULL)
             OR (p.IdNivelPermiso IS NOT NULL AND n.IdNivelPermiso IS NULL)
             OR (p.IdNivelPermiso <> n.IdNivelPermiso)
           ));
END;
GO
