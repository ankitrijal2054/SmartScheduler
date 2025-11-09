/**
 * useSignalR Hook
 * Custom hook for subscribing to real-time job status updates via SignalR
 */

import { useEffect, useRef, useCallback, useState } from "react";
import {
  signalRService,
  JobStatusUpdateEvent,
} from "@/services/signalRService";
import { config } from "@/utils/config";

interface UseSignalROptions {
  onJobStatusUpdate?: (event: JobStatusUpdateEvent) => void;
  onConnected?: () => void;
  onDisconnected?: () => void;
  onError?: (error: Error) => void;
}

interface UseSignalRReturn {
  isConnected: boolean;
  subscribe: (
    eventName: string,
    callback: (event: JobStatusUpdateEvent) => void
  ) => () => void;
  disconnect: () => Promise<void>;
}

export const useSignalR = (
  options: UseSignalROptions = {}
): UseSignalRReturn => {
  const [isConnected, setIsConnected] = useState(false);
  const unsubscribeRef = useRef<(() => void) | null>(null);

  /**
   * Handle job status updates
   */
  const handleJobStatusUpdate = useCallback(
    (event: JobStatusUpdateEvent) => {
      if (options.onJobStatusUpdate) {
        options.onJobStatusUpdate(event);
      }
    },
    [options.onJobStatusUpdate]
  );

  /**
   * Subscribe to real-time job status updates
   */
  const subscribe = useCallback(
    (eventName: string, callback: (event: JobStatusUpdateEvent) => void) => {
      return signalRService.subscribe(eventName, callback);
    },
    []
  );

  /**
   * Disconnect from SignalR
   */
  const disconnect = useCallback(async () => {
    if (unsubscribeRef.current) {
      unsubscribeRef.current();
      unsubscribeRef.current = null;
    }
    await signalRService.disconnect();
    setIsConnected(false);
  }, []);

  /**
   * Initialize SignalR connection on mount
   */
  useEffect(() => {
    const initializeConnection = async () => {
      try {
        // Connect to SignalR hub
        await signalRService.connect({
          url: `${config.api.baseUrl}/hubs/jobs`,
          reconnectInterval: 5000,
          maxRetries: 5,
        });

        setIsConnected(true);

        // Subscribe to job status updates
        unsubscribeRef.current = signalRService.subscribe(
          "JobStatusUpdated",
          handleJobStatusUpdate
        );

        // Call connected callback
        if (options.onConnected) {
          options.onConnected();
        }
      } catch (error) {
        console.error("SignalR connection error:", error);
        setIsConnected(false);
        if (options.onError && error instanceof Error) {
          options.onError(error);
        }
      }

      // Listen for connection state changes
      const handleConnected = () => {
        setIsConnected(true);
        if (options.onConnected) {
          options.onConnected();
        }
      };

      const handleDisconnected = () => {
        setIsConnected(false);
        if (options.onDisconnected) {
          options.onDisconnected();
        }
      };

      const handleError = (error: Error) => {
        if (options.onError) {
          options.onError(error);
        }
      };

      signalRService.on("connected", handleConnected);
      signalRService.on("disconnected", handleDisconnected);
      signalRService.on("error", handleError);

      return () => {
        signalRService.off("connected", handleConnected);
        signalRService.off("disconnected", handleDisconnected);
        signalRService.off("error", handleError);
      };
    };

    initializeConnection();
  }, [
    handleJobStatusUpdate,
    options.onConnected,
    options.onDisconnected,
    options.onError,
  ]);

  /**
   * Cleanup on unmount
   */
  useEffect(() => {
    return () => {
      if (unsubscribeRef.current) {
        unsubscribeRef.current();
      }
    };
  }, []);

  return {
    isConnected,
    subscribe,
    disconnect,
  };
};
