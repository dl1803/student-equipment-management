using Npgsql;
using QuanLyMuonTra.IntegrationTests.Database;

namespace QuanLyMuonTra.IntegrationTests.SinhVien;

public sealed class KhongLuuMatKhauSinhVienTests
{
    [Fact]
    public async Task NFR05_CacSchemaUngDung_ChiCoMotCotMatKhauLaCuaNhanVienQuanTri()
    {
        await using var ketNoi = await KetNoiKiemThu.MoAsync();
        await using var lenh = new NpgsqlCommand("""
            SELECT table_schema || '.' || table_name || '.' || column_name
            FROM information_schema.columns
            WHERE table_schema IN ('tai_khoan','sinh_vien','danh_muc','quy_dinh','muon_tra','vi_pham','bao_tri','thong_bao','nhat_ky','luu_tru')
              AND (column_name ILIKE '%mat_khau%' OR column_name ILIKE '%password%')
            ORDER BY 1
            """, ketNoi);
        var cacCot = new List<string>();

        await using var doc = await lenh.ExecuteReaderAsync();
        while (await doc.ReadAsync())
        {
            cacCot.Add(doc.GetString(0));
        }

        Assert.Equal(new[] { "tai_khoan.nguoi_dung_he_thong.mat_khau_bam" }, cacCot);
    }
}

