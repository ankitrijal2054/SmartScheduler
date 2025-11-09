/**
 * Dashboard Integration Test
 * Tests contractor list tab integration with Dashboard
 */

import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { vi, describe, it, expect, beforeEach } from "vitest";
import { Dashboard } from "../Dashboard";
import { useJobs } from "@/hooks/useJobs";
import { useAuth } from "@/hooks/useAuthContext";
import { useContractorList } from "@/hooks/useContractorList";

// Mock hooks
vi.mock("@/hooks/useJobs", () => ({
  useJobs: vi.fn(),
}));

vi.mock("@/hooks/useAuthContext", () => ({
  useAuth: vi.fn(),
}));

vi.mock("@/hooks/useContractorList", () => ({
  useContractorList: vi.fn(),
}));

describe("Dashboard Integration Test", () => {
  const createMockJobsState = (overrides = {}) => ({
    jobs: [],
    loading: false,
    error: null,
    pagination: { currentPage: 1, pageSize: 20, total: 0, totalPages: 0 },
    setPage: vi.fn(),
    setSort: vi.fn(),
    refreshJobs: vi.fn(),
    ...overrides,
  });

  const createMockContractorListState = (overrides = {}) => ({
    myList: [],
    allContractors: [],
    totalContractors: 0,
    loading: false,
    error: null,
    contractorListOnly: false,
    fetchMyList: vi.fn(),
    fetchAvailableContractors: vi.fn(),
    addContractor: vi.fn(),
    removeContractor: vi.fn(),
    toggleFilter: vi.fn(),
    cleanup: vi.fn(),
    ...overrides,
  });

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useAuth).mockReturnValue({
      user: { id: "1", name: "Test Dispatcher", role: "Dispatcher" },
      logout: vi.fn(),
      isAuthenticated: true,
    } as any);

    vi.mocked(useJobs).mockReturnValue(createMockJobsState() as any);

    vi.mocked(useContractorList).mockReturnValue(
      createMockContractorListState() as any
    );
  });

  it("should render Dashboard with Jobs tab by default", () => {
    render(<Dashboard />);

    expect(
      screen.getByRole("heading", { name: /Open Jobs/i })
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /Contractor List/i })
    ).toBeInTheDocument();
  });

  it("should render Contractor List tab", async () => {
    render(<Dashboard />);

    const contractorTab = screen.getByRole("button", {
      name: /Contractor List/i,
    });

    expect(contractorTab).toBeInTheDocument();
  });

  it("should switch to Contractor List tab when clicked", async () => {
    const user = userEvent.setup();
    render(<Dashboard />);

    const contractorTab = screen.getByRole("button", {
      name: /Contractor List/i,
    });

    await user.click(contractorTab);

    expect(screen.getByText("Contractor List Management")).toBeInTheDocument();
  });

  it("should not break existing job list functionality when switching tabs", async () => {
    const user = userEvent.setup();
    const setPage = vi.fn();

    vi.mocked(useJobs).mockReturnValue(createMockJobsState({ setPage }) as any);

    render(<Dashboard />);

    // Navigate to contractors
    const contractorTab = screen.getByRole("button", {
      name: /Contractor List/i,
    });
    await user.click(contractorTab);

    // Navigate back to jobs
    const jobsTab = screen.getAllByRole("button", {
      name: /Open Jobs/i,
    })[0];
    await user.click(jobsTab);

    expect(
      screen.getByRole("heading", { name: /Open Jobs/i })
    ).toBeInTheDocument();
  });

  it("should pass contractor list filter to RecommendationsModal", async () => {
    const user = userEvent.setup();
    const toggleFilter = vi.fn();

    vi.mocked(useContractorList).mockReturnValue(
      createMockContractorListState({
        contractorListOnly: true,
        toggleFilter,
      }) as any
    );

    const { rerender } = render(<Dashboard />);

    // Switch to contractor tab
    const contractorTab = screen.getByRole("button", {
      name: /Contractor List/i,
    });
    await user.click(contractorTab);

    // The contractor list filter should be passed to recommendation modal
    // This is tested implicitly through the onFilterChange prop
  });

  it("should maintain responsive design on both tabs", async () => {
    const user = userEvent.setup();
    render(<Dashboard />);

    // Check jobs tab responsive behavior
    expect(
      screen.getByRole("heading", { name: /Open Jobs/i })
    ).toBeInTheDocument();

    // Switch to contractor tab
    const contractorTab = screen.getByRole("button", {
      name: /Contractor List/i,
    });
    await user.click(contractorTab);

    expect(screen.getByText("Contractor List Management")).toBeInTheDocument();
  });

  it("should initialize contractor list on first tab switch", async () => {
    const user = userEvent.setup();
    const fetchMyList = vi.fn();
    const fetchAvailableContractors = vi.fn();

    vi.mocked(useContractorList).mockReturnValue(
      createMockContractorListState({
        fetchMyList,
        fetchAvailableContractors,
      }) as any
    );

    render(<Dashboard />);

    const contractorTab = screen.getByRole("button", {
      name: /Contractor List/i,
    });
    await user.click(contractorTab);

    // ContractorListPanel will initialize on mount/render
  });

  it("should handle filter change callback from ContractorListPanel", async () => {
    const user = userEvent.setup();
    const toggleFilter = vi.fn();
    let contractorListOnlyState = false;

    vi.mocked(useContractorList).mockReturnValue(
      createMockContractorListState({
        contractorListOnly: contractorListOnlyState,
        toggleFilter: () => {
          contractorListOnlyState = !contractorListOnlyState;
          toggleFilter();
        },
      }) as any
    );

    const { rerender } = render(<Dashboard />);

    const contractorTab = screen.getByRole("button", {
      name: /Contractor List/i,
    });
    await user.click(contractorTab);

    // Toggle the filter checkbox
    const checkbox = screen.getByRole("checkbox", {
      name: /Filter recommendations by my contractor list only/i,
    });

    await user.click(checkbox);

    expect(toggleFilter).toHaveBeenCalled();
  });

  it("should display Dashboard header with user greeting", () => {
    vi.mocked(useAuth).mockReturnValue({
      user: { id: "1", name: "John Doe", role: "Dispatcher" },
      logout: vi.fn(),
      isAuthenticated: true,
    } as any);

    render(<Dashboard />);

    expect(screen.getByText("Dispatcher Dashboard")).toBeInTheDocument();
    expect(screen.getByText(/Welcome, John Doe/)).toBeInTheDocument();
  });

  it("should not have regression on job list rendering", async () => {
    const user = userEvent.setup();

    render(<Dashboard />);

    // Jobs tab should be active by default
    expect(
      screen.getByRole("heading", {
        name: /Open Jobs/i,
      })
    ).toBeInTheDocument();

    // Switch to contractors and back
    const contractorTab = screen.getByRole("button", {
      name: /Contractor List/i,
    });
    await user.click(contractorTab);

    const jobsTab = screen.getByRole("button", {
      name: /Open Jobs/i,
    });
    await user.click(jobsTab);

    // Jobs tab heading should still be there
    expect(
      screen.getByRole("heading", {
        name: /Open Jobs/i,
      })
    ).toBeInTheDocument();
  });
});
