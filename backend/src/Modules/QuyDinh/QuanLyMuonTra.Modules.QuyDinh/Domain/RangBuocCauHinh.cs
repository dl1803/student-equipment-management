using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.QuyDinh.Domain;

internal static class RangBuocCauHinh
{
    internal static void YeuCau(bool dieuKien, string thongDiep)
    {
        if (!dieuKien)
        {
            throw new NghiepVuException(MaLoi.GiaTriCauHinhKhongHopLe, thongDiep);
        }
    }
}
