/**
 * Job Type Definitions
 * Shared interfaces for job-related data across the frontend application
 */

import { Contractor } from "./Contractor";

export type JobStatus = "Pending" | "Assigned" | "InProgress" | "Completed";
export type JobType = "Flooring" | "HVAC" | "Plumbing" | "Electrical" | "Other";

export interface Job {
  id: string;
  customerId: string;
  customerName?: string;
  location: string;
  desiredDateTime: string;
  jobType: JobType;
  description: string;
  status: JobStatus;
  currentAssignedContractorId: string | null;
  assignedContractorName?: string;
  assignedContractorRating?: number;
  createdAt: string;
  updatedAt: string;
  reassignmentCount?: number; // Optional: track number of times job was reassigned
}

export interface JobResponse {
  id: string;
  customerId: string;
  customerName?: string;
  location: string;
  desiredDateTime: string;
  jobType: string;
  description: string;
  status: string;
  currentAssignedContractorId: string | null;
  assignedContractorName?: string;
  assignedContractorRating?: number;
  createdAt: string;
  updatedAt: string;
  reassignmentCount?: number; // Optional: track number of times job was reassigned
}

/**
 * JobDetail - Extended job with full contractor and assignment info
 * Used for customer job tracking page
 */
export interface JobDetail extends Job {
  assignment?: {
    id: string;
    contractorId: string;
    status: string;
    assignedAt: string;
    acceptedAt?: string | null;
    completedAt?: string | null;
    estimatedArrivalTime?: string | null;
  };
  contractor?: Contractor & {
    phoneNumber?: string;
    averageRating?: number;
    reviewCount?: number;
  };
}

export interface JobDetailResponse {
  id: string;
  customerId: string;
  location: string;
  desiredDateTime: string;
  jobType: string;
  description: string;
  status: string;
  currentAssignedContractorId: string | null;
  createdAt: string;
  updatedAt: string;
  assignment?: {
    id: string;
    contractorId: string;
    status: string;
    assignedAt: string;
    acceptedAt?: string | null;
    completedAt?: string | null;
    estimatedArrivalTime?: string | null;
  };
  contractor?: {
    id: string;
    name: string;
    phoneNumber?: string;
    averageRating?: number;
    reviewCount?: number;
    isActive?: boolean;
  };
}

export interface PaginationMeta {
  page: number;
  limit: number;
  total: number;
  totalPages: number;
}

export interface PaginatedJobsResponse {
  data: JobResponse[];
  pagination: PaginationMeta;
}

export interface JobsQueryParams {
  page?: number;
  limit?: number;
  status?: JobStatus;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
}
