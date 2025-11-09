/**
 * Contractor Service
 * API calls for contractor-related endpoints
 * Handles authentication via Authorization header
 */

import axios from "axios";
import { config } from "@/utils/config";
import { Assignment } from "@/types/Assignment";
import { JobDetails } from "@/types/JobDetails";
import {
  ContractorProfileData,
  JobHistoryResponse,
  JobHistoryFilterOptions,
  JobHistoryPaginationParams,
} from "@/types/ContractorProfile";

const api = axios.create({
  baseURL: config.api.baseUrl,
});

// Add token to requests
api.interceptors.request.use((req) => {
  const token = localStorage.getItem(config.auth.jwtStorageKey);
  if (token) {
    req.headers.Authorization = `Bearer ${token}`;
  }
  return req;
});

export const contractorService = {
  /**
   * Get contractor's assignments filtered by status
   */
  async getAssignments(
    status?: "Pending" | "Accepted" | "InProgress" | "Completed"
  ): Promise<
    (Assignment & {
      jobType?: string;
      location?: string;
      scheduledTime?: string;
      customerName?: string;
      description?: string;
    })[]
  > {
    const params = status ? { status } : {};
    const response = await api.get<{
      data: (Assignment & {
        jobType?: string;
        location?: string;
        scheduledTime?: string;
        customerName?: string;
        description?: string;
      })[];
    }>("/api/v1/contractors/assignments", { params });
    return response.data.data;
  },

  /**
   * Get detailed information about a specific assignment
   */
  async getAssignmentDetails(assignmentId: string): Promise<
    Assignment & {
      jobType?: string;
      location?: string;
      scheduledTime?: string;
      customerName?: string;
      description?: string;
      estimatedDuration?: number;
      estimatedPay?: number;
    }
  > {
    const response = await api.get(
      `/api/v1/contractors/assignments/${assignmentId}`
    );
    return response.data.data;
  },

  /**
   * Get complete job details for modal display (Story 5.2)
   * Includes job info, customer profile, and past job history
   */
  async getJobDetails(assignmentId: string): Promise<JobDetails> {
    const response = await api.get<{ data: JobDetails }>(
      `/api/v1/contractors/assignments/${assignmentId}`
    );
    return response.data.data;
  },

  /**
   * Accept a job assignment
   * Used in Story 5.2
   */
  async acceptAssignment(assignmentId: string): Promise<void> {
    await api.post(`/api/v1/contractors/assignments/${assignmentId}/accept`);
  },

  /**
   * Decline a job assignment
   * Used in Story 5.2
   */
  async declineAssignment(
    assignmentId: string,
    reason?: string
  ): Promise<void> {
    await api.post(`/api/v1/contractors/assignments/${assignmentId}/decline`, {
      reason,
    });
  },

  /**
   * Mark a job as in-progress
   * Transitions status from 'Accepted' to 'InProgress'
   * Publishes event for real-time customer notification
   * Used in Story 5.3
   */
  async markInProgress(assignmentId: string): Promise<Assignment> {
    const response = await api.patch<{ data: Assignment }>(
      `/api/v1/assignments/${assignmentId}/mark-in-progress`
    );
    return response.data.data;
  },

  /**
   * Mark a job as completed
   * Transitions status from 'InProgress' to 'Completed'
   * Publishes event to trigger completion email
   * Used in Story 5.3
   */
  async markComplete(assignmentId: string): Promise<Assignment> {
    const response = await api.patch<{ data: Assignment }>(
      `/api/v1/assignments/${assignmentId}/mark-complete`
    );
    return response.data.data;
  },

  /**
   * Get contractor's profile with aggregated statistics
   * Includes: name, average rating, job counts, acceptance rate, recent reviews
   * Used in Story 5.4
   */
  async getProfile(): Promise<ContractorProfileData> {
    const response = await api.get<{ data: ContractorProfileData }>(
      "/api/v1/contractors/profile"
    );
    return response.data.data;
  },

  /**
   * Get contractor's job history with optional filtering and pagination
   * Used in Story 5.4
   * @param filters Optional date filters (startDate, endDate in ISO 8601 format)
   * @param pagination Pagination parameters (skip, take)
   */
  async getJobHistory(
    pagination: JobHistoryPaginationParams,
    filters?: JobHistoryFilterOptions
  ): Promise<JobHistoryResponse> {
    const params: Record<string, any> = {
      skip: pagination.skip,
      take: pagination.take,
    };

    if (filters?.startDate) {
      params.startDate = filters.startDate;
    }

    if (filters?.endDate) {
      params.endDate = filters.endDate;
    }

    const response = await api.get<{ data: JobHistoryResponse }>(
      "/api/v1/contractors/job-history",
      { params }
    );
    return response.data.data;
  },
};
