using Oracle.ManagedDataAccess.Client;

namespace BecasPosgrado.Helpers
{
    /// <summary>
    /// Traduce los códigos de error más comunes de Oracle (ORA-XXXXX) a mensajes
    /// entendibles para el usuario final, en vez de mostrar el texto crudo de la
    /// excepción (que incluye nombres de restricciones, esquema, etc.).
    /// </summary>
    public static class OracleErrorHelper
    {
        public static string MensajeAmigable(OracleException ex)
        {
            // Errores de negocio lanzados a propósito desde PL/SQL con RAISE_APPLICATION_ERROR
            // (rango reservado de Oracle 20000-20999, ej. "ya tiene 3 solicitudes activas").
            // Esos mensajes ya están pensados para el usuario final, así que se muestran
            // tal cual, quitando solo el prefijo técnico "ORA-20001: " y el stack que Oracle agrega después.
            if (ex.Number is >= 20000 and <= 20999)
            {
                var mensaje = ex.Message;
                var dosPuntos = mensaje.IndexOf(':');
                if (mensaje.StartsWith("ORA-", StringComparison.Ordinal) && dosPuntos > 0)
                {
                    mensaje = mensaje[(dosPuntos + 1)..].Trim();
                }
                var salto = mensaje.IndexOf('\n');
                if (salto > 0)
                {
                    mensaje = mensaje[..salto].Trim();
                }
                return mensaje;
            }

            return ex.Number switch
            {
                // Borrar un registro que todavía tiene hijos relacionados (FK).
                2292 => "No se puede eliminar: hay otros registros que dependen de este elemento.",

                // Insertar/actualizar una referencia a un padre que no existe (FK).
                2291 => "El registro relacionado seleccionado no existe o fue eliminado.",

                // Restricción UNIQUE violada (usuario, email, etc. duplicados).
                1 => "Ya existe un registro con ese mismo valor (revisa usuario, email o datos únicos).",

                // Restricción CHECK violada.
                2290 => "Uno de los valores ingresados no cumple con las reglas permitidas.",

                // Columna NOT NULL sin valor.
                1400 => "Falta completar un campo obligatorio.",

                // Fila bloqueada por otra transacción concurrente.
                54 => "Este registro está siendo modificado por otra persona en este momento. Intenta de nuevo en unos segundos.",

                _ => "Ocurrió un error al procesar la solicitud. Intenta nuevamente."
            };
        }
    }
}
