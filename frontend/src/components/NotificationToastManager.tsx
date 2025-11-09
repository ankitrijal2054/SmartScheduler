/**
 * NotificationToastManager Component
 * Automatically shows toast notifications when new notifications are added
 * Works for all user roles
 */

import React, { useEffect, useRef } from "react";
import { useNotifications } from "@/hooks/useNotifications";
import { useToast } from "@/components/shared/Toast";
import { ToastContainer } from "@/components/shared/Toast";

export const NotificationToastManager: React.FC = () => {
  const { notifications } = useNotifications();
  const { toasts, success, info, warning, removeToast } = useToast();
  const previousNotificationsRef = useRef<Set<string>>(new Set());
  const lastNotificationIdRef = useRef<string | null>(null);

  useEffect(() => {
    // Get the most recent notification
    const latestNotification = notifications[0];

    if (
      latestNotification &&
      latestNotification.id !== lastNotificationIdRef.current &&
      !previousNotificationsRef.current.has(latestNotification.id)
    ) {
      lastNotificationIdRef.current = latestNotification.id;
      previousNotificationsRef.current.add(latestNotification.id);

      // Show toast based on notification type
      const message = latestNotification.message;

      // Determine toast type based on notification type
      if (
        latestNotification.type === "JobCompleted" ||
        latestNotification.type === "NewJobAssigned" ||
        latestNotification.type === "ContractorAssigned"
      ) {
        success(message);
      } else if (
        latestNotification.type === "JobCancelled" ||
        latestNotification.type === "ContractorUnavailable"
      ) {
        warning(message);
      } else {
        info(message);
      }
    }

    // Update previous notifications set (keep only recent ones)
    if (previousNotificationsRef.current.size > 50) {
      const idsToKeep = new Set(notifications.slice(0, 20).map((n) => n.id));
      previousNotificationsRef.current = idsToKeep;
    }
  }, [notifications, success, info, warning]);

  return <ToastContainer toasts={toasts} onRemove={removeToast} />;
};
