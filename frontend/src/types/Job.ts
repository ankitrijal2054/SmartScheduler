/**
 * Job Type Definitions
 * Shared interfaces for job-related data across the frontend application
 */

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
