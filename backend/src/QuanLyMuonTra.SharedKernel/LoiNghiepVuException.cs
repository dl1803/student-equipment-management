namespace QuanLyMuonTra.SharedKernel;

public sealed class LoiNghiepVuException : Exception
{
    public LoiNghiepVuException(string maLoi, string thongDiep)
        : base(thongDiep)
    {
        MaHttp = MaLoiNghiepVu.LayMaHttp(maLoi);
        MaLoi = maLoi;
    }

    public string MaLoi { get; }

    public int MaHttp { get; }
}

