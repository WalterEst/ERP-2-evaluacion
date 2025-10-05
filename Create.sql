SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.PerfilPantallaAcceso', 'U') IS NOT NULL DROP TABLE dbo.PerfilPantallaAcceso;
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
    CONSTRAINT FK_Pantalla_Padre FOREIGN KEY(IdPadre) REFERENCES dbo.Pantalla(IdPantalla)
);
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

CREATE TABLE dbo.PerfilPantallaAcceso
(
    IdPerfilPantallaAcceso INT IDENTITY(1,1) PRIMARY KEY,
    IdPerfil INT NOT NULL,
    IdPantalla INT NOT NULL,
    PuedeVer BIT NOT NULL CONSTRAINT DF_PPA_Ver DEFAULT(0),
    PuedeCrear BIT NOT NULL CONSTRAINT DF_PPA_Crear DEFAULT(0),
    PuedeEditar BIT NOT NULL CONSTRAINT DF_PPA_Editar DEFAULT(0),
    PuedeEliminar BIT NOT NULL CONSTRAINT DF_PPA_Eliminar DEFAULT(0),
    PuedeExportar BIT NOT NULL CONSTRAINT DF_PPA_Exportar DEFAULT(0),
    Activo BIT NOT NULL CONSTRAINT DF_PPA_Activo DEFAULT(1),
    FechaOtorgado DATETIME NOT NULL CONSTRAINT DF_PPA_Fecha DEFAULT (GETDATE()),
    OtorgadoPor NVARCHAR(40) NULL,
    CONSTRAINT UQ_PerfilPantalla UNIQUE(IdPerfil, IdPantalla),
    CONSTRAINT FK_PPA_Perfil FOREIGN KEY(IdPerfil) REFERENCES dbo.Perfil(IdPerfil) ON DELETE CASCADE,
    CONSTRAINT FK_PPA_Pantalla FOREIGN KEY(IdPantalla) REFERENCES dbo.Pantalla(IdPantalla) ON DELETE CASCADE
);
GO
