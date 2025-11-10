import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { ProtectedRoute } from "../ProtectedRoute";
import * as useAuthModule from "@/hooks/useAuthContext";

// Mock the useAuth hook
vi.mock("@/hooks/useAuthContext", () => ({
  useAuth: vi.fn(),
}));

// Mock LoadingSpinner
vi.mock("@/components/shared/LoadingSpinner", () => ({
  LoadingSpinner: () => <div data-testid="loading-spinner">Loading...</div>,
}));

const MockComponent = () => (
  <div data-testid="protected-content">Protected Content</div>
);

describe("ProtectedRoute", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should show loading spinner when loading", () => {
    const mockUseAuth = useAuthModule.useAuth as any;
    mockUseAuth.mockReturnValue({
      isAuthenticated: false,
      isLoading: true,
      user: null,
    });

    render(
      <BrowserRouter>
        <ProtectedRoute>
          <MockComponent />
        </ProtectedRoute>
      </BrowserRouter>
    );

    expect(screen.getByTestId("loading-spinner")).toBeInTheDocument();
  });

  it("should redirect to login when not authenticated", () => {
    const mockUseAuth = useAuthModule.useAuth as any;
    mockUseAuth.mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      user: null,
    });

    render(
      <BrowserRouter>
        <ProtectedRoute>
          <MockComponent />
        </ProtectedRoute>
      </BrowserRouter>
    );

    // Should not show protected content
    expect(screen.queryByTestId("protected-content")).not.toBeInTheDocument();
    // Should redirect (can verify by checking location)
    expect(window.location.pathname).toBe("/");
  });

  it("should render content when authenticated without role requirement", () => {
    const mockUseAuth = useAuthModule.useAuth as any;
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      user: {
        id: "user-123",
        email: "test@example.com",
        role: "Dispatcher",
      },
    });

    render(
      <BrowserRouter>
        <ProtectedRoute>
          <MockComponent />
        </ProtectedRoute>
      </BrowserRouter>
    );

    expect(screen.getByTestId("protected-content")).toBeInTheDocument();
  });

  it("should render content when role matches required role", () => {
    const mockUseAuth = useAuthModule.useAuth as any;
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      user: {
        id: "user-123",
        email: "test@example.com",
        role: "Dispatcher",
      },
    });

    render(
      <BrowserRouter>
        <ProtectedRoute requiredRole="Dispatcher">
          <MockComponent />
        </ProtectedRoute>
      </BrowserRouter>
    );

    expect(screen.getByTestId("protected-content")).toBeInTheDocument();
  });

  it("should redirect to unauthorized when role does not match", () => {
    const mockUseAuth = useAuthModule.useAuth as any;
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      user: {
        id: "user-123",
        email: "test@example.com",
        role: "Customer",
      },
    });

    render(
      <BrowserRouter>
        <ProtectedRoute requiredRole="Dispatcher">
          <MockComponent />
        </ProtectedRoute>
      </BrowserRouter>
    );

    // Should not show protected content
    expect(screen.queryByTestId("protected-content")).not.toBeInTheDocument();
  });
});


