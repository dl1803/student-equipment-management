using Microsoft.AspNetCore.Identity;
using Npgsql;
using QuanLyMuonTra.IntegrationTests.Database;
using Xunit;
using Xunit.Abstractions;

namespace QuanLyMuonTra.IntegrationTests.SinhVien;

public sealed class ThemSinhVienScriptTests(ITestOutputHelper output)
{
    [Fact]
    public async Task TaoAccSinhVien24520000()
    {
        var chuoiKetNoi = KetNoiKiemThu.LayChuoiKetNoi();
        var bam = new PasswordHasher<object>().HashPassword(null!, "Test@1234");
        output.WriteLine($"Mat khau bam: {bam}");

        await using var conn = await KetNoiKiemThu.MoAsync();

        // 1. Truong gia lap: sinh_vien_truong
        await using (var cmd = new NpgsqlCommand($"""
            INSERT INTO truong_gia_lap.sinh_vien_truong (
                ma_sv, mat_khau_bam, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky
            )
            VALUES (
                '24520000', @bam, 'Sinh Viên 24520000', 'KTPM2024', 'Công nghệ Phần mềm', '24520000@gm.uit.edu.vn', 'DAI_HOC', DATE '2028-12-31', 'DANG_HOC', 'HK1-2026'
            )
            ON CONFLICT (ma_sv) DO UPDATE SET
                mat_khau_bam = EXCLUDED.mat_khau_bam,
                ho_ten = EXCLUDED.ho_ten,
                email = EXCLUDED.email;
            """, conn))
        {
            cmd.Parameters.AddWithValue("bam", bam);
            var dong = await cmd.ExecuteNonQueryAsync();
            output.WriteLine($"truong_gia_lap.sinh_vien_truong rows affected: {dong}");
        }

        // 2. App: sinh_vien.sinh_vien
        await using (var cmd = new NpgsqlCommand("""
            INSERT INTO sinh_vien.sinh_vien (
                ma_sv, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky, trang_thai_quyen_muon
            )
            VALUES (
                '24520000', 'Sinh Viên 24520000', 'KTPM2024', 'Công nghệ Phần mềm', '24520000@gm.uit.edu.vn', 'DAI_HOC', DATE '2028-12-31', 'DANG_HOC', 'HK1-2026', 'BINH_THUONG'
            )
            ON CONFLICT (ma_sv) DO UPDATE SET
                ho_ten = EXCLUDED.ho_ten,
                email = EXCLUDED.email,
                trang_thai_quyen_muon = 'BINH_THUONG';
            """, conn))
        {
            var dong = await cmd.ExecuteNonQueryAsync();
            output.WriteLine($"sinh_vien.sinh_vien rows affected: {dong}");
        }

        // 3. Verify
        await using (var cmd = new NpgsqlCommand("SELECT ma_sv, ho_ten, email, ma_nhom FROM sinh_vien.sinh_vien WHERE ma_sv = '24520000'", conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            Assert.True(await reader.ReadAsync());
            output.WriteLine($"Verified sinh_vien: ma_sv={reader.GetString(0)}, ho_ten={reader.GetString(1)}, email={reader.GetString(2)}, ma_nhom={reader.GetString(3)}");
        }
    }
}

