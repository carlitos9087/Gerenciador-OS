using System.Net.Http;
using System.Net.Http.Headers;

namespace Gerenciador_de_Ordens_de_servico
{
    public static class ApiConfig
    {
        // HttpClient único compartilhado por toda a aplicação.
        // O handler ignora erros de certificado SSL — necessário para
        // https://localhost em desenvolvimento (certificado autoassinado).
        public static readonly HttpClient Http = new HttpClient(
            new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }
        );

        public static string Token { get; set; } = string.Empty;

        // Chame após o login. Configura o header Bearer em todas as requisições.
        public static void SalvarToken(string token)
        {
            Token = token;
            Http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // Modelo que espelha o LoginResponse da API
    // POST /auth/login retorna: { "token": "eyJ...", "usuario": { ... } }
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UsuarioResponse? Usuario { get; set; }
    }
}