/**
 * NotificationToast Component
 * Temporary toast notification that auto-dismisses after 5 seconds.
 * Can be clicked to trigger action (e.g., open job details modal).
 */

import React, { useEffect, useState } from "react";
import { X } from "lucide-react";
import { Notification } from "@/types/NotificationMessages";

export interface NotificationToastProps {
  notification: Notification;
  onDismiss: () => void;
  onClickAction: () => void;
}

const AUTO_DISMISS_DELAY = 5000; // 5 seconds

export const NotificationToast: React.FC<NotificationToastProps> = ({
  notification,
  onDismiss,
  onClickAction,
}) => {
  const [isExiting, setIsExiting] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => {
      setIsExiting(true);
      setTimeout(onDismiss, 300); // Allow animation to complete
    }, AUTO_DISMISS_DELAY);

    return () => clearTimeout(timer);
  }, [onDismiss]);

  const handleDismiss = () => {
    setIsExiting(true);
    setTimeout(onDismiss, 300);
  };

  const getNotificationColor = () => {
    switch (notification.type) {
      case "NewJobAssigned":
        return "bg-green-50 border-green-200";
      case "JobReassigned":
      case "JobCancelled":
        return "bg-orange-50 border-orange-200";
      case "ScheduleUpdated":
        return "bg-blue-50 border-blue-200";
      case "RatingReceived":
        return "bg-purple-50 border-purple-200";
      default:
        return "bg-gray-50 border-gray-200";
    }
  };

  const getTitleColor = () => {
    switch (notification.type) {
      case "NewJobAssigned":
        return "text-green-900";
      case "JobReassigned":
      case "JobCancelled":
        return "text-orange-900";
      case "ScheduleUpdated":
        return "text-blue-900";
      case "RatingReceived":
        return "text-purple-900";
      default:
        return "text-gray-900";
    }
  };

  return (
    <div
      className={`
        transform transition-all duration-300 ease-in-out
        ${
          isExiting ? "translate-x-full opacity-0" : "translate-x-0 opacity-100"
        }
      `}
    >
      <div
        className={`
          rounded-lg border-l-4 p-4 shadow-lg cursor-pointer
          ${getNotificationColor()}
          hover:shadow-xl hover:scale-102 transition-all
        `}
        onClick={onClickAction}
      >
        <div className="flex items-start justify-between">
          <div className="flex-1">
            <h3 className={`font-semibold ${getTitleColor()}`}>
              {notification.title || notification.type}
            </h3>
            <p className="text-sm text-gray-700 mt-1">{notification.message}</p>
          </div>
          <button
            onClick={(e) => {
              e.stopPropagation();
              handleDismiss();
            }}
            className="ml-2 flex-shrink-0 inline-flex text-gray-400 hover:text-gray-600"
          >
            <X size={18} />
          </button>
        </div>
      </div>
    </div>
  );
};
