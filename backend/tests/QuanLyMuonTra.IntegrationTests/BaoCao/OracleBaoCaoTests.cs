using System.Text.RegularExpressions;
using Npgsql;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.IntegrationTests.MuonTra;

namespace QuanLyMuonTra.IntegrationTests.BaoCao;

public partial class OracleBaoCaoTests
{
    private static readonly Dictionary<string, int> SoCotMoiBaoCao = new()
    {
        ["tinh-trang-kho"] = 10,
        ["qua-han"] = 10,
        ["hu-hong-mat"] = 12,
        ["tan-suat-muon"] = 4,
        ["vi-pham-sinh-vien"] = 8,
        ["bao-tri-thanh-ly"] = 11
    };

    [Fact]
    public async Task UC16_OracleCot_SoCotMoiBaoCaoDungSoDem()
    {
        var oracle = await OracleCsv.DocAsync("BaoCao", "D12-N-oracle-cot-bao-cao.csv");

        var soCot = oracle
            .GroupBy(d => d["Loai"])
            .ToDictionary(nhom => nhom.Key, nhom => nhom.Count());

        Assert.Equal(
            SoCotMoiBaoCao.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => $"{x.Key}={x.Value}").ToList(),
            soCot.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => $"{x.Key}={x.Value}").ToList());
    }

    [Fact]
    public async Task UC16_OracleCot_KhopBangCotTrongApiNotesMuc9()
    {
        var oracle = await OracleCsv.DocAsync("BaoCao", "D12-N-oracle-cot-bao-cao.csv");
        var apiNotes = await DocApiNotesAsync();

        Assert.Equal(SoCotMoiBaoCao.Keys.OrderBy(k => k, StringComparer.Ordinal), apiNotes.Keys.OrderBy(k => k, StringComparer.Ordinal));
        foreach (var (loai, bang) in apiNotes)
        {
            var kyVong = oracle
                .Where(d => d["Loai"] == loai)
                .OrderBy(d => int.Parse(d["Stt"]))
                .Select(d => $"{d["KhoaJson"]}|{d["RongDuoc"]}|{d["TieuDeCot"]}")
                .ToList();
            var thucTe = bang.Cot.Select(c => $"{c.Khoa}|{(c.RongDuoc ? "CO" : "KHONG")}|{c.TieuDe}").ToList();
            Assert.Equal(kyVong, thucTe);
        }
    }

    [Fact]
    public async Task UC16_OracleMoc_KhopMocLocThoiGianTrongApiNotesMuc9()
    {
        var oracle = await OracleCsv.DocAsync("BaoCao", "D12-N-oracle-moc-ngay-bao-cao.csv");
        var apiNotes = await DocApiNotesAsync();

        foreach (var (loai, bang) in apiNotes)
        {
            var dong = Assert.Single(oracle, d => d["Loai"] == loai);
            Assert.Equal(bang.Moc, dong["MocLoc"]);
            Assert.Equal(loai == "tinh-trang-kho" ? "KHONG" : "CO", dong["LocTheoNgay"]);
        }
    }

    [CanKetNoiTestFact]
    public async Task UC16_OracleBienNgay_GomHaiDauTheoNgayLichVietNam()
    {
        var oracle = await OracleCsv.DocAsync("BaoCao", "D12-N-oracle-bien-ngay.csv");
        await using var ketNoi = await KetNoiKiemThu.MoAsync();

        foreach (var dong in oracle)
        {
            await using var lenh = new NpgsqlCommand("""
                SELECT (CASE WHEN @kieu = 'DATE' THEN @moc::date ELSE (@moc::timestamptz AT TIME ZONE 'Asia/Ho_Chi_Minh')::date END)
                           >= COALESCE(NULLIF(@tu, '')::date, '-infinity'::date)
                   AND (CASE WHEN @kieu = 'DATE' THEN @moc::date ELSE (@moc::timestamptz AT TIME ZONE 'Asia/Ho_Chi_Minh')::date END)
                           <= COALESCE(NULLIF(@den, '')::date, 'infinity'::date)
                """, ketNoi);
            lenh.Parameters.AddWithValue("kieu", dong["KieuMoc"]);
            lenh.Parameters.AddWithValue("moc", dong["GiaTriMoc"]);
            lenh.Parameters.AddWithValue("tu", dong["TuNgay"]);
            lenh.Parameters.AddWithValue("den", dong["DenNgay"]);

            var trongKy = (bool)(await lenh.ExecuteScalarAsync())!;

            Assert.Equal(dong["KyVong"], trongKy ? "CO" : "KHONG");
        }
    }

    [GeneratedRegex("^### 9\\.\\d+ `([a-z\\-]+)`")]
    private static partial Regex TieuDeBaoCao();

    [GeneratedRegex("Mốc lọc thời gian: (.*?)\\. Căn cứ")]
    private static partial Regex MocLoc();

    private static async Task<Dictionary<string, BangBaoCao>> DocApiNotesAsync()
    {
        var duongDan = Path.Combine(AppContext.BaseDirectory, "API-Notes.md");
        var ketQua = new Dictionary<string, BangBaoCao>();
        BangBaoCao? hienTai = null;
        foreach (var dong in await File.ReadAllLinesAsync(duongDan))
        {
            var tieuDe = TieuDeBaoCao().Match(dong);
            if (tieuDe.Success)
            {
                hienTai = new BangBaoCao();
                ketQua[tieuDe.Groups[1].Value] = hienTai;
                continue;
            }

            if (dong.StartsWith("## ", StringComparison.Ordinal) || dong.StartsWith("### ", StringComparison.Ordinal))
            {
                hienTai = null;
                continue;
            }

            if (hienTai is null)
            {
                continue;
            }

            if (dong.StartsWith("| `", StringComparison.Ordinal))
            {
                var o = dong.Trim().Trim('|').Split('|').Select(c => c.Trim()).ToArray();
                hienTai.Cot.Add((o[0].Trim('`'), o[2] == "có", o[3]));
                continue;
            }

            var moc = MocLoc().Match(dong);
            if (moc.Success)
            {
                hienTai.Moc = moc.Groups[1].Value.Replace("`", string.Empty);
            }
        }

        return ketQua;
    }

    private sealed class BangBaoCao
    {
        public List<(string Khoa, bool RongDuoc, string TieuDe)> Cot { get; } = [];

        public string Moc { get; set; } = string.Empty;
    }
}
