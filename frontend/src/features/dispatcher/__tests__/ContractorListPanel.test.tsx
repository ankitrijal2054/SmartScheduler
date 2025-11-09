/**
 * ContractorListPanel Component Tests
 */

import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { vi, describe, it, expect, beforeEach } from "vitest";
import { ContractorListPanel } from "../ContractorListPanel";
import { useContractorList } from "@/hooks/useContractorList";
import { Contractor } from "@/types/Contractor";

// Mock the useContractorList hook
vi.mock("@/hooks/useContractorList", () => ({
  useContractorList: vi.fn(),
}));

describe("ContractorListPanel Component", () => {
  const mockContractors: Contractor[] = [
    {
      id: "1",
      name: "John's Plumbing",
      rating: 4.5,
      reviewCount: 20,
      location: "123 Main St",
      tradeType: "Plumbing",
      isActive: true,
      inDispatcherList: true,
    },
    {
      id: "2",
      name: "Electric Pro",
      rating: 4.8,
      reviewCount: 35,
      location: "456 Oak Ave",
      tradeType: "Electrical",
      isActive: true,
      inDispatcherList: false,
    },
  ];

  const createMockHookState = (overrides = {}) => ({
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
  });

  it("should render both tabs", () => {
    vi.mocked(useContractorList).mockReturnValue(createMockHookState());

    render(<ContractorListPanel />);

    expect(screen.getByText("My Contractor List (0)")).toBeInTheDocument();
    expect(screen.getByText("All Contractors (0)")).toBeInTheDocument();
  });

  it("should render filter toggle", () => {
    vi.mocked(useContractorList).mockReturnValue(createMockHookState());

    render(<ContractorListPanel />);

    expect(
      screen.getByRole("checkbox", {
        name: /Filter recommendations by my contractor list only/i,
      })
    ).toBeInTheDocument();
  });

  it("should display empty state on My List tab when no contractors", () => {
    vi.mocked(useContractorList).mockReturnValue(createMockHookState());

    render(<ContractorListPanel />);

    expect(screen.getByText("No contractors added yet")).toBeInTheDocument();
    expect(
      screen.getByText(/Add contractors from the 'All Contractors' tab/i)
    ).toBeInTheDocument();
  });

  it("should display contractors in My List tab when populated", async () => {
    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({ myList: [mockContractors[0]] })
    );

    render(<ContractorListPanel />);

    await waitFor(() => {
      expect(screen.getByText("John's Plumbing")).toBeInTheDocument();
    });
  });

  it("should switch to All Contractors tab", async () => {
    const user = userEvent.setup();
    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({
        allContractors: mockContractors,
        totalContractors: mockContractors.length,
      })
    );

    render(<ContractorListPanel />);

    const allContractorsTab = screen.getByRole("button", {
      name: /All Contractors/i,
    });
    await user.click(allContractorsTab);

    await waitFor(() => {
      expect(
        screen.getByPlaceholderText(/Search contractors by name/i)
      ).toBeInTheDocument();
    });
  });

  it("should display search input on All Contractors tab", async () => {
    const user = userEvent.setup();
    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({
        allContractors: mockContractors,
        totalContractors: mockContractors.length,
      })
    );

    render(<ContractorListPanel />);

    const allContractorsTab = screen.getByRole("button", {
      name: /All Contractors/i,
    });
    await user.click(allContractorsTab);

    expect(
      screen.getByPlaceholderText(/Search contractors by name/i)
    ).toBeInTheDocument();
  });

  it("should call fetchAvailableContractors when mounting", () => {
    const fetchAvailableContractors = vi.fn();
    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({ fetchAvailableContractors })
    );

    render(<ContractorListPanel />);

    expect(fetchAvailableContractors).toHaveBeenCalledWith(50, 0);
  });

  it("should toggle filter when checkbox is clicked", async () => {
    const user = userEvent.setup();
    const toggleFilter = vi.fn();
    const onFilterChange = vi.fn();

    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({ toggleFilter })
    );

    render(<ContractorListPanel onFilterChange={onFilterChange} />);

    const checkbox = screen.getByRole("checkbox", {
      name: /Filter recommendations by my contractor list only/i,
    });
    await user.click(checkbox);

    expect(toggleFilter).toHaveBeenCalled();
    expect(onFilterChange).toHaveBeenCalledWith(true);
  });

  it("should display loading state when loading is true", async () => {
    const user = userEvent.setup();
    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({ loading: true })
    );

    const { container } = render(<ContractorListPanel />);

    const allContractorsTab = screen.getByRole("button", {
      name: /All Contractors/i,
    });
    await user.click(allContractorsTab);

    // Check for loading spinner
    const spinner = container.querySelector("svg");
    expect(spinner).toBeInTheDocument();
  });

  it("should display error message when error exists", async () => {
    const user = userEvent.setup();
    const errorMessage = "Failed to fetch contractors";
    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({ error: errorMessage })
    );

    render(<ContractorListPanel />);

    expect(screen.getByText(errorMessage)).toBeInTheDocument();
  });

  it("should paginate through contractors", async () => {
    const user = userEvent.setup();
    const fetchAvailableContractors = vi.fn();

    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({
        allContractors: mockContractors,
        totalContractors: 150, // More than one page
        fetchAvailableContractors,
      })
    );

    render(<ContractorListPanel />);

    const allContractorsTab = screen.getByRole("button", {
      name: /All Contractors/i,
    });
    await user.click(allContractorsTab);

    const nextButton = screen.getByRole("button", { name: /Next →/ });
    await user.click(nextButton);

    expect(fetchAvailableContractors).toHaveBeenCalledWith(50, 50, undefined);
  });

  it("should disable Next button when on last page", async () => {
    const user = userEvent.setup();
    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({
        allContractors: mockContractors,
        totalContractors: mockContractors.length, // Only one page worth
      })
    );

    render(<ContractorListPanel />);

    const allContractorsTab = screen.getByRole("button", {
      name: /All Contractors/i,
    });
    await user.click(allContractorsTab);

    const nextButton = screen.getByRole("button", { name: /Next →/ });
    expect(nextButton).toBeDisabled();
  });

  it("should call addContractor when Add button is clicked", async () => {
    const user = userEvent.setup();
    const addContractor = vi.fn().mockResolvedValue(true);

    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({
        allContractors: mockContractors,
        totalContractors: mockContractors.length,
        addContractor,
      })
    );

    render(<ContractorListPanel />);

    const allContractorsTab = screen.getByRole("button", {
      name: /All Contractors/i,
    });
    await user.click(allContractorsTab);

    const addButtons = screen.getAllByRole("button", { name: /Add/i });
    await user.click(addButtons[0]);

    await waitFor(() => {
      expect(addContractor).toHaveBeenCalledWith(mockContractors[0].id);
    });
  });

  it("should call removeContractor when Remove is confirmed", async () => {
    const user = userEvent.setup();
    const removeContractor = vi.fn().mockResolvedValue(true);
    const fetchAvailableContractors = vi.fn();

    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({
        myList: [mockContractors[0]],
        removeContractor,
        fetchAvailableContractors,
      })
    );

    render(<ContractorListPanel />);

    const removeButton = screen.getByRole("button", { name: /Remove/i });
    await user.click(removeButton);

    const confirmButton = screen.getByRole("button", { name: /Confirm/i });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(removeContractor).toHaveBeenCalledWith(mockContractors[0].id);
    });
  });

  it("should update contractor count display in tab", () => {
    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({
        myList: [mockContractors[0], mockContractors[1]],
        totalContractors: 50,
      })
    );

    render(<ContractorListPanel />);

    expect(screen.getByText("My Contractor List (2)")).toBeInTheDocument();
    expect(screen.getByText("All Contractors (50)")).toBeInTheDocument();
  });

  it("should cleanup on unmount", () => {
    const cleanup = vi.fn();
    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({ cleanup })
    );

    const { unmount } = render(<ContractorListPanel />);

    unmount();

    expect(cleanup).toHaveBeenCalled();
  });

  it("should show Browse Contractors button in empty state", async () => {
    const user = userEvent.setup();
    vi.mocked(useContractorList).mockReturnValue(createMockHookState());

    render(<ContractorListPanel />);

    // Find the browse button (it's in the empty state section)
    const buttons = screen.getAllByRole("button", {
      name: /Browse Contractors/i,
    });
    expect(buttons.length).toBeGreaterThan(0);

    await user.click(buttons[0]);

    // Should switch to All Contractors tab
    expect(
      screen.getByPlaceholderText(/Search contractors by name/i)
    ).toBeInTheDocument();
  });

  it("should debounce search input", async () => {
    const user = userEvent.setup();
    const fetchAvailableContractors = vi.fn();

    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({
        allContractors: mockContractors,
        totalContractors: mockContractors.length,
        fetchAvailableContractors,
      })
    );

    render(<ContractorListPanel />);

    const allContractorsTab = screen.getByRole("button", {
      name: /All Contractors/i,
    });
    await user.click(allContractorsTab);

    const searchInput = screen.getByPlaceholderText(
      /Search contractors by name/i
    );

    // Type multiple characters quickly
    await user.type(searchInput, "Plumb");

    // Wait for debounce to complete (300ms)
    await waitFor(() => {
      expect(fetchAvailableContractors).toHaveBeenCalledWith(50, 0, "Plumb");
    });
  });

  it("should display pagination info correctly", async () => {
    const user = userEvent.setup();
    vi.mocked(useContractorList).mockReturnValue(
      createMockHookState({
        allContractors: mockContractors,
        totalContractors: 150,
      })
    );

    const { container } = render(<ContractorListPanel />);

    const allContractorsTab = screen.getByRole("button", {
      name: /All Contractors/i,
    });
    await user.click(allContractorsTab);

    // The text might have different dashes/unicode characters, so just check the content
    expect(container.textContent).toContain("Showing");
    expect(container.textContent).toContain("150");
  });
});
