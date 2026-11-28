using QuanLyMuonTra.Modules.MuonTra.Domain;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed record KetQuaKiemTraYeuCau(
    string MaThietBi,
    KetQuaKiemTra KetQua,
    string? MoTa,
    Guid? MaKhungPhat,
    IReadOnlyList<Guid> DsMaAnh);
