using API_Rifa.Data;
using API_Rifa.Extensions;
using API_Rifa.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Drawing.Printing;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
app.MapControllers();

app.Run();
