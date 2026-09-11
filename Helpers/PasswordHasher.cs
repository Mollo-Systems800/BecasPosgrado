using System.Security.Cryptography;

namespace BecasPosgrado.Helpers
{
    /// <summary>
    /// Hash de contraseñas con PBKDF2 (parte de .NET, no requiere paquetes externos).
    /// Formato guardado en la columna CLAVE: "PBKDF2$&lt;iteraciones&gt;$&lt;saltBase64&gt;$&lt;hashBase64&gt;".
    ///
    /// Compatibilidad con datos antiguos: los usuarios sembrados originalmente (cadmin,
    /// aperez, etc.) tenían la contraseña en texto plano ("hash123"). Verify() reconoce
    /// ambos formatos, y ObtenerPostulantePorUsuario reemplaza automáticamente el valor
    /// en texto plano por un hash real la primera vez que ese usuario inicia sesión
    /// correctamente, sin necesidad de correr una migración aparte.
    /// </summary>
    public static class PasswordHasher
    {
        private const int Iteraciones = 100_000;
        private const int TamanioSalt = 16; // bytes
        private const int TamanioHash = 32; // bytes (SHA-256)

        public static bool EsHashPbkdf2(string valorGuardado) =>
            !string.IsNullOrEmpty(valorGuardado) && valorGuardado.StartsWith("PBKDF2$", StringComparison.Ordinal);

        public static string Hash(string claveEnTextoPlano)
        {
            var salt = RandomNumberGenerator.GetBytes(TamanioSalt);
            var hash = Rfc2898DeriveBytes.Pbkdf2(claveEnTextoPlano, salt, Iteraciones, HashAlgorithmName.SHA256, TamanioHash);
            return $"PBKDF2${Iteraciones}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verifica una clave ingresada contra el valor guardado. Acepta tanto el formato
        /// hasheado (PBKDF2$...) como el texto plano heredado, para no romper a los
        /// usuarios sembrados antes de este cambio.
        /// </summary>
        public static bool Verify(string claveIngresada, string valorGuardado)
        {
            if (string.IsNullOrEmpty(valorGuardado)) return false;

            if (!EsHashPbkdf2(valorGuardado))
            {
                // Valor heredado en texto plano.
                return valorGuardado == claveIngresada;
            }

            var partes = valorGuardado.Split('$');
            if (partes.Length != 4) return false;

            if (!int.TryParse(partes[1], out var iteraciones)) return false;
            var salt = Convert.FromBase64String(partes[2]);
            var hashGuardado = Convert.FromBase64String(partes[3]);

            var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(claveIngresada, salt, iteraciones, HashAlgorithmName.SHA256, hashGuardado.Length);
            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashGuardado);
        }
    }
}
