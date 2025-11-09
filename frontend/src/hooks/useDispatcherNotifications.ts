/**
 * useDispatcherNotifications Hook
 * Connects to SignalR NotificationHub and handles dispatcher-specific notifications
 */

import { useEffect, useCallback } from "react";
import { useAuthContext } from "@/hooks/useAuthContext";
import { useNotifications } from "@/hooks/useNotifications";
import { signalRService } from "@/services/signalRService";
import { DispatcherNotificationType } from "@/types/NotificationMessages";
import { config } from "@/utils/config";

export const useDispatcherNotifications = () => {
  const { user } = useAuthContext();
  const { addNotification } = useNotifications();

  const handleNewJobCreated = useCallback(
    (data: {
      jobId: string;
      jobType: string;
      location: string;
      customerName: string;
    }) => {
      const message = `New ${data.jobType} job created by ${data.customerName} at ${data.location}`;
      addNotification(
        message,
        "NewJobCreated" as DispatcherNotificationType,
        data.jobId,
        "New Job Created"
      );
    },
    [addNotification]
  );

  const handleJobAssigned = useCallback(
    (data: { jobId: string; contractorName: string; jobType: string }) => {
      const message = `${data.jobType} job assigned to ${data.contractorName}`;
      addNotification(
        message,
        "JobAssigned" as DispatcherNotificationType,
        data.jobId,
        "Job Assigned"
      );
    },
    [addNotification]
  );

  const handleJobStatusChanged = useCallback(
    (data: { jobId: string; status: string; jobType: string }) => {
      const message = `${data.jobType} job status changed to ${data.status}`;
      addNotification(
        message,
        "JobStatusChanged" as DispatcherNotificationType,
        data.jobId,
        "Status Updated"
      );
    },
    [addNotification]
  );

  const handleContractorUnavailable = useCallback(
    (data: { contractorName: string; jobId: string; reason?: string }) => {
      const message = data.reason
        ? `${data.contractorName} is unavailable: ${data.reason}`
        : `${data.contractorName} is unavailable`;
      addNotification(
        message,
        "ContractorUnavailable" as DispatcherNotificationType,
        data.jobId,
        "Contractor Unavailable"
      );
    },
    [addNotification]
  );

  useEffect(() => {
    if (!user?.id) return;

    // Register handlers
    signalRService.on("NewJobCreated", handleNewJobCreated);
    signalRService.on("JobAssigned", handleJobAssigned);
    signalRService.on("JobStatusChanged", handleJobStatusChanged);
    signalRService.on("ContractorUnavailable", handleContractorUnavailable);

    // Connect to SignalR notification hub
    signalRService
      .connect({
        url: `${config.api.baseUrl}/notifications`,
        reconnectInterval: 5000,
        maxRetries: 5,
      })
      .then(() => {
        // Join dispatcher group
        signalRService.invoke("JoinDispatcherGroup", user.id);
      })
      .catch((err) => {
        console.error("Failed to connect to SignalR:", err);
      });

    return () => {
      // Cleanup handlers
      signalRService.off("NewJobCreated", handleNewJobCreated);
      signalRService.off("JobAssigned", handleJobAssigned);
      signalRService.off("JobStatusChanged", handleJobStatusChanged);
      signalRService.off("ContractorUnavailable", handleContractorUnavailable);

      if (user?.id) {
        signalRService.invoke("LeaveDispatcherGroup", user.id);
      }
    };
  }, [
    user?.id,
    handleNewJobCreated,
    handleJobAssigned,
    handleJobStatusChanged,
    handleContractorUnavailable,
  ]);
};
