/**
 * NotificationBadge Component
 * Displays unread notification count in the header and opens NotificationCenter when clicked.
 */

import React from "react";
import { Bell } from "lucide-react";

export interface NotificationBadgeProps {
  unreadCount: number;
  onClick: () => void;
}

export const NotificationBadge: React.FC<NotificationBadgeProps> = ({
  unreadCount,
  onClick,
}) => {
  if (unreadCount === 0) {
    return (
      <button
        onClick={onClick}
        className="relative p-2 text-gray-600 hover:text-gray-900 transition-colors"
        aria-label="Notifications"
      >
        <Bell size={20} />
      </button>
    );
  }

  return (
    <button
      onClick={onClick}
      className="relative p-2 text-gray-600 hover:text-gray-900 transition-colors"
      aria-label={`Notifications - ${unreadCount} unread`}
    >
      <Bell size={20} />
      <span className="absolute top-1 right-1 inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-white transform translate-x-1/2 -translate-y-1/2 bg-red-600 rounded-full">
        {unreadCount > 99 ? "99+" : unreadCount}
      </span>
    </button>
  );
};
