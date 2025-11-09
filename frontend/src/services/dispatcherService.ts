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
  RecommendedContractor,
  TimeSlot,
  Contractor,
  PaginatedContractorsResponse,
  ContractorHistory,
} from "@/types/Contractor";
import { AssignmentRequest, AssignmentResponse } from "@/types/Assignment";
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
      // Backend response structure
      interface BackendRecommendationDto {
        contractorId: number;
        name: string;
        score: number;
        rating: number | null;
        reviewCount: number;
        distance: number;
        travelTime: number;
        availableTimeSlots: string[]; // ISO 8601 datetime strings
      }

      interface BackendRecommendationResponse {
        recommendations: BackendRecommendationDto[];
        message: string;
      }

      const response =
        await this.axiosInstance.get<BackendRecommendationResponse>(
          "/api/v1/recommendations",
          {
            params: {
              jobId: request.jobId,
              contractorListOnly: request.contractor_list_only ?? false,
            },
            timeout: 30000, // 30 second timeout for recommendations API (processing 50+ contractors can take time)
            cancelToken,
          }
        );

      // Map backend response to frontend format
      const mappedRecommendations: RecommendedContractor[] =
        response.data.recommendations.map((rec, index) => {
          // Convert DateTime strings to TimeSlot objects (1-hour windows)
          const timeSlots: TimeSlot[] = rec.availableTimeSlots.map((slot) => {
            const startTime = new Date(slot);
            const endTime = new Date(startTime.getTime() + 60 * 60 * 1000); // Add 1 hour
            return {
              startTime: startTime.toISOString(),
              endTime: endTime.toISOString(),
            };
          });

          return {
            contractorId: rec.contractorId.toString(),
            name: rec.name,
            rank: index + 1, // Rank is 1-based based on position in array
            score: rec.score,
            avgRating: rec.rating,
            reviewCount: rec.reviewCount,
            distance: rec.distance,
            travelTime: rec.travelTime,
            tradeType: request.jobType, // Use jobType from request since backend doesn't provide it
            availableTimeSlots: timeSlots,
          };
        });

      return {
        data: mappedRecommendations,
        metadata: {
          totalAvailable: mappedRecommendations.length,
          requestTime: new Date().toISOString(),
        },
      };
    } catch (error) {
      if (axios.isCancel(error)) {
        // Silently handle cancelled requests - they're expected when component unmounts or new request is initiated
        throw error; // Re-throw the cancel error so the hook can handle it
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
      // Backend expects contractorId as a query parameter, not in the request body
      // Backend response structure: { message, assignmentId, jobId, contractorId }
      interface BackendAssignmentResponse {
        message: string;
        assignmentId: number;
        jobId: number;
        contractorId: number;
      }

      const response = await this.axiosInstance.post<BackendAssignmentResponse>(
        `/api/v1/dispatcher/jobs/${jobId}/assign`,
        {}, // Empty body since contractorId is in query params
        {
          params: {
            contractorId: request.contractorId,
          },
          timeout: 5000, // 5 second timeout for assignment API
          cancelToken,
        }
      );

      // Map backend response to frontend format
      return {
        assignmentId: response.data.assignmentId.toString(),
        jobId: response.data.jobId.toString(),
        contractorId: response.data.contractorId.toString(),
        status: "Pending" as const, // Default status for new assignments
        createdAt: new Date().toISOString(), // Use current time since backend doesn't provide it
      };
    } catch (error) {
      if (axios.isCancel(error)) {
        // Silently handle cancelled requests - they're expected when component unmounts or new request is initiated
        throw error; // Re-throw the cancel error so the hook can handle it
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
        // Silently handle cancelled requests - they're expected when component unmounts or new request is initiated
        throw error; // Re-throw the cancel error so the hook can handle it
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
        // Silently handle cancelled requests - they're expected
        throw error; // Re-throw the cancel error so the hook can handle it
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
        // Silently handle cancelled requests - they're expected
        throw error; // Re-throw the cancel error so the hook can handle it
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
        // Silently handle cancelled requests - they're expected
        throw error; // Re-throw the cancel error so the hook can handle it
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
        // Silently handle cancelled requests - they're expected
        throw error; // Re-throw the cancel error so the hook can handle it
      }
      throw this.handleError(error);
    }
  }

  /**
   * Get contractor history and performance stats
   * @param contractorId Contractor identifier
   * @param limit Number of recent jobs to fetch (default 10, max 50)
   * @param offset Pagination offset (default 0)
   * @param cancelToken Optional axios cancel token for cleanup
   * @returns Contractor history with stats and job history
   */
  async getContractorHistory(
    contractorId: string,
    limit: number = 10,
    offset: number = 0,
    cancelToken?: CancelToken
  ): Promise<ContractorHistory> {
    try {
      const response = await this.axiosInstance.get<ContractorHistory>(
        `/api/v1/dispatcher/contractors/${contractorId}/history`,
        {
          params: {
            limit: Math.min(limit, 50),
            offset,
          },
          cancelToken,
        }
      );
      return response.data;
    } catch (error) {
      if (axios.isCancel(error)) {
        // Silently handle cancelled requests - they're expected when component unmounts
        throw error; // Re-throw the cancel error so the hook can handle it
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
