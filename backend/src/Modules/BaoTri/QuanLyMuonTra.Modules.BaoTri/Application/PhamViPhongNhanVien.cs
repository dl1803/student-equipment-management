using QuanLyMuonTra.Modules.TaiKhoan.Contracts;

namespace QuanLyMuonTra.Modules.BaoTri.Application;

internal static class PhamViPhongNhanVien
{
    public static async Task<Guid[]> LayAsync(
        IQuyenPhongService quyenPhong,
        Guid maNv,
        Guid? maPhong,
        CancellationToken ct)
    {
        var dsMaPhong = await quyenPhong.LayDsMaPhongAsync(maNv, ct);
        return maPhong is { } loc
            ? dsMaPhong.Where(p => p == loc).ToArray()
            : dsMaPhong.ToArray();
    }
}
