# ERP Evaluación 2

Solución mínima de WinForms (.NET 8) y SQL Server para la primera etapa del ERP.

## Pasos de instalación
1. Crear una base de datos vacía en SQL Server (por ejemplo `ERP2`).
2. Ejecutar el script [`Create.sql`](Create.sql) sobre la base de datos para generar el esquema.
3. Ejecutar el script [`Seed.sql`](Seed.sql) para cargar las pantallas base y el perfil `ADMIN`.
4. Abrir la solución `ERP 2 evaluacion.sln` en Visual Studio 2022 o posterior.
5. Ajustar la cadena de conexión `DefaultConnection` en `App.config` si es necesario.
6. Compilar y ejecutar la aplicación.

## Primer uso

Al iniciar la aplicación por primera vez, utilice la opción **"Crear super usuario"** del formulario de inicio de sesión. Solo puede existir un super usuario y se creará con acceso total a todas las pantallas, pudiendo gestionar desde allí al resto de usuarios y perfiles.

Después de crear al super usuario, podrá iniciar sesión con las credenciales definidas y continuar utilizando el menú para realizar el CRUD completo de usuarios, perfiles y accesos.
