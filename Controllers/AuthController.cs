using API_Rifa.Data;
using API_Rifa.Models;
using API_Rifa.Request;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly AppDbContext _context;

    public AuthController(AuthService authService, AppDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Valide o usuário (ex: verifique email/senha no banco de dados)
        var user = _authService.AuthenticateUser(request.Email, request.Password);

        if (user == null)
            return Unauthorized();

        // Gere o token
        var token = _authService.GenerateJwtToken(user.Id, user.Email);

        return Ok(new { Token = token });
    }

    [HttpPost("auth/register")]
    public async Task<IActionResult> Register([FromBody] RegistroRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("E-mail já cadastrado.");

        var freePlan = await _context.Plans.FirstOrDefaultAsync(p => p.Name == "Plano Free");
        if (freePlan == null)
            return StatusCode(500, "Plano Free não configurado.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash,
            Whatsapp = request.Whatsapp,
            Document = request.Document,
            Status = true, 
            Is_Admin = false, 
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow,
            Last_Login = null
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _authService.GenerateJwtToken(user.Id, user.Email);

        return Ok(new
        {
            message = "Usuário registrado com sucesso!",
            token,
            user = new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Whatsapp,
                user.Document,
                user.Is_Admin
            }
        });
    }

}