using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class DichLoiMienNghiepVu
{
    public static LoiNghiepVuException Chuyen(NghiepVuException loi)
    {
        return new LoiNghiepVuException(loi.MaLoi, loi.Message);
    }
}
