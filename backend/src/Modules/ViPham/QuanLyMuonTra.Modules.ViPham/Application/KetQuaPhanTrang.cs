namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed record KetQuaPhanTrang<T>(int TongSo, int Trang, int KichThuoc, IReadOnlyList<T> DanhSach);

