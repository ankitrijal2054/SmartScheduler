/**
 * Dispatcher Service
 * API client for dispatcher-specific endpoints
 */

import axios, { AxiosInstance, CancelToken } from "axios";
import { config } from "@/utils/config";
import { PaginatedJobsResponse, JobsQueryParams } from "@/types/Job";

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
