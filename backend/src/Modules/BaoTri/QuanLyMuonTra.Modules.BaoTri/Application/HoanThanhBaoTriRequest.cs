using System.Text.Json.Serialization;

namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed record HoanThanhBaoTriRequest(
    [property: JsonRequired] string NoiDungThucHien,
    [property: JsonRequired] string KetQua,
    [property: JsonRequired] DateTimeOffset NgayHoanThanh);
