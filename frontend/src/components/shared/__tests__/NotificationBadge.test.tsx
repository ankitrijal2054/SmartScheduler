/**
 * NotificationBadge Component Tests
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { NotificationBadge } from "../NotificationBadge";

// Mock useNotifications
vi.mock("@/hooks/useNotifications", () => ({
  useNotifications: () => ({
    notifications: [
      {
        id: "1",
        type: "NewJobAssigned",
        message: "New job assigned",
        createdAt: new Date().toISOString(),
      },
      {
        id: "2",
        type: "JobReassigned",
        message: "Job reassigned",
        createdAt: new Date().toISOString(),
      },
    ],
  }),
}));

// Mock NotificationCenter
vi.mock("@/features/contractor/NotificationCenter", () => ({
  NotificationCenter: ({ isOpen, onClose }: any) =>
    isOpen ? (
      <div data-testid="notification-center">
        <button onClick={onClose}>Close</button>
      </div>
    ) : null,
}));

describe("NotificationBadge", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render bell icon", () => {
    render(<NotificationBadge />);

    const button = screen.getByRole("button", {
      name: /Notifications \(\d+ unread\)/i,
    });
    expect(button).toBeInTheDocument();
  });

  it("should display notification count badge", () => {
    render(<NotificationBadge />);

    expect(screen.getByText("2")).toBeInTheDocument();
  });

  it("should open notification center when clicked", async () => {
    const user = userEvent.setup();
    render(<NotificationBadge />);

    const button = screen.getByRole("button", {
      name: /Notifications/i,
    });
    await user.click(button);

    await waitFor(() => {
      expect(screen.getByTestId("notification-center")).toBeInTheDocument();
    });
  });

  it("should have proper accessibility label", () => {
    render(<NotificationBadge />);

    const button = screen.getByRole("button", {
      name: /Notifications \(2 unread\)/i,
    });
    expect(button).toHaveAttribute(
      "aria-label",
      "Notifications (2 unread)"
    );
  });
});



