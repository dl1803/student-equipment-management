using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.Modules.BaoTri;
using QuanLyMuonTra.Modules.DanhMuc;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.Modules.MuonTra;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh;
using QuanLyMuonTra.Modules.QuyDinh.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.Modules.ViPham;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.IntegrationTests.DanhMuc;

internal static class XoaNgungTestSupport
{
    internal const string MaNv = "019c0000-0000-7000-8000-0000000d0a01";

    internal static ServiceProvider TaoDichVu(Func<IServiceCollection, IServiceCollection>? themModule = null)
    {
        var dichVu = new ServiceCollection()
            .AddInfrastructure(
                KetNoiKiemThu.LayChuoiKetNoi(),
                [
                    typeof(NguoiDungHeThong).Assembly,
                    typeof(ThietBi).Assembly,
                    typeof(ThamChieuMuonTraProvider).Assembly,
                    typeof(KhungPhat).Assembly,
                ])
            .AddScoped<IAdvisoryLockService, AdvisoryLockService>()
            .AddDanhMucModule();
        return (themModule is null ? dichVu : themModule(dichVu)).BuildServiceProvider();
    }

    internal static ServiceProvider TaoDichVuDayDu() =>
        TaoDichVu(s => s.AddQuyDinhModule().AddMuonTraModule().AddBaoTriModule().AddViPhamModule());

    internal static async Task NapDuLieuThuAsync(AppDbContext db)
    {
        var duongDan = Path.Combine(AppContext.BaseDirectory, "MuonTra", "D02-V-du-lieu-thu.sql");
        await db.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync(duongDan));
    }

    internal static Task<List<string>> LayChuoiAsync(AppDbContext db, FormattableString sql) =>
        db.Database.SqlQuery<string>(sql).ToListAsync();

    internal static Task<int> DemAsync(AppDbContext db, FormattableString sql) =>
        db.Database.SqlQuery<int>(sql).SingleAsync();
}
