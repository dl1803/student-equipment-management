using System.Text.RegularExpressions;
using Npgsql;

namespace QuanLyMuonTra.IntegrationTests.Database;

public class CauTrucCsdlTests
{
    private static readonly string[] CacSchema =
    [
        "tai_khoan", "sinh_vien", "danh_muc", "quy_dinh", "muon_tra", "vi_pham",
        "bao_tri", "thong_bao", "nhat_ky", "luu_tru", "truong_gia_lap"
    ];

    [Fact]
    public async Task CsdlMoi_CoDuMuoiMotSchemaVaBaMuoiLamBang()
    {
        await using var ketNoi = await KetNoiKiemThu.MoAsync();

        var soSchema = await DemAsync(ketNoi, "SELECT count(*) FROM pg_namespace WHERE nspname::text = ANY(@cacSchema)");
        var soBang = await DemAsync(ketNoi, "SELECT count(*) FROM pg_tables WHERE schemaname::text = ANY(@cacSchema)");

        Assert.Equal(11, soSchema);
        Assert.Equal(35, soBang);
    }

    [Fact]
    public async Task MoiBang_BatRlsVaChiCoPolicyChoAppBackend()
    {
        await using var ketNoi = await KetNoiKiemThu.MoAsync();

        var bangChuaBatRls = await LayDanhSachAsync(ketNoi, """
            SELECT n.nspname || '.' || c.relname
            FROM pg_class c
            JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE c.relkind = 'r' AND n.nspname::text = ANY(@cacSchema) AND NOT c.relrowsecurity
            ORDER BY 1
            """);
        var bangThieuPolicy = await LayDanhSachAsync(ketNoi, """
            SELECT t.schemaname || '.' || t.tablename
            FROM pg_tables t
            WHERE t.schemaname::text = ANY(@cacSchema)
              AND NOT EXISTS (
                SELECT 1 FROM pg_policies p
                WHERE p.schemaname = t.schemaname AND p.tablename = t.tablename AND 'app_backend' = ANY(p.roles))
            ORDER BY 1
            """);
        var policyChoRoleKhac = await LayDanhSachAsync(ketNoi, """
            SELECT schemaname || '.' || tablename || ' -> ' || policyname
            FROM pg_policies
            WHERE schemaname::text = ANY(@cacSchema) AND roles <> ARRAY['app_backend']::name[]
            ORDER BY 1
            """);

        Assert.Empty(bangChuaBatRls);
        Assert.Empty(bangThieuPolicy);
        Assert.Empty(policyChoRoleKhac);
    }

    [Fact]
    public async Task Policy_NhatKyChiDocVaThem_TruongGiaLapChiDoc()
    {
        await using var ketNoi = await KetNoiKiemThu.MoAsync();

        var nhatKy = await LayDanhSachAsync(ketNoi, "SELECT DISTINCT cmd FROM pg_policies WHERE schemaname = 'nhat_ky' ORDER BY 1");
        var truongGiaLap = await LayDanhSachAsync(ketNoi, "SELECT DISTINCT cmd FROM pg_policies WHERE schemaname = 'truong_gia_lap' ORDER BY 1");

        Assert.Equal(new[] { "INSERT", "SELECT" }, nhatKy.ToArray());
        Assert.Equal(new[] { "SELECT" }, truongGiaLap.ToArray());
    }

    [Theory]
    [InlineData("muon_tra.phieu_muon", "SELECT", true)]
    [InlineData("muon_tra.phieu_muon", "INSERT", true)]
    [InlineData("muon_tra.phieu_muon", "UPDATE", true)]
    [InlineData("muon_tra.phieu_muon", "DELETE", true)]
    [InlineData("muon_tra.phieu_muon", "TRUNCATE", false)]
    [InlineData("nhat_ky.nhat_ky_he_thong", "SELECT", true)]
    [InlineData("nhat_ky.nhat_ky_he_thong", "INSERT", true)]
    [InlineData("nhat_ky.nhat_ky_he_thong", "UPDATE", false)]
    [InlineData("nhat_ky.nhat_ky_he_thong", "DELETE", false)]
    [InlineData("nhat_ky.nhat_ky_he_thong", "TRUNCATE", false)]
    [InlineData("truong_gia_lap.sinh_vien_truong", "SELECT", true)]
    [InlineData("truong_gia_lap.sinh_vien_truong", "INSERT", false)]
    [InlineData("truong_gia_lap.sinh_vien_truong", "UPDATE", false)]
    [InlineData("truong_gia_lap.sinh_vien_truong", "DELETE", false)]
    public async Task QuyenAppBackend_DungThietKe(string bang, string quyen, bool duocPhep)
    {
        await using var ketNoi = await KetNoiKiemThu.MoAsync();
        await using var lenh = new NpgsqlCommand("SELECT has_table_privilege('app_backend', @bang, @quyen)", ketNoi);
        lenh.Parameters.AddWithValue("bang", bang);
        lenh.Parameters.AddWithValue("quyen", quyen);

        var thucTe = (bool)(await lenh.ExecuteScalarAsync())!;

        Assert.Equal(duocPhep, thucTe);
    }

    [Fact]
    public async Task NFR02_NhatKy_ChanUpdate()
    {
        await using var ketNoi = await KetNoiKiemThu.MoAsync();
        await using var giaoDich = await ketNoi.BeginTransactionAsync();
        var ma = await ThemDongNhatKyAsync(ketNoi, giaoDich);

        var loi = await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await using var lenh = new NpgsqlCommand("UPDATE nhat_ky.nhat_ky_he_thong SET thao_tac = 'SUA' WHERE ma_nhat_ky = @ma", ketNoi, giaoDich);
            lenh.Parameters.AddWithValue("ma", ma);
            await lenh.ExecuteNonQueryAsync();
        });

        Assert.Contains("chi duoc them moi", loi.MessageText);
    }

    [Fact]
    public async Task NFR02_NhatKy_ChanDelete()
    {
        await using var ketNoi = await KetNoiKiemThu.MoAsync();
        await using var giaoDich = await ketNoi.BeginTransactionAsync();
        var ma = await ThemDongNhatKyAsync(ketNoi, giaoDich);

        var loi = await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await using var lenh = new NpgsqlCommand("DELETE FROM nhat_ky.nhat_ky_he_thong WHERE ma_nhat_ky = @ma", ketNoi, giaoDich);
            lenh.Parameters.AddWithValue("ma", ma);
            await lenh.ExecuteNonQueryAsync();
        });

        Assert.Contains("chi duoc them moi", loi.MessageText);
    }

    [Fact]
    public async Task NFR02_NhatKy_ChanTruncate()
    {
        await using var ketNoi = await KetNoiKiemThu.MoAsync();
        await using var giaoDich = await ketNoi.BeginTransactionAsync();

        var loi = await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await using var lenh = new NpgsqlCommand("TRUNCATE nhat_ky.nhat_ky_he_thong", ketNoi, giaoDich);
            await lenh.ExecuteNonQueryAsync();
        });

        Assert.Contains("chi duoc them moi", loi.MessageText);
    }

    [Theory]
    [InlineData("muon_tra", "phieu_yeu_cau_muon", "ma_yeu_cau", "YC")]
    [InlineData("muon_tra", "dang_ky_cho", "ma_dang_ky", "DK")]
    [InlineData("muon_tra", "phieu_muon", "ma_phieu", "PM")]
    [InlineData("vi_pham", "bien_ban_vi_pham", "ma_bien_ban", "BB")]
    [InlineData("bao_tri", "quyet_dinh_thanh_ly", "ma_quyet_dinh", "QD")]
    public async Task MaChungTu_SequenceVuotSauChuSo_KhongBiCat(string schema, string bang, string cot, string tienTo)
    {
        await using var ketNoi = await KetNoiKiemThu.MoAsync();
        var bieuThuc = await LayBieuThucMacDinhAsync(ketNoi, schema, bang, cot);

        foreach (var (soThuTu, hauToMongDoi) in new[] { (1L, "000001"), (999999L, "999999"), (1000000L, "1000000") })
        {
            var thayNextval = Regex.Replace(bieuThuc, @"nextval\('[^']+'::regclass\)", $"({soThuTu}::bigint)");
            Assert.DoesNotContain("nextval", thayNextval);

            await using var lenh = new NpgsqlCommand("SELECT " + thayNextval, ketNoi);
            var ma = (string)(await lenh.ExecuteScalarAsync())!;

            Assert.Matches($"^{tienTo}-[0-9]{{6}}-[0-9]{{6,}}$", ma);
            Assert.EndsWith("-" + hauToMongDoi, ma);
        }
    }

    private static async Task<long> DemAsync(NpgsqlConnection ketNoi, string sql)
    {
        await using var lenh = new NpgsqlCommand(sql, ketNoi);
        lenh.Parameters.AddWithValue("cacSchema", CacSchema);
        return (long)(await lenh.ExecuteScalarAsync())!;
    }

    private static async Task<List<string>> LayDanhSachAsync(NpgsqlConnection ketNoi, string sql)
    {
        await using var lenh = new NpgsqlCommand(sql, ketNoi);
        if (sql.Contains("@cacSchema"))
        {
            lenh.Parameters.AddWithValue("cacSchema", CacSchema);
        }

        var ketQua = new List<string>();
        await using var doc = await lenh.ExecuteReaderAsync();
        while (await doc.ReadAsync())
        {
            ketQua.Add(doc.GetString(0));
        }

        return ketQua;
    }

    private static async Task<Guid> ThemDongNhatKyAsync(NpgsqlConnection ketNoi, NpgsqlTransaction giaoDich)
    {
        var ma = Guid.NewGuid();
        await using var lenh = new NpgsqlCommand(
            "INSERT INTO nhat_ky.nhat_ky_he_thong (ma_nhat_ky, thao_tac, doi_tuong) VALUES (@ma, 'KIEM_THU', 'kiem_thu')",
            ketNoi,
            giaoDich);
        lenh.Parameters.AddWithValue("ma", ma);
        await lenh.ExecuteNonQueryAsync();

        return ma;
    }

    private static async Task<string> LayBieuThucMacDinhAsync(NpgsqlConnection ketNoi, string schema, string bang, string cot)
    {
        await using var lenh = new NpgsqlCommand("""
            SELECT pg_get_expr(d.adbin, d.adrelid)
            FROM pg_attrdef d
            JOIN pg_attribute a ON a.attrelid = d.adrelid AND a.attnum = d.adnum
            WHERE d.adrelid = to_regclass(@bang) AND a.attname = @cot
            """, ketNoi);
        lenh.Parameters.AddWithValue("bang", $"{schema}.{bang}");
        lenh.Parameters.AddWithValue("cot", cot);

        return (string)(await lenh.ExecuteScalarAsync())!;
    }
}

