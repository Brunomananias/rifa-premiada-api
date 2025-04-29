using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using static Org.BouncyCastle.Math.EC.ECCurve;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    //public async Task<AuthResponse> Authenticate(LoginDto loginDto)
    //{
    //    var user = await _context.AdminUsers
    //        .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Status);

    //    if (user == null || !VerifyPassword(loginDto.Senha, user.Senha))
    //        return null;

    //    var token = GenerateJwtToken(user);

    //    return new AuthResponse
    //    {
    //        Token = token,
    //        Expiration = DateTime.Now.AddMinutes(_configuration.GetValue<int>("Jwt:ExpireMinutes")),
    //        Nome = user.Nome,
    //        Email = user.Email
    //    };
    //}

    // Services/AuthService.cs
    public User? AuthenticateUser(string email, string password)
    {
        // Exemplo com Entity Framework Core:
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
            return null;

        // Verifique a senha (use BCrypt ou similar!)
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        return isPasswordValid ? user : null;
    }

    public string GenerateJwtToken(int userId, string email)
    {
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email)
                // Adicione outras claims conforme necessário
            }),
            Expires = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JwtSettings:ExpirationDays")),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha512Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }
}