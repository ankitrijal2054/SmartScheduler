/**
 * Utility functions to format notification messages.
 */

import { format } from "date-fns";

/**
 * Format a job assigned message.
 */
export function formatJobAssignedMessage(
  jobType: string,
  location: string,
  scheduledTime: string
): string {
  const formattedTime = formatTime(scheduledTime);
  return `New ${jobType} job at ${location} scheduled for ${formattedTime}`;
}

/**
 * Format a job reassigned message.
 */
export function formatReassignedMessage(reason?: string): string {
  if (reason) {
    return `Your assignment has been reassigned: ${reason}`;
  }
  return "Your assignment has been reassigned";
}

/**
 * Format a job cancelled message.
 */
export function formatCancelledMessage(reason?: string): string {
  if (reason) {
    return `Job cancelled: ${reason}`;
  }
  return "Job has been cancelled";
}

/**
 * Format a schedule updated message.
 */
export function formatScheduleUpdatedMessage(
  oldTime: string,
  newTime: string
): string {
  const oldFormatted = formatTime(oldTime);
  const newFormatted = formatTime(newTime);
  return `Your schedule has been updated from ${oldFormatted} to ${newFormatted}`;
}

/**
 * Format a time string to human-readable format.
 */
export function formatTime(dateString: string): string {
  try {
    const date = new Date(dateString);
    return format(date, "MMM d, yyyy 'at' h:mm a");
  } catch {
    return dateString;
  }
}

/**
 * Format a relative time (e.g., "2 hours ago").
 */
export function formatRelativeTime(dateString: string): string {
  try {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) {
      return "just now";
    } else if (diffMins < 60) {
      return `${diffMins} minute${diffMins > 1 ? "s" : ""} ago`;
    } else if (diffHours < 24) {
      return `${diffHours} hour${diffHours > 1 ? "s" : ""} ago`;
    } else if (diffDays < 7) {
      return `${diffDays} day${diffDays > 1 ? "s" : ""} ago`;
    }
    return format(date, "MMM d, yyyy");
  } catch {
    return dateString;
  }
}
