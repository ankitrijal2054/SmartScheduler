/**
 * Customer Service
 * API client for customer-facing endpoints
 */

import axios, { AxiosInstance } from "axios";
import { config } from "@/utils/config";
import {
  CreateJobRequest,
  JobCreationResponse,
  ContractorProfileResponse,
  CreateReviewRequest,
  Review,
} from "@/types/Customer";
import { JobDetail, JobDetailResponse } from "@/types/Job";

class CustomerService {
  private axiosInstance: AxiosInstance;

  constructor() {
    this.axiosInstance = axios.create({
      baseURL: config.api.baseUrl,
      timeout: 10000,
    });

    // Request interceptor: Add JWT token to headers
    this.axiosInstance.interceptors.request.use((cfg) => {
      const token = localStorage.getItem(config.auth.jwtStorageKey);
      if (token) {
        cfg.headers.Authorization = `Bearer ${token}`;
      }
      return cfg;
    });

    // Response interceptor: Handle errors
    this.axiosInstance.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          // Token expired or invalid - redirect to login
          localStorage.removeItem(config.auth.jwtStorageKey);
          window.location.href = "/login";
        }
        return Promise.reject(error);
      }
    );
  }

  /**
   * Submit a new job
   * @param request Job submission request data
   * @returns Created job response with job ID
   * @throws Error if submission fails
   */
  async submitJob(request: CreateJobRequest): Promise<JobCreationResponse> {
    try {
      const response = await this.axiosInstance.post<{
        data: JobCreationResponse;
        message: string;
      }>("/api/v1/jobs", request);

      // Normalize IDs to strings
      const normalizedData: JobCreationResponse = {
        ...response.data.data,
        id: String(response.data.data.id), // Convert number to string
        customerId: String(response.data.data.customerId), // Convert number to string
      };

      return normalizedData;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  /**
   * Fetch a single job by ID with full details
   * @param jobId Job unique identifier
   * @returns Job detail with contractor and assignment info
   * @throws Error if job not found or fetch fails
   */
  async getJobById(jobId: string): Promise<JobDetail> {
    try {
      const response = await this.axiosInstance.get<{
        data: JobDetailResponse;
        message: string;
      }>(`/api/v1/customer/jobs/${jobId}`);

      const jobData = response.data.data;

      // Normalize response to JobDetail
      const job: JobDetail = {
        id: String(jobData.id), // Convert number to string
        customerId: String(jobData.customerId), // Convert number to string
        location: jobData.location,
        desiredDateTime: jobData.desiredDateTime,
        jobType: (jobData.jobType as any) || "Other",
        description: jobData.description,
        status: (jobData.status as any) || "Pending",
        currentAssignedContractorId: jobData.currentAssignedContractorId
          ? String(jobData.currentAssignedContractorId)
          : null, // Convert number to string or keep null
        createdAt: jobData.createdAt,
        updatedAt: jobData.updatedAt,
      };

      if (jobData.assignment) {
        job.assignment = {
          id: String(jobData.assignment.id), // Convert number to string
          contractorId: String(jobData.assignment.contractorId), // Convert number to string
          status: jobData.assignment.status,
          assignedAt: jobData.assignment.assignedAt,
          acceptedAt: jobData.assignment.acceptedAt || null,
          completedAt: jobData.assignment.completedAt || null,
          estimatedArrivalTime: jobData.assignment.estimatedArrivalTime || null,
        };
      }

      if (jobData.contractor) {
        job.contractor = {
          id: String(jobData.contractor.id), // Convert number to string
          name: jobData.contractor.name,
          rating: jobData.contractor.averageRating || null,
          reviewCount: jobData.contractor.reviewCount || 0,
          location: "",
          tradeType: (jobData.jobType as any) || "Other",
          isActive: jobData.contractor.isActive || true,
          phoneNumber: jobData.contractor.phoneNumber,
          averageRating: jobData.contractor.averageRating,
        };
      }

      return job;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  /**
   * Fetch contractor profile with reviews
   * @param contractorId Contractor unique identifier
   * @returns Contractor profile with reviews array
   * @throws Error if contractor not found or fetch fails
   */
  async getContractorProfile(
    contractorId: string
  ): Promise<ContractorProfileResponse> {
    try {
      const response = await this.axiosInstance.get<{
        data: ContractorProfileResponse;
        message: string;
      }>(`/api/v1/customer/contractors/${contractorId}/profile`);
      return response.data.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  /**
   * Submit a rating/review for a completed job
   * @param jobId Job unique identifier
   * @param request Rating submission request data
   * @returns Submitted review with confirmation details
   * @throws Error if submission fails
   */
  async submitRating(
    jobId: string,
    request: CreateReviewRequest
  ): Promise<Review> {
    try {
      const response = await this.axiosInstance.post<{
        data: Review;
        message: string;
      }>(`/api/v1/customer/jobs/${jobId}/review`, request);
      return response.data.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  /**
   * Handle and normalize API errors to user-friendly messages
   * @param error Any error from axios
   * @returns Error object with user-friendly message
   */
  private handleError(error: unknown): Error {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const errorData = error.response?.data as
        | {
            message?: string;
            errors?: Record<string, string[]>;
          }
        | undefined;

      switch (status) {
        case 400:
          const fieldErrors = errorData?.errors
            ? Object.entries(errorData.errors)
                .map(([field, msgs]) => `${field}: ${msgs.join(", ")}`)
                .join("; ")
            : "Invalid input. Please check your entries.";
          return new Error(fieldErrors);

        case 401:
          return new Error("Your session has expired. Please log in again.");

        case 403:
          return new Error(
            "You don't have permission to perform this action. Please log in as a customer."
          );

        case 404:
          return new Error(
            errorData?.message ||
              "Job not found or not in completed status. You can only rate completed jobs."
          );

        case 409:
          return new Error(
            errorData?.message || "You have already rated this job."
          );

        case 500:
          return new Error(
            "Server error. Please try again later or contact support."
          );

        default:
          return new Error(
            errorData?.message ||
              "Failed to submit request. Please check your connection and try again."
          );
      }
    }

    if (error instanceof Error) {
      return error;
    }

    return new Error("An unexpected error occurred. Please try again.");
  }
}

export const customerService = new CustomerService();
