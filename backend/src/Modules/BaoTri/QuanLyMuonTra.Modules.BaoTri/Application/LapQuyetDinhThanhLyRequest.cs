using System.Text.Json.Serialization;

namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed record LapQuyetDinhThanhLyRequest(
    [property: JsonRequired] DateOnly NgayThanhLy,
    [property: JsonRequired] IReadOnlyList<string> DsMaThietBi);
