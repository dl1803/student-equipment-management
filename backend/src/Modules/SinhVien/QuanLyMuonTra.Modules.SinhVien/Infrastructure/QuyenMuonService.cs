using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.SinhVien.Infrastructure;

public sealed class QuyenMuonService(DbContext db, IAuditLogger nhatKy) : IQuyenMuonService
{
    public Task<KetQuaDoiQuyenMuon> KhoaAsync(string maSv, CancellationToken ct) =>
        DoiAsync(maSv, MaTrangThaiQuyenMuon.BinhThuong, MaTrangThaiQuyenMuon.TamKhoa, ct);

    public Task<KetQuaDoiQuyenMuon> MoKhoaAsync(string maSv, CancellationToken ct) =>
        DoiAsync(maSv, MaTrangThaiQuyenMuon.TamKhoa, MaTrangThaiQuyenMuon.BinhThuong, ct);

    private async Task<KetQuaDoiQuyenMuon> DoiAsync(string maSv, string nguon, string dich, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Đổi quyền mượn phải chạy trong giao dịch của bên gọi.");
        }

        var soDong = await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE sinh_vien.sinh_vien
            SET trang_thai_quyen_muon = {dich}
            WHERE ma_sv = {maSv} AND trang_thai_quyen_muon = {nguon}
            """, ct);

        if (soDong == 1)
        {
            await nhatKy.GhiAsync(
                "SUA",
                "sinh_vien.sinh_vien",
                maSv,
                new Dictionary<string, object?> { ["TrangThaiQuyenMuon"] = nguon },
                new Dictionary<string, object?> { ["TrangThaiQuyenMuon"] = dich },
                ct);

            return new KetQuaDoiQuyenMuon(nguon, dich);
        }

        var hienTai = await db.Database.SqlQuery<string>($"""
                SELECT trang_thai_quyen_muon AS "Value" FROM sinh_vien.sinh_vien WHERE ma_sv = {maSv}
                """)
            .FirstOrDefaultAsync(ct)
            ?? throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy sinh viên.");

        return new KetQuaDoiQuyenMuon(hienTai, hienTai);
    }
}

