import axios, { AxiosError } from "axios";
import { config } from "@/utils/config";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface SignupRequest {
  email: string;
  password: string;
  role: "Dispatcher" | "Customer" | "Contractor";
}

export interface TokenResponse {
  accessToken: string;
  refreshToken?: string;
  expiresIn: number;
  tokenType: string;
}

export interface AuthResponse {
  user: {
    id: string;
    email: string;
    role: "Dispatcher" | "Customer" | "Contractor";
  };
  token: TokenResponse;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export class AuthService {
  private apiBaseUrl = config.apiBaseUrl;
  private authEndpoint = `${this.apiBaseUrl}/api/v1/auth`;

  /**
   * Login with email and password
   */
  async login(email: string, password: string): Promise<TokenResponse> {
    try {
      const response = await axios.post<TokenResponse>(
        `${this.authEndpoint}/login`,
        { email, password },
        { withCredentials: true }
      );
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  /**
   * Sign up with email, password, and role
   */
  async signup(
    email: string,
    password: string,
    role: "Dispatcher" | "Customer" | "Contractor"
  ): Promise<TokenResponse> {
    try {
      const response = await axios.post<TokenResponse>(
        `${this.authEndpoint}/signup`,
        { email, password, role },
        { withCredentials: true }
      );
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  /**
   * Refresh JWT token
   */
  async refreshToken(refreshToken: string): Promise<TokenResponse> {
    try {
      const response = await axios.post<TokenResponse>(
        `${this.authEndpoint}/refresh`,
        { refreshToken },
        { withCredentials: true }
      );
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  /**
   * Logout (revoke refresh token)
   */
  async logout(refreshToken: string): Promise<void> {
    try {
      await axios.post(
        `${this.authEndpoint}/logout`,
        { refreshToken },
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem(
              config.auth.jwtStorageKey
            )}`,
          },
          withCredentials: true,
        }
      );
    } catch (error) {
      // Log error but don't throw - logout should always clear local state
      console.error("Logout error:", error);
    }
  }

  /**
   * Decode JWT and extract payload (without verification)
   * IMPORTANT: Only for client-side role extraction. Never trust this for security.
   */
  decodeToken(
    token: string
  ): { exp: number; role: string; email: string } | null {
    try {
      const parts = token.split(".");
      if (parts.length !== 3) return null;

      const decoded = JSON.parse(
        atob(parts[1].replace(/-/g, "+").replace(/_/g, "/"))
      );
      return decoded;
    } catch (error) {
      console.error("Token decode error:", error);
      return null;
    }
  }

  /**
   * Check if JWT token is expired
   */
  isTokenExpired(token: string): boolean {
    const decoded = this.decodeToken(token);
    if (!decoded || !decoded.exp) return true;

    // Check if expiring within 5 minutes (buffer for refresh)
    const now = Math.floor(Date.now() / 1000);
    return decoded.exp < now + 300; // 5 minute buffer
  }

  /**
   * Extract role from JWT token
   */
  extractRole(token: string): "Dispatcher" | "Customer" | "Contractor" | null {
    const decoded = this.decodeToken(token);
    if (!decoded) return null;
    return decoded.role as "Dispatcher" | "Customer" | "Contractor";
  }

  /**
   * Handle and normalize errors
   */
  private handleError(error: unknown): Error {
    if (error instanceof AxiosError) {
      // Network error
      if (!error.response) {
        return new Error("Network error. Please check your connection.");
      }

      // Server error (4xx, 5xx)
      const status = error.response.status;
      const errorData = error.response.data as any;

      if (status === 400) {
        return new Error(
          errorData?.error?.message ||
            "Invalid request. Please check your input."
        );
      }

      if (status === 409) {
        return new Error(errorData?.error?.message || "Email already exists.");
      }

      if (status === 401) {
        return new Error(errorData?.error?.message || "Invalid credentials.");
      }

      if (status >= 500) {
        return new Error("Server error. Please try again later.");
      }

      return new Error(
        errorData?.error?.message || "An error occurred. Please try again."
      );
    }

    return new Error("An unexpected error occurred.");
  }
}

// Export singleton instance
export const authService = new AuthService();

