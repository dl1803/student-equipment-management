using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.DanhMuc;
using QuanLyMuonTra.Modules.BaoTri;
using QuanLyMuonTra.Modules.BaoTri.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.BaoTri;

internal static class BaoTriTestSupport
{
    internal const string MaNv = "019c0000-0000-7000-8000-0000000c1221";
    internal const string MaPhongP1 = "019c0000-0000-7000-8000-0000000c1201";
    internal const string MaPhongP2 = "019c0000-0000-7000-8000-0000000c1202";
    internal const string MaLoaiL1 = "019c0000-0000-7000-8000-0000000c1211";

    internal static readonly DateTimeOffset BayGio = new(2026, 11, 23, 5, 0, 0, TimeSpan.Zero);

    internal static ServiceProvider TaoDichVu() =>
        XoaNgungTestSupport.TaoDichVu(s => s
            .AddBaoTriModule()
            .AddEfConfigurationsFromAssembly(typeof(BaoTriSuaChua).Assembly)
            .AddMemoryCache()
            .AddScoped<IQuyenPhongService, QuyenPhongService>()
            .AddSingleton<IClock>(new DongHoCoDinh(BayGio)));

    private sealed class DongHoCoDinh(DateTimeOffset thoiDiem) : IClock
    {
        public DateTimeOffset UtcNow => thoiDiem;
    }
}

internal sealed class BaoTriMoiTruong : IAsyncDisposable
{
    private readonly ServiceProvider _dichVu;
    private readonly AsyncServiceScope _phamVi;
    private readonly IDbContextTransaction _giaoDich;

    private BaoTriMoiTruong(
        ServiceProvider dichVu,
        AsyncServiceScope phamVi,
        AppDbContext db,
        IDbContextTransaction giaoDich)
    {
        _dichVu = dichVu;
        _phamVi = phamVi;
        Db = db;
        _giaoDich = giaoDich;
    }

    public AppDbContext Db { get; }

    public static async Task<BaoTriMoiTruong> TaoAsync()
    {
        var dichVu = BaoTriTestSupport.TaoDichVu();
        var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        var giaoDich = await db.Database.BeginTransactionAsync();
        await NapTepAsync(db, "D12-N-du-lieu-thu-bao-tri.sql");
        await NapTepAsync(db, "D13-N-quyen-phong.sql");
        return new BaoTriMoiTruong(dichVu, phamVi, db, giaoDich);
    }

    public T Lay<T>() where T : notnull => _phamVi.ServiceProvider.GetRequiredService<T>();

    public Task<int> ThucThiAsync(string sql) => Db.Database.ExecuteSqlRawAsync(sql);

    public Task<List<string>> LayChuoiAsync(FormattableString sql) =>
        Db.Database.SqlQuery<string>(sql).ToListAsync();

    public Task<int> DemAsync(FormattableString sql) =>
        Db.Database.SqlQuery<int>(sql).SingleAsync();

    public async ValueTask DisposeAsync()
    {
        await _giaoDich.DisposeAsync();
        await _phamVi.DisposeAsync();
        await _dichVu.DisposeAsync();
    }

    private static async Task NapTepAsync(AppDbContext db, string tenTep)
    {
        var duongDan = Path.Combine(AppContext.BaseDirectory, "BaoTri", tenTep);
        await db.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync(duongDan));
    }
}
