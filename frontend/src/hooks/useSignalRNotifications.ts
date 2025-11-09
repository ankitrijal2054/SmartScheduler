/**
 * useSignalRNotifications Hook
 * Connects to SignalR NotificationHub and handles real-time notifications
 * Automatically reconnects on disconnection with exponential backoff
 * Assumes signalRService is configured with the hub connection
 */

import { useEffect, useRef, useState, useCallback } from "react";
import { useAuthContext } from "@/hooks/useAuthContext";
import { useNotifications } from "@/hooks/useNotifications";
import { signalRService } from "@/services/signalRService";
import { NotificationType } from "@/types/NotificationMessages";

interface UseSignalRNotificationsState {
  isConnected: boolean;
  error: string | null;
}

const RECONNECT_DELAYS = [1000, 2000, 5000, 10000]; // Exponential backoff in ms
const MAX_RECONNECT_ATTEMPTS = 5;

export const useSignalRNotifications = () => {
  const { user } = useAuthContext();
  const { addNotification } = useNotifications();
  const [state, setState] = useState<UseSignalRNotificationsState>({
    isConnected: false,
    error: null,
  });
  const reconnectCountRef = useRef(0);
  const reconnectTimeoutRef = useRef<number>();

  const handleNewJobAssigned = useCallback(
    (data: {
      jobId: string;
      jobType: string;
      location: string;
      scheduledTime: string;
    }) => {
      const message = `New ${data.jobType} job at ${data.location}`;
      addNotification(message, "NewJobAssigned", data.jobId);
      
      // Optional: Play notification sound
      try {
        const audio = new Audio(
          "data:audio/wav;base64,UklGRiYAAABXQVZFZm10IBAAAAABAAEAQB8AAAB9AAACABAAZGF0YQIAAAAAAA=="
        );
        audio.play().catch(() => {
          // Silently fail if audio can't play
        });
      } catch (err) {
        // Silently fail
      }
    },
    [addNotification]
  );

  const handleJobReassigned = useCallback(
    (data: { jobId: string; reason?: string }) => {
      addNotification("Your assignment has been reassigned", "JobReassigned", data.jobId);
    },
    [addNotification]
  );

  const handleJobCancelled = useCallback(
    (data: { jobId: string; reason?: string }) => {
      addNotification("A job assignment was cancelled", "JobCancelled", data.jobId);
    },
    [addNotification]
  );

  const handleJobStatusUpdated = useCallback(
    (data: { jobId: string; status: string }) => {
      addNotification(
        `Job status updated to ${data.status}`,
        "JobStatusUpdated",
        data.jobId
      );
    },
    [addNotification]
  );

  const connect = useCallback(async () => {
    if (!user?.id) return;

    try {
      // Register handlers before connecting
      signalRService.on(
        "NewJobAssigned",
        (data: Parameters<typeof handleNewJobAssigned>[0]) => {
          handleNewJobAssigned(data);
        }
      );
      signalRService.on("JobReassigned", handleJobReassigned);
      signalRService.on("JobCancelled", handleJobCancelled);
      signalRService.on("JobStatusUpdated", handleJobStatusUpdated);

      await signalRService.connect();
      await signalRService.joinGroup(`contractor-${user.id}`);

      reconnectCountRef.current = 0;
      setState({ isConnected: true, error: null });
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to connect";
      setState({ isConnected: false, error: errorMessage });

      // Attempt reconnection with exponential backoff
      if (reconnectCountRef.current < MAX_RECONNECT_ATTEMPTS) {
        const delay =
          RECONNECT_DELAYS[
            Math.min(reconnectCountRef.current, RECONNECT_DELAYS.length - 1)
          ];
        reconnectTimeoutRef.current = window.setTimeout(() => {
          reconnectCountRef.current += 1;
          connect();
        }, delay);
      }
    }
  }, [user?.id, handleNewJobAssigned, handleJobReassigned, handleJobCancelled, handleJobStatusUpdated]);

  const manualReconnect = useCallback(() => {
    reconnectCountRef.current = 0;
    if (reconnectTimeoutRef.current) {
      clearTimeout(reconnectTimeoutRef.current);
    }
    connect();
  }, [connect]);

  // Connect on mount and cleanup on unmount
  useEffect(() => {
    connect();

    return () => {
      if (reconnectTimeoutRef.current) {
        clearTimeout(reconnectTimeoutRef.current);
      }
      // Don't disconnect here as signalRService may be used by other components
    };
  }, [connect]);

  return {
    ...state,
    manualReconnect,
  };
};



