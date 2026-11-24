namespace QuanLyMuonTra.Modules.MuonTra.Contracts;

public record GiuChoHetHanEvent(
    Guid MaDangKy,
    string MaSv,
    Guid MaPhong,
    Guid MaLoai,
    DateTimeOffset ThoiDiemHetHan
);
