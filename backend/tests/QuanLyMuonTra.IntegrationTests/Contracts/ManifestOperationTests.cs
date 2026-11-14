using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace QuanLyMuonTra.IntegrationTests.Contracts;

public class ManifestOperationTests(ITestOutputHelper output)
{
    private const string DaCai = "DA_CAI";
    private const int SoThaoTacBaseline = 88;

    [Fact]
    public void Manifest_KhopChinhXacVoiCacThaoTacDaCaiTrongMa()
    {
        var manifest = DocManifest();
        var trongMa = LayThaoTacTrongMa();

        var tatCa = manifest.Select(m => m.Khoa).ToHashSet();
        var daCai = manifest.Where(m => m.TrangThai == DaCai).Select(m => m.Khoa).ToHashSet();

        output.WriteLine($"Manifest: {tatCa.Count} thao tác; đã cài: {daCai.Count}; còn thiếu: {tatCa.Count - daCai.Count}");

        Assert.True(tatCa.Count >= SoThaoTacBaseline);
        Assert.Empty(trongMa.Where(k => !tatCa.Contains(k)).Order());
        Assert.Empty(trongMa.Where(k => tatCa.Contains(k) && !daCai.Contains(k)).Order());
        Assert.Empty(daCai.Where(k => !trongMa.Contains(k)).Order());
    }

    private static List<(string Khoa, string TrangThai)> DocManifest()
    {
        var duongDan = Path.Combine(AppContext.BaseDirectory, "C0-manifest-operation.csv");

        return File.ReadAllLines(duongDan)
            .Skip(1)
            .Where(dong => !string.IsNullOrWhiteSpace(dong))
            .Select(dong => dong.Split(',').Select(o => o.Trim('"')).ToArray())
            .Select(o => (Khoa: $"{o[0].ToUpperInvariant()} {o[1]}", TrangThai: o[2]))
            .ToList();
    }

    private static HashSet<string> LayThaoTacTrongMa()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=khong-dung;Username=khong-dung;Password=khong-dung");
            });

        var nguon = factory.Services.GetRequiredService<EndpointDataSource>();
        var ketQua = new HashSet<string>();

        foreach (var endpoint in nguon.Endpoints.OfType<RouteEndpoint>())
        {
            var duongDan = "/" + Regex.Replace(endpoint.RoutePattern.RawText ?? string.Empty, @"\{([^}:?]+)[^}]*\}", "{$1}").TrimStart('/');
            if (duongDan.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                duongDan.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var phuongThuc = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? ["GET"];

            foreach (var method in phuongThuc)
            {
                ketQua.Add($"{method.ToUpperInvariant()} {duongDan}");
            }
        }

        return ketQua;
    }
}

