using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace RifaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // POST: api/Users/register-or-get
        [HttpPost("register-or-get")]
        public async Task<ActionResult<User>> RegisterOrGetUser(User user)
        {
            // Verificar se já existe um usuário com o mesmo WhatsApp
            var existingUser = await _context.Users
                                             .FirstOrDefaultAsync(u => u.Whatsapp == user.Whatsapp);

            if (existingUser != null)
            {
                // Se encontrar, retorna o usuário existente
                return existingUser;
            }

            // Caso não encontre, cria um novo usuário
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [HttpGet("compradores")]
        public async Task<ActionResult<IEnumerable<object>>> GetCompradores()
        {
            var users = await _context.Users
                .Include(u => u.NumbersSold)
                .ToListAsync(); // forçar execução para trabalhar em memória

            var compradores = users
                .Where(u => u.NumbersSold.Any(ns => ns.PaymentStatus == "paid")) // Só usuários que têm pelo menos 1 pago
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Whatsapp,
                    QuantidadeNumeros = u.NumbersSold
                        .Where(ns => ns.PaymentStatus == "paid") // Só pega números pagos
                        .Sum(ns => string.IsNullOrWhiteSpace(ns.Numbers)
                            ? 0
                            : ns.Numbers.Split(',', StringSplitOptions.RemoveEmptyEntries).Length),
                    TotalPago = u.NumbersSold
                        .Where(ns => ns.PaymentStatus == "paid") // Só pega valor de números pagos
                        .Sum(ns => ns.Value),
                    statusPagamento = "Pago",
                    DataCadastro = u.Created_At.ToString("dd/MM/yyyy HH:mm")
                })
                .ToList();


            return Ok(compradores);
        }

        [HttpGet("user-email")]
        public async Task<ActionResult<string>> GetUserEmail([FromQuery] int idUser)
        {
            var userEmail = await _context.Users
                .Where(x => x.Id == idUser)
                .Select(x => x.Email)
                .FirstOrDefaultAsync(); // já que só retorna 1 e não uma lista

            if (string.IsNullOrEmpty(userEmail))
                return NotFound("Usuário não encontrado");

            return Ok(userEmail);
        }

        [HttpPut("plan")]
        public async Task<IActionResult> UpdateUserPlan([FromBody] UpdateUserPlanRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);

            if (user == null)
                return NotFound("Usuário não encontrado");

            user.Plan_id = request.PlanId;

            await _context.SaveChangesAsync();

            // (Opcional) Buscar o nome do plano, se quiser retornar
            var plan = await _context.Plans.FindAsync(request.PlanId);

            return Ok(new { newPlanId = user.Plan_id, newPlanName = plan?.Name });
        }

        [HttpGet("test-db")]
        public IActionResult TestDbConnection()
        {
            try
            {
                _context.Database.OpenConnection();
                _context.Database.CloseConnection();
                return Ok("Conexão com o banco de dados bem-sucedida");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro na conexão com o banco: {ex.Message}");
            }
        }


        public class UpdateUserPlanRequest
        {
            public int UserId { get; set; }
            public int PlanId { get; set; }
            public string PlanName { get; set; } // Se necessário
        }



    }
}
