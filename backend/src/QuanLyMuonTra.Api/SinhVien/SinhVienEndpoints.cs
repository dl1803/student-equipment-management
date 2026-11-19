using Microsoft.AspNetCore.Mvc;
using QuanLyMuonTra.Api.Auth;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Api.SinhVien;

public static class SinhVienEndpoints
{
    public static IEndpointRouteBuilder MapSinhVienEndpoints(this IEndpointRouteBuilder duongDan)
    {
        duongDan.MapGet("/api/v1/sv/toi", async (
                [FromServices] ICurrentUser nguoiDung,
                [FromServices] LayHoSoSinhVienHandler boXuLy,
                CancellationToken ct) =>
            {
                var maSv = nguoiDung.MaSv
                    ?? throw new LoiNghiepVuException("CHUA_XAC_THUC", "Thiếu thông tin sinh viên đăng nhập.");
                var hoSo = await boXuLy.XuLyAsync(maSv, 0, ct)
                    ?? throw new LoiNghiepVuException("CHUA_XAC_THUC", "Hồ sơ sinh viên không tồn tại.");

                return Results.Ok(hoSo);
            })
            .RequireAuthorization(ChinhSach.SinhVien);

        duongDan.MapGet("/api/v1/quay/sinh-vien/the/{maThe}", async (
                string maThe,
                [FromServices] LayHoSoSinhVienHandler boXuLy,
                CancellationToken ct) =>
            {
                var hoSo = await boXuLy.XuLyAsync(maThe, LayHoSoSinhVienHandler.SoThietBiMuonThemKhiQuetThe, ct)
                    ?? throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy sinh viên theo mã thẻ.");

                return Results.Ok(hoSo);
            })
            .RequireAuthorization(ChinhSach.NhanVien);

        return duongDan;
    }
}

