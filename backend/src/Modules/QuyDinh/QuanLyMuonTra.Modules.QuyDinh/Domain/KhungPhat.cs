using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.QuyDinh.Domain;

public sealed class KhungPhat : IAuditable
{
    private KhungPhat()
    {
    }

    internal KhungPhat(string tenKhung, decimal tyLePhanTram)
    {
        KiemTra(tenKhung, tyLePhanTram);
        MaKhung = Guid.CreateVersion7();
        TenKhung = tenKhung;
        TyLePhanTram = tyLePhanTram;
    }

    public Guid MaKhung { get; private set; }

    public string TenKhung { get; private set; } = string.Empty;

    public decimal TyLePhanTram { get; private set; }

    public bool DangSuDung { get; private set; } = true;

    internal void CapNhat(string tenKhung, decimal tyLePhanTram)
    {
        KiemTra(tenKhung, tyLePhanTram);
        TenKhung = tenKhung;
        TyLePhanTram = tyLePhanTram;
    }

    internal void Tat()
    {
        DangSuDung = false;
    }

    private static void KiemTra(string tenKhung, decimal tyLePhanTram)
    {
        RangBuocCauHinh.YeuCau(!string.IsNullOrWhiteSpace(tenKhung), "Tên khung phạt không được để trống.");
        RangBuocCauHinh.YeuCau(tyLePhanTram > 0 && tyLePhanTram <= 100, "Tỷ lệ khung phạt phải lớn hơn 0 và không vượt quá 100.");
        RangBuocCauHinh.YeuCau(decimal.Round(tyLePhanTram, 2) == tyLePhanTram, "Tỷ lệ khung phạt chỉ được có tối đa hai chữ số thập phân.");
    }
}
