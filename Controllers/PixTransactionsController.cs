using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace RifaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PixTransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PixTransactionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PixTransactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PixTransaction>>> GetPixTransactions()
        {
            return await _context.Pix_Transactions
                .Include(pt => pt.Number_Sold)
                .Include(pt => pt.User)
                .ToListAsync();
        }

        // GET: api/PixTransactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PixTransaction>> GetPixTransaction(int id)
        {
            var pixTransaction = await _context.Pix_Transactions
                .Include(p => p.Number_Sold)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pixTransaction == null)
            {
                return NotFound();
            }

            return pixTransaction;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            if (request == null || request.User == null || string.IsNullOrEmpty(request.User.Whatsapp))
                return BadRequest("Dados inválidos");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Whatsapp == request.User.Whatsapp);

            if (user == null)
            {
                user = new User
                {
                    Name = request.User.Name,
                    Whatsapp = request.User.Whatsapp
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var transaction = new PixTransaction
            {
                Pix_Key = Guid.NewGuid().ToString(),
                Value = request.Price,
                Status = "aguardando_pagamento",
                QrCodeUrl = "https://via.placeholder.com/300?text=QR+Code+Pix",
                UserId = user.Id
            };



            _context.Pix_Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                transactionId = transaction.Id,
                pixCode = transaction.Pix_Key,
                qrCodeUrl = transaction.QrCodeUrl
            });
        }


        // POST: api/PixTransactions/confirm
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            var transaction = await _context.Pix_Transactions
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == request.TransactionId);

            if (transaction == null)
                return NotFound("Transação não encontrada");

            if (transaction.Status == "pago")
                return BadRequest("Pagamento já confirmado");

            var numberSold = new NumberSold
            {
                RaffleId = request.RaffleId,
                Numbers = string.Join(",", request.Numbers.Select(n => n.ToString())), // Sem formatação
                UserId = transaction.UserId,
                PaymentStatus = "Pago",
                Value = transaction.Value
            };


            _context.Numbers_Sold.Add(numberSold);
            transaction.Number_Sold = numberSold;
            transaction.Status = "pago";

            // Atualizar a coluna SoldNumbers da rifa (como JSON)
            var raffle = await _context.Raffles.FindAsync(request.RaffleId);
            if (raffle == null)
                return NotFound("Rifa não encontrada");

            // Carregar os números vendidos anteriores (se existirem)
            var vendidos = new List<int>();
            if (!string.IsNullOrEmpty(raffle.SoldNumbers))
            {
                vendidos = JsonConvert.DeserializeObject<List<int>>(raffle.SoldNumbers);
            }

            // Adicionar os novos números vendidos
            vendidos.AddRange(request.Numbers);

            // Atualizar o campo como JSON
            raffle.SoldNumbers = JsonConvert.SerializeObject(vendidos);

            await _context.SaveChangesAsync();

            return Ok("Pagamento confirmado e números registrados.");
        }


        // DELETE: api/PixTransactions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePixTransaction(int id)
        {
            var pixTransaction = await _context.Pix_Transactions.FindAsync(id);
            if (pixTransaction == null)
            {
                return NotFound();
            }

            _context.Pix_Transactions.Remove(pixTransaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PixTransactionExists(int id)
        {
            return _context.Pix_Transactions.Any(e => e.Id == id);
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus([FromQuery] string pixKey)
        {
            var transaction = await _context.Pix_Transactions
                .FirstOrDefaultAsync(x => x.Pix_Key == pixKey);

            if (transaction == null)
                return NotFound();

            return Ok(new { status = transaction.Status });
        }
    }
}
