using API_Rifa.Data;
using API_Rifa.Models;
using API_Rifa.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RifaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RafflesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RaffleService _raffleService;

        public RafflesController(AppDbContext context, RaffleService raffleService)
        {
            _context = context;
            _raffleService = raffleService;
        }

        // GET: api/Raffles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Raffle>>> GetRaffles(int idUsuarioLogado)
        {
            var raffles = await _context.Raffles
                .Where(r => r.user_id == idUsuarioLogado)
                .Select(r => new Raffle
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    Price = r.Price,
                    Total_Numbers = r.Total_Numbers,
                    Start_Date = r.Start_Date,
                    End_Date = r.End_Date,
                    Image_Url = r.Image_Url,
                    SoldNumbers = string.Join(",", _context.Numbers_Sold
                        .Where(n => n.RaffleId == r.Id && n.PaymentStatus == "paid")
                        .Select(n => n.Numbers)
                        .ToList())
                })               
                .ToListAsync();

            return raffles;
        }

        // GET: api/Raffles
        [HttpGet("all-raffles")]
        public async Task<ActionResult<IEnumerable<Raffle>>> GetRaffles()
        {
            var raffles = await _context.Raffles
                .Select(r => new Raffle
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    Price = r.Price,
                    Total_Numbers = r.Total_Numbers,
                    Start_Date = r.Start_Date,
                    End_Date = r.End_Date,
                    Image_Url = r.Image_Url,
                    SoldNumbers = string.Join(",", _context.Numbers_Sold
                        .Where(n => n.RaffleId == r.Id && n.PaymentStatus == "paid")
                        .Select(n => n.Numbers)
                        .ToList())
                })
                .ToListAsync();

            return raffles;
        }

        // GET: api/Raffles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Raffle>> GetRaffle(int id)
        {
            var raffle = await _context.Raffles
                .Where(r => r.Id == id)
                .Select(r => new Raffle
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    Price = r.Price,
                    Total_Numbers = r.Total_Numbers,
                    Start_Date = r.Start_Date,
                    End_Date = r.End_Date,
                    Image_Url = r.Image_Url,
                    SoldNumbers = string.Join(",", _context.Numbers_Sold
                        .Where(n => n.RaffleId == r.Id && n.PaymentStatus == "paid")
                        .Select(n => n.Numbers)
                        .ToList())
                })
                .FirstOrDefaultAsync();

            if (raffle == null)
            {
                return NotFound();
            }

            return raffle;
        }


        // POST: api/Raffles
        [HttpPost]
        public async Task<ActionResult<Raffle>> PostRaffle(Raffle raffle)
        {
            _context.Raffles.Add(raffle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRaffle), new { id = raffle.Id }, raffle);
        }

        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            // Verifique se a imagem foi enviada
            if (image == null || image.Length == 0)
                return BadRequest("No image uploaded.");

            // Gera um nome único para a imagem
            var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);

            // Caminho completo para salvar a imagem
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "images");

            // Verifique se a pasta existe, se não, crie a pasta
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);  // Cria a pasta caso não exista
            }

            // Caminho completo onde a imagem será salva
            var filePath = Path.Combine(directoryPath, fileName);

            // Salva a imagem na pasta
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);  // Copia o arquivo enviado para o destino
            }

            // Caminho relativo para a imagem, que será salvo no banco de dados
            var relativePath = $"assets/images/{fileName}";

            // URL completa para a imagem (se necessário no frontend)
            var imageUrl = $"{Request.Scheme}://{Request.Host}/{relativePath}";

            // Salve o caminho relativo no banco de dados ou retorne a URL
            return Ok(new
            {
                url = imageUrl,    // URL completa (se necessário para o frontend)
                path = relativePath  // Caminho relativo para armazenar no banco de dados
            });
        }




        // PUT: api/Raffles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRaffle(int id, Raffle raffle)
        {
            var existingRaffle = await _context.Raffles.FindAsync(id);

            if (existingRaffle == null)
            {
                return NotFound("Rifa não encontrada.");
            }

            existingRaffle.Title = raffle.Title;
            existingRaffle.Description = raffle.Description;
            existingRaffle.Price = raffle.Price;
            existingRaffle.Total_Numbers = raffle.Total_Numbers;
            existingRaffle.Start_Date = raffle.Start_Date;
            existingRaffle.End_Date = raffle.End_Date;
            existingRaffle.Image_Url = raffle.Image_Url;
            existingRaffle.UpdatedAt = DateTime.Now;

            try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateException ex)
    {
        return StatusCode(500, $"Erro ao atualizar rifa: {ex.Message}");
    }

    return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRaffle(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Encontrar e deletar transações PIX relacionadas
                var pixTransactions = await _context.Pix_Transactions
                    .Include(p => p.NumberSold)
                    .Where(p => p.NumberSold.RaffleId == id)
                    .ToListAsync();

                _context.Pix_Transactions.RemoveRange(pixTransactions);

                // 2. Deletar números vendidos
                var numbersSold = await _context.Numbers_Sold
                    .Where(n => n.RaffleId == id)
                    .ToListAsync();

                _context.Numbers_Sold.RemoveRange(numbersSold);

                // 3. Deletar a rifa
                var raffle = await _context.Raffles.FindAsync(id);
                if (raffle == null)
                    return NotFound();

                _context.Raffles.Remove(raffle);

                // 4. Executar todas as operações
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Não foi possível deletar a rifa");
            }
        }


        private bool RaffleExists(int id)
        {
            return _context.Raffles.Any(e => e.Id == id);
        }

        [HttpPost("{id}/sortear")]
        public async Task<IActionResult> SortearNumero(int id)
        {
            var (numeroSorteado, nomeComprador) = await _raffleService.SortearNumeroAsync(id);

            if (numeroSorteado == null)
                return NotFound();

            return Ok(new { numeroSorteado, nomeComprador });
        }
    }
}
