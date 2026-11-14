namespace QuanLyMuonTra.SharedKernel;

public class NghiepVuException : Exception
{
    public NghiepVuException(string maLoi, string message)
        : base(message)
    {
        MaLoi = maLoi;
    }

    public string MaLoi { get; }
}
