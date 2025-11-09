/**
 * JobDetails Type Definitions
 * Complete job information for contractor modal display (Story 5.2)
 * Includes job, customer, and payment details with history
 */

import { Assignment } from "./Assignment";
import { Job } from "./Job";
import { Contractor } from "./Contractor";

/**
 * Customer info as shown in job details modal
 */
export interface JobDetailCustomer {
  id: string;
  name: string;
  rating: number | null; // Average rating (null if no reviews)
  reviewCount: number;
  phoneNumber?: string;
}

/**
 * Review record showing past job history with this customer
 */
export interface JobDetailReview {
  id: string;
  jobId: string;
  jobType: string;
  rating: number; // 1-5 stars
  comment: string | null;
  createdAt: string; // ISO 8601 datetime
}

/**
 * Complete job details shown in the modal
 * Combines assignment, job, customer, and review history
 */
export interface JobDetails {
  // Assignment info
  assignmentId: string;
  status: "Pending" | "Accepted" | "Declined" | "Cancelled";
  assignedAt: string; // ISO 8601 datetime
  acceptedAt?: string | null;

  // Job info
  jobId: string;
  jobType: string; // e.g., "HVAC", "Plumbing"
  location: string;
  desiredDateTime: string; // ISO 8601 datetime
  description: string;
  estimatedDuration?: number | null; // Minutes
  estimatedPay?: number | null; // Dollar amount

  // Customer info
  customer: JobDetailCustomer;

  // Customer's past jobs with this contractor
  pastReviews: JobDetailReview[];
}

/**
 * API response wrapper for job details
 */
export interface JobDetailsResponse {
  data: JobDetails;
}

/**
 * State for useJobDetails hook
 */
export interface UseJobDetailsState {
  jobDetails: JobDetails | null;
  loading: boolean;
  error: string | null;
}
