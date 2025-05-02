using MercadoPago.Client.Payment;
using MercadoPago.Client;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using QRCoder;
using API_Rifa.Services;
using API_Rifa.Models;
using API_Rifa.Data;
using System.Security.Cryptography;

namespace API_Rifa.Controllers
{
        [Route("api/[controller]")]
        [ApiController]
        public class MercadoPagoController : Controller
        {
            private readonly HttpClient _httpClient;
            private readonly MercadoPagoService _mercadoPagoService;
            private readonly AppDbContext _context;
            private readonly string _mercadoPagoAccessToken;

        public MercadoPagoController(HttpClient httpClient, MercadoPagoService mercadoPagoService, AppDbContext appDbContext, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _mercadoPagoService = mercadoPagoService;
            _context = appDbContext;
            _mercadoPagoAccessToken = configuration.GetValue<string>("MercadoPagoSettings:AccessToken");
        }

        public class PreferenciaResponse
            {
                public string PreferenceId { get; set; }
                public string InitPoint { get; set; }  // Adicionando o initPoint
            }

            public class PreferenciaRequest
            {
                public string Title { get; set; }
                public int Quantity { get; set; }
                public decimal UnitPrice { get; set; }
            }

            //[HttpPost]
            //public async Task<IActionResult> CriarPreferencia([FromBody] PreferenciaRequest preferenceRequest)
            //{
            //    var preference = new
            //    {
            //        items = new[] {
            //    new {
            //        title = preferenceRequest.Title,
            //        quantity = preferenceRequest.Quantity,
            //        unit_price = preferenceRequest.UnitPrice,
            //    }
            //}

            //    };

            //    var content = new StringContent(JsonConvert.SerializeObject(preference), Encoding.UTF8, "application/json");

            //    // Envia a requisição para criar a preferência
            //    var response = await _httpClient.PostAsync("https://api.mercadopago.com/checkout/preferences?access_token=" + MercadoPagoAccessToken, content);

            //    if (response.IsSuccessStatusCode)
            //    {
            //        var result = await response.Content.ReadAsStringAsync();
            //        var preferenceResponse = JsonConvert.DeserializeObject<dynamic>(result);

            //        var preferenceId = preferenceResponse.id;
            //        var initPoint = preferenceResponse.init_point;
            //        return Ok(new PreferenciaResponse
            //        {
            //            PreferenceId = preferenceId,
            //            InitPoint = initPoint
            //        });
            //    }

            //    return BadRequest("Erro ao criar a preferência");
            //}
            //[HttpPost]
            //public async Task<IActionResult> CriarPreferencia([FromBody] PreferenciaRequest preferenceRequest)
            //{
            //    var preference = new
            //    {
            //        items = new[] {
            //    new {
            //        title = preferenceRequest.Title,
            //        quantity = preferenceRequest.Quantity,
            //        unit_price = preferenceRequest.UnitPrice,
            //        currency_id = "BRL"
            //    }
            //},
            //        payment_methods = new
            //        {
            //            // Removido default_payment_method_id para evitar conflito
            //            excluded_payment_methods = new[] {
            //        new { id = "credit_card" } // Você pode excluir o que quiser, mas NÃO o Pix
            //    }
            //        }
            //    };

            //    var content = new StringContent(JsonConvert.SerializeObject(preference), Encoding.UTF8, "application/json");

            //    var response = await _httpClient.PostAsync("https://api.mercadopago.com/checkout/preferences?access_token=" + MercadoPagoAccessToken, content);

            //    if (response.IsSuccessStatusCode)
            //    {
            //        var result = await response.Content.ReadAsStringAsync();
            //        var preferenceResponse = JsonConvert.DeserializeObject<dynamic>(result);

            //        var preferenceId = preferenceResponse.id;
            //        var initPoint = preferenceResponse.init_point;

            //        return Ok(new PreferenciaResponse
            //        {
            //            PreferenceId = preferenceId,
            //            InitPoint = initPoint
            //        });
            //    }

            //    var errorContent = await response.Content.ReadAsStringAsync();
            //    return BadRequest($"Erro ao criar a preferência: {errorContent}");
            //}

            [HttpPost]
            public async Task<IActionResult> CriarPagamentoPix([FromBody] PixRequest request)
            {
                try
                {
                    var idempotencyKey = Guid.NewGuid().ToString();
                    var pagamento = new
                    {
                        transaction_amount = request.Valor,
                        description = request.Descricao,
                        payment_method_id = "pix",
                        payer = new
                        {
                            email = request.Email
                        }
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(pagamento),
                                                 Encoding.UTF8,
                                                 "application/json");

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _mercadoPagoAccessToken);
                    _httpClient.DefaultRequestHeaders.Add("X-Idempotency-Key", idempotencyKey);

                    var response = await _httpClient.PostAsync(
                        "https://api.mercadopago.com/v1/payments",
                        content);

                    var result = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return BadRequest(new
                        {
                            error = "Falha ao criar pagamento",
                            details = result
                        });
                    }

                    var jsonResponse = JObject.Parse(result);

                // Verificação segura dos campos
                 string qrCodeText = jsonResponse["point_of_interaction"]?["transaction_data"]?["qr_code"]?.ToString();
                 string qrBase64 = _mercadoPagoService.GerarQRCode(qrCodeText);

                    string qrText = jsonResponse["point_of_interaction"]?["transaction_data"]?["qr_code"]?.ToString();
                    string paymentId = jsonResponse["id"]?.ToString();

                    if (string.IsNullOrEmpty(paymentId))
                    {
                        return StatusCode(500, new
                        {
                            error = "PaymentId não encontrado na resposta",
                            fullResponse = jsonResponse
                        });
                    }

                    return Ok(new
                    {
                        QrCodeBase64 = qrBase64,
                        QrCodeText = qrText,
                        PaymentId = paymentId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        error = ex.Message,
                        stackTrace = ex.StackTrace
                    });
                }
            }

        [HttpGet("status/{paymentId}")]
            public async Task<IActionResult> CheckPaymentStatus(long paymentId)
            {
                try
                {
                    var requestOptions = new RequestOptions
                    {
                        AccessToken = _mercadoPagoAccessToken
                    };

                    // 3. Criar o cliente e buscar o pagamento
                    var client = new PaymentClient();
                    Payment payment = await client.GetAsync(paymentId, requestOptions);

                    // 4. Verificar se o pagamento foi encontrado
                    if (payment == null)
                    {
                        return NotFound(new { error = "Pagamento não encontrado" });
                    }

                    // 5. Retornar o status em lowercase
                    return Ok(new
                    {
                        status = payment.Status?.ToLower() ?? "unknown",
                        lastUpdated = payment.DateLastUpdated
                    });
                }
                catch (MercadoPago.Error.MercadoPagoApiException apiEx)
                {
                    // Erro específico da API do Mercado Pago
                    return StatusCode((int)apiEx.StatusCode, new
                    {
                        error = apiEx.Message,
                        apiError = apiEx.ApiError?.ToString()
                    });
                }
                catch (Exception ex)
                {
                    // Outros erros
                    return StatusCode(500, new
                    {
                        error = ex.Message,
                        stackTrace = ex.StackTrace
                    });
                }
            }
        [HttpPost("webhook")]
        public async Task<IActionResult> MercadoPagoWebhook(
                        [FromBody] WebhookPayload payload,
                        [FromHeader(Name = "X-Hub-Signature")] string signature)
        {
            // Verifica se os campos obrigatórios estão presentes
            if (payload?.Data?.CurrencyId == null)
                return BadRequest("CurrencyId é obrigatório.");

            if (payload?.Data?.Payer?.FirstName == null)
                return BadRequest("FirstName (nome) é obrigatório.");

            if (payload?.Data?.Payer?.LastName == null)
                return BadRequest("LastName (sobrenome) é obrigatório.");

            // Validação da assinatura (HMAC SHA256)...
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            var secret = "1c97e6a4f3612e8a39dbe4acaa284edf1dee7a926454125fa187cbf3066ffa82"; // Definida no painel do Mercado Pago

            //// Gera o hash HMAC SHA256
            //using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            //{
            //    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
            //    var hashString = "sha256=" + BitConverter.ToString(hash).Replace("-", "").ToLower();

            //    if (signature != hashString)
            //    {
            //        return Unauthorized("Assinatura inválida");
            //    }
            //}

            // Processa o pagamento
            if (payload?.Data?.Id == null)
                return BadRequest("ID de pagamento ausente");

            long paymentId = payload.Data.Id;

            var requestOptions = new RequestOptions
            {
                AccessToken = "APP_USR-2192516474846552-042518-abae9606c2b5f610e4a128ce37342a13-125493739"
            };

            var client = new PaymentClient();
            var payment = await client.GetAsync(paymentId, requestOptions);

            if (payment == null)
                return NotFound("Pagamento não encontrado");

            if (payment.Status == "approved")
            {
                // Atualizar transação no banco e status do usuário
            }

            return Ok("Pagamento ainda não aprovado");
        }



        public static string GerarAssinatura(string corpoRequisicao, string chaveSecreta)
        {
            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(chaveSecreta)))
            {
                byte[] hashBytes = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(corpoRequisicao));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public class WebhookPayload
        {
            public string Topic { get; set; }
            public long Id { get; set; }
            public WebhookData Data { get; set; }
        }

        public class WebhookData
        {
            public long Id { get; set; }
            public string Status { get; set; }
            public double Amount { get; set; }
            public string CurrencyId { get; set; }
            public string Description { get; set; }
            public DateTime DateApproved { get; set; }
            public Payer Payer { get; set; }
        }

        public class Payer
        {
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }


        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutAssinaturaRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.IdUser);

                var pixTransaction = new PixTransactionAdmin
                {
                    Pix_Key = Guid.NewGuid().ToString(),
                    Value = request.Price,
                    Status = "pending",
                    UserId = request.IdUser,
                };

                _context.Pix_TransactionsAdmin.Add(pixTransaction);
                await _context.SaveChangesAsync();
                
                return Ok(new
                {
                    success = true,
                    transactionId = pixTransaction.Id,
                    pixCode = pixTransaction.Pix_Key,
                    paymentStatus = "pending"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    message = "Erro ao processar checkout"
                });
            }
        }

        public class CheckoutAssinaturaRequest
        {
            public int IdUser { get; set; }
            public decimal Price { get; set; }
        }
        public class PixRequest
            {
                public string Email { get; set; }
                public string Descricao { get; set; }
                public decimal Valor { get; set; }
            }


        }
    }
