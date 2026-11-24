using Npgsql;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.IntegrationTests.MuonTra;

namespace QuanLyMuonTra.IntegrationTests.BaoTri;

public class BaoTriDuLieuThuTests
{
    private const string BieuThucNgayDenHan = """
        COALESCE((bt.lan_cuoi AT TIME ZONE 'Asia/Ho_Chi_Minh')::date, t.ngay_nhap_kho) + l.chu_ky_bao_tri
        """;

    private const string TruyVanDenHan = """
        FROM danh_muc.thiet_bi t
        JOIN danh_muc.loai_thiet_bi l ON l.ma_loai = t.ma_loai
        LEFT JOIN LATERAL (
            SELECT max(b.ngay_hoan_thanh) AS lan_cuoi
            FROM bao_tri.bao_tri_sua_chua b
            WHERE b.ma_thiet_bi = t.ma_thiet_bi
              AND b.loai_thuc_hien = 'BAO_TRI_DINH_KY'
              AND b.ngay_hoan_thanh IS NOT NULL) bt ON true
        WHERE t.ma_thiet_bi LIKE 'D12-TB-%'
          AND t.tinh_trang IN ('SAN_SANG', 'DANG_DUOC_MUON')
        """;

    [CanKetNoiTestFact]
    public async Task UC11_DuLieuThuD12_NapDuMuoiHaiThietBi()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await NapDuLieuThuAsync(gd);

        var soThietBi = await DocChuoiAsync(gd, "SELECT count(*)::text FROM danh_muc.thiet_bi WHERE ma_thiet_bi LIKE 'D12-TB-%'");

        Assert.Equal("12", Assert.Single(soThietBi));
    }

    [CanKetNoiTestFact]
    public async Task UC11_DuLieuThuD12_NgayDenHanVaThuTuKhopOracle()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await NapDuLieuThuAsync(gd);
        var oracle = await OracleCsv.DocAsync("BaoTri", "D12-N-oracle-bao-tri.csv");

        var ngayDenHanThucTe = await DocChuoiAsync(
            gd,
            $"SELECT t.ma_thiet_bi || '|' || to_char({BieuThucNgayDenHan}, 'YYYY-MM-DD') {TruyVanDenHan} ORDER BY t.ma_thiet_bi");
        var thuTuThucTe = await DocChuoiAsync(
            gd,
            $"SELECT t.ma_thiet_bi {TruyVanDenHan} AND @ngayThamChieu::date >= {BieuThucNgayDenHan} ORDER BY {BieuThucNgayDenHan}, t.ma_thiet_bi",
            ("ngayThamChieu", "2026-11-23"));

        var ngayDenHanKyVong = oracle
            .Where(d => d["NgayDenHan"] != string.Empty)
            .Select(d => d["MaThietBi"] + "|" + d["NgayDenHan"])
            .OrderBy(d => d, StringComparer.Ordinal)
            .ToList();
        var thuTuKyVong = oracle
            .Where(d => d["ThuTuDenHanTai20261123"] != string.Empty)
            .OrderBy(d => int.Parse(d["ThuTuDenHanTai20261123"]))
            .Select(d => d["MaThietBi"])
            .ToList();
        Assert.Equal(ngayDenHanKyVong, ngayDenHanThucTe);
        Assert.Equal(thuTuKyVong, thuTuThucTe);
    }

    [CanKetNoiTestFact]
    public async Task UC11_DuLieuThuD12_ChoTiepNhanVaBanGhiMoKhopOracle()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await NapDuLieuThuAsync(gd);
        var oracle = await OracleCsv.DocAsync("BaoTri", "D12-N-oracle-bao-tri.csv");

        var choTiepNhanThucTe = await DocChuoiAsync(gd, """
            SELECT t.ma_thiet_bi
            FROM danh_muc.thiet_bi t
            WHERE t.ma_thiet_bi LIKE 'D12-TB-%'
              AND t.tinh_trang = 'DANG_BAO_TRI_SUA_CHUA'
              AND NOT EXISTS (
                SELECT 1 FROM bao_tri.bao_tri_sua_chua b
                WHERE b.ma_thiet_bi = t.ma_thiet_bi AND b.ket_qua IS NULL)
            ORDER BY t.ma_thiet_bi
            """);
        var banGhiMoThucTe = await DocChuoiAsync(gd, """
            SELECT ma_thiet_bi
            FROM bao_tri.bao_tri_sua_chua
            WHERE ket_qua IS NULL AND ma_thiet_bi LIKE 'D12-TB-%'
            ORDER BY ma_thiet_bi
            """);

        Assert.Equal(LayMa(oracle, "ChoTiepNhanSuaChua"), choTiepNhanThucTe);
        Assert.Equal(LayMa(oracle, "CoBanGhiMo"), banGhiMoThucTe);
    }

    [CanKetNoiTestFact]
    public async Task UC11_DuLieuThuD12_DangMuonVaConBaoHanhKhopOracle()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await NapDuLieuThuAsync(gd);
        var oracle = await OracleCsv.DocAsync("BaoTri", "D12-N-oracle-bao-tri.csv");

        var dangMuonThucTe = await DocChuoiAsync(gd, """
            SELECT c.ma_thiet_bi
            FROM muon_tra.chi_tiet_phieu_muon c
            JOIN muon_tra.phieu_muon p ON p.ma_phieu = c.ma_phieu
            WHERE p.trang_thai IN ('DANG_MUON', 'QUA_HAN')
              AND c.ket_qua_kiem_tra IS NULL
              AND c.ma_thiet_bi LIKE 'D12-TB-%'
            ORDER BY c.ma_thiet_bi
            """);
        var conBaoHanhThucTe = await DocChuoiAsync(
            gd,
            "SELECT ma_thiet_bi FROM danh_muc.thiet_bi WHERE ma_thiet_bi LIKE 'D12-TB-%' AND thoi_han_bao_hanh >= @ngayThamChieu::date ORDER BY ma_thiet_bi",
            ("ngayThamChieu", "2026-11-23"));

        var dangMuonKyVong = oracle
            .Where(d => d["TinhTrang"] == "DANG_DUOC_MUON")
            .Select(d => d["MaThietBi"])
            .OrderBy(d => d, StringComparer.Ordinal)
            .ToList();
        Assert.Equal(dangMuonKyVong, dangMuonThucTe);
        Assert.Equal(LayMa(oracle, "ConBaoHanhTai20261123"), conBaoHanhThucTe);
    }

    private static List<string> LayMa(IReadOnlyList<IReadOnlyDictionary<string, string>> oracle, string cot)
    {
        return oracle
            .Where(d => d[cot] == "CO")
            .Select(d => d["MaThietBi"])
            .OrderBy(d => d, StringComparer.Ordinal)
            .ToList();
    }

    private static async Task NapDuLieuThuAsync(GiaoDichTestRollback gd)
    {
        var duongDan = Path.Combine(AppContext.BaseDirectory, "BaoTri", "D12-N-du-lieu-thu-bao-tri.sql");
        var sql = await File.ReadAllTextAsync(duongDan);
        await using var lenh = new NpgsqlCommand(sql, gd.KetNoi, gd.GiaoDich);
        await lenh.ExecuteNonQueryAsync();
    }

    private static async Task<List<string>> DocChuoiAsync(GiaoDichTestRollback gd, string sql, params (string Ten, string GiaTri)[] thamSo)
    {
        await using var lenh = new NpgsqlCommand(sql, gd.KetNoi, gd.GiaoDich);
        foreach (var (ten, giaTri) in thamSo)
        {
            lenh.Parameters.AddWithValue(ten, giaTri);
        }

        var ketQua = new List<string>();
        await using var doc = await lenh.ExecuteReaderAsync();
        while (await doc.ReadAsync())
        {
            ketQua.Add(doc.GetString(0));
        }

        return ketQua;
    }
}
