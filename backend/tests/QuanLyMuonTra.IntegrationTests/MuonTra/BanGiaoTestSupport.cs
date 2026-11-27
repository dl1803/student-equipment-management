using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

internal static class BanGiaoTestSupport
{
    public const string FileDuLieu = "D14-V-du-lieu-thu.sql";
    public const string SvA = "D14-SV-A";
    public const string SvB = "D14-SV-B";
    public const string PmA = "PM-261126-914001";
    public const string PmB = "PM-261126-914002";
    public const string PmDaTra = "PM-261120-914003";
    public const string T1 = "D14-TB-T1";
    public const string T5 = "D14-TB-T5";
    public const string T6 = "D14-TB-T6";
    public const string T7 = "D14-TB-T7";
    public const string T8 = "D14-TB-T8";

    public static readonly Guid Nv1 = Guid.Parse("019c0000-0000-7000-8000-0000000d14a1");
    public static readonly Guid P1 = Guid.Parse("019c0000-0000-7000-8000-0000000d14b1");
    public static readonly Guid P2 = Guid.Parse("019c0000-0000-7000-8000-0000000d14b2");
    public static readonly Guid CtT1 = Guid.Parse("019c0000-0000-7000-8000-0000000d14f1");
    public static readonly Guid CtT5 = Guid.Parse("019c0000-0000-7000-8000-0000000d14f5");

    public static async Task<DocPhieuMuonTestSupport.DocPhieuTestContext> TaoContextAsync(GiaoDichTestRollback gd)
    {
        await gd.NapDuLieuThuAsync(FileDuLieu);
        var options = new DbContextOptionsBuilder<DocPhieuMuonTestSupport.DocPhieuTestContext>().UseNpgsql(gd.KetNoi).Options;
        var db = new DocPhieuMuonTestSupport.DocPhieuTestContext(options);
        await db.Database.UseTransactionAsync(gd.GiaoDich, CancellationToken.None);

        return db;
    }

    public static Task<string> DauVetAsync(DbContext db)
    {
        return db.Database.SqlQuery<string>($"""
            SELECT md5(
                coalesce((SELECT string_agg(c::text, '|' ORDER BY c.ma_chi_tiet) FROM muon_tra.chi_tiet_phieu_muon c WHERE c.ma_thiet_bi LIKE 'D14-TB-%'), '') ||
                coalesce((SELECT string_agg(p::text, '|' ORDER BY p.ma_phieu) FROM muon_tra.phieu_muon p WHERE p.ma_sv LIKE 'D14-SV-%'), '') ||
                coalesce((SELECT string_agg(t::text, '|' ORDER BY t.ma_thiet_bi) FROM danh_muc.thiet_bi t WHERE t.ma_thiet_bi LIKE 'D14-TB-%'), '')) AS "Value"
            """).SingleAsync();
    }
}
