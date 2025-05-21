using API_Rifa.Data;
using API_Rifa.Models;
using API_Rifa.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace API_Rifa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagguePaymentController : Controller
    {
            private readonly PagguePaymentService _pagguePaymentService;
            private readonly AppDbContext _context;

            // Injeção de dependência do serviço que irá se comunicar com a API da Paggue
            public PagguePaymentController(PagguePaymentService pagguePaymentService, AppDbContext context)
            {
                _pagguePaymentService = pagguePaymentService;
                _context = context;
            }

        [HttpPost("gerar-pix")]
        public async Task<IActionResult> CriarPagamentoPix([FromBody] PaggueRequest request)
        {
            try
            {
                // Criação do pagamento Pix
                var pagamentoResponse = await _pagguePaymentService.CreatePixPaymentAsync(request);

                // Gerar o QR Code com o código Pix retornado
                var qrCodeBase64 = _pagguePaymentService.GerarQRCode(pagamentoResponse.Payment);

                // Retorna a resposta do pagamento, incluindo o QR Code gerado
                var response = new
                {
                    qrCodeBase64,
                    paymentId = pagamentoResponse.Hash,
                    externalId = pagamentoResponse.External_Id,
                    paymentCode = pagamentoResponse.Payment
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro ao processar pagamento: " + ex.Message);
            }
        }



        [HttpGet("status/{paymentId}")]
        public async Task<ActionResult<PaymentStatusResponse>> GetPaymentStatus(string paymentId, int rifaId)
        {
            try
            {
                var rifa = await _context.Raffles
                    .FirstOrDefaultAsync(r => r.Id == rifaId);

                if (rifa == null)
                    return NotFound("Rifa não encontrada.");

                var userId = rifa.User_id;
                var status = await _pagguePaymentService.GetPaymentStatusAsync(paymentId, userId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao consultar status: {ex.Message}");
            }
        }
    }
    }

