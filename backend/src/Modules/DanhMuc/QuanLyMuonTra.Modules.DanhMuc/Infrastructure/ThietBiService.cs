using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

public sealed class ThietBiService : IThietBiService
{
    private readonly DbContext _db;
    private readonly IAuditLogger _nhatKy;

    public ThietBiService(DbContext db, IAuditLogger nhatKy)
    {
        _db = db;
        _nhatKy = nhatKy;
    }

    public async Task<bool> DatDangDuocMuonAsync(string maThietBi, Guid maPhong, Guid maLoai, CancellationToken ct)
    {
        DamBaoCoGiaoDich();
        var soDong = await _db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE danh_muc.thiet_bi
            SET tinh_trang = {TinhTrangThietBiMa.DangDuocMuon}
            WHERE ma_thiet_bi = {maThietBi}
              AND tinh_trang = {TinhTrangThietBiMa.SanSang}
              AND ma_phong = {maPhong}
              AND ma_loai = {maLoai}
              AND ngung_su_dung = false
            """, ct);
        return await GhiNhatKyNeuThanhCongAsync(soDong, maThietBi, TinhTrangThietBi.SanSang, TinhTrangThietBi.DangDuocMuon, ct);
    }

    public Task<bool> NhanTraNguyenVenAsync(string maThietBi, CancellationToken ct) =>
        ChuyenAsync(maThietBi, TinhTrangThietBi.DangDuocMuon, TinhTrangThietBi.SanSang, ct);

    public Task<bool> NhanTraHuHongAsync(string maThietBi, CancellationToken ct) =>
        ChuyenAsync(maThietBi, TinhTrangThietBi.DangDuocMuon, TinhTrangThietBi.DangBaoTriSuaChua, ct);

    public Task<bool> DanhDauMatAsync(string maThietBi, CancellationToken ct) =>
        ChuyenAsync(maThietBi, TinhTrangThietBi.DangDuocMuon, TinhTrangThietBi.DaBaoMat, ct);

    public Task<bool> ChuyenBaoTriAsync(string maThietBi, CancellationToken ct) =>
        ChuyenAsync(maThietBi, TinhTrangThietBi.SanSang, TinhTrangThietBi.DangBaoTriSuaChua, ct);

    public Task<bool> HoanThanhBaoTriAsync(string maThietBi, CancellationToken ct) =>
        ChuyenAsync(maThietBi, TinhTrangThietBi.DangBaoTriSuaChua, TinhTrangThietBi.SanSang, ct);

    /// <summary>
    /// Chuyển tình trạng thiết bị sang ĐÃ THANH LÝ nếu đang SẴN SÀNG hoặc ĐANG BẢO TRÌ SỬA CHỮA.
    /// Yêu cầu bên gọi chạy trong giao dịch và giữ khóa tư vấn cặp (phòng, loại) trước khi gọi.
    /// </summary>
    public async Task<bool> ThanhLyAsync(string maThietBi, CancellationToken ct)
    {
        if (await ChuyenAsync(maThietBi, TinhTrangThietBi.SanSang, TinhTrangThietBi.DaThanhLy, ct))
        {
            return true;
        }

        return await ChuyenAsync(maThietBi, TinhTrangThietBi.DangBaoTriSuaChua, TinhTrangThietBi.DaThanhLy, ct);
    }

    private async Task<bool> ChuyenAsync(string maThietBi, TinhTrangThietBi nguon, TinhTrangThietBi dich, CancellationToken ct)
    {
        DamBaoCoGiaoDich();
        var maNguon = TinhTrangThietBiMa.ToMa(nguon);
        var maDich = TinhTrangThietBiMa.ToMa(dich);
        var soDong = await _db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE danh_muc.thiet_bi
            SET tinh_trang = {maDich}
            WHERE ma_thiet_bi = {maThietBi}
              AND tinh_trang = {maNguon}
            """, ct);
        return await GhiNhatKyNeuThanhCongAsync(soDong, maThietBi, nguon, dich, ct);
    }

    private async Task<bool> GhiNhatKyNeuThanhCongAsync(
        int soDong,
        string maThietBi,
        TinhTrangThietBi nguon,
        TinhTrangThietBi dich,
        CancellationToken ct)
    {
        if (soDong != 1)
        {
            return false;
        }

        await _nhatKy.GhiAsync(
            "SUA",
            "danh_muc.thiet_bi",
            maThietBi,
            new Dictionary<string, object?> { ["TinhTrang"] = nguon },
            new Dictionary<string, object?> { ["TinhTrang"] = dich },
            ct);
        return true;
    }

    private void DamBaoCoGiaoDich()
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Chuyển tình trạng thiết bị phải chạy trong giao dịch của bên gọi.");
        }
    }
}
