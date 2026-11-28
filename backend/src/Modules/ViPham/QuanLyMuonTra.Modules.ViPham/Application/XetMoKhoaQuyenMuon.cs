using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.ViPham.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed record LyDoConKhoaDto(string MaLoi, string ThongDiep);

public sealed record KetQuaXetMoKhoaDto(
    string MaSV,
    string TrangThaiQuyenMuon,
    bool DaMoKhoa,
    IReadOnlyList<LyDoConKhoaDto> DsLyDoConKhoa);

public sealed class XetMoKhoaQuyenMuon(
    DbContext db,
    IBienBanChuaNopReader bienBanChuaNop,
    IQuyenMuonService quyenMuon)
{
    private const string MaLoiSvBiKhoaQuyenMuon = "SV_BI_KHOA_QUYEN_MUON";

    public async Task<KetQuaXetMoKhoaDto> XetAsync(string maSv, CancellationToken ct)
    {
        await GiaoDichViPham.KhoaDongSinhVienAsync(db, maSv, ct);

        var trangThai = await db.Database.SqlQuery<string>($"""
                SELECT trang_thai_quyen_muon AS "Value" FROM sinh_vien.sinh_vien WHERE ma_sv = {maSv}
                """)
            .FirstOrDefaultAsync(ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy sinh viên.");

        var soBienBanChuaNop = (await bienBanChuaNop.LayDsChuaNopAsync(maSv, ct)).Count;
        var soThietBiChuaTra = await db.Database.SqlQuery<long>($"""
                SELECT count(*) AS "Value"
                FROM muon_tra.chi_tiet_phieu_muon c
                JOIN muon_tra.phieu_muon p ON p.ma_phieu = c.ma_phieu
                WHERE p.ma_sv = {maSv}
                  AND p.trang_thai = 'QUA_HAN'
                  AND c.ket_qua_kiem_tra IS NULL
                """)
            .SingleAsync(ct);

        var dsLyDo = new List<LyDoConKhoaDto>();
        if (trangThai == MaTrangThaiQuyenMuon.CamMuonDaiHan)
        {
            dsLyDo.Add(new LyDoConKhoaDto(MaLoiSvBiKhoaQuyenMuon, "Sinh viên đang bị cấm mượn dài hạn."));
        }

        if (soBienBanChuaNop > 0)
        {
            dsLyDo.Add(new LyDoConKhoaDto(
                MaLoiSvBiKhoaQuyenMuon,
                $"Còn {soBienBanChuaNop} biên bản vi phạm chưa nộp."));
        }

        if (soThietBiChuaTra > 0)
        {
            dsLyDo.Add(new LyDoConKhoaDto(
                MaLoiSvBiKhoaQuyenMuon,
                $"Còn {soThietBiChuaTra} thiết bị chưa trả của phiếu quá hạn."));
        }

        var trangThaiSau = trangThai;
        var daMoKhoa = false;
        if (trangThai == MaTrangThaiQuyenMuon.TamKhoa && dsLyDo.Count == 0)
        {
            var ketQua = await quyenMuon.MoKhoaAsync(maSv, ct);
            trangThaiSau = ketQua.TrangThaiSau;
            daMoKhoa = ketQua.DaThayDoi;
        }

        if (trangThaiSau == MaTrangThaiQuyenMuon.BinhThuong)
        {
            dsLyDo.Clear();
        }

        return new KetQuaXetMoKhoaDto(maSv, trangThaiSau, daMoKhoa, dsLyDo);
    }
}

