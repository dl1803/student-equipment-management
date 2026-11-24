using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Infrastructure.LuuTru;

public sealed class DonAnhMoCoJob(
    DbContext db,
    IAnhStorage luuTru,
    IClock dongHo,
    ILogger<DonAnhMoCoJob> nhatKy) : IJob
{
    public const int SoDongToiDa = 200;

    public static readonly TimeSpan HanChuaGan = TimeSpan.FromHours(24);

    public string MaJob => "J9";

    public string TenJob => "DonAnhMoCo";

    public TimeSpan ChuKy => TimeSpan.FromDays(1);

    public async Task<int> ChayAsync(CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "J9 DonAnhMoCo phải chạy trong giao dịch đang mở của bộ chạy lịch.");
        }

        var hanChot = dongHo.UtcNow - HanChuaGan;

        var dsAnh = await db.Database.SqlQuery<DongAnhMoCo>($"""
                SELECT ma_anh AS "MaAnh", bucket AS "Bucket", khoa_luu_tru AS "KhoaLuuTru"
                FROM luu_tru.anh_tai_len
                WHERE doi_tuong_id IS NULL AND thoi_diem < {hanChot}
                ORDER BY thoi_diem, ma_anh
                LIMIT {SoDongToiDa}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(ct);

        var soDaDon = 0;

        foreach (var anh in dsAnh)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                await luuTru.XoaAsync(anh.Bucket, anh.KhoaLuuTru, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception loi)
            {
                nhatKy.LogWarning(
                    loi,
                    "J9 không xóa được object {Bucket}/{Khoa} của ảnh {MaAnh}; giữ dòng siêu dữ liệu, thử lại ở lần chạy sau.",
                    anh.Bucket,
                    anh.KhoaLuuTru,
                    anh.MaAnh);
                continue;
            }

            await db.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM luu_tru.anh_tai_len WHERE ma_anh = {anh.MaAnh}",
                ct);

            soDaDon++;
        }

        if (dsAnh.Count == SoDongToiDa)
        {
            nhatKy.LogWarning(
                "J9 đã xử lý đủ {SoDong} ảnh (giới hạn mỗi lần chạy); phần còn lại chờ lần chạy sau.",
                SoDongToiDa);
        }

        return soDaDon;
    }

    public sealed class DongAnhMoCo
    {
        public Guid MaAnh { get; set; }

        public string Bucket { get; set; } = string.Empty;

        public string KhoaLuuTru { get; set; } = string.Empty;
    }
}

