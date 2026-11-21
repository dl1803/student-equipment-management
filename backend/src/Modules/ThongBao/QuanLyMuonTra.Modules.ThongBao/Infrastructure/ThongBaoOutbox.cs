using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.ThongBao.Application;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ThongBao.Infrastructure;

public sealed class ThongBaoOutbox(DbContext db, IClock dongHo, INhanVienEmailReader emailNhanVien) : IThongBaoOutbox
{
    private const string KenhUngDung = "UNG_DUNG";
    private const string KenhEmail = "EMAIL";
    private const string ChoGui = "CHO_GUI";
    private const string DaGui = "DA_GUI";

    public async Task GhiAsync(YeuCauGhiThongBao yeuCau, CancellationToken ct)
    {
        var coSv = !string.IsNullOrWhiteSpace(yeuCau.MaSvNhan);
        var coNv = yeuCau.MaNvNhan is not null;
        if (coSv == coNv)
        {
            throw new ArgumentException("Thông báo phải có đúng một người nhận: sinh viên hoặc nhân viên.", nameof(yeuCau));
        }

        if ((yeuCau.DoiTuongLoai is null) != (yeuCau.DoiTuongId is null))
        {
            throw new ArgumentException("Đối tượng nguồn phải có đủ loại và mã, hoặc bỏ trống cả hai.", nameof(yeuCau));
        }

        var noiDungUngDung = MauThongBao.TaoNoiDungUngDung(yeuCau.LoaiThongBao, yeuCau.ThamSo);
        var noiDungEmail = MauThongBao.TaoNoiDungEmailHtml(yeuCau.LoaiThongBao, yeuCau.ThamSo);

        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Ghi thông báo phải nằm trong giao dịch của thao tác nghiệp vụ.");
        }

        await ChenAsync(yeuCau, KenhUngDung, DaGui, noiDungUngDung, ct);

        var coEmailNguoiNhan = coSv || await CoEmailNhanVienAsync(yeuCau.MaNvNhan!.Value, ct);
        if (coEmailNguoiNhan)
        {
            await ChenAsync(yeuCau, KenhEmail, ChoGui, noiDungEmail, ct);
        }
    }

    private async Task<bool> CoEmailNhanVienAsync(Guid maNv, CancellationToken ct) =>
        await emailNhanVien.LayEmailAsync(maNv, ct) is not null;

    private Task<int> ChenAsync(YeuCauGhiThongBao yeuCau, string kenhGui, string trangThaiGui, string noiDung, CancellationToken ct)
    {
        var bayGio = dongHo.UtcNow;

        return db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO thong_bao.thong_bao
                (ma_thong_bao, loai_thong_bao, noi_dung, kenh_gui, thoi_diem_gui, ma_sv_nhan, ma_nv_nhan,
                 trang_thai_gui, so_lan_thu, khoa_trung_lap, doi_tuong_loai, doi_tuong_id)
            VALUES
                ({Guid.CreateVersion7()}, {yeuCau.LoaiThongBao}, {noiDung}, {kenhGui}, {bayGio}, {yeuCau.MaSvNhan}, {yeuCau.MaNvNhan},
                 {trangThaiGui}, 0, {yeuCau.KhoaTrungLap}, {yeuCau.DoiTuongLoai}, {yeuCau.DoiTuongId})
            ON CONFLICT (khoa_trung_lap, kenh_gui) WHERE khoa_trung_lap IS NOT NULL DO NOTHING
            """,
            ct);
    }
}

