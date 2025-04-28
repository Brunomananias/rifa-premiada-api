using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Rifa.Services
{
    public class RaffleService
    {
        private readonly AppDbContext _context;

        public RaffleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(int? numeroSorteado, string nomeComprador)> SortearNumeroAsync(int idRifaEscolhido)
        {
            // Buscar as vendas de números e os compradores (join com users)
            var resultado = await _context.Numbers_Sold
                .Where(ns => ns.RaffleId == idRifaEscolhido && ns.PaymentStatus == "Paid")
                .Join(_context.Users,
                      ns => ns.UserId,
                      u => u.Id,
                      (ns, u) => new { ns.Numbers, u.Name })
                .ToListAsync();

            if (!resultado.Any())
                return (null, null);

            var numerosDisponiveis = resultado
                .SelectMany(r => r.Numbers.Split(','))
                .Select(s => int.Parse(s.Trim()))
                .ToList();

            var random = new Random();
            var numeroSorteado = numerosDisponiveis[random.Next(numerosDisponiveis.Count)];

            // Obter o nome do comprador do número sorteado
            var comprador = resultado.FirstOrDefault(r => r.Numbers.Contains(numeroSorteado.ToString()));
            var nomeComprador = comprador?.Name;

            return (numeroSorteado, nomeComprador);
        }


    }

}
