using System.Text;
using Npgsql;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using Xunit.Abstractions;

namespace QuanLyMuonTra.IntegrationTests.C3;

[Collection(C3BoSuuTap.Ten)]
public sealed class ExplainTruyVanDocCsdlTests(ITestOutputHelper output)
{
    private const string SoLuongThietBiDo = "2000";

    private const string CotThietBi =
        "t.ma_thiet_bi, t.ma_loai, l.ten_loai, t.ma_phong, p.ten_phong, t.ten_thiet_bi, t.mo_ta, " +
        "t.thong_so_ky_thuat, t.ngay_nhap_kho, t.thoi_han_bao_hanh, t.tinh_trang, t.vi_tri_ke, t.vi_tri_tu, " +
        "t.ngung_su_dung, (SELECT g.gia_tri FROM danh_muc.lich_su_gia_tri g WHERE g.ma_thiet_bi = t.ma_thiet_bi " +
        "ORDER BY g.ngay_cap_nhat DESC, g.ma_lich_su DESC LIMIT 1) AS gia_tri";

    private const string TuBangThietBi =
        " FROM danh_muc.thiet_bi t JOIN danh_muc.loai_thiet_bi l ON l.ma_loai = t.ma_loai " +
        "JOIN danh_muc.phong_thiet_bi p ON p.ma_phong = t.ma_phong";

    private static readonly (string Ten, string Sql)[] TruyVan =
    [
        ("DsThietBi trang 1, không lọc",
            "SELECT " + CotThietBi + TuBangThietBi + " ORDER BY t.ma_thiet_bi LIMIT 20 OFFSET 0"),
        ("DsThietBi lọc loại + phòng + tình trạng",
            "SELECT " + CotThietBi + TuBangThietBi +
            " WHERE t.ma_loai = '019c0000-0000-7000-8000-000000000011' " +
            "AND t.ma_phong = '019c0000-0000-7000-8000-000000000001' AND t.tinh_trang = 'SAN_SANG' " +
            "ORDER BY t.ma_thiet_bi LIMIT 20 OFFSET 0"),
        ("DsThietBi lọc theo tên",
            "SELECT " + CotThietBi + TuBangThietBi +
            " WHERE lower(t.ten_thiet_bi) LIKE '%explain 15%' ORDER BY t.ma_thiet_bi LIMIT 20 OFFSET 0"),
        ("DsThietBi tổng số",
            "SELECT count(*) FROM danh_muc.thiet_bi t"),
        ("LayThietBi theo mã",
            "SELECT " + CotThietBi + TuBangThietBi + " WHERE t.ma_thiet_bi = 'EXPL-00001'"),
        ("DsLoaiThietBi đang sử dụng",
            "SELECT ma_loai, ten_loai, mo_ta, hang_san_xuat, chu_ky_bao_tri, phi_tre_moi_ngay, ngung_su_dung " +
            "FROM danh_muc.loai_thiet_bi WHERE ngung_su_dung = false ORDER BY ten_loai, ma_loai LIMIT 20 OFFSET 0"),
        ("DsPhongThietBi",
            "SELECT ma_phong, ten_phong, co_so FROM danh_muc.phong_thiet_bi " +
            "ORDER BY ten_phong, ma_phong LIMIT 20 OFFSET 0"),
        ("DsNhaCungCap",
            "SELECT * FROM danh_muc.nha_cung_cap ORDER BY ten_ncc, ma_ncc LIMIT 20 OFFSET 0"),
        ("DsKhungPhat đang sử dụng",
            "SELECT * FROM quy_dinh.khung_phat WHERE dang_su_dung = true ORDER BY ty_le_phan_tram, ma_khung"),
        ("LayQuyDinhNhom",
            "SELECT * FROM quy_dinh.quy_dinh_muon WHERE ma_nhom = 'DAI_HOC'")
    ];

    [CanKetNoiTestFact]
    public async Task UC12_13_Explain_TruyVanDoc_GhiMocSoSanh()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await C3TestSupport.NapDuLieuThuNccAsync(gd);
        await C3TestSupport.ThucThiAsync(gd, SqlBoSungThietBi);

        output.WriteLine("Nguồn: DB kiểm thử, giao dịch rollback; dữ liệu = seed C3 + " + SoLuongThietBiDo + " thiết bị EXPL-*");
        foreach (var bang in new[] { "danh_muc.thiet_bi", "danh_muc.lich_su_gia_tri", "danh_muc.loai_thiet_bi", "danh_muc.phong_thiet_bi", "danh_muc.nha_cung_cap", "quy_dinh.khung_phat", "quy_dinh.quy_dinh_muon" })
        {
            await using var dem = new NpgsqlCommand("SELECT count(*) FROM " + bang, gd.KetNoi, gd.GiaoDich);
            output.WriteLine(bang + ": " + await dem.ExecuteScalarAsync() + " dòng");
        }

        foreach (var (ten, sql) in TruyVan)
        {
            var ketQua = await ChayExplainAsync(gd, sql);
            output.WriteLine("=== " + ten + " ===");
            output.WriteLine(ketQua);
            Assert.Contains("Execution Time", ketQua);
        }
    }

    private static async Task<string> ChayExplainAsync(GiaoDichTestRollback gd, string sql)
    {
        await using var lenh = new NpgsqlCommand("EXPLAIN (ANALYZE, BUFFERS) " + sql, gd.KetNoi, gd.GiaoDich);
        await using var doc = await lenh.ExecuteReaderAsync();
        var ketQua = new StringBuilder();
        while (await doc.ReadAsync())
        {
            ketQua.AppendLine(doc.GetString(0));
        }

        return ketQua.ToString();
    }

    private const string SqlBoSungThietBi =
        "INSERT INTO danh_muc.thiet_bi (ma_thiet_bi, ma_loai, ma_phong, ten_thiet_bi, ngay_nhap_kho) " +
        "SELECT 'EXPL-' || lpad(g::text, 5, '0'), '019c0000-0000-7000-8000-000000000011', " +
        "'019c0000-0000-7000-8000-000000000001', 'Thiết bị EXPLAIN ' || g, DATE '2026-01-05' " +
        "FROM generate_series(1, " + SoLuongThietBiDo + ") AS g; " +
        "INSERT INTO danh_muc.lich_su_gia_tri (ma_lich_su, ma_thiet_bi, gia_tri) " +
        "SELECT gen_random_uuid(), 'EXPL-' || lpad(g::text, 5, '0'), 1000000 " +
        "FROM generate_series(1, " + SoLuongThietBiDo + ") AS g; " +
        "ANALYZE danh_muc.thiet_bi; ANALYZE danh_muc.lich_su_gia_tri;";
}
