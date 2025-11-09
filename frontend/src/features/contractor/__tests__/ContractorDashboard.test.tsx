/**
 * ContractorDashboard Component Tests
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { AuthContext } from "@/contexts/AuthContext";
import { NotificationProvider } from "@/contexts/NotificationContext";
import { ContractorDashboard } from "../ContractorDashboard";

// Mock useSignalRNotifications
vi.mock("@/hooks/useSignalRNotifications", () => ({
  useSignalRNotifications: () => ({
    isConnected: true,
    error: null,
    manualReconnect: vi.fn(),
  }),
}));

// Mock useContractorJobs
vi.mock("@/hooks/useContractorJobs", () => ({
  useContractorJobs: () => ({
    jobs: [],
    loading: false,
    error: null,
    refetch: vi.fn(),
  }),
}));

const mockAuthContext = {
  user: {
    id: "1",
    email: "contractor@test.com",
    role: "Contractor" as const,
    name: "John Doe",
  },
  token: "test-token",
  isAuthenticated: true,
  isLoading: false,
  error: null,
  login: vi.fn(),
  logout: vi.fn(),
  hasRole: vi.fn(),
};

describe("ContractorDashboard", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render dashboard with connection status", async () => {
    render(
      <BrowserRouter>
        <AuthContext.Provider value={mockAuthContext}>
          <NotificationProvider>
            <ContractorDashboard />
          </NotificationProvider>
        </AuthContext.Provider>
      </BrowserRouter>
    );

    await waitFor(() => {
      // There are multiple "Contractor Dashboard" texts (header and content), so use getAllByText
      const dashboardTitles = screen.getAllByText("Contractor Dashboard");
      expect(dashboardTitles.length).toBeGreaterThan(0);
      expect(screen.getByText("Connected")).toBeInTheDocument();
    });
  });

  it("should display welcome message with contractor name", async () => {
    render(
      <BrowserRouter>
        <AuthContext.Provider value={mockAuthContext}>
          <NotificationProvider>
            <ContractorDashboard />
          </NotificationProvider>
        </AuthContext.Provider>
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Welcome back/)).toBeInTheDocument();
      expect(screen.getByText("John Doe")).toBeInTheDocument();
    });
  });

  it("should display job tabs", async () => {
    render(
      <BrowserRouter>
        <AuthContext.Provider value={mockAuthContext}>
          <NotificationProvider>
            <ContractorDashboard />
          </NotificationProvider>
        </AuthContext.Provider>
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText("Pending")).toBeInTheDocument();
      expect(screen.getByText("Active")).toBeInTheDocument();
      expect(screen.getByText("Completed")).toBeInTheDocument();
    });
  });
});
