using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

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
            return await _context.Pix_Transactions.ToListAsync();
        }

        // GET: api/PixTransactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PixTransaction>> GetPixTransaction(int id)
        {
            var pixTransaction = await _context.Pix_Transactions
                .Include(p => p.Customer)
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
            // 1. Validação dos dados
            if (request == null || request.Customer == null || string.IsNullOrEmpty(request.Customer.Whatsapp))
                return BadRequest("Dados inválidos");

            if (request.Numbers == null || !request.Numbers.Any())
                return BadRequest("Nenhum número selecionado");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(u => u.Whatsapp == request.Customer.Whatsapp);
                var userIdDono = await _context.Raffles
                                   .Where(r => r.Id == request.RaffleId)
                                   .Select(r => r.User_id)
                                   .FirstOrDefaultAsync();
                if (customer == null)
                {
                    customer = new Customer
                    {
                        Name = request.Customer.Name,
                        Whatsapp = request.Customer.Whatsapp
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                // 3. Criar os números vendidos (com status inicial 'reserved')
                var numbersSold = new List<NumberSold>();
                if(request.Numbers != null)
                {
                    var numberSold = new NumberSold
                    {
                        Numbers = request.Numbers,
                        RaffleId = request.RaffleId,
                        CustomerId = customer.Id,
                        UserId = userIdDono,
                        PaymentStatus = "reserved",
                        Value = request.Price
                    };
                    _context.Numbers_Sold.Add(numberSold);
                    numbersSold.Add(numberSold);
                    await _context.SaveChangesAsync();
                }
                // 4. Criar transação Pix
                var pixTransaction = new PixTransaction
                {
                    Pix_Key = Guid.NewGuid().ToString(),
                    Value = request.Price,
                    Status = "pending",
                    CustomerId = customer.Id,
                    NumberSoldId = numbersSold.First().Id, // Associa ao primeiro número criado
                };

                _context.Pix_Transactions.Add(pixTransaction);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // 5. Retornar resposta
                return Ok(new
                {
                    success = true,
                    transactionId = pixTransaction.Id,
                    numbersSoldIds = numbersSold.Select(n => n.Id),
                    pixCode = pixTransaction.Pix_Key,
                    paymentStatus = "pending"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    message = "Erro ao processar checkout"
                });
            }
        }

        // POST: api/PixTransactions/confirm
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            var transaction = await _context.Pix_Transactions
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(p => p.Id == request.TransactionId);

            if (transaction == null)
                return NotFound("Transação não encontrada");

            if (transaction.Status == "pago")
                return BadRequest("Pagamento já confirmado");

            var numberSold = new NumberSold
            {
                RaffleId = request.RaffleId,
                Numbers = string.Join(",", request.Numbers.Select(n => n.ToString())), // Sem formatação
                CustomerId = transaction.CustomerId,
                PaymentStatus = "Pago",
                Value = transaction.Value
            };


            _context.Numbers_Sold.Add(numberSold);
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

        [HttpPut("update-pix-status/{paymentId}")]
        public async Task<IActionResult> UpdatePixStatus(int paymentId, [FromBody] UpdatePixStatusRequest request)
        {
            try
            {
                var transaction = await _context.Pix_Transactions.FirstOrDefaultAsync(p => p.Id == paymentId);
                var transactionAdmin = await _context.Pix_TransactionsAdmin.FirstOrDefaultAsync(p => p.Id == paymentId);
                var numbersSold = await _context.Numbers_Sold.FirstOrDefaultAsync(p => p.Id == transaction.NumberSoldId);


                if (transaction == null)
                {
                    transactionAdmin.Status = request.Status;
                    transactionAdmin.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    transaction.Status = request.Status;
                    transaction.UpdatedAt = DateTime.UtcNow;
                }

                // Atualiza a transação Pix
               

                // Atualiza também o NumberSold relacionado
                if (transaction.NumberSoldId != null)
                {
                    numbersSold.PaymentStatus = request.Status == "approved" ? "paid" : "reserved";
                    numbersSold.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Status atualizado com sucesso",
                    transactionId = transaction.Id,
                    numberSoldId = transaction.NumberSoldId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        public class UpdatePixStatusRequest
        {
            [Required]
            public string Status { get; set; } // "approved", "pending", "rejected"

        }
    }
}
