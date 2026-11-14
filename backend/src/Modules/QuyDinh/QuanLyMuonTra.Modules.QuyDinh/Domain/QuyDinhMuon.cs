namespace QuanLyMuonTra.Modules.QuyDinh.Domain;

public sealed class QuyDinhMuon
{
    private QuyDinhMuon()
    {
    }

    public Guid MaQuyDinh { get; private set; }

    public string MaNhom { get; private set; } = string.Empty;

    public Guid? MaLoai { get; private set; }

    public int? SoThietBiToiDa { get; private set; }

    public int ThoiGianMuonToiDaGio { get; private set; }
}
