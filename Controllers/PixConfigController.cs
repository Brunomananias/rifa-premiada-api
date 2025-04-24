using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc;

namespace API_Rifa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PixConfigController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PixConfigController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PixConfig>> Get()
        {
            var config = await _context.PixConfigs.FirstOrDefaultAsync();
            return config ?? new PixConfig();
        }

        [HttpPost]
        public async Task<IActionResult> SaveConfig(PixConfig model)
        {
            var config = await _context.PixConfigs.FirstOrDefaultAsync();

            if (config == null)
            {
                _context.PixConfigs.Add(model);
            }
            else
            {
                config.PixKey = model.PixKey;
                config.PixCopiaCola = model.PixCopiaCola;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{raffleId}")]
        public async Task<ActionResult<PixConfig>> GetPixConfigForRaffle(int raffleId)
        {
            var association = await _context.RifaPixAssociations
                .Include(r => r.PixConfig) // Inclui a configuração Pix associada
                .FirstOrDefaultAsync(r => r.RaffleId == raffleId);

            if (association == null)
            {
                return NotFound();
            }

            return association.PixConfig;
        }



    }

}
