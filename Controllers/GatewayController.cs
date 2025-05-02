using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Rifa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GatewayController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GatewayController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveSettings([FromBody] GatewaySettings settings)
        {
            var existing = await _context.GatewaySettings
                .FirstOrDefaultAsync(s => s.UserId == settings.UserId);

            if (existing != null)
            {
                existing.ClientKey = settings.ClientKey;
                existing.ClientSecret = settings.ClientSecret;
            }
            else
            {
                _context.GatewaySettings.Add(settings);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<GatewaySettings>> GetSettings(int userId)
        {
            var settings = await _context.GatewaySettings.FirstOrDefaultAsync(s => s.UserId == userId);
            if (settings == null) return NotFound();
            return settings;
        }
    }

}
