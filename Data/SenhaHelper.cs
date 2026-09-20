using System.Security.Cryptography;
using System.Text;

namespace CadastroUsuariosApp.Data
{
    /// <summary>
    /// Utilitário simples de hash de senha (SHA256) para fins didáticos.
    /// Em um sistema real, prefira BCrypt, Argon2 ou ASP.NET Core Identity.
    /// </summary>
    public static class SenhaHelper
    {
        public static string GerarHash(string senha)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(senha);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
