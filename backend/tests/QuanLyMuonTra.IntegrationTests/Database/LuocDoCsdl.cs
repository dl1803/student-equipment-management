using Npgsql;

namespace QuanLyMuonTra.IntegrationTests.Database;

public static class LuocDoCsdl
{
    public static readonly string[] CacSchema =
    [
        "tai_khoan", "sinh_vien", "danh_muc", "quy_dinh", "muon_tra", "vi_pham",
        "bao_tri", "thong_bao", "nhat_ky", "luu_tru", "truong_gia_lap"
    ];

    private const string SqlDauVanTay = """
        SELECT 'cot|' || n.nspname || '.' || c.relname || '.' || a.attname || '|' || format_type(a.atttypid, a.atttypmod) || '|' || a.attnotnull || '|' || coalesce(pg_get_expr(d.adbin, d.adrelid), '')
        FROM pg_attribute a
        JOIN pg_class c ON c.oid = a.attrelid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        LEFT JOIN pg_attrdef d ON d.adrelid = a.attrelid AND d.adnum = a.attnum
        WHERE n.nspname::text = ANY(@cacSchema) AND c.relkind = 'r' AND a.attnum > 0 AND NOT a.attisdropped
        UNION ALL
        SELECT 'rb|' || n.nspname || '.' || c.relname || '.' || k.conname || '|' || pg_get_constraintdef(k.oid)
        FROM pg_constraint k
        JOIN pg_class c ON c.oid = k.conrelid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE n.nspname::text = ANY(@cacSchema)
        UNION ALL
        SELECT 'idx|' || schemaname || '.' || indexname || '|' || indexdef
        FROM pg_indexes
        WHERE schemaname::text = ANY(@cacSchema)
        ORDER BY 1
        """;

    public static async Task<int> DemSchemaAsync(NpgsqlConnection ketNoi)
    {
        return await DemAsync(ketNoi, "SELECT count(*) FROM pg_namespace WHERE nspname::text = ANY(@cacSchema)");
    }

    public static async Task<int> DemBangAsync(NpgsqlConnection ketNoi)
    {
        return await DemAsync(ketNoi, "SELECT count(*) FROM pg_tables WHERE schemaname::text = ANY(@cacSchema)");
    }

    public static async Task<IReadOnlyList<string>> LayDauVanTayAsync(NpgsqlConnection ketNoi)
    {
        await using var lenh = new NpgsqlCommand(SqlDauVanTay, ketNoi);
        lenh.Parameters.AddWithValue("cacSchema", CacSchema);
        var ketQua = new List<string>();
        await using var doc = await lenh.ExecuteReaderAsync();
        while (await doc.ReadAsync())
        {
            ketQua.Add(doc.GetString(0));
        }

        return ketQua;
    }

    public static IReadOnlyList<string> SoSanh(IReadOnlyCollection<string> truoc, IReadOnlyCollection<string> sau)
    {
        var chenhLech = new List<string>();
        chenhLech.AddRange(truoc.Except(sau).Select(dong => "- " + dong));
        chenhLech.AddRange(sau.Except(truoc).Select(dong => "+ " + dong));
        return chenhLech;
    }

    private static async Task<int> DemAsync(NpgsqlConnection ketNoi, string sql)
    {
        await using var lenh = new NpgsqlCommand(sql, ketNoi);
        lenh.Parameters.AddWithValue("cacSchema", CacSchema);
        return Convert.ToInt32(await lenh.ExecuteScalarAsync());
    }
}
