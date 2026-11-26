using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.ViPham.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed class DeXuatBienBanHandler(
    IPhieuMuonReader phieuMuon,
    IDanhMucReader danhMuc,
    IQuyDinhReader quyDinh,
    IQuyenPhongService quyenPhong)
{
    private const string KetQuaHuHong = "HU_HONG";
    private const string KetQuaThieuPhuKien = "THIEU_PHU_KIEN";
    private const string KetQuaMat = "MAT";

    public async Task<DeXuatBienBanDto> XuLyAsync(Guid maNv, string maPhieu, CancellationToken ct)
    {
        KiemTraDocViPham.KiemTraMaPhieu(maPhieu);

        var phieu = await phieuMuon.LayPhieuMuonAsync(maPhieu, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy phiếu mượn.");
        var dsChiTiet = (await phieuMuon.LayChiTietTheoPhieuAsync(maPhieu, ct))
            .OrderBy(c => c.MaThietBi, StringComparer.Ordinal)
            .ToList();

        var thietBiTheoMa = new Dictionary<string, ThietBiDto>();
        foreach (var chiTiet in dsChiTiet)
        {
            thietBiTheoMa[chiTiet.MaThietBi] = await danhMuc.LayThietBiAsync(chiTiet.MaThietBi, ct)
                ?? throw new InvalidOperationException($"Thiếu thiết bị {chiTiet.MaThietBi} của phiếu {maPhieu}.");
        }

        var dsMaPhong = await quyenPhong.LayDsMaPhongAsync(maNv, ct);
        if (!thietBiTheoMa.Values.Any(t => dsMaPhong.Contains(t.MaPhong)))
        {
            throw new LoiNghiepVuException(MaLoi.PhongChuaCapQuyen, "Nhân viên chưa được cấp quyền phòng của phiếu mượn này.");
        }

        var thamSo = await quyDinh.LayThamSoAsync(ct);
        var dsKhoan = new List<KhoanDeXuatDto>();
        foreach (var chiTiet in dsChiTiet)
        {
            if (chiTiet.KetQuaKiemTra is null)
            {
                continue;
            }

            var thietBi = thietBiTheoMa[chiTiet.MaThietBi];

            if (chiTiet.NgayGioTraThucTe is { } ngayTra)
            {
                var soNgayTre = TinhPhi.SoNgayTre(phieu.NgayGioHenTra, ngayTra, thamSo.AnHanPhut);
                if (soNgayTre > 0)
                {
                    var phiTre = TinhPhi.PhiTre(soNgayTre, chiTiet.PhiTreMoiNgay, thietBi.GiaTriHienHanh);
                    dsKhoan.Add(new KhoanDeXuatDto(
                        chiTiet.MaThietBi,
                        thietBi.TenThietBi,
                        MaLoaiViPham.TraTre,
                        soNgayTre,
                        (long)phiTre,
                        null));
                }
            }

            if (chiTiet.KetQuaKiemTra is KetQuaHuHong or KetQuaThieuPhuKien)
            {
                var maKhung = chiTiet.MaKhung
                    ?? throw new InvalidOperationException($"Thiết bị {chiTiet.MaThietBi} hư hỏng nhưng thiếu khung phạt.");
                var khung = await quyDinh.LayKhungPhatAsync(maKhung, ct)
                    ?? throw new InvalidOperationException($"Thiếu khung phạt {maKhung}.");
                var boiThuong = TinhPhi.BoiThuongHuHong(thietBi.GiaTriHienHanh, khung.TyLePhanTram);
                dsKhoan.Add(new KhoanDeXuatDto(
                    chiTiet.MaThietBi,
                    thietBi.TenThietBi,
                    MaLoaiViPham.HuHong,
                    null,
                    (long)boiThuong,
                    maKhung));
            }
            else if (chiTiet.KetQuaKiemTra == KetQuaMat)
            {
                var boiThuong = TinhPhi.BoiThuongMat(thietBi.GiaTriHienHanh);
                dsKhoan.Add(new KhoanDeXuatDto(
                    chiTiet.MaThietBi,
                    thietBi.TenThietBi,
                    MaLoaiViPham.Mat,
                    null,
                    (long)boiThuong,
                    null));
            }
        }

        return new DeXuatBienBanDto(phieu.MaPhieu, dsKhoan, dsKhoan.Sum(k => k.SoTien));
    }
}

