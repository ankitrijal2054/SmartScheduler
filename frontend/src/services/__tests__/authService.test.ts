import { describe, it, expect, vi, beforeEach } from "vitest";
import axios from "axios";
import { authService, TokenResponse } from "../authService";

vi.mock("axios");
const mockedAxios = axios as any;

describe("AuthService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe("login", () => {
    it("should successfully login with valid credentials", async () => {
      const mockResponse: TokenResponse = {
        accessToken: "token_123",
        refreshToken: "refresh_123",
        expiresIn: 3600,
        tokenType: "Bearer",
      };

      mockedAxios.post.mockResolvedValueOnce({ data: mockResponse });

      const result = await authService.login("test@example.com", "password123");

      expect(result).toEqual(mockResponse);
      expect(mockedAxios.post).toHaveBeenCalledWith(
        expect.stringContaining("/api/v1/auth/login"),
        { email: "test@example.com", password: "password123" },
        { withCredentials: true }
      );
    });

    it("should throw error on invalid credentials", async () => {
      mockedAxios.post.mockRejectedValueOnce({
        response: {
          status: 401,
          data: { error: { message: "Invalid email or password" } },
        },
      } as any);

      await expect(
        authService.login("test@example.com", "wrongpassword")
      ).rejects.toThrow();
    });

    it("should handle network errors", async () => {
      mockedAxios.post.mockRejectedValueOnce({} as any);

      await expect(
        authService.login("test@example.com", "password123")
      ).rejects.toThrow();
    });
  });

  describe("signup", () => {
    it("should successfully signup with valid data", async () => {
      const mockResponse: TokenResponse = {
        accessToken: "token_456",
        refreshToken: "refresh_456",
        expiresIn: 3600,
        tokenType: "Bearer",
      };

      mockedAxios.post.mockResolvedValueOnce({ data: mockResponse });

      const result = await authService.signup(
        "newuser@example.com",
        "password123",
        "Dispatcher"
      );

      expect(result).toEqual(mockResponse);
      expect(mockedAxios.post).toHaveBeenCalledWith(
        expect.stringContaining("/api/v1/auth/signup"),
        {
          email: "newuser@example.com",
          password: "password123",
          role: "Dispatcher",
        },
        { withCredentials: true }
      );
    });

    it("should throw error on duplicate email", async () => {
      mockedAxios.post.mockRejectedValueOnce({
        response: {
          status: 409,
          data: {
            error: { message: "An account with this email already exists" },
          },
        },
      } as any);

      await expect(
        authService.signup("existing@example.com", "password123", "Customer")
      ).rejects.toThrow();
    });
  });

  describe("refreshToken", () => {
    it("should successfully refresh token", async () => {
      const mockResponse: TokenResponse = {
        accessToken: "new_token_789",
        expiresIn: 3600,
        tokenType: "Bearer",
      };

      mockedAxios.post.mockResolvedValueOnce({ data: mockResponse });

      const result = await authService.refreshToken("refresh_token_123");

      expect(result).toEqual(mockResponse);
      expect(mockedAxios.post).toHaveBeenCalledWith(
        expect.stringContaining("/api/v1/auth/refresh"),
        { refreshToken: "refresh_token_123" },
        { withCredentials: true }
      );
    });

    it("should throw error on invalid refresh token", async () => {
      mockedAxios.post.mockRejectedValueOnce({
        response: {
          status: 401,
          data: { error: { message: "Invalid refresh token" } },
        },
      } as any);

      await expect(
        authService.refreshToken("invalid_refresh_token")
      ).rejects.toThrow();
    });
  });

  describe("logout", () => {
    it("should call logout endpoint", async () => {
      mockedAxios.post.mockResolvedValueOnce({ data: {} });

      await authService.logout("refresh_token_123");

      expect(mockedAxios.post).toHaveBeenCalledWith(
        expect.stringContaining("/api/v1/auth/logout"),
        { refreshToken: "refresh_token_123" },
        expect.any(Object)
      );
    });

    it("should not throw error on logout failure", async () => {
      mockedAxios.post.mockRejectedValueOnce(new Error("Network error"));

      // Should not throw
      await expect(
        authService.logout("refresh_token_123")
      ).resolves.toBeUndefined();
    });
  });

  describe("decodeToken", () => {
    it("should decode valid JWT", () => {
      // Sample JWT (not a real one, but valid format for testing)
      const payload = {
        sub: "123",
        email: "test@example.com",
        role: "Dispatcher",
        exp: Math.floor(Date.now() / 1000) + 3600,
      };
      const encodedPayload = btoa(JSON.stringify(payload))
        .replace(/\+/g, "-")
        .replace(/\//g, "_");
      const token = `header.${encodedPayload}.signature`;

      const result = authService.decodeToken(token);

      expect(result).toEqual(payload);
    });

    it("should return null for invalid JWT", () => {
      const result = authService.decodeToken("invalid.jwt");
      expect(result).toBeNull();
    });
  });

  describe("isTokenExpired", () => {
    it("should return true for expired token", () => {
      const payload = { exp: Math.floor(Date.now() / 1000) - 100 };
      const encodedPayload = btoa(JSON.stringify(payload))
        .replace(/\+/g, "-")
        .replace(/\//g, "_");
      const token = `header.${encodedPayload}.signature`;

      const result = authService.isTokenExpired(token);
      expect(result).toBe(true);
    });

    it("should return true if token expires within 5 minutes", () => {
      const payload = { exp: Math.floor(Date.now() / 1000) + 200 }; // 3 min 20 sec
      const encodedPayload = btoa(JSON.stringify(payload))
        .replace(/\+/g, "-")
        .replace(/\//g, "_");
      const token = `header.${encodedPayload}.signature`;

      const result = authService.isTokenExpired(token);
      expect(result).toBe(true);
    });

    it("should return false for valid token", () => {
      const payload = { exp: Math.floor(Date.now() / 1000) + 7200 }; // 2 hours
      const encodedPayload = btoa(JSON.stringify(payload))
        .replace(/\+/g, "-")
        .replace(/\//g, "_");
      const token = `header.${encodedPayload}.signature`;

      const result = authService.isTokenExpired(token);
      expect(result).toBe(false);
    });
  });

  describe("extractRole", () => {
    it("should extract role from token", () => {
      const payload = { role: "Contractor" };
      const encodedPayload = btoa(JSON.stringify(payload))
        .replace(/\+/g, "-")
        .replace(/\//g, "_");
      const token = `header.${encodedPayload}.signature`;

      const result = authService.extractRole(token);
      expect(result).toBe("Contractor");
    });

    it("should return null if role not in token", () => {
      const payload = { sub: "123" };
      const encodedPayload = btoa(JSON.stringify(payload))
        .replace(/\+/g, "-")
        .replace(/\//g, "_");
      const token = `header.${encodedPayload}.signature`;

      const result = authService.extractRole(token);
      expect(result).toBeFalsy();
    });
  });
});
