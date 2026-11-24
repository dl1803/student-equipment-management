namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed record KetQuaPhanTrang<T>(int TongSo, int Trang, int KichThuoc, IReadOnlyList<T> DanhSach);
