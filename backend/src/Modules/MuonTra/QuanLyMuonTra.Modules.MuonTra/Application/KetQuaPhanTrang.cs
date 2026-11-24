namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed record KetQuaPhanTrang<T>(int TongSo, int Trang, int KichThuoc, IReadOnlyList<T> DanhSach);
