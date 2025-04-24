using Microsoft.AspNetCore.Mvc;
using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class WebhookController : ControllerBase
{
    private readonly AppDbContext _context;

    public WebhookController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("pix")]
    public async Task<IActionResult> ReceberWebhook([FromBody] dynamic data)
    {
        try
        {
            string pixKey = data?.pix_key;
            string status = data?.status;

            if (pixKey == null || status == null)
                return BadRequest("Webhook inválido");

            var transaction = await _context.Pix_Transactions
                .FirstOrDefaultAsync(t => t.Pix_Key == pixKey);

            if (transaction != null)
            {
                transaction.Status = status;
                await _context.SaveChangesAsync();
                return Ok();
            }

            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro: {ex.Message}");
        }
    }
}
