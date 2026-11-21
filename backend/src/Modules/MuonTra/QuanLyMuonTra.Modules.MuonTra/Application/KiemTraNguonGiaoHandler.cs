using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class KiemTraNguonGiaoHandler(
    DbContext db,
    IClock dongHo,
    IDanhMucReader danhMuc,
    IQuyDinhReader quyDinh,
    IMuonTraReader muonTra,
    IAvailabilityService khaDung,
    IQuyenPhongService quyenPhong,
    IHoSoSinhVienReader hoSoReader)
{
    private const string TrangThaiDaDuyet = "DA_DUYET";

    private static readonly Regex MauMaDangKy = new(@"\ADK-[0-9]{6}-[0-9]{6,}\z", RegexOptions.CultureInvariant);

    public async Task<KetQuaKiemTraNguonDto> KiemTraPhieuYeuCauAsync(Guid maNv, string maYeuCau, CancellationToken ct)
    {
        var phieu = await PhieuYeuCauMuonQuayTruyCap.LayVaKiemQuyenAsync(db, danhMuc, quyenPhong, maNv, maYeuCau, ct);
        var thoiDiem = dongHo.UtcNow.ToUniversalTime();

        var dsLyDo = new List<LyDoDto>();
        var lyDoNguon = KiemTraDieuKienGiao.KiemTraGiuCho(
            phieu.MaYeuCau,
            phieu.TrangThai == TrangThaiDaDuyet,
            phieu.HanGiuCho,
            thoiDiem);
        if (lyDoNguon is not null)
        {
            dsLyDo.Add(lyDoNguon);
        }

        return await TongHopAsync(phieu.MaSV, phieu.MaPhong, phieu.TenPhong, phieu.DsLoaiVaSoLuong, dsLyDo, thoiDiem, ct);
    }

    public async Task<KetQuaKiemTraNguonDto> KiemTraDangKyChoAsync(Guid maNv, string maDangKy, CancellationToken ct)
    {
        if (!MauMaDangKy.IsMatch(maDangKy))
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Mã đăng ký chờ không đúng định dạng.");
        }

        var dangKy = await db.Set<DangKyCho>().AsNoTracking().SingleOrDefaultAsync(d => d.MaDangKy == maDangKy, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy đăng ký chờ.");

        await quyenPhong.EnsureAsync(maNv, dangKy.MaPhong, ct);

        var thoiDiem = dongHo.UtcNow.ToUniversalTime();
        var phong = await danhMuc.LayPhongAsync(dangKy.MaPhong, ct);
        var loai = await danhMuc.LayLoaiAsync(dangKy.MaLoai, ct);
        var dsLoai = new List<LoaiVaSoLuongDto> { new(dangKy.MaLoai, loai?.TenLoai ?? string.Empty, 1) };

        var dsLyDo = new List<LyDoDto>();
        var lyDoNguon = KiemTraDieuKienGiao.KiemTraGiuCho(
            dangKy.MaDangKy,
            dangKy.TrangThai == TrangThaiDangKyCho.DaThongBao,
            dangKy.HanGiuCho,
            thoiDiem);
        if (lyDoNguon is not null)
        {
            dsLyDo.Add(lyDoNguon);
        }

        return await TongHopAsync(dangKy.MaSv, dangKy.MaPhong, phong?.TenPhong ?? string.Empty, dsLoai, dsLyDo, thoiDiem, ct);
    }

    public async Task<KetQuaKiemTraNguonDto> KiemTraTrucTiepAsync(
        Guid maNv,
        string maSv,
        Guid maPhong,
        IReadOnlyList<LoaiVaSoLuongYeuCau> dsLoaiYeuCau,
        CancellationToken ct)
    {
        await quyenPhong.EnsureAsync(maNv, maPhong, ct);

        var phong = await danhMuc.LayPhongAsync(maPhong, ct)
            ?? throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Phòng thiết bị không tồn tại.");

        var thoiDiem = dongHo.UtcNow.ToUniversalTime();
        var dsLoai = new List<LoaiVaSoLuongDto>();
        var dsLyDo = new List<LyDoDto>();

        foreach (var dong in dsLoaiYeuCau)
        {
            var loai = await danhMuc.LayLoaiAsync(dong.MaLoai, ct);
            if (loai is null || loai.NgungSuDung)
            {
                throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Loại thiết bị không tồn tại hoặc đã ngừng sử dụng.");
            }

            dsLoai.Add(new LoaiVaSoLuongDto(dong.MaLoai, loai.TenLoai, dong.SoLuong));

            var soLuongKhaDung = await khaDung.TinhKhaDungAsync(maPhong, dong.MaLoai, ct);
            var lyDoKhaDung = KiemTraDieuKienGiao.KiemTraKhaDung(loai.TenLoai, soLuongKhaDung, dong.SoLuong);
            if (lyDoKhaDung is not null)
            {
                dsLyDo.Add(lyDoKhaDung);
            }
        }

        return await TongHopAsync(maSv, maPhong, phong.TenPhong, dsLoai, dsLyDo, thoiDiem, ct);
    }

    private async Task<KetQuaKiemTraNguonDto> TongHopAsync(
        string maSv,
        Guid maPhong,
        string tenPhong,
        IReadOnlyList<LoaiVaSoLuongDto> dsLoai,
        List<LyDoDto> dsLyDo,
        DateTimeOffset thoiDiem,
        CancellationToken ct)
    {
        var hoSo = await hoSoReader.LayTheoMaSvAsync(maSv, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy sinh viên.");

        var quyDinhNhom = await quyDinh.LayQuyDinhNhomAsync(hoSo.MaNhom, ct)
            ?? throw new InvalidOperationException($"Thiếu quy định mượn chung của nhóm {hoSo.MaNhom}.");
        var soDangCam = await muonTra.DemThietBiDangCamAsync(maSv, ct);

        dsLyDo.AddRange(KiemTraDieuKienGiao.TongHopLyDoSinhVien(
            hoSo,
            VnTime.NgayLich(thoiDiem),
            quyDinhNhom,
            soDangCam,
            dsLoai.Sum(d => (long)d.SoLuong)));

        var dsViTri = new List<ViTriLayHangDto>();
        foreach (var dong in dsLoai)
        {
            var dsViTriLoai = await khaDung.DsViTriSanSangAsync(maPhong, dong.MaLoai, ct);
            dsViTri.AddRange(dsViTriLoai.Select(v => new ViTriLayHangDto(
                dong.MaLoai,
                dong.TenLoai,
                tenPhong,
                v.ViTriKe,
                v.ViTriTu,
                v.SoThietBiSanSang)));
        }

        return new KetQuaKiemTraNguonDto(
            dsLyDo.Count == 0,
            dsLyDo,
            new SinhVienTomTatDto(hoSo.MaSv, hoSo.HoTen, hoSo.Lop ?? string.Empty),
            maPhong,
            tenPhong,
            dsLoai,
            dsViTri);
    }
}
