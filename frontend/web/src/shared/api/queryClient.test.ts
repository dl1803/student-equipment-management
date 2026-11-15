import {
  MutationObserver,
  QueryObserver,
  onlineManager,
} from "@tanstack/react-query";
import { afterEach, describe, expect, it, vi } from "vitest";
import { taoQueryClient } from "./queryClient";

afterEach(() => {
  onlineManager.setOnline(true);
});

describe("taoQueryClient", () => {
  it("FA 4.7: mặc định staleTime 0, retry 0, không tải lại khi đổi cửa sổ; mutation retry 0, networkMode always", () => {
    const { queries, mutations } = taoQueryClient().getDefaultOptions();
    expect(queries).toMatchObject({
      staleTime: 0,
      retry: 0,
      refetchOnWindowFocus: false,
    });
    expect(mutations).toMatchObject({ retry: 0, networkMode: "always" });
  });

  it("FA-08: mutation lỗi không tự gửi lại", async () => {
    const queryClient = taoQueryClient();
    const mutationFn = vi.fn().mockRejectedValue({ loai: "HTTP", status: 500 });
    const observer = new MutationObserver(queryClient, { mutationFn });

    await expect(observer.mutate(undefined)).rejects.toEqual({
      loai: "HTTP",
      status: 500,
    });
    expect(mutationFn).toHaveBeenCalledTimes(1);
  });

  it("FA 3.6: offline trước khi bấm → gọi HTTP ngay và nhận lỗi mạng, không giữ chờ có mạng", async () => {
    onlineManager.setOnline(false);
    const queryClient = taoQueryClient();
    const mutationFn = vi.fn().mockRejectedValue({ loai: "MANG" });
    const observer = new MutationObserver(queryClient, { mutationFn });

    await expect(observer.mutate(undefined)).rejects.toEqual({ loai: "MANG" });

    expect(mutationFn).toHaveBeenCalledTimes(1);
    expect(
      queryClient
        .getMutationCache()
        .getAll()
        .some((m) => m.state.isPaused),
    ).toBe(false);
  });

  it("FA 3.6: nối mạng lại không tự phát thao tác ghi đã bấm", async () => {
    onlineManager.setOnline(false);
    const queryClient = taoQueryClient();
    const mutationFn = vi.fn().mockRejectedValue({ loai: "MANG" });
    const observer = new MutationObserver(queryClient, { mutationFn });
    await expect(observer.mutate(undefined)).rejects.toEqual({ loai: "MANG" });

    onlineManager.setOnline(true);
    await queryClient.resumePausedMutations();

    expect(mutationFn).toHaveBeenCalledTimes(1);
  });

  it("FA 3.4: truy vấn khi offline vẫn chạy để báo lỗi mạng kèm Thử lại, và không tự thử lại", async () => {
    onlineManager.setOnline(false);
    const queryClient = taoQueryClient();
    const queryFn = vi.fn().mockRejectedValue({ loai: "MANG" });
    const observer = new QueryObserver(queryClient, {
      queryKey: ["quay", "phieu-yeu-cau"],
      queryFn,
    });
    const huy = observer.subscribe(() => undefined);

    await vi.waitFor(() =>
      expect(observer.getCurrentResult().status).toBe("error"),
    );
    expect(queryFn).toHaveBeenCalledTimes(1);
    expect(observer.getCurrentResult().fetchStatus).toBe("idle");
    huy();
  });
});
