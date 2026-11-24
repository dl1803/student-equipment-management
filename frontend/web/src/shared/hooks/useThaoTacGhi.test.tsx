// @vitest-environment jsdom
import {
  QueryClientProvider,
  useMutation,
  type QueryClient,
} from "@tanstack/react-query";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { ApiError } from "@/shared/api/apiError";
import { taoQueryClient } from "@/shared/api/queryClient";
import { useThaoTacGhi } from "./useThaoTacGhi";

type MutationFn = (bien: number) => Promise<string>;

function BieuMauGhi({ mutationFn }: { mutationFn: MutationFn }) {
  const mutation = useMutation<string, ApiError, number>({ mutationFn });
  const { gui, biKhoa, chuaBietKetQua, xacNhanDaKiemTra } =
    useThaoTacGhi(mutation);
  return (
    <div>
      <button type="button" disabled={biKhoa} onClick={() => gui(1)}>
        Lập phiếu mượn
      </button>
      <button
        type="button"
        onClick={() => {
          // Hai lần gọi trong cùng một sự kiện: không có lần vẽ lại nào ở giữa.
          gui(1);
          gui(1);
        }}
      >
        Bấm đúp đồng bộ
      </button>
      {chuaBietKetQua ? <p>Chưa xác định được kết quả</p> : null}
      <button type="button" onClick={xacNhanDaKiemTra}>
        Đã kiểm tra
      </button>
    </div>
  );
}

let queryClient: QueryClient;

beforeEach(() => {
  queryClient = taoQueryClient();
});

function hienThi(mutationFn: MutationFn) {
  render(
    <QueryClientProvider client={queryClient}>
      <BieuMauGhi mutationFn={mutationFn} />
    </QueryClientProvider>,
  );
  return screen.getByRole("button", { name: "Lập phiếu mượn" });
}

function hoanSau<T>(giaTri: T, ms = 10): Promise<T> {
  return new Promise((r) => setTimeout(() => r(giaTri), ms));
}

describe("useThaoTacGhi", () => {
  it("FA 3.11: hai lần gửi đồng bộ trong một sự kiện → chỉ một yêu cầu", async () => {
    const mutationFn = vi.fn<MutationFn>(() => hoanSau("ok"));
    hienThi(mutationFn);

    fireEvent.click(screen.getByRole("button", { name: "Bấm đúp đồng bộ" }));

    await waitFor(() => expect(mutationFn).toHaveBeenCalledTimes(1));
  });

  it("FA 3.11: bấm hai lần liên tiếp → chỉ gửi một yêu cầu; nút vô hiệu khi đang gửi", async () => {
    const mutationFn = vi.fn<MutationFn>(() => hoanSau("ok"));
    const nut = hienThi(mutationFn);

    fireEvent.click(nut);
    fireEvent.click(nut);

    await waitFor(() => expect(nut).toHaveProperty("disabled", true));
    expect(mutationFn).toHaveBeenCalledTimes(1);
    await waitFor(() => expect(nut).toHaveProperty("disabled", false));
  });

  it("FA 3.11: gửi xong thành công → mở khóa, gửi được lần sau", async () => {
    const mutationFn = vi.fn<MutationFn>(() => hoanSau("ok"));
    const nut = hienThi(mutationFn);

    fireEvent.click(nut);
    await waitFor(() => expect(mutationFn).toHaveBeenCalledTimes(1));
    await waitFor(() => expect(queryClient.isMutating()).toBe(0));
    await waitFor(() => expect(nut).toHaveProperty("disabled", false));
    fireEvent.click(nut);

    await waitFor(() => expect(mutationFn).toHaveBeenCalledTimes(2));
  });

  it.each([["MANG"], ["QUA_THOI_GIAN"], ["HUY"]] as const)(
    "FA 3.11: lỗi %s sau khi đã phát thao tác ghi → 'Chưa xác định được kết quả', giữ khóa tới khi đã kiểm tra",
    async (loai) => {
      const mutationFn = vi.fn<MutationFn>(() =>
        Promise.reject<string>({ loai } satisfies ApiError),
      );
      const nut = hienThi(mutationFn);

      fireEvent.click(nut);

      await screen.findByText("Chưa xác định được kết quả");
      expect(nut).toHaveProperty("disabled", true);
      fireEvent.click(screen.getByRole("button", { name: "Bấm đúp đồng bộ" }));
      expect(mutationFn).toHaveBeenCalledTimes(1); // Không gửi lại ngay, không coi là rollback

      fireEvent.click(screen.getByRole("button", { name: "Đã kiểm tra" }));
      await waitFor(() => expect(nut).toHaveProperty("disabled", false));
      expect(screen.queryByText("Chưa xác định được kết quả")).toBeNull();
      fireEvent.click(nut);
      await waitFor(() => expect(mutationFn).toHaveBeenCalledTimes(2));
    },
  );

  it("FA 3.4: lỗi nghiệp vụ có phản hồi (422) → biết chắc thất bại, mở khóa để sửa rồi gửi lại", async () => {
    const mutationFn = vi.fn<MutationFn>(() =>
      Promise.reject<string>({
        loai: "HTTP",
        status: 422,
        maLoi: "THIET_BI_KHONG_SAN_SANG",
      } satisfies ApiError),
    );
    const nut = hienThi(mutationFn);

    fireEvent.click(nut);
    await waitFor(() => expect(nut).toHaveProperty("disabled", false));

    expect(screen.queryByText("Chưa xác định được kết quả")).toBeNull();
    fireEvent.click(nut);
    await waitFor(() => expect(mutationFn).toHaveBeenCalledTimes(2));
  });
});
