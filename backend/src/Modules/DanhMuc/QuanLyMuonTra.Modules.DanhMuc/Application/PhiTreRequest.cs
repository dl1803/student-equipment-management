using System.Text.Json.Serialization;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed record PhiTreRequest([property: JsonRequired] long PhiTreMoiNgay);
