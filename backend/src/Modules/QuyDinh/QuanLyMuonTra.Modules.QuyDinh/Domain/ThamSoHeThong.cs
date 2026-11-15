namespace QuanLyMuonTra.Modules.QuyDinh.Domain;

public sealed class ThamSoHeThong
{
    private ThamSoHeThong()
    {
    }

    internal ThamSoHeThong(
        int soNgayKhoa,
        int thoiGianGiuChoPhut,
        int anHanPhut,
        int soGioGiaHanToiDa,
        int nguongViPham,
        int soLanDangNhapSaiToiDa,
        TimeOnly? gioMoCua,
        TimeOnly? gioDongCua)
    {
        CapNhat(soNgayKhoa, thoiGianGiuChoPhut, anHanPhut, soGioGiaHanToiDa, nguongViPham, soLanDangNhapSaiToiDa, gioMoCua, gioDongCua);
    }

    public short Id { get; private set; } = 1;

    public int SoNgayKhoa { get; private set; }

    public int ThoiGianGiuChoPhut { get; private set; }

    public int AnHanPhut { get; private set; }

    public int SoGioGiaHanToiDa { get; private set; }

    public int NguongViPham { get; private set; }

    public int SoLanDangNhapSaiToiDa { get; private set; }

    public TimeOnly? GioMoCua { get; private set; }

    public TimeOnly? GioDongCua { get; private set; }

    internal void CapNhat(
        int soNgayKhoa,
        int thoiGianGiuChoPhut,
        int anHanPhut,
        int soGioGiaHanToiDa,
        int nguongViPham,
        int soLanDangNhapSaiToiDa,
        TimeOnly? gioMoCua,
        TimeOnly? gioDongCua)
    {
        RangBuocCauHinh.YeuCau(soNgayKhoa >= 0, "Số ngày khóa không được âm.");
        RangBuocCauHinh.YeuCau(thoiGianGiuChoPhut > 0, "Thời gian giữ chỗ phải lớn hơn 0 phút.");
        RangBuocCauHinh.YeuCau(anHanPhut >= 0, "Ân hạn không được âm.");
        RangBuocCauHinh.YeuCau(soGioGiaHanToiDa > 0, "Số giờ gia hạn tối đa phải lớn hơn 0.");
        RangBuocCauHinh.YeuCau(nguongViPham > 0, "Ngưỡng vi phạm phải lớn hơn 0.");
        RangBuocCauHinh.YeuCau(soLanDangNhapSaiToiDa > 0, "Số lần nhập sai mật khẩu tối đa phải lớn hơn 0.");
        RangBuocCauHinh.YeuCau(gioMoCua.HasValue == gioDongCua.HasValue, "Giờ mở cửa và giờ đóng cửa phải cùng để trống hoặc cùng có giá trị.");
        RangBuocCauHinh.YeuCau(!gioMoCua.HasValue || gioMoCua.Value < gioDongCua!.Value, "Giờ mở cửa phải nhỏ hơn giờ đóng cửa.");

        SoNgayKhoa = soNgayKhoa;
        ThoiGianGiuChoPhut = thoiGianGiuChoPhut;
        AnHanPhut = anHanPhut;
        SoGioGiaHanToiDa = soGioGiaHanToiDa;
        NguongViPham = nguongViPham;
        SoLanDangNhapSaiToiDa = soLanDangNhapSaiToiDa;
        GioMoCua = gioMoCua;
        GioDongCua = gioDongCua;
    }
}
