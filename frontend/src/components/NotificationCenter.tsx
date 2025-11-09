/**
 * NotificationCenter Component
 * Modal/drawer showing notification history with the ability to mark as read and navigate to job details.
 */

import React from "react";
import { X } from "lucide-react";
import { Notification } from "@/types/NotificationMessages";
import { formatRelativeTime } from "@/utils/notificationFormatter";

export interface NotificationCenterProps {
  isOpen: boolean;
  notifications: Notification[];
  onClose: () => void;
  onNotificationClick: (notification: Notification) => void;
  onMarkAsRead: (notificationId: string) => void;
}

export const NotificationCenter: React.FC<NotificationCenterProps> = ({
  isOpen,
  notifications,
  onClose,
  onNotificationClick,
  onMarkAsRead,
}) => {
  if (!isOpen) return null;

  const unreadNotifications = notifications.filter(
    (n) => !n.isRead && n.read !== true
  );

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 transition-opacity"
        onClick={onClose}
      />

      {/* Modal */}
      <div className="relative z-10 flex items-center justify-center min-h-screen p-4">
        <div className="relative bg-white rounded-lg shadow-xl max-w-md w-full max-h-96 flex flex-col">
          {/* Header */}
          <div className="flex items-center justify-between p-4 border-b border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900">
              Notifications
            </h2>
            <button
              onClick={onClose}
              className="inline-flex items-center justify-center p-1 text-gray-500 hover:text-gray-700 rounded-md hover:bg-gray-100"
            >
              <X size={20} />
            </button>
          </div>

          {/* Notification List */}
          <div className="flex-1 overflow-y-auto">
            {notifications.length === 0 ? (
              <div className="flex items-center justify-center h-32 text-gray-500">
                <p>No notifications yet</p>
              </div>
            ) : (
              <ul className="divide-y divide-gray-200">
                {notifications.map((notification) => (
                  <li key={notification.id} className="hover:bg-gray-50">
                    <button
                      onClick={() => onNotificationClick(notification)}
                      className="w-full text-left p-4 transition-colors"
                    >
                      <div className="flex items-start justify-between">
                        <div className="flex-1">
                          <p className="font-medium text-gray-900">
                            {notification.title || notification.type}
                          </p>
                          <p className="text-sm text-gray-600 mt-1 line-clamp-2">
                            {notification.message}
                          </p>
                          <p className="text-xs text-gray-400 mt-2">
                            {formatRelativeTime(notification.createdAt)}
                          </p>
                        </div>
                        {!notification.isRead && notification.read !== true && (
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              onMarkAsRead(notification.id);
                            }}
                            className="ml-2 flex-shrink-0 inline-flex items-center justify-center w-2 h-2 bg-blue-600 rounded-full"
                            aria-label="Mark as read"
                          />
                        )}
                      </div>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          {/* Footer */}
          {unreadNotifications.length > 0 && (
            <div className="px-4 py-3 border-t border-gray-200 bg-gray-50 text-sm text-gray-600">
              {unreadNotifications.length} unread notification
              {unreadNotifications.length !== 1 ? "s" : ""}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
