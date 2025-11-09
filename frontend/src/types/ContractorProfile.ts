/**
 * Contractor Profile Type Definitions
 * Interfaces for contractor rating, earnings, and job history
 */

/**
 * Customer review of a contractor
 */
export interface CustomerReview {
  id: number;
  rating: number;
  comment: string | null;
  customerName: string;
  jobType: string;
  createdAt: string; // ISO 8601
}

/**
 * Contractor profile with aggregated statistics
 */
export interface ContractorProfileData {
  id: number;
  name: string;
  averageRating: number | null;
  reviewCount: number;
  totalJobsAssigned: number;
  totalJobsAccepted: number;
  totalJobsCompleted: number;
  acceptanceRate: number; // 0-100 percentage
  totalEarnings?: number | null;
  createdAt: string; // ISO 8601
  recentReviews: CustomerReview[];
}

/**
 * Job history item (past job assignment)
 */
export interface JobHistoryItem {
  id: number;
  jobId: number;
  jobType: string;
  location: string;
  scheduledDateTime: string; // ISO 8601
  status: "Pending" | "Accepted" | "InProgress" | "Completed" | "Cancelled";
  customerName: string;
  customerRating: number | null;
  customerReviewText: string | null;
  acceptedAt: string | null; // ISO 8601
  completedAt: string | null; // ISO 8601
}

/**
 * Job history response with pagination metadata
 */
export interface JobHistoryResponse {
  assignments: JobHistoryItem[];
  totalCount: number;
}

/**
 * Filter options for job history
 */
export interface JobHistoryFilterOptions {
  startDate?: string; // ISO 8601
  endDate?: string; // ISO 8601
}

/**
 * Pagination parameters for job history
 */
export interface JobHistoryPaginationParams {
  skip: number;
  take: number;
}
