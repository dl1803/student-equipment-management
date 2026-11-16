using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.C3;

[CollectionDefinition(C3BoSuuTap.Ten)]
public sealed class C3BoSuuTap
{
    public const string Ten = "C3-du-lieu-thu";
}

public sealed class C3TestContext(DbContextOptions<C3TestContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyDanhMucConfigurations();
        modelBuilder.ApplyQuyDinhConfigurations();
    }
}

public static class C3TestSupport
{
    public static async Task<C3TestContext> TaoContextAsync(GiaoDichTestRollback gd)
    {
        var options = new DbContextOptionsBuilder<C3TestContext>().UseNpgsql(gd.KetNoi).Options;
        var db = new C3TestContext(options);
        await db.Database.UseTransactionAsync(gd.GiaoDich);
        return db;
    }

    public static Task NapDuLieuThuAsync(GiaoDichTestRollback gd)
    {
        return NapTepAsync(gd, "C3-du-lieu-thu.sql");
    }

    public static Task NapDuLieuThuNccAsync(GiaoDichTestRollback gd)
    {
        return NapTepAsync(gd, "C3-du-lieu-thu-ncc.sql");
    }

    public static Task NapQuyDinhBatBuocAsync(GiaoDichTestRollback gd)
    {
        return NapTepAsync(gd, "quy-dinh-bat-buoc.sql");
    }

    public static async Task ThucThiAsync(GiaoDichTestRollback gd, string sql)
    {
        await using var lenh = new NpgsqlCommand(sql, gd.KetNoi, gd.GiaoDich);
        await lenh.ExecuteNonQueryAsync();
    }

    private static async Task NapTepAsync(GiaoDichTestRollback gd, string tenTep)
    {
        var sql = (await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, tenTep)))
            .Replace("BEGIN;", string.Empty)
            .Replace("COMMIT;", string.Empty);
        await ThucThiAsync(gd, sql);
    }
}
