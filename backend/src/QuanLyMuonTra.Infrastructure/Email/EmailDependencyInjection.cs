using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Infrastructure.Email;

public static class EmailDependencyInjection
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services, EmailOptions tuyChon)
    {
        var provider = string.IsNullOrWhiteSpace(tuyChon.Provider) ? "Log" : tuyChon.Provider.Trim();

        if (string.Equals(provider, "Log", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IEmailSender, LogEmailSender>();

            return services;
        }

        if (string.Equals(provider, "Http", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(tuyChon.ApiKey) || string.IsNullOrWhiteSpace(tuyChon.From))
            {
                throw new InvalidOperationException("Email__Provider=Http yêu cầu cấu hình Email__ApiKey và Email__From.");
            }

            var apiKey = tuyChon.ApiKey;
            var diaChiGui = tuyChon.From;
            services.AddSingleton<IEmailSender>(_ => new HttpEmailSender(TaoHttpClient(), apiKey, diaChiGui));

            return services;
        }

        throw new InvalidOperationException($"Email__Provider không hợp lệ: '{tuyChon.Provider}'. Chỉ chấp nhận Log hoặc Http.");
    }

    private static HttpClient TaoHttpClient() =>
        new(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) })
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
}

