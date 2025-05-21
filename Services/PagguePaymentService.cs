using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QRCoder;
using System.Text;

namespace API_Rifa.Services
{
    public class PagguePaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        public PagguePaymentService(HttpClient httpClient, TokenService tokenService, AppDbContext context)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<PaggueResponse> CreatePixPaymentAsync(PaggueRequest request)
        {
            var url = "https://ms.paggue.io/cashin/api/billing_order"; // Endpoint da Paggue para criar pagamento Pix

            var jsonRequest = JsonConvert.SerializeObject(new
            {
                payer_name = request.PayerName,
                amount = (int)(request.Amount * 100),   // Certifique-se de que Amount é um valor numérico válido
                expiration = request.Expiration,  // Certifique-se de que Expiration é um número válido (em minutos)
                external_id = request.ExternalId,
                description = request.Description,
                meta = new
                {
                    extra_data = request.Meta.ExtraData,
                    webhook_url = request.Meta.WebhookUrl
                }
            });
            var raffle = await _context.Raffles
                .FirstOrDefaultAsync(r => r.Id == request.raffleId);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var companyId = await GetCompanyIdAsync(raffle.User_id);
            // Definindo o cabeçalho de autenticação
            var token = await _tokenService.ObterTokenAsync(raffle.User_id);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Add("x-company-id", companyId);
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                // Se a resposta for bem-sucedida, deserializamos o JSON de resposta
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var pagamentoResponse = JsonConvert.DeserializeObject<PaggueResponse>(jsonResponse);
                return pagamentoResponse;
            }
            else
            {
                // Caso ocorra algum erro, podemos lançar uma exceção ou retornar um erro customizado
                throw new Exception("Erro ao criar o pagamento: " + response.ReasonPhrase);
            }
        }

        public async Task<string> GetCompanyIdAsync(int userId)
        {
            var settings = await _context.GatewaySettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            return settings?.CompanyId; // Retorna o CompanyId associado ao UserId
        }


        public string GerarQRCode(string codigoPix)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(codigoPix, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            return Convert.ToBase64String(qrCodeBytes);
        }

        public async Task<PaymentStatusResponse> GetPaymentStatusAsync(string paymentId, int userId)
        {
            var url = $"https://ms.paggue.io/cashin/api/billing_order/{paymentId}"; // URL da Paggue para consultar o status

            // Definindo cabeçalhos, incluindo o token de autenticação
            var token = await _tokenService.ObterTokenAsync(userId);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Add("x-company-id", "172012");

            // Fazendo a requisição para obter o status do pagamento
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Se a resposta for bem-sucedida, deserializa o status
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var paymentStatus = JsonConvert.DeserializeObject<PaymentStatusResponse>(jsonResponse);
                return paymentStatus;
            }
            else
            {
                // Caso ocorra algum erro, lança uma exceção ou retorna uma resposta de erro
                throw new Exception("Erro ao obter o status do pagamento: " + response.ReasonPhrase);
            }
        }

    }
}
