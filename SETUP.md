# BecasPosgrado — Guía rápida (Persona B)

Este proyecto ya viene con: Models, la capa de acceso a datos (`Data/OracleDbContext.cs`)
conectada a las funciones reales de Persona A (`F_REGISTRAR_SOLICITUD`, `F_ACEPTAR_SOLICITUD`,
`P_CONSULTAR_OFERTAS`), login por Session con roles ADMIN/POSTULANTE, el ABM de
Universidades/Sedes/Programas/Ofertas, el flujo de postulación y la gestión de solicitudes.

## 0. Instalar Oracle Database XE + SQL Developer (si todavía no los tienes)

En este equipo no se detectó SQL Developer ni Oracle instalados todavía. Antes de poder
ejecutar los scripts necesitas:

1. Instalar **Oracle Database XE** (motor de base de datos). Al final del instalador te
   pide poner la contraseña de `SYS`/`SYSTEM`/`PDBADMIN` — usa `20070405` (la misma que
   ya está puesta en `appsettings.json`) para que todo quede consistente.
2. Instalar **Oracle SQL Developer** (el IDE gráfico) y crear una conexión nueva:
   - Username: `SYSTEM`
   - Password: `20070405`
   - Hostname: `localhost`
   - Port: `1521`
   - SID: `XE`

(Los links e instrucciones detalladas de instalación están en el mensaje original que te
pasaron; si quieres los repito paso a paso.)

## 1. Preparar la base de datos local

Ya tienes los 4 scripts de Persona A copiados dentro de este proyecto, en la carpeta
`Database/`, para que no tengas que ir a buscarlos a Teams/OneDrive cada vez. Ábrelos en
SQL Developer conectado a tu Oracle XE local y ejecútalos EN ESTE ORDEN (F5 o el botón
"Run Script"):

1. `Database/1_crear_tablas.sql`
2. `Database/2_insertar_datos.sql`
3. `Database/3_funciones_y_procedimientos.sql`
4. `Database/4_pruebas_unitarias.sql` — esta es la batería de pruebas que te pasó tu
   compañero por Teams; ejecútala al final para comprobar que las funciones responden
   bien (revisa la pestaña "Script Output" / DBMS Output en SQL Developer).

## 2. Cadena de conexión

`appsettings.json` ya tiene tu contraseña (`20070405`) puesta:

```json
"ConnectionStrings": {
  "OracleConnection": "Data Source=localhost:1521/XE;User Id=SYSTEM;Password=20070405;"
}
```

**Ojo con subir esto a git tal cual.** Si vas a comitear `appsettings.json` (como pide el
plan de Persona B), considera dejar un placeholder ahí antes del `git add`, o mover la
cadena real a `appsettings.Development.json` (que normalmente sí se ignora) y dejar un
placeholder en el que sí se sube.

## 3. Restaurar y correr

Desde la carpeta del proyecto (donde está `BecasPosgrado.csproj`):

```
dotnet restore
dotnet build
dotnet run
```

La primera vez, `dotnet restore` va a descargar `Oracle.ManagedDataAccess.Core` desde
NuGet (necesitas internet). Luego abre la URL que te muestre la consola (por defecto
algo como `http://localhost:5000`).

## 4. Usuarios de prueba (vienen en insertar_datos.sql)

| Usuario  | Clave   | Rol        |
|----------|---------|------------|
| cadmin   | hash123 | ADMIN      |
| aperez   | hash123 | POSTULANTE |
| lgomez   | hash123 | POSTULANTE |

## 5. Subir a Git (repo ya existente con solo el README)

Como el repositorio de Persona A ya existe en GitHub con un commit inicial (README),
conecta esta carpeta a él así:

```
cd "Desktop\BD becas de posgrado\BecasPosgrado"
git init
git remote add origin https://github.com/Mollo-Systems800/BecasPosgrado.git
git fetch origin
git merge origin/main --allow-unrelated-histories -m "Merge inicial con README de Persona A"

git add .
git commit -m "Fase 0-2: proyecto .NET, Models, DAL con integracion real a PL/SQL, UI y controladores"
git push origin main
```

De ahí en adelante, antes de trabajar cada día:

```
git pull origin main
```

Y para futuros cambios (siguiendo las fases del plan original):

```
git add Data/ Controllers/
git commit -m "Fase 3: integracion completa con funciones PL/SQL y manejo de excepciones"
git push origin main
```

## 6. Qué falta / próximos pasos

- Probar el flujo completo con Oracle real: login, listar ofertas, postularse (revisa que
  el mensaje que devuelve `F_REGISTRAR_SOLICITUD` aparezca en la alerta superior),
  aceptar solicitudes como ADMIN.
- Las contraseñas en `insertar_datos.sql` están en texto plano (`hash123`) — está bien
  para el alcance académico de esta práctica, pero si tu compañero decide hashearlas más
  adelante, el login (`ObtenerPostulantePorUsuario`) habría que actualizarlo para comparar
  contra el hash en vez de texto plano.
- La tabla `COMPONENTE` ya tiene su modelo (`Models/Componente.cs`) por si más adelante
  se pide una pantalla de ABM para ella; no tiene controlador/vistas todavía.
