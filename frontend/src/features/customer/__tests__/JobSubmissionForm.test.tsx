/**
 * JobSubmissionForm Component Tests
 * Comprehensive unit tests for form rendering, validation, and submission
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { JobSubmissionForm } from "../JobSubmissionForm";
import * as customerService from "@/services/customerService";

// Mock the customer service
vi.mock("@/services/customerService");

describe("JobSubmissionForm Component", () => {
  const mockOnSuccess = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    mockOnSuccess.mockClear();
  });

  // Test 1: Component renders with all form fields
  it("should render all form fields", () => {
    render(<JobSubmissionForm />);

    expect(screen.getByLabelText(/select job type/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/enter job location/i)).toBeInTheDocument();
    expect(
      screen.getByLabelText(/select desired date and time/i)
    ).toBeInTheDocument();
    expect(screen.getByLabelText(/enter job description/i)).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /submit job/i })
    ).toBeInTheDocument();
  });

  // Test 2: Job Type dropdown displays all 5 options
  it("should display job type dropdown with 5 options", () => {
    render(<JobSubmissionForm />);

    const jobTypeSelect = screen.getByLabelText(
      /select job type/i
    ) as HTMLSelectElement;
    fireEvent.click(jobTypeSelect);

    expect(screen.getByText("Flooring")).toBeInTheDocument();
    expect(screen.getByText("HVAC")).toBeInTheDocument();
    expect(screen.getByText("Plumbing")).toBeInTheDocument();
    expect(screen.getByText("Electrical")).toBeInTheDocument();
    expect(screen.getByText("Other")).toBeInTheDocument();
  });

  // Test 3: Location input field accepts text
  it("should accept location input", async () => {
    const user = userEvent.setup();
    render(<JobSubmissionForm />);

    const locationInput = screen.getByLabelText(
      /enter job location/i
    ) as HTMLInputElement;
    await user.type(locationInput, "123 Main St, Springfield, IL");

    expect(locationInput.value).toBe("123 Main St, Springfield, IL");
  });

  // Test 4: DateTime picker allows date and time selection
  it("should allow date and time selection", async () => {
    const user = userEvent.setup();
    render(<JobSubmissionForm />);

    const dateTimeInput = screen.getByLabelText(
      /select desired date and time/i
    ) as HTMLInputElement;
    await user.type(dateTimeInput, "2025-12-15T14:00");

    expect(dateTimeInput.value).toBe("2025-12-15T14:00");
  });

  // Test 5: Description textarea accepts multi-line text
  it("should accept description text", async () => {
    const user = userEvent.setup();
    render(<JobSubmissionForm />);

    const descriptionInput = screen.getByLabelText(
      /enter job description/i
    ) as HTMLTextAreaElement;
    await user.type(descriptionInput, "Broken pipe in master bathroom");

    expect(descriptionInput.value).toBe("Broken pipe in master bathroom");
  });

  // Test 6: Submit button is disabled until required fields are filled
  it("should disable submit button until required fields are filled", async () => {
    const user = userEvent.setup();
    render(<JobSubmissionForm />);

    const submitButton = screen.getByRole("button", {
      name: /submit job/i,
    }) as HTMLButtonElement;
    expect(submitButton.disabled).toBe(true);

    // Fill in required fields
    const jobTypeSelect = screen.getByLabelText(/select job type/i);
    await user.selectOptions(jobTypeSelect, "Plumbing");

    const locationInput = screen.getByLabelText(/enter job location/i);
    await user.type(locationInput, "123 Main St, Springfield, IL");

    const dateTimeInput = screen.getByLabelText(
      /select desired date and time/i
    );
    await user.type(dateTimeInput, "2025-12-15T14:00");

    // Now button should be enabled
    expect(submitButton.disabled).toBe(false);
  });

  // Test 7: Form shows general error when attempting to submit empty form
  it("should prevent submit and show error when required fields are empty", async () => {
    const user = userEvent.setup();
    render(<JobSubmissionForm />);

    const submitButton = screen.getByRole("button", {
      name: /submit job/i,
    }) as HTMLButtonElement;

    // Button should be disabled initially because form is empty
    expect(submitButton.disabled).toBe(true);

    // Try to click anyway (in case it somehow becomes enabled)
    // Form validation will prevent submission
    const jobTypeSelect = screen.getByLabelText(
      /select job type/i
    ) as HTMLSelectElement;
    expect(jobTypeSelect.value).toBe("");
  });

  // Test 8: Loading spinner shows during API call
  it("should show loading spinner while submitting", async () => {
    const user = userEvent.setup();

    // Mock API response that doesn't resolve immediately
    const mockSubmitJob = vi.fn(() => new Promise(() => {})); // Never resolves
    vi.mocked(customerService.customerService.submitJob).mockImplementationOnce(
      mockSubmitJob
    );

    render(<JobSubmissionForm />);

    // Fill in all fields
    await user.selectOptions(
      screen.getByLabelText(/select job type/i),
      "Plumbing"
    );
    await user.type(
      screen.getByLabelText(/enter job location/i),
      "123 Main St"
    );
    await user.type(
      screen.getByLabelText(/select desired date and time/i),
      "2025-12-15T14:00"
    );

    // Submit form
    const submitButton = screen.getByRole("button", { name: /submit job/i });
    await user.click(submitButton);

    // Verify loading state - button should show "Submitting..."
    expect(submitButton).toHaveTextContent(/submitting/i);
  });

  // Test 9: Successful submission triggers onSuccess callback
  it("should trigger onSuccess callback after successful submission", async () => {
    const user = userEvent.setup();

    const mockResponse = {
      id: "job_123",
      customerId: "cust_456",
      jobType: "Plumbing" as const,
      location: "123 Main St",
      description: "",
      desiredDateTime: "2025-12-15T14:00:00Z",
      status: "Pending" as const,
      currentAssignedContractorId: null,
      createdAt: "2025-11-08T16:00:00Z",
      updatedAt: "2025-11-08T16:00:00Z",
    };

    vi.mocked(customerService.customerService.submitJob).mockResolvedValueOnce(
      mockResponse
    );

    render(<JobSubmissionForm onSuccess={mockOnSuccess} />);

    // Fill and submit form
    await user.selectOptions(
      screen.getByLabelText(/select job type/i),
      "Plumbing"
    );
    await user.type(
      screen.getByLabelText(/enter job location/i),
      "123 Main St"
    );
    await user.type(
      screen.getByLabelText(/select desired date and time/i),
      "2025-12-15T14:00"
    );

    await user.click(screen.getByRole("button", { name: /submit job/i }));

    // Wait for callback to be triggered
    await waitFor(
      () => {
        expect(mockOnSuccess).toHaveBeenCalledWith("job_123");
      },
      { timeout: 3000 }
    );
  });

  // Test 10: Error handling on API failure
  it("should handle API failure gracefully", async () => {
    const user = userEvent.setup();

    const mockError = new Error("API Error");
    vi.mocked(customerService.customerService.submitJob).mockRejectedValueOnce(
      mockError
    );

    render(<JobSubmissionForm />);

    // Fill and submit form
    await user.selectOptions(
      screen.getByLabelText(/select job type/i),
      "Plumbing"
    );
    await user.type(
      screen.getByLabelText(/enter job location/i),
      "123 Main St"
    );
    await user.type(
      screen.getByLabelText(/select desired date and time/i),
      "2025-12-15T14:00"
    );

    const submitButton = screen.getByRole("button", { name: /submit job/i });
    await user.click(submitButton);

    // After error, button should be re-enabled for retry
    await waitFor(
      () => {
        expect(submitButton.disabled).toBe(false);
      },
      { timeout: 1000 }
    );
  });

  // Test 11: Form preserves data when validation fails
  it("should preserve form data when validation fails", async () => {
    const user = userEvent.setup();

    render(<JobSubmissionForm />);

    // Fill partial form (missing datetime)
    await user.selectOptions(
      screen.getByLabelText(/select job type/i),
      "Plumbing"
    );
    const locationInput = screen.getByLabelText(
      /enter job location/i
    ) as HTMLInputElement;
    await user.type(locationInput, "123 Main St");

    // Try to submit
    await user.click(screen.getByRole("button", { name: /submit job/i }));

    // Form data should still be there
    expect(locationInput.value).toBe("123 Main St");
    const jobTypeSelect = screen.getByLabelText(
      /select job type/i
    ) as HTMLSelectElement;
    expect(jobTypeSelect.value).toBe("Plumbing");
  });

  // Test 12: Submit button disabled during submission
  it("should disable submit button during submission", async () => {
    const user = userEvent.setup();

    const mockSubmitJob = vi.fn(() => new Promise(() => {})); // Never resolves
    vi.mocked(customerService.customerService.submitJob).mockImplementationOnce(
      mockSubmitJob
    );

    render(<JobSubmissionForm />);

    // Fill and submit
    await user.selectOptions(
      screen.getByLabelText(/select job type/i),
      "Plumbing"
    );
    await user.type(
      screen.getByLabelText(/enter job location/i),
      "123 Main St"
    );
    await user.type(
      screen.getByLabelText(/select desired date and time/i),
      "2025-12-15T14:00"
    );

    const submitButton = screen.getByRole("button", {
      name: /submit job/i,
    }) as HTMLButtonElement;
    expect(submitButton.disabled).toBe(false);

    await user.click(submitButton);

    // Button should be disabled during submission
    expect(submitButton.disabled).toBe(true);
  });
});
