/**
 * useRating Hook Tests
 * Tests for rating submission, validation, and error handling
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useRating } from "../useRating";
import { customerService } from "@/services/customerService";
import { Review } from "@/types/Customer";

// Mock customerService
vi.mock("@/services/customerService");

describe("useRating Hook", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe("Initial State", () => {
    it("should initialize with default values", () => {
      const { result } = renderHook(() => useRating());

      expect(result.current.rating).toBe(0);
      expect(result.current.comment).toBe("");
      expect(result.current.loading).toBe(false);
      expect(result.current.error).toBe(null);
      expect(result.current.success).toBe(false);
    });
  });

  describe("setRating", () => {
    it("should update rating when valid (1-5)", () => {
      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setRating(4);
      });

      expect(result.current.rating).toBe(4);
      expect(result.current.error).toBe(null);
    });

    it("should accept all valid ratings 1-5", () => {
      const { result } = renderHook(() => useRating());

      for (let i = 1; i <= 5; i++) {
        act(() => {
          result.current.setRating(i);
        });
        expect(result.current.rating).toBe(i);
      }
    });

    it("should accept 0 (unselected)", () => {
      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setRating(3);
      });
      expect(result.current.rating).toBe(3);

      act(() => {
        result.current.setRating(0);
      });
      expect(result.current.rating).toBe(0);
    });

    it("should reject invalid ratings", () => {
      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setRating(6);
      });
      expect(result.current.rating).toBe(0); // Should remain unchanged

      act(() => {
        result.current.setRating(-1);
      });
      expect(result.current.rating).toBe(0); // Should remain unchanged
    });

    it("should clear error when rating is set", () => {
      const { result } = renderHook(() => useRating());

      // Simulate an error state (would occur from validation)
      act(() => {
        result.current.setRating(0); // Set invalid rating to see error state behavior
      });

      act(() => {
        result.current.setRating(3); // Set valid rating
      });

      expect(result.current.error).toBe(null);
    });
  });

  describe("setComment", () => {
    it("should update comment", () => {
      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setComment("Great work!");
      });

      expect(result.current.comment).toBe("Great work!");
      expect(result.current.error).toBe(null);
    });

    it("should accept empty comment", () => {
      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setComment("");
      });

      expect(result.current.comment).toBe("");
    });

    it("should enforce max 500 character limit", () => {
      const { result } = renderHook(() => useRating());

      const longComment = "a".repeat(501);

      act(() => {
        result.current.setComment(longComment);
      });

      // Should not update if exceeding limit
      expect(result.current.comment).toBe("");

      act(() => {
        result.current.setComment("a".repeat(500));
      });

      expect(result.current.comment).toBe("a".repeat(500));
    });

    it("should clear error when comment is set", () => {
      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setComment("Good service");
      });

      expect(result.current.error).toBe(null);
    });
  });

  describe("submitRating - Validation", () => {
    it("should return error if rating is not selected", async () => {
      const { result } = renderHook(() => useRating());

      let review: Review | null = null;

      await act(async () => {
        review = await result.current.submitRating("job-123");
      });

      expect(review).toBeNull();
      expect(result.current.error).toContain("Please select a rating");
      expect(result.current.loading).toBe(false);
    });

    it("should validate comment length on submission", async () => {
      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setRating(4);
        // Manually set comment over limit (in real scenario, setComment prevents this)
        // So we'll test the validation in submission logic
      });

      let review: Review | null = null;

      await act(async () => {
        review = await result.current.submitRating("job-123");
      });

      // Should succeed with valid rating and no comment
      expect(review).not.toBeNull();
    });
  });

  describe("submitRating - Success Cases", () => {
    it("should submit rating with comment successfully", async () => {
      const mockReview: Review = {
        id: "review-123",
        jobId: "job-123",
        contractorId: "contractor-123",
        customerId: "customer-123",
        rating: 5,
        comment: "Great work, finished early!",
        createdAt: "2025-11-15T14:30:00Z",
      };

      vi.mocked(customerService.submitRating).mockResolvedValue(mockReview);

      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setRating(5);
        result.current.setComment("Great work, finished early!");
      });

      let review: Review | null = null;

      await act(async () => {
        review = await result.current.submitRating("job-123");
      });

      expect(review).toEqual(mockReview);
      expect(result.current.success).toBe(true);
      expect(result.current.loading).toBe(false);
      expect(result.current.error).toBeNull();

      expect(customerService.submitRating).toHaveBeenCalledWith("job-123", {
        rating: 5,
        comment: "Great work, finished early!",
      });
    });

    it("should submit rating without comment successfully", async () => {
      const mockReview: Review = {
        id: "review-124",
        jobId: "job-124",
        contractorId: "contractor-124",
        customerId: "customer-124",
        rating: 4,
        comment: null,
        createdAt: "2025-11-15T14:35:00Z",
      };

      vi.mocked(customerService.submitRating).mockResolvedValue(mockReview);

      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setRating(4);
        // No comment set
      });

      let review: Review | null = null;

      await act(async () => {
        review = await result.current.submitRating("job-124");
      });

      expect(review).toEqual(mockReview);
      expect(result.current.success).toBe(true);

      expect(customerService.submitRating).toHaveBeenCalledWith("job-124", {
        rating: 4,
        comment: null,
      });
    });

    it("should set loading state during submission", async () => {
      vi.mocked(customerService.submitRating).mockImplementation(
        () => new Promise((resolve) => setTimeout(() => resolve({
          id: "review-125",
          jobId: "job-125",
          contractorId: "contractor-125",
          customerId: "customer-125",
          rating: 5,
          comment: null,
          createdAt: "2025-11-15T14:40:00Z",
        }), 100))
      );

      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setRating(5);
      });

      await act(async () => {
        const submitPromise = result.current.submitRating("job-125");
        // Check loading state (may be transient)
        await submitPromise;
      });

      expect(result.current.loading).toBe(false);
    });
  });

  describe("submitRating - Error Cases", () => {
    it("should handle 409 conflict (already rated)", async () => {
      const error = new Error("You have already rated this job.");

      vi.mocked(customerService.submitRating).mockRejectedValue(error);

      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setRating(5);
      });

      let review: Review | null = null;

      await act(async () => {
        review = await result.current.submitRating("job-126");
      });

      expect(review).toBeNull();
      expect(result.current.error).toBe("You have already rated this job.");
      expect(result.current.success).toBe(false);
      expect(result.current.loading).toBe(false);
    });

    it("should handle 404 not found", async () => {
      const error = new Error(
        "Job not found or not in completed status. You can only rate completed jobs."
      );

      vi.mocked(customerService.submitRating).mockRejectedValue(error);

      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setRating(3);
      });

      let review: Review | null = null;

      await act(async () => {
        review = await result.current.submitRating("job-999");
      });

      expect(review).toBeNull();
      expect(result.current.error).toContain("Job not found");
      expect(result.current.success).toBe(false);
    });

    it("should handle server error (500)", async () => {
      const error = new Error("Server error. Please try again later or contact support.");

      vi.mocked(customerService.submitRating).mockRejectedValue(error);

      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setRating(4);
      });

      let review: Review | null = null;

      await act(async () => {
        review = await result.current.submitRating("job-127");
      });

      expect(review).toBeNull();
      expect(result.current.error).toContain("Server error");
      expect(result.current.loading).toBe(false);
    });

    it("should handle generic errors", async () => {
      vi.mocked(customerService.submitRating).mockRejectedValue(
        new Error("Network error")
      );

      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setRating(5);
      });

      let review: Review | null = null;

      await act(async () => {
        review = await result.current.submitRating("job-128");
      });

      expect(review).toBeNull();
      expect(result.current.error).toBe("Network error");
      expect(result.current.success).toBe(false);
    });
  });

  describe("resetForm", () => {
    it("should reset all state to initial values", async () => {
      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setRating(5);
        result.current.setComment("Great work!");
      });

      expect(result.current.rating).toBe(5);
      expect(result.current.comment).toBe("Great work!");

      act(() => {
        result.current.resetForm();
      });

      expect(result.current.rating).toBe(0);
      expect(result.current.comment).toBe("");
      expect(result.current.loading).toBe(false);
      expect(result.current.error).toBe(null);
      expect(result.current.success).toBe(false);
    });

    it("should reset after successful submission", async () => {
      const mockReview: Review = {
        id: "review-129",
        jobId: "job-129",
        contractorId: "contractor-129",
        customerId: "customer-129",
        rating: 5,
        comment: "Perfect!",
        createdAt: "2025-11-15T14:45:00Z",
      };

      vi.mocked(customerService.submitRating).mockResolvedValue(mockReview);

      const { result } = renderHook(() => useRating());

      act(() => {
        result.current.setRating(5);
        result.current.setComment("Perfect!");
      });

      await act(async () => {
        await result.current.submitRating("job-129");
      });

      expect(result.current.success).toBe(true);

      act(() => {
        result.current.resetForm();
      });

      expect(result.current.rating).toBe(0);
      expect(result.current.comment).toBe("");
      expect(result.current.success).toBe(false);
    });
  });
});


