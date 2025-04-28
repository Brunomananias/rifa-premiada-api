using API_Rifa.Models;
using API_Rifa.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_Rifa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagguePaymentController : Controller
    {
            private readonly PagguePaymentService _pagguePaymentService;

            // Injeção de dependência do serviço que irá se comunicar com a API da Paggue
            public PagguePaymentController(PagguePaymentService pagguePaymentService)
            {
                _pagguePaymentService = pagguePaymentService;
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
        public async Task<ActionResult<PaymentStatusResponse>> GetPaymentStatus(string paymentId)
        {
            try
            {
                var status = await _pagguePaymentService.GetPaymentStatusAsync(paymentId);
                return Ok(status); // Retorna o status do pagamento como resposta
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao consultar status: {ex.Message}");
            }
        }
    }
    }

