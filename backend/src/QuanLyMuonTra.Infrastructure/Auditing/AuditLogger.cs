using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Infrastructure.Auditing;

public sealed class AuditLogger(AppDbContext db, ICurrentUser nguoiDung, IClock dongHo) : IAuditLogger
{
    public async Task GhiAsync(
        string thaoTac,
        string doiTuong,
        string? doiTuongId,
        IReadOnlyDictionary<string, object?>? giaTriTruoc,
        IReadOnlyDictionary<string, object?>? giaTriSau,
        CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Ghi nhật ký phải nằm trong giao dịch của thao tác nghiệp vụ.");
        }

        var truoc = giaTriTruoc is null ? null : NhatKyJson.ChuyenSangJson(giaTriTruoc);
        var sau = giaTriSau is null ? null : NhatKyJson.ChuyenSangJson(giaTriSau);

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO nhat_ky.nhat_ky_he_thong
                (ma_nhat_ky, thoi_diem, ma_nguoi_dung, ma_sv, thao_tac, doi_tuong, doi_tuong_id, gia_tri_truoc, gia_tri_sau)
            VALUES
                ({Guid.CreateVersion7()}, {dongHo.UtcNow}, {nguoiDung.MaNguoiDung}, {nguoiDung.MaSv}, {thaoTac}, {doiTuong}, {doiTuongId}, {truoc}::jsonb, {sau}::jsonb)
            """,
            ct);
    }
}

