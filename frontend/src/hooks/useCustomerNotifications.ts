/**
 * useCustomerNotifications Hook
 * Connects to SignalR NotificationHub and handles customer-specific notifications
 */

import { useEffect, useCallback } from "react";
import { useAuthContext } from "@/hooks/useAuthContext";
import { useNotifications } from "@/hooks/useNotifications";
import { signalRService } from "@/services/signalRService";
import { CustomerNotificationType } from "@/types/NotificationMessages";
import { config } from "@/utils/config";

export const useCustomerNotifications = () => {
  const { user } = useAuthContext();
  const { addNotification } = useNotifications();

  const handleJobAssigned = useCallback(
    (data: {
      jobId: string;
      contractorName: string;
      contractorRating?: number;
      jobType: string;
    }) => {
      const ratingText = data.contractorRating
        ? ` (${data.contractorRating}⭐)`
        : "";
      const message = `Your ${data.jobType} job has been assigned to ${data.contractorName}${ratingText}`;
      addNotification(
        message,
        "ContractorAssigned" as CustomerNotificationType,
        data.jobId,
        "Contractor Assigned"
      );
    },
    [addNotification]
  );

  const handleJobInProgress = useCallback(
    (data: { jobId: string; contractorName: string; jobType: string }) => {
      const message = `${data.contractorName} has started working on your ${data.jobType} job`;
      addNotification(
        message,
        "JobInProgress" as CustomerNotificationType,
        data.jobId,
        "Job In Progress"
      );
    },
    [addNotification]
  );

  const handleJobCompleted = useCallback(
    (data: { jobId: string; contractorName: string; jobType: string }) => {
      const message = `Your ${data.jobType} job has been completed by ${data.contractorName}. Please rate your experience!`;
      addNotification(
        message,
        "JobCompleted" as CustomerNotificationType,
        data.jobId,
        "Job Completed"
      );
    },
    [addNotification]
  );

  const handleJobCancelled = useCallback(
    (data: { jobId: string; reason?: string; jobType: string }) => {
      const message = data.reason
        ? `Your ${data.jobType} job has been cancelled: ${data.reason}`
        : `Your ${data.jobType} job has been cancelled`;
      addNotification(
        message,
        "JobCancelled" as CustomerNotificationType,
        data.jobId,
        "Job Cancelled"
      );
    },
    [addNotification]
  );

  const handleJobReassigned = useCallback(
    (data: {
      jobId: string;
      newContractorName: string;
      previousContractorName: string;
      jobType: string;
    }) => {
      const message = `Your ${data.jobType} job has been reassigned from ${data.previousContractorName} to ${data.newContractorName}`;
      addNotification(
        message,
        "JobReassigned" as CustomerNotificationType,
        data.jobId,
        "Job Reassigned"
      );
    },
    [addNotification]
  );

  const handleScheduleUpdated = useCallback(
    (data: { jobId: string; newDateTime: string; jobType: string }) => {
      const newDate = new Date(data.newDateTime).toLocaleString();
      const message = `Your ${data.jobType} job schedule has been updated to ${newDate}`;
      addNotification(
        message,
        "ScheduleUpdated" as CustomerNotificationType,
        data.jobId,
        "Schedule Updated"
      );
    },
    [addNotification]
  );

  useEffect(() => {
    if (!user?.id) return;

    // Register handlers
    signalRService.on("JobAssigned", handleJobAssigned);
    signalRService.on("JobInProgress", handleJobInProgress);
    signalRService.on("JobCompleted", handleJobCompleted);
    signalRService.on("JobCancelled", handleJobCancelled);
    signalRService.on("JobReassigned", handleJobReassigned);
    signalRService.on("ScheduleUpdated", handleScheduleUpdated);

    // Connect to SignalR notification hub
    signalRService
      .connect({
        url: `${config.api.baseUrl}/notifications`,
        reconnectInterval: 5000,
        maxRetries: 5,
      })
      .then(() => {
        // Join customer group (using user ID as customer ID)
        signalRService.invoke("JoinCustomerGroup", user.id);
      })
      .catch((err) => {
        console.error("Failed to connect to SignalR:", err);
      });

    return () => {
      // Cleanup handlers
      signalRService.off("JobAssigned", handleJobAssigned);
      signalRService.off("JobInProgress", handleJobInProgress);
      signalRService.off("JobCompleted", handleJobCompleted);
      signalRService.off("JobCancelled", handleJobCancelled);
      signalRService.off("JobReassigned", handleJobReassigned);
      signalRService.off("ScheduleUpdated", handleScheduleUpdated);

      if (user?.id) {
        signalRService.invoke("LeaveCustomerGroup", user.id);
      }
    };
  }, [
    user?.id,
    handleJobAssigned,
    handleJobInProgress,
    handleJobCompleted,
    handleJobCancelled,
    handleJobReassigned,
    handleScheduleUpdated,
  ]);
};
