namespace QuanLyMuonTra.SharedKernel;

public interface ICurrentUser
{
    Guid? MaNguoiDung { get; }

    string? MaSv { get; }
}

