using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

public sealed class FifoService(
    DbContext db,
    IClock dongHo,
    IDanhMucReader danhMuc,
    IQuyDinhReader quyDinh,
    IAvailabilityService khaDung,
    IAdvisoryLockService khoa,
    IThongBaoOutbox outbox) : IFifoService
{
    public const string TienToKhoaTrungLap = "GIU_CHO";
    public const string DoiTuongLoai = "DANG_KY_CHO";

    public async Task<DangKyChoDto?> CapLuotChoAsync(Guid maPhong, Guid maLoai, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Cấp lượt phải nằm trong giao dịch của thao tác kích hoạt.");
        }

        await khoa.LockPhongLoaiAsync(maPhong, maLoai, ct);

        if (await khaDung.TinhKhaDungAsync(maPhong, maLoai, ct) <= 0)
        {
            return null;
        }

        var maDauHang = await db.Database.SqlQuery<string>($"""
                SELECT ma_dang_ky AS "Value"
                FROM muon_tra.dang_ky_cho
                WHERE ma_phong = {maPhong} AND ma_loai = {maLoai} AND trang_thai = 'DANG_CHO'
                ORDER BY thoi_diem_dang_ky, ma_dang_ky
                LIMIT 1
                """)
            .SingleOrDefaultAsync(ct);
        if (maDauHang is null)
        {
            return null;
        }

        var dangKy = await db.Set<DangKyCho>().SingleAsync(d => d.MaDangKy == maDauHang, ct);
        var thamSo = await quyDinh.LayThamSoAsync(ct);
        var hanGiuCho = dongHo.UtcNow.ToUniversalTime().AddMinutes(thamSo.ThoiGianGiuChoPhut);

        try
        {
            dangKy.ThongBaoDenLuot(hanGiuCho);
        }
        catch (NghiepVuException loi)
        {
            throw DichLoiMienNghiepVu.Chuyen(loi);
        }

        await db.SaveChangesAsync(ct);

        await DangKyChoTruyVan.DanhSoLaiHangChoAsync(db, maPhong, maLoai, ct);

        var tenLoai = (await danhMuc.LayLoaiAsync(maLoai, ct))?.TenLoai
            ?? throw new InvalidOperationException($"Không tìm thấy loại thiết bị {maLoai} của đăng ký chờ {dangKy.MaDangKy}.");

        await outbox.GhiAsync(
            new YeuCauGhiThongBao(
                MaLoaiThongBao.ThietBiChoSanSang,
                dangKy.MaSv,
                null,
                new Dictionary<string, string>
                {
                    [ThamSoThongBao.TenLoai] = tenLoai,
                    [ThamSoThongBao.HanGiuCho] = string.Create(
                        CultureInfo.InvariantCulture,
                        $"{VnTime.GioLich(hanGiuCho):HH:mm} ngày {VnTime.NgayLich(hanGiuCho):dd/MM/yyyy}")
                },
                DoiTuongLoai,
                dangKy.MaDangKy,
                $"{TienToKhoaTrungLap}:{dangKy.MaDangKy}"),
            ct);

        return new DangKyChoDto(dangKy.MaDangKy, dangKy.MaSv, maPhong, maLoai, null, hanGiuCho, "DA_THONG_BAO");
    }
}
