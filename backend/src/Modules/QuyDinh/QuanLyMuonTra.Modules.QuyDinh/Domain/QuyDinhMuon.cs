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

    internal static QuyDinhMuon TaoChung(string maNhom, int soThietBiToiDa, int thoiGianMuonToiDaGio)
    {
        KiemTraNhomVaThoiGian(maNhom, thoiGianMuonToiDaGio);
        RangBuocCauHinh.YeuCau(soThietBiToiDa > 0, "Số thiết bị tối đa phải lớn hơn 0.");

        return new QuyDinhMuon
        {
            MaQuyDinh = Guid.CreateVersion7(),
            MaNhom = maNhom,
            SoThietBiToiDa = soThietBiToiDa,
            ThoiGianMuonToiDaGio = thoiGianMuonToiDaGio,
        };
    }

    internal static QuyDinhMuon TaoNgoaiLe(string maNhom, Guid maLoai, int thoiGianMuonToiDaGio)
    {
        KiemTraNhomVaThoiGian(maNhom, thoiGianMuonToiDaGio);

        return new QuyDinhMuon
        {
            MaQuyDinh = Guid.CreateVersion7(),
            MaNhom = maNhom,
            MaLoai = maLoai,
            ThoiGianMuonToiDaGio = thoiGianMuonToiDaGio,
        };
    }

    private static void KiemTraNhomVaThoiGian(string maNhom, int thoiGianMuonToiDaGio)
    {
        RangBuocCauHinh.YeuCau(!string.IsNullOrWhiteSpace(maNhom), "Mã nhóm sinh viên không được để trống.");
        RangBuocCauHinh.YeuCau(thoiGianMuonToiDaGio > 0, "Thời gian mượn tối đa phải lớn hơn 0 giờ.");
    }
}
