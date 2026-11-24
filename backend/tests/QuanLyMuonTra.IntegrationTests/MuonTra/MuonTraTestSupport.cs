using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuanLyMuonTra.IntegrationTests.Database;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public sealed class CanKetNoiTestFactAttribute : FactAttribute
{
    public const string TenBienMoiTruong = "ConnectionStrings__Test";

    public CanKetNoiTestFactAttribute()
    {
        try
        {
            _ = KetNoiKiemThu.LayChuoiKetNoi();
        }
        catch
        {
            Skip = "Chưa cấu hình ConnectionStrings:Test (qua user-secrets hoặc biến môi trường).";
        }
    }
}

public sealed class GiaoDichTestRollback : IAsyncDisposable
{
    private GiaoDichTestRollback(NpgsqlConnection ketNoi, NpgsqlTransaction giaoDich, MuonTraTestContext db)
    {
        KetNoi = ketNoi;
        GiaoDich = giaoDich;
        Db = db;
    }

    public NpgsqlConnection KetNoi { get; }

    public NpgsqlTransaction GiaoDich { get; }

    public MuonTraTestContext Db { get; }

    public static async Task<GiaoDichTestRollback> BatDauAsync()
    {
        var chuoiKetNoi = KetNoiKiemThu.LayChuoiKetNoi();
        var ketNoi = new NpgsqlConnection(chuoiKetNoi);
        await ketNoi.OpenAsync();
        var giaoDich = await ketNoi.BeginTransactionAsync();
        var options = new DbContextOptionsBuilder<MuonTraTestContext>().UseNpgsql(ketNoi).Options;
        var db = new MuonTraTestContext(options);
        await db.Database.UseTransactionAsync(giaoDich);
        return new GiaoDichTestRollback(ketNoi, giaoDich, db);
    }

    public async Task NapDuLieuThuAsync(string tenTep = "D02-V-du-lieu-thu.sql")
    {
        var duongDan = Path.Combine(AppContext.BaseDirectory, "MuonTra", tenTep);
        var sql = await File.ReadAllTextAsync(duongDan);
        await using var lenh = new NpgsqlCommand(sql, KetNoi, GiaoDich);
        await lenh.ExecuteNonQueryAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await GiaoDich.DisposeAsync();
        await Db.DisposeAsync();
        await KetNoi.DisposeAsync();
    }
}
