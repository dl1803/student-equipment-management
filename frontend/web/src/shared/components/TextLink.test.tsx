// @vitest-environment jsdom
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { TextLink } from "./TextLink";

describe("TextLink", () => {
  it("CMP-03: là liên kết thật có href, gạch chân, màu token liên kết", () => {
    render(<TextLink href="/nv/bien-ban">Xem biên bản</TextLink>);
    const lienKet = screen.getByRole("link", { name: "Xem biên bản" });
    expect(lienKet.getAttribute("href")).toBe("/nv/bien-ban");
    expect(lienKet.className).toContain("underline");
    expect(lienKet.className).toContain("text-text-link");
  });

  it("CMP-03: liên kết là mã dùng type.code, không xuống dòng", () => {
    render(
      <TextLink href="/nv/nhan-tra/kiem-tra?maPhieu=PM-1" laMa>
        PM-261024-000123
      </TextLink>,
    );
    const lienKet = screen.getByRole("link", { name: "PM-261024-000123" });
    expect(lienKet.className).toContain("type-code");
    expect(lienKet.className).toContain("whitespace-nowrap");
  });
});
