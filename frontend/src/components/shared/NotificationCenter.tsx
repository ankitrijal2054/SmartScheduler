/**
 * NotificationCenter Component
 * Modal displaying list of recent notifications with dismiss and clear all actions
 * Works for all user roles (Dispatcher, Customer, Contractor)
 */

import React from "react";
import { X, Bell, Trash2 } from "lucide-react";
import { useNotifications } from "@/hooks/useNotifications";
import { Notification } from "@/types/NotificationMessages";
import { formatDistanceToNow } from "date-fns";
import { useNavigate } from "react-router-dom";
import { useAuthContext } from "@/hooks/useAuthContext";

interface NotificationCenterProps {
  isOpen: boolean;
  onClose: () => void;
}

const NOTIFICATION_TYPE_CONFIG: Record<
  string,
  { icon: string; color: string; label: string }
> = {
  // Contractor notifications
  NewJobAssigned: {
    icon: "🆕",
    color: "text-blue-600",
    label: "New Job Assigned",
  },
  JobReassigned: {
    icon: "🔄",
    color: "text-orange-600",
    label: "Job Reassigned",
  },
  JobCancelled: {
    icon: "❌",
    color: "text-red-600",
    label: "Job Cancelled",
  },
  JobStatusUpdated: {
    icon: "📝",
    color: "text-purple-600",
    label: "Job Status Updated",
  },
  ScheduleUpdated: {
    icon: "📅",
    color: "text-indigo-600",
    label: "Schedule Updated",
  },
  RatingReceived: {
    icon: "⭐",
    color: "text-yellow-600",
    label: "Rating Received",
  },
  // Dispatcher notifications
  NewJobCreated: {
    icon: "✨",
    color: "text-green-600",
    label: "New Job Created",
  },
  JobAssigned: {
    icon: "✅",
    color: "text-blue-600",
    label: "Job Assigned",
  },
  JobStatusChanged: {
    icon: "🔄",
    color: "text-purple-600",
    label: "Status Changed",
  },
  ContractorUnavailable: {
    icon: "⚠️",
    color: "text-orange-600",
    label: "Contractor Unavailable",
  },
  UrgentJobCreated: {
    icon: "🚨",
    color: "text-red-600",
    label: "Urgent Job",
  },
  // Customer notifications
  JobInProgress: {
    icon: "🔧",
    color: "text-blue-600",
    label: "Job In Progress",
  },
  JobCompleted: {
    icon: "✅",
    color: "text-green-600",
    label: "Job Completed",
  },
  ContractorAssigned: {
    icon: "👤",
    color: "text-indigo-600",
    label: "Contractor Assigned",
  },
};

export const NotificationCenter: React.FC<NotificationCenterProps> = ({
  isOpen,
  onClose,
}) => {
  const { notifications, dismissNotification, clearAll } = useNotifications();
  const navigate = useNavigate();
  const { user } = useAuthContext();

  const handleNotificationClick = (notification: Notification) => {
    if (notification.jobId) {
      // Navigate to job details based on user role
      if (user?.role === "Customer") {
        navigate(`/customer/jobs/${notification.jobId}`);
      } else if (user?.role === "Dispatcher") {
        // For dispatcher, could navigate to job details if implemented
        console.log("Navigate to job:", notification.jobId);
      }
      onClose();
    }
  };

  const unreadCount = notifications.filter((n) => !n.isRead && !n.read).length;

  if (!isOpen) return null;

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity"
        onClick={onClose}
        role="presentation"
      />

      {/* Modal */}
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div className="bg-white rounded-lg shadow-xl max-w-md w-full max-h-96 flex flex-col">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <div>
              <h2 className="text-lg font-semibold text-gray-900">
                Notifications
              </h2>
              {unreadCount > 0 && (
                <p className="text-sm text-gray-500 mt-1">
                  {unreadCount} unread
                </p>
              )}
            </div>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 transition"
              aria-label="Close"
            >
              <X size={20} />
            </button>
          </div>

          {/* Notifications List */}
          <div className="flex-1 overflow-y-auto">
            {notifications.length === 0 ? (
              <div className="p-8 text-center">
                <Bell size={40} className="mx-auto text-gray-300 mb-3" />
                <p className="text-gray-500 text-sm">No notifications yet</p>
              </div>
            ) : (
              <div className="divide-y divide-gray-200">
                {notifications.map((notification) => {
                  const config = NOTIFICATION_TYPE_CONFIG[
                    notification.type
                  ] || {
                    icon: "📬",
                    color: "text-gray-600",
                    label: "Notification",
                  };

                  const timeAgo = formatDistanceToNow(
                    new Date(notification.createdAt),
                    { addSuffix: true }
                  );

                  const isUnread = !notification.isRead && !notification.read;

                  return (
                    <div
                      key={notification.id}
                      onClick={() => handleNotificationClick(notification)}
                      className={`p-4 hover:bg-gray-50 transition flex items-start gap-3 cursor-pointer ${
                        isUnread ? "bg-blue-50" : ""
                      }`}
                    >
                      <span className="text-xl flex-shrink-0">
                        {config.icon}
                      </span>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2">
                          <p
                            className={`font-medium text-sm ${
                              isUnread ? "text-gray-900" : "text-gray-700"
                            }`}
                          >
                            {config.label}
                          </p>
                          {isUnread && (
                            <span className="w-2 h-2 bg-blue-600 rounded-full"></span>
                          )}
                        </div>
                        <p className="text-gray-600 text-sm mt-1">
                          {notification.message}
                        </p>
                        <p className="text-gray-400 text-xs mt-2">{timeAgo}</p>
                      </div>
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          dismissNotification(notification.id);
                        }}
                        className="p-1 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded transition flex-shrink-0"
                        aria-label="Dismiss notification"
                      >
                        <X size={16} />
                      </button>
                    </div>
                  );
                })}
              </div>
            )}
          </div>

          {/* Footer */}
          {notifications.length > 0 && (
            <div className="p-4 border-t border-gray-200 flex gap-2">
              <button
                onClick={clearAll}
                className="flex-1 flex items-center justify-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 rounded-lg transition"
              >
                <Trash2 size={16} />
                Clear All
              </button>
              <button
                onClick={onClose}
                className="flex-1 px-4 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-lg transition"
              >
                Close
              </button>
            </div>
          )}
        </div>
      </div>
    </>
  );
};
