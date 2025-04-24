using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using API_Rifa.Data; // Seu DbContext
using API_Rifa.Models; // Suas entidades

namespace API_Rifa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MercadoPagoWebhookController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MercadoPagoWebhookController> _logger;

        public MercadoPagoWebhookController(AppDbContext context, ILogger<MercadoPagoWebhookController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Este é o endpoint que o Mercado Pago irá chamar com o Webhook
        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            try
            {
                // Recebendo o payload do Webhook
                var notification = await Request.ReadFromJsonAsync<Notification>();

                if (notification == null)
                {
                    return BadRequest("Invalid data received.");
                }

                // Usando o SDK do Mercado Pago para buscar o pagamento
                var paymentClient = new PaymentClient();
                var payment = await paymentClient.GetAsync(notification.Data.Id);

                if (payment.Status == "approved")
                {
                    var pixKey = payment.PointOfInteraction.TransactionData.QrCode;

                    // Encontrando a transação no banco de dados pela chave Pix
                    var transaction = _context.Pix_Transactions
                        .FirstOrDefault(p => p.Pix_Key == pixKey);

                    if (transaction != null)
                    {
                        // Atualizando o status da transação para "pago"
                        transaction.Status = "paid";
                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"Pagamento confirmado: {pixKey}");
                    }
                    else
                    {
                        _logger.LogWarning($"Transação não encontrada para o PixKey: {pixKey}");
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar o Webhook do Mercado Pago: {ex.Message}");
                return StatusCode(500, "Erro interno");
            }
        }
    }

    // Modelo para deserializar o webhook do Mercado Pago
    public class Notification
    {
        public NotificationData Data { get; set; }
    }

    public class NotificationData
    {
        public long Id { get; set; }
    }
}
