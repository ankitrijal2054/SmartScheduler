/**
 * Dispatcher Service
 * API client for dispatcher-specific endpoints
 */

import axios, { AxiosInstance, CancelToken } from "axios";
import { config } from "@/utils/config";
import { PaginatedJobsResponse, JobsQueryParams } from "@/types/Job";
import {
  RecommendationRequest,
  RecommendationResponse,
} from "@/types/Contractor";
import {
  AssignmentRequest,
  AssignmentResponse,
  AssignmentErrorCode,
} from "@/types/Assignment";

class DispatcherService {
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
   * Get all jobs for dispatcher dashboard
   * @param params Query parameters (pagination, sorting, filtering)
   * @param cancelToken Optional axios cancel token for cleanup
   * @returns Paginated jobs response
   */
  async getJobs(
    params: JobsQueryParams = {},
    cancelToken?: CancelToken
  ): Promise<PaginatedJobsResponse> {
    try {
      const response = await this.axiosInstance.get<PaginatedJobsResponse>(
        "/api/v1/jobs",
        {
          params: {
            page: params.page ?? 1,
            pageSize: params.limit ?? 20,
            status: params.status,
            sortBy: params.sortBy ?? "desiredDateTime",
            sortOrder: params.sortOrder ?? "asc",
          },
          cancelToken,
        }
      );
      return response.data;
    } catch (error) {
      if (axios.isCancel(error)) {
        console.log("Request cancelled");
        throw new Error("Request cancelled");
      }
      throw this.handleError(error);
    }
  }

  /**
   * Get contractor recommendations for a job
   * @param request Recommendation request containing job details and optional filters
   * @param cancelToken Optional axios cancel token for cleanup
   * @returns Recommendation response with top 5 contractors
   */
  async getRecommendations(
    request: RecommendationRequest,
    cancelToken?: CancelToken
  ): Promise<RecommendationResponse> {
    try {
      const response = await this.axiosInstance.post<RecommendationResponse>(
        "/api/v1/recommendations",
        request,
        {
          timeout: 5000, // 5 second timeout for recommendations API
          cancelToken,
        }
      );
      return response.data;
    } catch (error) {
      if (axios.isCancel(error)) {
        console.log("Recommendations request cancelled");
        throw new Error("Recommendations request cancelled");
      }
      throw this.handleError(error);
    }
  }

  /**
   * Assign a job to a contractor (dispatcher action)
   * @param jobId Job identifier
   * @param request Assignment request with contractorId
   * @param cancelToken Optional axios cancel token for cleanup
   * @returns Assignment response with assignment data
   */
  async assignJob(
    jobId: string,
    request: AssignmentRequest,
    cancelToken?: CancelToken
  ): Promise<AssignmentResponse> {
    try {
      const response = await this.axiosInstance.post<{
        data: AssignmentResponse;
      }>(`/api/v1/dispatcher/jobs/${jobId}/assign`, request, {
        timeout: 5000, // 5 second timeout for assignment API
        cancelToken,
      });
      return response.data.data;
    } catch (error) {
      if (axios.isCancel(error)) {
        console.log("Assignment request cancelled");
        throw new Error("Assignment request cancelled");
      }
      throw this.handleAssignmentError(error);
    }
  }

  /**
   * Handle and normalize assignment API errors
   * Maps HTTP status codes to user-friendly error messages
   */
  private handleAssignmentError(error: unknown): Error {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const errorCode = error.response?.data?.error?.code;

      // Map specific error codes to messages
      switch (errorCode) {
        case "JOB_ALREADY_ASSIGNED":
          return new Error(
            "This job is already assigned. Please refresh and try again."
          );
        case "CONTRACTOR_NO_LONGER_AVAILABLE":
          return new Error("Contractor no longer available; please try again");
        case "INVALID_JOB_ID":
        case "JOB_NOT_FOUND":
          return new Error("Job not found");
        case "INVALID_CONTRACTOR_ID":
        case "CONTRACTOR_NOT_FOUND":
          return new Error("Contractor not found");
        default:
          break;
      }

      // Map HTTP status codes
      switch (status) {
        case 400:
          return new Error("Invalid request data");
        case 401:
          return new Error("Unauthorized: Invalid or expired session");
        case 403:
          return new Error("Forbidden: Only dispatchers can assign jobs");
        case 404:
          return new Error("Job or contractor not found");
        case 409:
          return new Error("Contractor no longer available; please try again");
        case 500:
          return new Error("Server error: Unable to complete assignment");
        default:
          return new Error(
            error.response?.data?.error?.message || error.message
          );
      }
    }
    return error instanceof Error ? error : new Error("Unknown error occurred");
  }

  /**
   * Handle and normalize API errors
   */
  private handleError(error: unknown): Error {
    if (axios.isAxiosError(error)) {
      const message = error.response?.data?.error?.message || error.message;
      return new Error(message);
    }
    return error instanceof Error ? error : new Error("Unknown error occurred");
  }
}

export const dispatcherService = new DispatcherService();
