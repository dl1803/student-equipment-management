using System.Buffers.Binary;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

public sealed class AdvisoryLockService : IAdvisoryLockService
{
    private readonly DbContext _db;

    public AdvisoryLockService(DbContext db)
    {
        _db = db;
    }

    public async Task LockPhongLoaiAsync(Guid maPhong, Guid maLoai, CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Khóa tư vấn theo giao dịch chỉ được lấy sau khi đã mở giao dịch.");
        }

        var khoa = SuyKhoa(maPhong, maLoai);
        await _db.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({khoa})", ct);
    }

    public async Task LockDanhSachPhongLoaiAsync(IEnumerable<(Guid MaPhong, Guid MaLoai)> danhSach, CancellationToken ct)
    {
        foreach (var (maPhong, maLoai) in SapXepCacCap(danhSach))
        {
            await LockPhongLoaiAsync(maPhong, maLoai, ct);
        }
    }

    internal static long SuyKhoa(Guid maPhong, Guid maLoai)
    {
        Span<byte> dauVao = stackalloc byte[32];
        maPhong.TryWriteBytes(dauVao[..16]);
        maLoai.TryWriteBytes(dauVao[16..]);

        Span<byte> bam = stackalloc byte[32];
        SHA256.HashData(dauVao, bam);

        return BinaryPrimitives.ReadInt64LittleEndian(bam);
    }

    internal static IReadOnlyList<(Guid MaPhong, Guid MaLoai)> SapXepCacCap(IEnumerable<(Guid MaPhong, Guid MaLoai)> danhSach)
    {
        return danhSach
            .Distinct()
            .OrderBy(cap => cap.MaPhong)
            .ThenBy(cap => cap.MaLoai)
            .ToList();
    }
}
