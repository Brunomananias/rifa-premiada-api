using API_Rifa.Data;
using API_Rifa.Extensions;
using API_Rifa.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Drawing.Printing;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["JwtSettings:SecretKey"]
            )),
            ValidateIssuer = false, // Altere para true se usar Issuer
            ValidateAudience = false // Altere para true se usar Audience
        };
    });
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adicionar HttpClient
builder.Services.AddHttpClient(); // Linha adicionada

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.Configure<PaggueSettings>(builder.Configuration.GetSection("PaggueSettings"));

builder.Services.AddProjectServices();

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
// Ativar CORS
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapGet("/", () => "API Online!");
app.MapControllers();
var port = Environment.GetEnvironmentVariable("PORT") ?? "5167"; // Porta local como fallback
app.Run($"http://0.0.0.0:{port}");
