import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { SessionTimeoutModal } from "../../components/modals/SessionTimeoutModal";

describe("SessionTimeoutModal", () => {
  const defaultProps = {
    isOpen: true,
    secondsRemaining: 120,
    onExtendSession: vi.fn(),
    onLogout: vi.fn(),
  };

  it("renders nothing when isOpen is false", () => {
    const { container } = render(
      <SessionTimeoutModal {...defaultProps} isOpen={false} />,
    );
    expect(container.firstChild).toBeNull();
  });

  it("renders modal when isOpen is true", () => {
    render(<SessionTimeoutModal {...defaultProps} />);
    expect(screen.getByText("Session Expiring Soon")).toBeInTheDocument();
  });

  it("displays countdown in MM:SS format", () => {
    render(<SessionTimeoutModal {...defaultProps} secondsRemaining={90} />);
    expect(screen.getByText("1:30")).toBeInTheDocument();
  });

  it("displays 0:05 for 5 seconds remaining", () => {
    render(<SessionTimeoutModal {...defaultProps} secondsRemaining={5} />);
    expect(screen.getByText("0:05")).toBeInTheDocument();
  });

  it("calls onExtendSession when Extend Session button is clicked", async () => {
    const user = userEvent.setup();
    const onExtend = vi.fn();
    render(
      <SessionTimeoutModal {...defaultProps} onExtendSession={onExtend} />,
    );

    await user.click(screen.getByText("Extend Session"));

    expect(onExtend).toHaveBeenCalledOnce();
  });

  it("calls onLogout when Logout Now button is clicked", async () => {
    const user = userEvent.setup();
    const onLogout = vi.fn();
    render(<SessionTimeoutModal {...defaultProps} onLogout={onLogout} />);

    await user.click(screen.getByText("Logout Now"));

    expect(onLogout).toHaveBeenCalledOnce();
  });

  it("shows inactivity warning message", () => {
    render(<SessionTimeoutModal {...defaultProps} />);
    expect(
      screen.getByText(/your session will expire due to inactivity/i),
    ).toBeInTheDocument();
  });
});
