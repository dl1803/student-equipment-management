namespace QuanLyMuonTra.Modules.DanhMuc.Application;

internal static class AnhThietBi
{
    internal static void ChanNeuCoAnh(IReadOnlyList<Guid>? dsMaAnh)
    {
        if (dsMaAnh is { Count: > 0 })
        {
            throw new InvalidOperationException("Chưa có bộ gắn ảnh danh mục cho thiết bị (chờ K-2 và N10).");
        }
    }
}
