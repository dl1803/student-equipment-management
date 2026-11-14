namespace QuanLyMuonTra.Modules.QuyDinh.Domain;

public sealed class ThamSoHeThong
{
    private ThamSoHeThong()
    {
    }

    public short Id { get; private set; }

    public int SoNgayKhoa { get; private set; }

    public int ThoiGianGiuChoPhut { get; private set; }

    public int AnHanPhut { get; private set; }

    public int SoGioGiaHanToiDa { get; private set; }

    public int NguongViPham { get; private set; }

    public int SoLanDangNhapSaiToiDa { get; private set; }

    public TimeOnly? GioMoCua { get; private set; }

    public TimeOnly? GioDongCua { get; private set; }
}
