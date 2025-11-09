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
  Contractor,
  PaginatedContractorsResponse,
} from "@/types/Contractor";
import {
  AssignmentRequest,
  AssignmentResponse,
  AssignmentErrorCode,
} from "@/types/Assignment";
import {
  ReassignmentRequest,
  ReassignmentResponse,
} from "@/types/Reassignment";

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
   * Reassign a job to a different contractor (dispatcher action)
   * @param jobId Job identifier
   * @param request Reassignment request with newContractorId and optional reason
   * @param cancelToken Optional axios cancel token for cleanup
   * @returns Reassignment response with reassignment data
   */
  async reassignJob(
    jobId: string,
    request: ReassignmentRequest,
    cancelToken?: CancelToken
  ): Promise<ReassignmentResponse> {
    try {
      const response = await this.axiosInstance.put<{
        data: ReassignmentResponse;
      }>(`/api/v1/dispatcher/jobs/${jobId}/reassign`, request, {
        timeout: 5000, // 5 second timeout for reassignment API
        cancelToken,
      });
      return response.data.data;
    } catch (error) {
      if (axios.isCancel(error)) {
        console.log("Reassignment request cancelled");
        throw new Error("Reassignment request cancelled");
      }
      throw this.handleReassignmentError(error);
    }
  }

  /**
   * Get contractor list for current dispatcher
   * @param cancelToken Optional axios cancel token for cleanup
   * @returns Array of contractors in dispatcher's list
   */
  async getContractorList(cancelToken?: CancelToken): Promise<Contractor[]> {
    try {
      const response = await this.axiosInstance.get<Contractor[]>(
        "/api/v1/dispatcher/contractor-list",
        { cancelToken }
      );
      return response.data;
    } catch (error) {
      if (axios.isCancel(error)) {
        console.log("Contractor list request cancelled");
        throw new Error("Request cancelled");
      }
      throw this.handleError(error);
    }
  }

  /**
   * Add contractor to dispatcher's list
   * @param contractorId Contractor identifier
   * @param cancelToken Optional axios cancel token for cleanup
   * @returns Updated contractor list
   */
  async addContractorToList(
    contractorId: string,
    cancelToken?: CancelToken
  ): Promise<Contractor[]> {
    try {
      const response = await this.axiosInstance.post<Contractor[]>(
        `/api/v1/dispatcher/contractor-list/${contractorId}`,
        {},
        { cancelToken }
      );
      return response.data;
    } catch (error) {
      if (axios.isCancel(error)) {
        console.log("Add contractor request cancelled");
        throw new Error("Request cancelled");
      }
      throw this.handleError(error);
    }
  }

  /**
   * Remove contractor from dispatcher's list
   * @param contractorId Contractor identifier
   * @param cancelToken Optional axios cancel token for cleanup
   * @returns Updated contractor list
   */
  async removeContractorFromList(
    contractorId: string,
    cancelToken?: CancelToken
  ): Promise<Contractor[]> {
    try {
      const response = await this.axiosInstance.delete<Contractor[]>(
        `/api/v1/dispatcher/contractor-list/${contractorId}`,
        { cancelToken }
      );
      return response.data;
    } catch (error) {
      if (axios.isCancel(error)) {
        console.log("Remove contractor request cancelled");
        throw new Error("Request cancelled");
      }
      throw this.handleError(error);
    }
  }

  /**
   * Get all available contractors (for adding to list)
   * @param limit Number of contractors to fetch (default 50, max 100)
   * @param offset Pagination offset (default 0)
   * @param search Optional search term to filter by name
   * @param cancelToken Optional axios cancel token for cleanup
   * @returns Paginated contractors response
   */
  async getAvailableContractors(
    limit: number = 50,
    offset: number = 0,
    search?: string,
    cancelToken?: CancelToken
  ): Promise<PaginatedContractorsResponse> {
    try {
      const response =
        await this.axiosInstance.get<PaginatedContractorsResponse>(
          "/api/v1/dispatcher/contractors",
          {
            params: {
              limit: Math.min(limit, 100),
              offset,
              ...(search && { search }),
            },
            cancelToken,
          }
        );
      return response.data;
    } catch (error) {
      if (axios.isCancel(error)) {
        console.log("Available contractors request cancelled");
        throw new Error("Request cancelled");
      }
      throw this.handleError(error);
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
   * Handle and normalize reassignment API errors
   * Maps HTTP status codes to user-friendly error messages
   */
  private handleReassignmentError(error: unknown): Error {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const errorCode = error.response?.data?.error?.code;

      // Map specific error codes to messages
      switch (errorCode) {
        case "JOB_NOT_ASSIGNED":
          return new Error(
            "This job is not currently assigned for reassignment"
          );
        case "CONTRACTOR_NO_LONGER_AVAILABLE":
          return new Error(
            "New contractor no longer available; please try again"
          );
        case "CONTRACTOR_ALREADY_ASSIGNED":
          return new Error(
            "Selected contractor is already assigned to this job"
          );
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
          return new Error("Forbidden: Only dispatchers can reassign jobs");
        case 404:
          return new Error("Job or contractor not found");
        case 409:
          return new Error("Contractor no longer available; please try again");
        case 500:
          return new Error("Server error: Unable to complete reassignment");
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
