using System.Text.Json.Serialization;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Api.SinhVien;

public sealed record SinhVienTheInfoResponse(
    [property: JsonPropertyName("maSV")] string MaSv,
    string HoTen,
    string? Lop,
    string? Khoa,
    string MaNhom,
    string TenNhom,
    DateOnly HanThe,
    string TrangThaiHoc,
    string TrangThaiQuyenMuon,
    int SoThietBiDangMuon,
    int SoThietBiToiDa,
    IReadOnlyList<BienBanChuaNopDto> DsBienBanChuaNop,
    bool HopLe,
    IReadOnlyList<LyDoKhongHopLeDto> DsLyDoKhongHopLe);

public sealed class LayHoSoSinhVienHandler(
    IHoSoSinhVienReader hoSoReader,
    IMuonTraReader muonTraReader,
    IBienBanChuaNopReader bienBanReader,
    IQuyDinhReader quyDinhReader,
    IClock dongHo)
{
    private const int SoThietBiMuonThemKhiQuetThe = 1;

    public async Task<SinhVienTheInfoResponse?> XuLyAsync(string maSv, CancellationToken ct)
    {
        var hoSo = await hoSoReader.LayTheoMaSvAsync(maSv, ct);
        if (hoSo is null)
        {
            return null;
        }

        var quyDinh = await quyDinhReader.LayQuyDinhNhomAsync(hoSo.MaNhom, ct)
            ?? throw new InvalidOperationException($"Thiếu quy định mượn cho nhóm {hoSo.MaNhom}.");
        var soDangCam = await muonTraReader.DemThietBiDangCamAsync(hoSo.MaSv, ct);
        var dsBienBan = await bienBanReader.LayDsChuaNopAsync(hoSo.MaSv, ct);

        var vuotGioiHan = quyDinh.VuotGioiHanSoThietBi(soDangCam, SoThietBiMuonThemKhiQuetThe);
        var dsLyDo = DieuKienMuonSinhVien.XacDinhLyDoKhongHopLe(hoSo, VnTime.NgayLich(dongHo.UtcNow), vuotGioiHan);

        return new SinhVienTheInfoResponse(
            hoSo.MaSv,
            hoSo.HoTen,
            hoSo.Lop,
            hoSo.Khoa,
            hoSo.MaNhom,
            hoSo.TenNhom,
            hoSo.HanThe,
            hoSo.TrangThaiHoc,
            hoSo.TrangThaiQuyenMuon,
            soDangCam,
            quyDinh.SoThietBiToiDa,
            dsBienBan,
            dsLyDo.Count == 0,
            dsLyDo);
    }
}

