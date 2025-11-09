/**
 * Customer Type Definitions
 * Shared interfaces for customer-facing functionality
 */

import { JobType } from "./Job";

/**
 * Form data for job submission
 * Represents the user input before API submission
 */
export interface JobSubmissionFormData {
  jobType: JobType | "";
  location: string;
  description: string;
  desiredDateTime: string;
}

/**
 * Request payload for creating a job
 * Maps to the API endpoint POST /api/v1/jobs
 */
export interface CreateJobRequest {
  jobType: JobType;
  location: string;
  description: string;
  desiredDateTime: string;
}

/**
 * Response from successful job creation
 */
export interface JobCreationResponse {
  id: string;
  customerId: string;
  jobType: JobType;
  location: string;
  description: string;
  desiredDateTime: string;
  status: "Pending";
  currentAssignedContractorId: null;
  createdAt: string;
  updatedAt: string;
}

/**
 * Form validation error state
 */
export interface FormValidationErrors {
  jobType?: string;
  location?: string;
  description?: string;
  desiredDateTime?: string;
}

/**
 * Job submission hook state
 */
export interface JobSubmissionState {
  formData: JobSubmissionFormData;
  loading: boolean;
  error: string | null;
  success: boolean;
  submittedJobId: string | null;
  validationErrors: FormValidationErrors;
}
