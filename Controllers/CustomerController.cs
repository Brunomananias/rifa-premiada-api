using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Rifa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
        {
            var customers = await _context.Customers.ToListAsync();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetById(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [HttpPost]
        public async Task<ActionResult<Customer>> Create(Customer dto)
        {
            var customer = new Customer
            {
                Name = dto.Name,
                Whatsapp = dto.Whatsapp,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Customer dto)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            customer.Name = dto.Name;
            customer.Whatsapp = dto.Whatsapp;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("customers")]
        public async Task<ActionResult<IEnumerable<object>>> GetCompradores()
        {
            var customers = await _context.Customers
                .Include(c => c.NumbersSold)
                .ToListAsync();

            var compradores = customers
                .Where(c => c.NumbersSold.Any(ns => ns.PaymentStatus == "paid"))
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Whatsapp,
                    QuantidadeNumeros = c.NumbersSold
                        .Where(ns => ns.PaymentStatus == "paid")
                        .Sum(ns => string.IsNullOrWhiteSpace(ns.Numbers)
                            ? 0
                            : ns.Numbers.Split(',', StringSplitOptions.RemoveEmptyEntries).Length),
                    TotalPago = c.NumbersSold
                        .Where(ns => ns.PaymentStatus == "paid")
                        .Sum(ns => ns.Value),
                    statusPagamento = "Pago",
                    DataCadastro = c.CreatedAt
                })
                .ToList();

            return Ok(compradores);
        }

    }
}