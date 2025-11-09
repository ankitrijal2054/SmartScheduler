/**
 * Contractor Service
 * API calls for contractor-related endpoints
 * Handles authentication via Authorization header
 */

import axios from "axios";
import { config } from "@/utils/config";
import { Assignment } from "@/types/Assignment";

const api = axios.create({
  baseURL: config.apiBaseUrl,
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
    }>("/api/v1/contractor/assignments", { params });
    return response.data.data;
  },

  /**
   * Get detailed information about a specific assignment
   */
  async getAssignmentDetails(
    assignmentId: string
  ): Promise<
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
      `/api/v1/contractor/assignments/${assignmentId}`
    );
    return response.data.data;
  },

  /**
   * Accept a job assignment
   * Used in Story 5.2
   */
  async acceptAssignment(assignmentId: string): Promise<void> {
    await api.post(`/api/v1/contractor/assignments/${assignmentId}/accept`);
  },

  /**
   * Decline a job assignment
   * Used in Story 5.2
   */
  async declineAssignment(
    assignmentId: string,
    reason?: string
  ): Promise<void> {
    await api.post(`/api/v1/contractor/assignments/${assignmentId}/decline`, {
      reason,
    });
  },
};



