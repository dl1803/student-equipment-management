using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using QuanLyMuonTra.Infrastructure.LuuTru;
using QuanLyMuonTra.IntegrationTests.Anh;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public sealed class MoiTruongTichHopMuonTra : IAsyncDisposable
{
    private const string BiMatJwt = "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len";
    private readonly WebApplicationFactory<Program> _factory;
    private readonly AsyncServiceScope _phamVi;
    private readonly IDbContextTransaction? _giaoDich;

    private MoiTruongTichHopMuonTra(
        WebApplicationFactory<Program> factory,
        AsyncServiceScope phamVi,
        IDbContextTransaction? giaoDich,
        AnhStorageGia luuTru)
    {
        _factory = factory;
        _phamVi = phamVi;
        _giaoDich = giaoDich;
        LuuTru = luuTru;
        Db = phamVi.ServiceProvider.GetRequiredService<DbContext>();
    }

    public DbContext Db { get; }

    public AnhStorageGia LuuTru { get; }

    public static async Task<MoiTruongTichHopMuonTra> TaoAsync(
        Guid maNv,
        DateTimeOffset bayGio,
        bool hoanTacCuoiTest,
        Action<IServiceCollection>? tuyChinh = null)
    {
        var luuTru = new AnhStorageGia();
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Default", KetNoiKiemThu.LayChuoiKetNoi());
                builder.UseSetting("Jwt:Secret", BiMatJwt);
                builder.UseSetting("Jwt:Issuer", "quan-ly-muon-tra-d10v");
                builder.UseSetting("Jwt:Audience", "quan-ly-muon-tra-d10v");
                builder.ConfigureTestServices(dichVu =>
                {
                    dichVu.AddSingleton<IClock>(new DongHoCoDinh(bayGio));
                    dichVu.AddSingleton<ICurrentUser>(new NguoiDungGia(maNv));
                    dichVu.AddSingleton<IAnhStorage>(luuTru);
                    tuyChinh?.Invoke(dichVu);
                });
            });

        var phamVi = factory.Services.CreateAsyncScope();
        IDbContextTransaction? giaoDich = null;
        if (hoanTacCuoiTest)
        {
            giaoDich = await phamVi.ServiceProvider.GetRequiredService<DbContext>().Database.BeginTransactionAsync();
        }

        return new MoiTruongTichHopMuonTra(factory, phamVi, giaoDich, luuTru);
    }

    public T Lay<T>() where T : notnull => _phamVi.ServiceProvider.GetRequiredService<T>();

    public async Task ThucThiSqlAsync(string sql)
    {
        var ketNoi = (NpgsqlConnection)Db.Database.GetDbConnection();
        if (ketNoi.State != ConnectionState.Open)
        {
            await ketNoi.OpenAsync();
        }

        await using var lenh = new NpgsqlCommand(sql, ketNoi);
        if (Db.Database.CurrentTransaction is { } giaoDich)
        {
            lenh.Transaction = (NpgsqlTransaction)giaoDich.GetDbTransaction();
        }

        await lenh.ExecuteNonQueryAsync();
    }

    public async Task NapDuLieuAsync(string tenTep, string sqlQuyenPhong)
    {
        var duongDan = Path.Combine(AppContext.BaseDirectory, "MuonTra", tenTep);
        await ThucThiSqlAsync(await File.ReadAllTextAsync(duongDan));
        await ThucThiSqlAsync(sqlQuyenPhong);
    }

    public async ValueTask DisposeAsync()
    {
        if (_giaoDich is not null)
        {
            await _giaoDich.DisposeAsync();
        }

        await _phamVi.DisposeAsync();
        await _factory.DisposeAsync();
    }
}
