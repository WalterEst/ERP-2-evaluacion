# ERP Evaluación 2

Solución mínima de WinForms (.NET 8) y SQL Server para la primera etapa del ERP.

## Pasos de instalación
1. Crear una base de datos vacía en SQL Server (por ejemplo `ERP2`).
2. Ejecutar el script [`Create.sql`](Create.sql) sobre la base de datos para generar el esquema.
3. Ejecutar el script [`Seed.sql`](Seed.sql) para cargar las pantallas base, los roles principales y el usuario super administrador por defecto.
4. Abrir la solución `ERP 2 evaluacion.sln` en Visual Studio 2022 o posterior.
5. Ajustar la cadena de conexión `DefaultConnection` en `App.config` si es necesario.
6. Compilar y ejecutar la aplicación.

## Primer uso

Después de ejecutar los scripts de base de datos, inicia sesión con el usuario super administrador preconfigurado:

| Usuario | Contraseña |
|---------|------------|
| `admin` | `admin123` |

Ese usuario posee el rol `SUPERADMIN`, con permisos completos sobre todas las pantallas. Desde su sesión puedes crear nuevos usuarios, roles y administrar los niveles de acceso del sistema.
