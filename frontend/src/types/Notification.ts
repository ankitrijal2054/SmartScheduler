/**
 * Notification type definitions for real-time job notifications.
 */

export type NotificationType =
  | "JobAssigned"
  | "JobReassigned"
  | "JobCancelled"
  | "ScheduleUpdated"
  | "RatingReceived";

/**
 * Represents a notification object.
 */
export interface Notification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  jobId?: string;
  data: Record<string, any>; // Flexible payload (jobType, location, time, etc.)
  isRead: boolean;
  createdAt: string; // ISO 8601
  expiresAt?: string; // Optional expiration
}

/**
 * Props for NotificationBadge component.
 */
export interface NotificationBadgeProps {
  unreadCount: number;
  onClick: () => void; // Opens NotificationCenter
  hasSound: boolean;
}

/**
 * Props for NotificationCenter modal/drawer component.
 */
export interface NotificationCenterProps {
  isOpen: boolean;
  notifications: Notification[];
  onClose: () => void;
  onNotificationClick: (notification: Notification) => void;
  onMarkAsRead: (notificationId: string) => void;
}

/**
 * Props for NotificationToast component.
 */
export interface NotificationToastProps {
  notification: Notification;
  onDismiss: () => void;
  onClickAction: () => void; // Navigate to job details modal
}
