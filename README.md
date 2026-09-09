# BecasPosgrado

Proyecto de BD III Becas de Grado mas documentacion

## Cómo correr el Frontend (Persona B - Interfaz e Integración)

Aplicación ASP.NET Core MVC (.NET 8) que consume las funciones/procedimientos PL/SQL de
Persona A (`F_REGISTRAR_SOLICITUD`, `F_ACEPTAR_SOLICITUD`, `P_CONSULTAR_OFERTAS`) mediante
Oracle.ManagedDataAccess.Core.

### Requisitos

- .NET 8 SDK
- Oracle Database XE 21c corriendo localmente (servicio `XEPDB1`), con el esquema ya creado

### 1. Preparar la base de datos

Ejecutar en Oracle SQL Developer, **en este orden**, los scripts de la carpeta `Database/`:

1. `Database/1_crear_tablas.sql`
2. `Database/2_insertar_datos.sql`
3. `Database/3_funciones_y_procedimientos.sql`
4. `Database/4_pruebas_unitarias.sql` (opcional, batería de pruebas)

### 2. Configurar la conexión

Este proyecto **no sube la contraseña real de Oracle a git**. Crea (o edita) el archivo
`BecasPosgrado/appsettings.Development.json` en tu máquina (está en `.gitignore`, no se
comitea) con tu cadena de conexión local:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "Data Source=localhost:1521/XEPDB1;User Id=SYSTEM;Password=TU_CLAVE_LOCAL;"
  }
}
```

### 3. Ejecutar

```
cd BecasPosgrado
dotnet restore
dotnet run
```

Abre en el navegador la URL que muestre la consola (por ejemplo `http://localhost:5284`).

### Usuarios de prueba (vienen en `2_insertar_datos.sql`)

| Usuario | Clave   | Rol        |
|---------|---------|------------|
| cadmin  | hash123 | ADMIN      |
| aperez  | hash123 | POSTULANTE |
| lgomez  | hash123 | POSTULANTE |
| mlopez  | hash123 | POSTULANTE |
| jdiaz   | hash123 | POSTULANTE |

### Qué está probado y funcionando

- Login por sesión con roles ADMIN / POSTULANTE
- ABM de Universidades, Sedes, Programas y Ofertas (solo ADMIN)
- Listado de ofertas activas y postulación (POSTULANTE) → llama a `F_REGISTRAR_SOLICITUD`
  y muestra el mensaje real que devuelve la función
- Listado y aceptación de solicitudes (ADMIN) → llama a `F_ACEPTAR_SOLICITUD`
- Manejo de excepciones de Oracle (`OracleException`) capturado en la capa de datos y
  mostrado al usuario como mensaje legible
