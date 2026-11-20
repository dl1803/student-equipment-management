namespace QuanLyMuonTra.Infrastructure.LuuTru;

public interface IAnhStorage
{
    Task UploadAsync(string bucket, string khoaLuuTru, string loaiNoiDung, byte[] noiDung, CancellationToken ct);

    Task<string> TaoUrlKyAsync(string bucket, string khoaLuuTru, TimeSpan hieuLuc, CancellationToken ct);

    Task XoaAsync(string bucket, string khoaLuuTru, CancellationToken ct);
}

