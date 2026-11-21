using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ThongBao.Application;

internal static class PhanTrang
{
    private const int KichThuocToiDa = 100;

    public static void KiemTra(int trang, int kichThuoc)
    {
        if (trang < 1)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Số trang phải từ 1 trở lên.");
        }

        if (kichThuoc < 1 || kichThuoc > KichThuocToiDa)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Kích thước trang phải từ 1 đến 100.");
        }
    }

    public static long ViTriBatDau(int trang, int kichThuoc)
    {
        return (long)(trang - 1) * kichThuoc;
    }

    public static KetQuaPhanTrang<T> Rong<T>(int tongSo, int trang, int kichThuoc)
    {
        return new KetQuaPhanTrang<T>(tongSo, trang, kichThuoc, []);
    }
}

