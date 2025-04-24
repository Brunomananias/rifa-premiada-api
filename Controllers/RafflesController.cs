using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RifaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RafflesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RafflesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Raffles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Raffle>>> GetRaffles()
        {
            return await _context.Raffles.ToListAsync();
        }

        // GET: api/Raffles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Raffle>> GetRaffle(int id)
        {
            var raffle = await _context.Raffles.FindAsync(id);

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
            if (id != raffle.Id)
            {
                return BadRequest();
            }

            _context.Entry(raffle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RaffleExists(id))
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRaffle(int id)
        {
            var raffle = await _context.Raffles.FindAsync(id);
            if (raffle == null)
                return NotFound();

            // Deleta números vendidos primeiro
            var numbers = _context.Numbers_Sold.Where(n => n.RaffleId == id);
            _context.Numbers_Sold.RemoveRange(numbers);

            // Depois deleta a rifa
            _context.Raffles.Remove(raffle);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool RaffleExists(int id)
        {
            return _context.Raffles.Any(e => e.Id == id);
        }
    }
}
