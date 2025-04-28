using API_Rifa.Models;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Runtime;
using System.Text;

namespace API_Rifa.Services
{
    public class TokenService
    {
        private readonly HttpClient _httpClient;
        private readonly PaggueSettings _settings;

        public TokenService(HttpClient httpClient, IOptions<PaggueSettings> settings)
            {
                _httpClient = httpClient;
                _settings = settings.Value;
        }

        public async Task<string> ObterTokenAsync()
        {
            var url = "https://ms.paggue.io/auth/v1/token"; // URL para obter o token

            // Dados do corpo da requisição com as credenciais
            var body = new Dictionary<string, string>
            {
                { "client_key", _settings.ClientKey },
                { "client_secret", _settings.ClientSecret },
                { "grant_type", "client_credentials" }
                };

            // Formatação dos dados para enviar no corpo da requisição
            var content = new FormUrlEncodedContent(body);

            try
            {
                // Envia a requisição POST
                var response = await _httpClient.PostAsync(url, content);

                // Verifica se a requisição foi bem-sucedida
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Exibe a resposta para depuração (verifique o JSON completo)
                    Console.WriteLine("Resposta da API: " + jsonResponse);

                    // Deserializa a resposta JSON
                    dynamic responseData = JsonConvert.DeserializeObject(jsonResponse);

                    // Retorna o token de acesso
                    return responseData.access_token;
                }
                else
                {
                    // Se a resposta não foi bem-sucedida, exibe o código de status
                    Console.WriteLine("Erro ao obter o token. Status: " + response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Captura erros e exibe a mensagem
                Console.WriteLine("Erro ao enviar a requisição: " + ex.Message);
                return null;
            }
        }

    }
}
   
