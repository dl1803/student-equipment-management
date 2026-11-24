namespace QuanLyMuonTra.Modules.SinhVien.Contracts;

public static class DieuKienMuonSinhVien
{
    private const string DangHoc = "DANG_HOC";
    private const string QuyenMuonBinhThuong = "BINH_THUONG";

    public static IReadOnlyList<LyDoKhongHopLeDto> XacDinhLyDoKhongHopLe(
        HoSoSinhVienDto hoSo,
        DateOnly ngayHienTai,
        bool vuotGioiHanThietBi)
    {
        ArgumentNullException.ThrowIfNull(hoSo);

        var dsLyDo = new List<LyDoKhongHopLeDto>();

        if (hoSo.HanThe < ngayHienTai)
        {
            dsLyDo.Add(new LyDoKhongHopLeDto("SV_THE_HET_HAN", "Thẻ sinh viên đã hết hạn."));
        }

        if (hoSo.TrangThaiHoc != DangHoc)
        {
            dsLyDo.Add(new LyDoKhongHopLeDto("SV_KHONG_CON_THEO_HOC", "Sinh viên không còn theo học."));
        }

        if (hoSo.TrangThaiQuyenMuon != QuyenMuonBinhThuong)
        {
            dsLyDo.Add(new LyDoKhongHopLeDto("SV_BI_KHOA_QUYEN_MUON", "Quyền mượn của sinh viên đang bị khóa."));
        }

        if (vuotGioiHanThietBi)
        {
            dsLyDo.Add(new LyDoKhongHopLeDto("VUOT_GIOI_HAN_THIET_BI", "Sinh viên đã đạt giới hạn số thiết bị được mượn."));
        }

        return dsLyDo;
    }
}

