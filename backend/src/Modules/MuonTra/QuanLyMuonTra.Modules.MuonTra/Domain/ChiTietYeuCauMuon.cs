using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Domain;

public class ChiTietYeuCauMuon
{
    private ChiTietYeuCauMuon()
    {
        MaYeuCau = string.Empty;
    }

    internal ChiTietYeuCauMuon(string maYeuCau, Guid maLoai, int soLuong)
    {
        if (soLuong <= 0)
        {
            throw new NghiepVuException(MaLoi.DuLieuKhongHopLe, "Số lượng yêu cầu phải lớn hơn 0.");
        }

        MaYeuCau = maYeuCau;
        MaLoai = maLoai;
        SoLuong = soLuong;
    }

    public string MaYeuCau { get; private set; }

    public Guid MaLoai { get; private set; }

    public int SoLuong { get; private set; }
}
