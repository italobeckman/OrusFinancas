using BCrypt.Net;

namespace OrusFinancas.Utils
{
    public static class PasswordHasher
    {
        // 1. Geração do Hash (Usado no Cadastro/Criação)
        public static string HashPassword(string password)
        {
            // BCrypt.HashPassword gera um salt aleatório e o incorpora no hash final.
            // O parâmetro 12 (work factor) define a complexidade; 12 é um bom padrão atual.
            return BCrypt.Net.BCrypt.HashPassword(password, 12);
        }

        // 2. Verificação do Hash (Usado no Login)
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // O BCrypt sabe extrair o salt do hashedPassword e reprocessar a 'password'
            // para ver se os hashes combinam.
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}