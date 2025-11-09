/**
 * Assignment Type Definitions
 * Shared interfaces for job assignment workflow
 */

/**
 * Assignment status enum representing the state of an assignment
 */
export type AssignmentStatus =
  | "Pending"
  | "Accepted"
  | "InProgress"
  | "Completed"
  | "Declined"
  | "Cancelled";

/**
 * Error codes for assignment-related errors
 */
export enum AssignmentErrorCode {
  JOB_ALREADY_ASSIGNED = "JOB_ALREADY_ASSIGNED",
  CONTRACTOR_NO_LONGER_AVAILABLE = "CONTRACTOR_NO_LONGER_AVAILABLE",
  INVALID_JOB_ID = "INVALID_JOB_ID",
  INVALID_CONTRACTOR_ID = "INVALID_CONTRACTOR_ID",
  JOB_NOT_FOUND = "JOB_NOT_FOUND",
  CONTRACTOR_NOT_FOUND = "CONTRACTOR_NOT_FOUND",
  UNAUTHORIZED = "UNAUTHORIZED",
  FORBIDDEN = "FORBIDDEN",
  INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR",
}

/**
 * Assignment request payload sent to backend
 * Contractor is assigned to a job by the dispatcher
 */
export interface AssignmentRequest {
  contractorId: string;
}

/**
 * Assignment response returned from backend
 * Represents a newly created assignment record
 */
export interface AssignmentResponse {
  assignmentId: string;
  jobId: string;
  contractorId: string;
  status: AssignmentStatus;
  createdAt: string; // ISO 8601 datetime string
}

/**
 * Assignment entity - full assignment record (for backend communication)
 */
export interface Assignment {
  id: string;
  jobId: string;
  contractorId: string;
  status: AssignmentStatus;
  createdAt: string; // ISO 8601 datetime string
  respondedAt?: string | null; // ISO 8601 datetime string, null if pending
}

/**
 * State management type for useJobAssignment hook
 */
export interface JobAssignmentState {
  isAssigning: boolean;
  error: string | null;
  successMessage: string | null;
  assignmentData: AssignmentResponse | null;
}
