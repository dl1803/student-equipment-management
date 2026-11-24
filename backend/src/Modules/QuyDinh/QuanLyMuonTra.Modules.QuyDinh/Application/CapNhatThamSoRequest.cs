using System.Text.Json.Serialization;

namespace QuanLyMuonTra.Modules.QuyDinh.Application;

public sealed record CapNhatThamSoRequest(
    [property: JsonRequired] int SoNgayKhoa,
    [property: JsonRequired] int ThoiGianGiuChoPhut,
    [property: JsonRequired] int AnHanPhut,
    [property: JsonRequired] int SoGioGiaHanToiDa,
    [property: JsonRequired] int NguongViPham,
    [property: JsonRequired] int SoLanDangNhapSaiToiDa,
    [property: JsonRequired, JsonConverter(typeof(GioQuayJsonConverter))] TimeOnly? GioMoCua,
    [property: JsonRequired, JsonConverter(typeof(GioQuayJsonConverter))] TimeOnly? GioDongCua);
