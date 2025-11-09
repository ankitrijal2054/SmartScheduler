/**
 * Reassignment Type Definitions
 * Shared interfaces for job reassignment workflow
 */

/**
 * Reassignment mode type for modal context
 * "assign" = initial job assignment flow
 * "reassign" = job reassignment workflow
 */
export type ReassignmentMode = "assign" | "reassign";

/**
 * Reassignment error codes for specific error scenarios
 */
export enum ReassignmentErrorCode {
  JOB_NOT_ASSIGNED = "JOB_NOT_ASSIGNED",
  CONTRACTOR_NO_LONGER_AVAILABLE = "CONTRACTOR_NO_LONGER_AVAILABLE",
  CONTRACTOR_ALREADY_ASSIGNED = "CONTRACTOR_ALREADY_ASSIGNED",
  INVALID_JOB_ID = "INVALID_JOB_ID",
  INVALID_CONTRACTOR_ID = "INVALID_CONTRACTOR_ID",
  JOB_NOT_FOUND = "JOB_NOT_FOUND",
  CONTRACTOR_NOT_FOUND = "CONTRACTOR_NOT_FOUND",
  UNAUTHORIZED = "UNAUTHORIZED",
  FORBIDDEN = "FORBIDDEN",
  INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR",
}

/**
 * Reassignment request payload sent to backend
 * Contains new contractor ID and optional reason for reassignment
 */
export interface ReassignmentRequest {
  newContractorId: string;
  reason?: string; // Optional reason for reassignment (e.g., "Original contractor cancelled")
}

/**
 * Reassignment response returned from backend
 * Contains information about the completed reassignment operation
 */
export interface ReassignmentResponse {
  newAssignmentId: string; // ID of newly created assignment
  oldAssignmentId: string; // ID of previous assignment (now cancelled)
  jobId: string;
  previousContractorId: string; // Original contractor (removed)
  newContractorId: string; // New contractor (now assigned)
  jobStatus: string; // Remains "Assigned" after reassignment
  createdAt: string; // ISO 8601 datetime string
}

/**
 * State management type for useJobReassignment hook
 */
export interface JobReassignmentState {
  isReassigning: boolean;
  error: string | null;
  successMessage: string | null;
  reassignmentData: ReassignmentResponse | null;
}
