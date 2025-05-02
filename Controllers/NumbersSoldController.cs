using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace RifaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NumbersSoldController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NumbersSoldController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/NumbersSold
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NumberSold>>> GetNumbersSold()
        {
            return await _context.Numbers_Sold.ToListAsync();
        }

        [HttpGet("quantity-purchases")]
        public async Task<long> GetQuantityPurchases()
        {
            var totalPaid = await _context.Numbers_Sold
                .Where(x => x.PaymentStatus == "paid")
                .CountAsync();
            return totalPaid;
        }

        [HttpGet("filtrar-cotas")]
        public async Task<IActionResult> FiltrarCotas(int rifa, DateTime inicio, DateTime fim)
        {
            // Ajustar a data de fim para o final do dia, se necessário
            fim = fim.Date.AddDays(1).AddMilliseconds(-1);

            // Buscar os registros filtrados pela rifa e pelas datas
            var numbersSold = await _context.Numbers_Sold
                .Where(n => n.RaffleId == rifa && n.PaymentStatus == "paid"
                            && n.CreatedAt >= inicio && n.CreatedAt <= fim)
                .ToListAsync();

            // Dividir os números da string, converter para inteiros e encontrar o maior e o menor
            var numbers = numbersSold
                .SelectMany(n => n.Numbers.Split(',') // Dividir os números por vírgula
                .Select(num => Convert.ToInt32(num))) // Converter para inteiro
                .ToList();

            // Encontrar a maior e menor cota usando LINQ
            var maiorCota = numbers.Any() ? numbers.Max() : 0;
            var menorCota = numbers.Any() ? numbers.Min() : 0;

            // Retornar os resultados
            return Ok(new { MaiorCota = maiorCota, MenorCota = menorCota });
        }




        // GET: api/NumbersSold/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NumberSold>> GetNumberSold(int id)
        {
            var numberSold = await _context.Numbers_Sold
                .Include(ns => ns.Raffle)
                .Include(ns => ns.CustomerId)
                .FirstOrDefaultAsync(ns => ns.Id == id);

            if (numberSold == null)
            {
                return NotFound();
            }

            return numberSold;
        }

        // POST: api/NumbersSold
        [HttpPost]
        public async Task<ActionResult<NumberSold>> PostNumberSold(NumberSold numberSold)
        {
            _context.Numbers_Sold.Add(numberSold);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNumberSold), new { id = numberSold.Id }, numberSold);
        }

        // PUT: api/NumbersSold/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNumberSold(int id, NumberSold numberSold)
        {
            if (id != numberSold.Id)
            {
                return BadRequest();
            }

            _context.Entry(numberSold).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NumberSoldExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/NumbersSold/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNumberSold(int id)
        {
            var numberSold = await _context.Numbers_Sold.FindAsync(id);
            if (numberSold == null)
            {
                return NotFound();
            }

            _context.Numbers_Sold.Remove(numberSold);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NumberSoldExists(int id)
        {
            return _context.Numbers_Sold.Any(e => e.Id == id);
        }

        [HttpGet("compras")]
        public async Task<IActionResult> GetCompras()
        {
            var compras = await (from ns in _context.Numbers_Sold
                                 join u in _context.Customers on ns.CustomerId equals u.Id
                                 join r in _context.Raffles on ns.RaffleId equals r.Id
                                 group ns by new
                                 {
                                     ns.Id,
                                     ns.CreatedAt,
                                     r.Title,
                                     u.Name,
                                     u.Whatsapp,
                                     ns.PaymentStatus,
                                     ns.Value
                                 } into g
                                 select new
                                 {
                                     compra_id = g.Key.Id,
                                     dataUpdated = g.Key.CreatedAt,
                                     nome_rifa = g.Key.Title,
                                     nome_usuario = g.Key.Name,
                                     whatsapp = g.Key.Whatsapp,
                                     quantidade_numbers = g.Count(),
                                     totalprice = (g.Count() * g.Key.Value).ToString("F2"),
                                     payment_status = g.Key.PaymentStatus
                                 }).OrderByDescending(c => c.dataUpdated).ToListAsync();

            return Ok(compras);
        }

        [HttpDelete("{id}/cancel-expirado")]

        public async Task CancelarCompraExpirada(int numberSoldId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Deletar as transações Pix associadas a este número vendido
                    var pixTransactions = await _context.Pix_Transactions
                        .Where(pt => pt.NumberSoldId == numberSoldId)
                        .ToListAsync();
                    _context.Pix_Transactions.RemoveRange(pixTransactions);

                    // Deletar o número vendido
                    var numberSold = await _context.Numbers_Sold
                        .Where(ns => ns.Id == numberSoldId)
                        .FirstOrDefaultAsync();

                    if (numberSold != null)
                    {
                        _context.Numbers_Sold.Remove(numberSold);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Erro ao cancelar a compra expirada: " + ex.Message);
                }
            }
        }


    }
}
