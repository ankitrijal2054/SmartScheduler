/**
 * Customer Service
 * API client for customer-facing endpoints
 */

import axios, { AxiosInstance } from "axios";
import { config } from "@/utils/config";
import { CreateJobRequest, JobCreationResponse } from "@/types/Customer";

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
