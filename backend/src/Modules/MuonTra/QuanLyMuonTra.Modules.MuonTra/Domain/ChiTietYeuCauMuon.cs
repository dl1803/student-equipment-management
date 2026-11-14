namespace QuanLyMuonTra.Modules.MuonTra.Domain;

public class ChiTietYeuCauMuon
{
    private ChiTietYeuCauMuon()
    {
        MaYeuCau = string.Empty;
    }

    internal ChiTietYeuCauMuon(string maYeuCau, Guid maLoai, int soLuong)
    {
        MaYeuCau = maYeuCau;
        MaLoai = maLoai;
        SoLuong = soLuong;
    }

    public string MaYeuCau { get; private set; }

    public Guid MaLoai { get; private set; }

    public int SoLuong { get; private set; }
}
