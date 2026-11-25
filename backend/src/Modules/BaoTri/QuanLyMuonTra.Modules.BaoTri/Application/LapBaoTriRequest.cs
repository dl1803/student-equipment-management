using System.Text.Json.Serialization;

namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed record LapBaoTriRequest(
    [property: JsonRequired] string MaThietBi,
    [property: JsonRequired] string LoaiThucHien,
    string? MaPhieuMuon = null);
