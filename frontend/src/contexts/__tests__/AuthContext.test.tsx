import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { AuthProvider } from "../AuthContext";
import { useAuth } from "@/hooks/useAuthContext";
import * as authServiceModule from "@/services/authService";

// Mock the authService
vi.mock("@/services/authService", () => ({
  authService: {
    login: vi.fn(),
    signup: vi.fn(),
    logout: vi.fn(),
    refreshToken: vi.fn(),
    isTokenExpired: vi.fn(),
    extractRole: vi.fn(),
    decodeToken: vi.fn(),
  },
}));

// Test component that uses auth context
const TestComponent = () => {
  const { user, token, isAuthenticated, isLoading, error, login, logout } =
    useAuth();

  return (
    <div>
      <div data-testid="loading">{isLoading ? "loading" : "ready"}</div>
      <div data-testid="authenticated">
        {isAuthenticated ? "authenticated" : "not-authenticated"}
      </div>
      {user && (
        <>
          <div data-testid="user-email">{user.email}</div>
          <div data-testid="user-role">{user.role}</div>
        </>
      )}
      {error && <div data-testid="error">{error}</div>}
      <button onClick={() => login("test@example.com", "password123")}>
        Login
      </button>
      <button onClick={logout}>Logout</button>
    </div>
  );
};

describe("AuthContext", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  it("should initialize with no user", async () => {
    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId("loading")).toHaveTextContent("ready");
      expect(screen.getByTestId("authenticated")).toHaveTextContent(
        "not-authenticated"
      );
    });
  });

  it("should load user from localStorage if valid token", async () => {
    const mockToken = "valid_token";
    const mockPayload = {
      sub: "user-123",
      email: "test@example.com",
      role: "Dispatcher",
      exp: Math.floor(Date.now() / 1000) + 3600,
    };

    localStorage.setItem("auth_token", mockToken);

    const authService = authServiceModule.authService as any;
    authService.isTokenExpired.mockReturnValue(false);
    authService.extractRole.mockReturnValue("Dispatcher");
    authService.decodeToken.mockReturnValue(mockPayload);

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId("authenticated")).toHaveTextContent(
        "authenticated"
      );
      expect(screen.getByTestId("user-email")).toHaveTextContent(
        "test@example.com"
      );
      expect(screen.getByTestId("user-role")).toHaveTextContent("Dispatcher");
    });
  });

  it("should clear expired token on initialization", async () => {
    const expiredToken = "expired_token";
    localStorage.setItem("auth_token", expiredToken);

    const authService = authServiceModule.authService as any;
    authService.isTokenExpired.mockReturnValue(true);

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(localStorage.getItem("auth_token")).toBeNull();
      expect(screen.getByTestId("authenticated")).toHaveTextContent(
        "not-authenticated"
      );
    });
  });

  it("should handle login", async () => {
    const mockTokenResponse = {
      accessToken: "new_token",
      refreshToken: "refresh_token",
      expiresIn: 3600,
      tokenType: "Bearer",
    };

    const mockPayload = {
      sub: "user-123",
      email: "test@example.com",
      role: "Customer",
      exp: Math.floor(Date.now() / 1000) + 3600,
    };

    const authService = authServiceModule.authService as any;
    authService.login.mockResolvedValue(mockTokenResponse);
    authService.extractRole.mockReturnValue("Customer");
    authService.decodeToken.mockReturnValue(mockPayload);

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    const loginButton = screen.getByText("Login");
    fireEvent.click(loginButton);

    await waitFor(() => {
      expect(authService.login).toHaveBeenCalledWith(
        "test@example.com",
        "password123"
      );
      expect(localStorage.getItem("auth_token")).toBe("new_token");
      expect(screen.getByTestId("authenticated")).toHaveTextContent(
        "authenticated"
      );
      expect(screen.getByTestId("user-email")).toHaveTextContent(
        "test@example.com"
      );
    });
  });

  it("should handle login error", async () => {
    const authService = authServiceModule.authService as any;
    authService.login.mockRejectedValue(new Error("Invalid credentials"));

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    const loginButton = screen.getByText("Login");
    fireEvent.click(loginButton);

    await waitFor(() => {
      expect(screen.getByTestId("error")).toHaveTextContent(
        "Invalid credentials"
      );
      expect(screen.getByTestId("authenticated")).toHaveTextContent(
        "not-authenticated"
      );
    });
  });

  it("should handle logout", async () => {
    const mockToken = "valid_token";
    const mockPayload = {
      sub: "user-123",
      email: "test@example.com",
      role: "Dispatcher",
      exp: Math.floor(Date.now() / 1000) + 3600,
    };

    localStorage.setItem("auth_token", mockToken);
    localStorage.setItem("refresh_token", "refresh_token");

    const authService = authServiceModule.authService as any;
    authService.isTokenExpired.mockReturnValue(false);
    authService.extractRole.mockReturnValue("Dispatcher");
    authService.decodeToken.mockReturnValue(mockPayload);
    authService.logout.mockResolvedValue(undefined);

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId("authenticated")).toHaveTextContent(
        "authenticated"
      );
    });

    const logoutButton = screen.getByText("Logout");
    fireEvent.click(logoutButton);

    await waitFor(() => {
      expect(authService.logout).toHaveBeenCalledWith("refresh_token");
      expect(localStorage.getItem("auth_token")).toBeNull();
      expect(localStorage.getItem("refresh_token")).toBeNull();
      expect(screen.getByTestId("authenticated")).toHaveTextContent(
        "not-authenticated"
      );
    });
  });
});

