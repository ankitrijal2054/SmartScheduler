/**
 * Customer Service
 * API client for customer-facing endpoints
 */

import axios, { AxiosInstance } from "axios";
import { config } from "@/utils/config";
import { CreateJobRequest, JobCreationResponse } from "@/types/Customer";
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
      return response.data.data;
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
        id: jobData.id,
        customerId: jobData.customerId,
        location: jobData.location,
        desiredDateTime: jobData.desiredDateTime,
        jobType: (jobData.jobType as any) || "Other",
        description: jobData.description,
        status: (jobData.status as any) || "Pending",
        currentAssignedContractorId: jobData.currentAssignedContractorId,
        createdAt: jobData.createdAt,
        updatedAt: jobData.updatedAt,
      };

      if (jobData.assignment) {
        job.assignment = {
          id: jobData.assignment.id,
          contractorId: jobData.assignment.contractorId,
          status: jobData.assignment.status,
          assignedAt: jobData.assignment.assignedAt,
          acceptedAt: jobData.assignment.acceptedAt || null,
          completedAt: jobData.assignment.completedAt || null,
          estimatedArrivalTime: jobData.assignment.estimatedArrivalTime || null,
        };
      }

      if (jobData.contractor) {
        job.contractor = {
          id: jobData.contractor.id,
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
            "You don't have permission to submit a job. Please log in as a customer."
          );

        case 409:
          return new Error(
            "A job with this date and time already exists. Please choose a different time."
          );

        case 500:
          return new Error(
            "Server error. Please try again later or contact support."
          );

        default:
          return new Error(
            errorData?.message ||
              "Failed to submit job. Please check your connection and try again."
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
