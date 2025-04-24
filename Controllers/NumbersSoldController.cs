using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.AspNetCore.Mvc;
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

        // GET: api/NumbersSold/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NumberSold>> GetNumberSold(int id)
        {
            var numberSold = await _context.Numbers_Sold
                .Include(ns => ns.Raffle)
                .Include(ns => ns.UserId)
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
    }
}
