using API_Rifa.Services;

namespace API_Rifa.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddProjectServices(this IServiceCollection services)
        {
            services.AddScoped<RaffleService>();
            services.AddScoped<PagguePaymentService>();
            services.AddScoped<TokenService>();
        }
    }
}
