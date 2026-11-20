using QuanLyMuonTra.Infrastructure.LuuTru;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.Anh;

public sealed class AnhStorageGia : IAnhStorage
{
    public List<(string Bucket, string Khoa, string LoaiNoiDung, int KichThuoc)> DaTai { get; } = [];

    public List<(string Bucket, string Khoa)> DaXoa { get; } = [];

    public List<TimeSpan> HieuLucDaKy { get; } = [];

    public Task UploadAsync(string bucket, string khoaLuuTru, string loaiNoiDung, byte[] noiDung, CancellationToken ct)
    {
        DaTai.Add((bucket, khoaLuuTru, loaiNoiDung, noiDung.Length));

        return Task.CompletedTask;
    }

    public Task<string> TaoUrlKyAsync(string bucket, string khoaLuuTru, TimeSpan hieuLuc, CancellationToken ct)
    {
        HieuLucDaKy.Add(hieuLuc);

        return Task.FromResult($"https://luu-tru-gia.test/{bucket}/{khoaLuuTru}?token=gia");
    }

    public Task XoaAsync(string bucket, string khoaLuuTru, CancellationToken ct)
    {
        DaXoa.Add((bucket, khoaLuuTru));

        return Task.CompletedTask;
    }
}

public sealed class NguoiDungGia(Guid? maNguoiDung) : ICurrentUser
{
    public Guid? MaNguoiDung => maNguoiDung;

    public string? MaSv => null;
}

public sealed class DongHoCoDinh(DateTimeOffset bayGio) : IClock
{
    public DateTimeOffset UtcNow => bayGio;
}

