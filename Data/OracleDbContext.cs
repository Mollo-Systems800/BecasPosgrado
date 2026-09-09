using System.Data;
using BecasPosgrado.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace BecasPosgrado.Data
{
    /// <summary>
    /// Capa de acceso a datos hecha con ADO.NET puro (Oracle.ManagedDataAccess.Core).
    /// Todas las excepciones de Oracle (OracleException) se dejan burbujear hacia
    /// el controlador, que es quien las captura y muestra el mensaje al usuario.
    /// </summary>
    public class OracleDbContext
    {
        private readonly string _connectionString;

        public OracleDbContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleConnection")
                ?? throw new InvalidOperationException("Falta la cadena de conexión 'OracleConnection' en appsettings.json");
        }

        private OracleConnection GetConnection() => new OracleConnection(_connectionString);

        // =====================================================================
        // LOGIN
        // =====================================================================

        // Consulta SELECT directa (no usa función PL/SQL) para autenticar al postulante.
        public Postulante? ObtenerPostulantePorUsuario(string usuario, string clave)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, nombre, apellido, email, telefono, direccion, fecha_nac, usuario, clave, rol
                FROM POSTULANTE
                WHERE usuario = :usuario AND clave = :clave";
            cmd.Parameters.Add(new OracleParameter("usuario", OracleDbType.Varchar2) { Value = usuario });
            cmd.Parameters.Add(new OracleParameter("clave", OracleDbType.Varchar2) { Value = clave });

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapPostulante(reader) : null;
        }

        private static Postulante MapPostulante(IDataRecord r) => new Postulante
        {
            Id = Convert.ToInt32(r["id"]),
            Nombre = r["nombre"].ToString()!,
            Apellido = r["apellido"].ToString()!,
            Email = r["email"].ToString()!,
            Telefono = r["telefono"] is DBNull ? null : r["telefono"].ToString(),
            Direccion = r["direccion"] is DBNull ? null : r["direccion"].ToString(),
            FechaNac = Convert.ToDateTime(r["fecha_nac"]),
            Usuario = r["usuario"].ToString()!,
            Clave = r["clave"].ToString()!,
            Rol = r["rol"].ToString()!
        };

        // =====================================================================
        // OFERTAS - listado directo (para la vista Ofertas/Index del postulante)
        // =====================================================================

        public List<Oferta> ListarOfertasActivas()
        {
            var lista = new List<Oferta>();
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT o.id, o.id_programa, o.id_sede, o.fecha_inicio, o.fecha_fin, o.cupos_disponibles, o.estado,
                       p.nombre AS programa, p.area, u.nombre AS universidad, s.nombre AS sede
                FROM OFERTA o
                JOIN PROGRAMA p ON o.id_programa = p.id
                JOIN SEDE s ON o.id_sede = s.id
                JOIN UNIVERSIDAD u ON s.id_universidad = u.id
                WHERE SYSDATE BETWEEN o.fecha_inicio AND o.fecha_fin
                  AND o.estado = 'Activa'
                ORDER BY o.fecha_inicio DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapOfertaJoin(reader));
            }
            return lista;
        }

        private static Oferta MapOfertaJoin(IDataRecord r) => new Oferta
        {
            Id = Convert.ToInt32(r["id"]),
            IdPrograma = Convert.ToInt32(r["id_programa"]),
            IdSede = Convert.ToInt32(r["id_sede"]),
            FechaInicio = Convert.ToDateTime(r["fecha_inicio"]),
            FechaFin = Convert.ToDateTime(r["fecha_fin"]),
            CuposDisponibles = Convert.ToInt32(r["cupos_disponibles"]),
            Estado = r["estado"].ToString()!,
            ProgramaNombre = r["programa"].ToString(),
            Area = r["area"].ToString(),
            UniversidadNombre = r["universidad"].ToString(),
            SedeNombre = r["sede"].ToString()
        };

        // =====================================================================
        // FUNCIONES / PROCEDIMIENTOS PL/SQL (Persona A)
        // =====================================================================

        // Llama F_REGISTRAR_SOLICITUD(p_id_postulante, p_id_oferta, p_resumen) RETURN VARCHAR2
        public string RegistrarSolicitud(int idPostulante, int idOferta, string resumen)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "F_REGISTRAR_SOLICITUD";
            cmd.BindByName = true; // los parámetros se enlazan por nombre (p_id_postulante, etc.), no por posición

            var retorno = new OracleParameter("retorno", OracleDbType.Varchar2, 500)
            {
                Direction = ParameterDirection.ReturnValue
            };
            cmd.Parameters.Add(retorno);
            cmd.Parameters.Add(new OracleParameter("p_id_postulante", OracleDbType.Int32) { Value = idPostulante });
            cmd.Parameters.Add(new OracleParameter("p_id_oferta", OracleDbType.Int32) { Value = idOferta });
            cmd.Parameters.Add(new OracleParameter("p_resumen", OracleDbType.Clob) { Value = resumen });

            cmd.ExecuteNonQuery();

            var valor = (OracleString)retorno.Value;
            return valor.IsNull ? string.Empty : valor.Value;
        }

        // Llama F_ACEPTAR_SOLICITUD(p_id_solicitud) RETURN VARCHAR2
        public string AceptarSolicitud(int idSolicitud)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "F_ACEPTAR_SOLICITUD";
            cmd.BindByName = true;

            var retorno = new OracleParameter("retorno", OracleDbType.Varchar2, 500)
            {
                Direction = ParameterDirection.ReturnValue
            };
            cmd.Parameters.Add(retorno);
            cmd.Parameters.Add(new OracleParameter("p_id_solicitud", OracleDbType.Int32) { Value = idSolicitud });

            cmd.ExecuteNonQuery();

            var valor = (OracleString)retorno.Value;
            return valor.IsNull ? string.Empty : valor.Value;
        }

        // Llama P_CONSULTAR_OFERTAS(p_area, p_id_universidad, p_cursor OUT SYS_REFCURSOR)
        public List<Oferta> ConsultarOfertasConFiltro(string? area, int? idUniversidad)
        {
            var lista = new List<Oferta>();
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "P_CONSULTAR_OFERTAS";
            cmd.BindByName = true;

            cmd.Parameters.Add(new OracleParameter("p_area", OracleDbType.Varchar2)
            {
                Value = string.IsNullOrWhiteSpace(area) ? DBNull.Value : area
            });
            cmd.Parameters.Add(new OracleParameter("p_id_universidad", OracleDbType.Int32)
            {
                Value = idUniversidad.HasValue ? idUniversidad.Value : DBNull.Value
            });

            var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(cursorParam);

            cmd.ExecuteNonQuery();

            using var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader();
            while (reader.Read())
            {
                lista.Add(new Oferta
                {
                    Id = Convert.ToInt32(reader["id_oferta"]),
                    ProgramaNombre = reader["programa"].ToString(),
                    Area = reader["area"].ToString(),
                    UniversidadNombre = reader["universidad"].ToString(),
                    SedeNombre = reader["sede"].ToString(),
                    FechaInicio = Convert.ToDateTime(reader["fecha_inicio"]),
                    FechaFin = Convert.ToDateTime(reader["fecha_fin"]),
                    CuposDisponibles = Convert.ToInt32(reader["cupos_disponibles"])
                });
            }
            return lista;
        }

        // =====================================================================
        // ABM UNIVERSIDADES (ADO.NET puro)
        // =====================================================================

        public List<Universidad> ListarUniversidades()
        {
            var lista = new List<Universidad>();
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, nombre, pais, ciudad, direccion, telefono FROM UNIVERSIDAD ORDER BY nombre";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Universidad
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Nombre = reader["nombre"].ToString()!,
                    Pais = reader["pais"].ToString()!,
                    Ciudad = reader["ciudad"].ToString()!,
                    Direccion = reader["direccion"] is DBNull ? null : reader["direccion"].ToString(),
                    Telefono = reader["telefono"] is DBNull ? null : reader["telefono"].ToString()
                });
            }
            return lista;
        }

        public Universidad? ObtenerUniversidad(int id)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, nombre, pais, ciudad, direccion, telefono FROM UNIVERSIDAD WHERE id = :id";
            cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = id });
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new Universidad
            {
                Id = Convert.ToInt32(reader["id"]),
                Nombre = reader["nombre"].ToString()!,
                Pais = reader["pais"].ToString()!,
                Ciudad = reader["ciudad"].ToString()!,
                Direccion = reader["direccion"] is DBNull ? null : reader["direccion"].ToString(),
                Telefono = reader["telefono"] is DBNull ? null : reader["telefono"].ToString()
            };
        }

        public void CrearUniversidad(Universidad u)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO UNIVERSIDAD (nombre, pais, ciudad, direccion, telefono)
                                 VALUES (:nombre, :pais, :ciudad, :direccion, :telefono)";
            cmd.Parameters.Add(new OracleParameter("nombre", OracleDbType.Varchar2) { Value = u.Nombre });
            cmd.Parameters.Add(new OracleParameter("pais", OracleDbType.Varchar2) { Value = u.Pais });
            cmd.Parameters.Add(new OracleParameter("ciudad", OracleDbType.Varchar2) { Value = u.Ciudad });
            cmd.Parameters.Add(new OracleParameter("direccion", OracleDbType.Varchar2) { Value = (object?)u.Direccion ?? DBNull.Value });
            cmd.Parameters.Add(new OracleParameter("telefono", OracleDbType.Varchar2) { Value = (object?)u.Telefono ?? DBNull.Value });
            cmd.ExecuteNonQuery();
        }

        public void ActualizarUniversidad(Universidad u)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE UNIVERSIDAD SET nombre = :nombre, pais = :pais, ciudad = :ciudad,
                                 direccion = :direccion, telefono = :telefono WHERE id = :id";
            cmd.Parameters.Add(new OracleParameter("nombre", OracleDbType.Varchar2) { Value = u.Nombre });
            cmd.Parameters.Add(new OracleParameter("pais", OracleDbType.Varchar2) { Value = u.Pais });
            cmd.Parameters.Add(new OracleParameter("ciudad", OracleDbType.Varchar2) { Value = u.Ciudad });
            cmd.Parameters.Add(new OracleParameter("direccion", OracleDbType.Varchar2) { Value = (object?)u.Direccion ?? DBNull.Value });
            cmd.Parameters.Add(new OracleParameter("telefono", OracleDbType.Varchar2) { Value = (object?)u.Telefono ?? DBNull.Value });
            cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = u.Id });
            cmd.ExecuteNonQuery();
        }

        public void EliminarUniversidad(int id)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM UNIVERSIDAD WHERE id = :id";
            cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = id });
            cmd.ExecuteNonQuery();
        }

        // =====================================================================
        // ABM SEDES
        // =====================================================================

        public List<Sede> ListarSedes()
        {
            var lista = new List<Sede>();
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT s.id, s.nombre, s.direccion, s.id_universidad, u.nombre AS universidad
                FROM SEDE s JOIN UNIVERSIDAD u ON s.id_universidad = u.id
                ORDER BY u.nombre, s.nombre";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Sede
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Nombre = reader["nombre"].ToString()!,
                    Direccion = reader["direccion"] is DBNull ? null : reader["direccion"].ToString(),
                    IdUniversidad = Convert.ToInt32(reader["id_universidad"]),
                    UniversidadNombre = reader["universidad"].ToString()
                });
            }
            return lista;
        }

        public Sede? ObtenerSede(int id)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, nombre, direccion, id_universidad FROM SEDE WHERE id = :id";
            cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = id });
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new Sede
            {
                Id = Convert.ToInt32(reader["id"]),
                Nombre = reader["nombre"].ToString()!,
                Direccion = reader["direccion"] is DBNull ? null : reader["direccion"].ToString(),
                IdUniversidad = Convert.ToInt32(reader["id_universidad"])
            };
        }

        public void CrearSede(Sede s)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO SEDE (nombre, direccion, id_universidad)
                                 VALUES (:nombre, :direccion, :idUniversidad)";
            cmd.Parameters.Add(new OracleParameter("nombre", OracleDbType.Varchar2) { Value = s.Nombre });
            cmd.Parameters.Add(new OracleParameter("direccion", OracleDbType.Varchar2) { Value = (object?)s.Direccion ?? DBNull.Value });
            cmd.Parameters.Add(new OracleParameter("idUniversidad", OracleDbType.Int32) { Value = s.IdUniversidad });
            cmd.ExecuteNonQuery();
        }

        public void ActualizarSede(Sede s)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE SEDE SET nombre = :nombre, direccion = :direccion, id_universidad = :idUniversidad
                                 WHERE id = :id";
            cmd.Parameters.Add(new OracleParameter("nombre", OracleDbType.Varchar2) { Value = s.Nombre });
            cmd.Parameters.Add(new OracleParameter("direccion", OracleDbType.Varchar2) { Value = (object?)s.Direccion ?? DBNull.Value });
            cmd.Parameters.Add(new OracleParameter("idUniversidad", OracleDbType.Int32) { Value = s.IdUniversidad });
            cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = s.Id });
            cmd.ExecuteNonQuery();
        }

        public void EliminarSede(int id)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM SEDE WHERE id = :id";
            cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = id });
            cmd.ExecuteNonQuery();
        }

        // =====================================================================
        // ABM PROGRAMAS
        // =====================================================================

        public List<Programa> ListarProgramas()
        {
            var lista = new List<Programa>();
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, nombre, descripcion, area, tipo_financiamiento FROM PROGRAMA ORDER BY nombre";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Programa
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Nombre = reader["nombre"].ToString()!,
                    Descripcion = reader["descripcion"] is DBNull ? null : reader["descripcion"].ToString(),
                    Area = reader["area"].ToString()!,
                    TipoFinanciamiento = reader["tipo_financiamiento"] is DBNull ? null : reader["tipo_financiamiento"].ToString()
                });
            }
            return lista;
        }

        public Programa? ObtenerPrograma(int id)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, nombre, descripcion, area, tipo_financiamiento FROM PROGRAMA WHERE id = :id";
            cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = id });
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new Programa
            {
                Id = Convert.ToInt32(reader["id"]),
                Nombre = reader["nombre"].ToString()!,
                Descripcion = reader["descripcion"] is DBNull ? null : reader["descripcion"].ToString(),
                Area = reader["area"].ToString()!,
                TipoFinanciamiento = reader["tipo_financiamiento"] is DBNull ? null : reader["tipo_financiamiento"].ToString()
            };
        }

        public void CrearPrograma(Programa p)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO PROGRAMA (nombre, descripcion, area, tipo_financiamiento)
                                 VALUES (:nombre, :descripcion, :area, :tipoFinanciamiento)";
            cmd.Parameters.Add(new OracleParameter("nombre", OracleDbType.Varchar2) { Value = p.Nombre });
            cmd.Parameters.Add(new OracleParameter("descripcion", OracleDbType.Varchar2) { Value = (object?)p.Descripcion ?? DBNull.Value });
            cmd.Parameters.Add(new OracleParameter("area", OracleDbType.Varchar2) { Value = p.Area });
            cmd.Parameters.Add(new OracleParameter("tipoFinanciamiento", OracleDbType.Varchar2) { Value = (object?)p.TipoFinanciamiento ?? DBNull.Value });
            cmd.ExecuteNonQuery();
        }

        public void ActualizarPrograma(Programa p)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE PROGRAMA SET nombre = :nombre, descripcion = :descripcion, area = :area,
                                 tipo_financiamiento = :tipoFinanciamiento WHERE id = :id";
            cmd.Parameters.Add(new OracleParameter("nombre", OracleDbType.Varchar2) { Value = p.Nombre });
            cmd.Parameters.Add(new OracleParameter("descripcion", OracleDbType.Varchar2) { Value = (object?)p.Descripcion ?? DBNull.Value });
            cmd.Parameters.Add(new OracleParameter("area", OracleDbType.Varchar2) { Value = p.Area });
            cmd.Parameters.Add(new OracleParameter("tipoFinanciamiento", OracleDbType.Varchar2) { Value = (object?)p.TipoFinanciamiento ?? DBNull.Value });
            cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = p.Id });
            cmd.ExecuteNonQuery();
        }

        public void EliminarPrograma(int id)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM PROGRAMA WHERE id = :id";
            cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = id });
            cmd.ExecuteNonQuery();
        }

        // =====================================================================
        // ABM OFERTAS (administración; distinto del listado activo para postulantes)
        // =====================================================================

        public List<Oferta> ListarTodasOfertas()
        {
            var lista = new List<Oferta>();
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT o.id, o.id_programa, o.id_sede, o.fecha_inicio, o.fecha_fin, o.cupos_disponibles, o.estado,
                       p.nombre AS programa, p.area, u.nombre AS universidad, s.nombre AS sede
                FROM OFERTA o
                JOIN PROGRAMA p ON o.id_programa = p.id
                JOIN SEDE s ON o.id_sede = s.id
                JOIN UNIVERSIDAD u ON s.id_universidad = u.id
                ORDER BY o.fecha_inicio DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapOfertaJoin(reader));
            }
            return lista;
        }

        public Oferta? ObtenerOferta(int id)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id, id_programa, id_sede, fecha_inicio, fecha_fin, cupos_disponibles, estado
                                 FROM OFERTA WHERE id = :id";
            cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = id });
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new Oferta
            {
                Id = Convert.ToInt32(reader["id"]),
                IdPrograma = Convert.ToInt32(reader["id_programa"]),
                IdSede = Convert.ToInt32(reader["id_sede"]),
                FechaInicio = Convert.ToDateTime(reader["fecha_inicio"]),
                FechaFin = Convert.ToDateTime(reader["fecha_fin"]),
                CuposDisponibles = Convert.ToInt32(reader["cupos_disponibles"]),
                Estado = reader["estado"].ToString()!
            };
        }

        public void CrearOferta(Oferta o)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO OFERTA (id_programa, id_sede, fecha_inicio, fecha_fin, cupos_disponibles, estado)
                                 VALUES (:idPrograma, :idSede, :fechaInicio, :fechaFin, :cupos, :estado)";
            cmd.Parameters.Add(new OracleParameter("idPrograma", OracleDbType.Int32) { Value = o.IdPrograma });
            cmd.Parameters.Add(new OracleParameter("idSede", OracleDbType.Int32) { Value = o.IdSede });
            cmd.Parameters.Add(new OracleParameter("fechaInicio", OracleDbType.Date) { Value = o.FechaInicio });
            cmd.Parameters.Add(new OracleParameter("fechaFin", OracleDbType.Date) { Value = o.FechaFin });
            cmd.Parameters.Add(new OracleParameter("cupos", OracleDbType.Int32) { Value = o.CuposDisponibles });
            cmd.Parameters.Add(new OracleParameter("estado", OracleDbType.Varchar2) { Value = o.Estado });
            cmd.ExecuteNonQuery();
        }

        public void ActualizarOferta(Oferta o)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE OFERTA SET id_programa = :idPrograma, id_sede = :idSede,
                                 fecha_inicio = :fechaInicio, fecha_fin = :fechaFin,
                                 cupos_disponibles = :cupos, estado = :estado WHERE id = :id";
            cmd.Parameters.Add(new OracleParameter("idPrograma", OracleDbType.Int32) { Value = o.IdPrograma });
            cmd.Parameters.Add(new OracleParameter("idSede", OracleDbType.Int32) { Value = o.IdSede });
            cmd.Parameters.Add(new OracleParameter("fechaInicio", OracleDbType.Date) { Value = o.FechaInicio });
            cmd.Parameters.Add(new OracleParameter("fechaFin", OracleDbType.Date) { Value = o.FechaFin });
            cmd.Parameters.Add(new OracleParameter("cupos", OracleDbType.Int32) { Value = o.CuposDisponibles });
            cmd.Parameters.Add(new OracleParameter("estado", OracleDbType.Varchar2) { Value = o.Estado });
            cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = o.Id });
            cmd.ExecuteNonQuery();
        }

        public void EliminarOferta(int id)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM OFERTA WHERE id = :id";
            cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = id });
            cmd.ExecuteNonQuery();
        }

        // =====================================================================
        // SOLICITUDES (listados; el alta/aceptación va por las funciones PL/SQL)
        // =====================================================================

        public List<Solicitud> ListarTodasSolicitudes()
        {
            var lista = new List<Solicitud>();
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT sol.id, sol.id_postulante, sol.id_oferta, sol.fecha_postulacion, sol.resumen_interes, sol.estado,
                       pos.nombre || ' ' || pos.apellido AS postulante, prog.nombre AS programa, u.nombre AS universidad
                FROM SOLICITUD sol
                JOIN POSTULANTE pos ON sol.id_postulante = pos.id
                JOIN OFERTA o ON sol.id_oferta = o.id
                JOIN PROGRAMA prog ON o.id_programa = prog.id
                JOIN SEDE s ON o.id_sede = s.id
                JOIN UNIVERSIDAD u ON s.id_universidad = u.id
                ORDER BY sol.fecha_postulacion DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapSolicitud(reader));
            }
            return lista;
        }

        public List<Solicitud> ListarSolicitudesPorPostulante(int idPostulante)
        {
            var lista = new List<Solicitud>();
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT sol.id, sol.id_postulante, sol.id_oferta, sol.fecha_postulacion, sol.resumen_interes, sol.estado,
                       pos.nombre || ' ' || pos.apellido AS postulante, prog.nombre AS programa, u.nombre AS universidad
                FROM SOLICITUD sol
                JOIN POSTULANTE pos ON sol.id_postulante = pos.id
                JOIN OFERTA o ON sol.id_oferta = o.id
                JOIN PROGRAMA prog ON o.id_programa = prog.id
                JOIN SEDE s ON o.id_sede = s.id
                JOIN UNIVERSIDAD u ON s.id_universidad = u.id
                WHERE sol.id_postulante = :idPostulante
                ORDER BY sol.fecha_postulacion DESC";
            cmd.Parameters.Add(new OracleParameter("idPostulante", OracleDbType.Int32) { Value = idPostulante });
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapSolicitud(reader));
            }
            return lista;
        }

        private static Solicitud MapSolicitud(IDataRecord r) => new Solicitud
        {
            Id = Convert.ToInt32(r["id"]),
            IdPostulante = Convert.ToInt32(r["id_postulante"]),
            IdOferta = Convert.ToInt32(r["id_oferta"]),
            FechaPostulacion = Convert.ToDateTime(r["fecha_postulacion"]),
            ResumenInteres = r["resumen_interes"].ToString()!,
            Estado = r["estado"].ToString()!,
            PostulanteNombreCompleto = r["postulante"].ToString(),
            ProgramaNombre = r["programa"].ToString(),
            UniversidadNombre = r["universidad"].ToString()
        };
    }
}
