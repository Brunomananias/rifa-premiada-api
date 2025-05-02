using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Runtime;
using System.Text;

namespace API_Rifa.Services
{
    public class TokenService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public TokenService(AppDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<string> ObterTokenAsync(int userId)
        {
            var settings = await _context.GatewaySettings.FirstOrDefaultAsync(g => g.UserId == userId);

            if (settings == null || string.IsNullOrEmpty(settings.ClientKey) || string.IsNullOrEmpty(settings.ClientSecret))
            {
                Console.WriteLine("Chaves não encontradas para o usuário.");
                return null;
            }

            var url = "https://ms.paggue.io/auth/v1/token";

            var body = new Dictionary<string, string>
        {
            { "client_key", settings.ClientKey },
            { "client_secret", settings.ClientSecret },
            { "grant_type", "client_credentials" }
        };

            var content = new FormUrlEncodedContent(body);

            try
            {
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JsonConvert.DeserializeObject(jsonResponse);
                    return responseData.access_token;
                }
                else
                {
                    Console.WriteLine("Erro ao obter token: " + response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao enviar requisição: " + ex.Message);
                return null;
            }
        }

    }
}
   
