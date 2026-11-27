using Microsoft.EntityFrameworkCore;
using static QuanLyMuonTra.IntegrationTests.MuonTra.BanGiaoTestSupport;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class DuLieuThuD14Tests
{
    [CanKetNoiTestFact]
    public async Task D14_DuLieuThu_NapDuocVaDungHinhDangChoLoD15()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);

        var soPhieu = await db.Database.SqlQuery<int>($"""
            SELECT count(*)::integer AS "Value" FROM muon_tra.phieu_muon WHERE ma_sv LIKE 'D14-SV-%'
            """).SingleAsync();
        var dangHieuLuc = await db.Database.SqlQuery<int>($"""
            SELECT count(*)::integer AS "Value" FROM muon_tra.chi_tiet_phieu_muon
            WHERE ma_thiet_bi LIKE 'D14-TB-%' AND ket_qua_kiem_tra IS NULL
            """).SingleAsync();
        var trongLo = await db.Database.SqlQuery<int>($"""
            SELECT count(*)::integer AS "Value" FROM muon_tra.chi_tiet_phieu_muon
            WHERE ma_phieu = {PmA} AND ket_qua_kiem_tra IS NULL
            """).SingleAsync();
        var thietBiKhongCoPhieuHieuLuc = await db.Database.SqlQuery<int>($"""
            SELECT count(*)::integer AS "Value" FROM danh_muc.thiet_bi t
            WHERE t.ma_thiet_bi LIKE 'D14-TB-%'
              AND NOT EXISTS (SELECT 1 FROM muon_tra.chi_tiet_phieu_muon c WHERE c.ma_thiet_bi = t.ma_thiet_bi AND c.ket_qua_kiem_tra IS NULL)
            """).SingleAsync();

        Assert.Equal(3, soPhieu);
        Assert.Equal(6, dangHieuLuc);
        Assert.Equal(4, trongLo);
        Assert.Equal(2, thietBiKhongCoPhieuHieuLuc);
    }
}
