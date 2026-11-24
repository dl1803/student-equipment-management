using System.Text.Json.Serialization;

namespace QuanLyMuonTra.Modules.QuyDinh.Application;

public sealed record KhungPhatRequest(
    [property: JsonRequired] string TenKhung,
    [property: JsonRequired] decimal TyLePhanTram);
