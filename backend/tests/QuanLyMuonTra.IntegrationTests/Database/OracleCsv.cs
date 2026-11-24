namespace QuanLyMuonTra.IntegrationTests.Database;

public static class OracleCsv
{
    public static async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> DocAsync(string thuMuc, string tenTep)
    {
        var duongDan = Path.Combine(AppContext.BaseDirectory, thuMuc, tenTep);
        var dong = (await File.ReadAllLinesAsync(duongDan)).Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
        var tieuDe = dong[0].Split(',');
        var ketQua = new List<IReadOnlyDictionary<string, string>>();
        foreach (var d in dong.Skip(1))
        {
            var o = d.Split(',');
            if (o.Length != tieuDe.Length)
            {
                throw new InvalidOperationException($"{tenTep}: dòng '{d}' có {o.Length} ô, tiêu đề có {tieuDe.Length}.");
            }

            ketQua.Add(tieuDe.Select((t, i) => (t, i)).ToDictionary(x => x.t, x => o[x.i]));
        }

        return ketQua;
    }
}
