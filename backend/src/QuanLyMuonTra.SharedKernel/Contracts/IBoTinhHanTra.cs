namespace QuanLyMuonTra.SharedKernel.Contracts;

public interface IBoTinhHanTra
{
    DateTimeOffset TinhHanTra(DateTimeOffset gioGiao, int thoiGianMuonGio, TimeOnly? gioDongCua);
}
