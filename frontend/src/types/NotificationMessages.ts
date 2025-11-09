/**
 * Notification Messages Type Definitions
 * Prepared for Epic 6 integration (email and SignalR notifications)
 *
 * NOTE: This file documents the notification messages that should be sent
 * during job reassignment. The actual notification integration (SignalR +
 * Email) is implemented in Epic 6 Stories 6.3-6.6.
 */

/**
 * Notification message templates for job reassignment
 * These are scaffolded here to document requirements
 */
export const REASSIGNMENT_NOTIFICATIONS = {
  /**
   * Notification for old/previous contractor
   * Sent when their assignment is cancelled due to reassignment
   *
   * Expected fields:
   * - jobId: string
   * - jobType: string
   * - newContractorName: string
   * - customerName: string
   *
   * Subject: "Your assignment for [Job Type] has been reassigned"
   * Body: "Unfortunately, your assignment for [Job Type] at [Location] has been
   *        reassigned to [New Contractor Name]. A new contractor,
   *        [New Contractor Name], will now handle this job."
   */
  OLD_CONTRACTOR_REASSIGNMENT: "old_contractor_reassignment",

  /**
   * Notification for new contractor
   * Sent when they are assigned to a job that was previously assigned to another contractor
   *
   * Expected fields:
   * - jobId: string
   * - jobType: string
   * - location: string
   * - desiredDateTime: string
   * - customerName: string
   * - reasonForReassignment?: string
   *
   * Subject: "New job assignment: [Job Type]"
   * Body: "You've been assigned a new job: [Job Type] at [Location] on
   *        [Date/Time]. This replaces a previous assignment for this job."
   */
  NEW_CONTRACTOR_ASSIGNMENT: "new_contractor_assignment",

  /**
   * Notification for customer
   * Sent when their job is reassigned to a different contractor
   *
   * Expected fields:
   * - jobId: string
   * - newContractorName: string
   * - previousContractorName: string
   *
   * Subject: "Your job has been reassigned to [New Contractor Name]"
   * Body: "Your job has been reassigned from [Previous Contractor Name] to
   *        [New Contractor Name]. You will receive an update once the new
   *        contractor begins work."
   */
  CUSTOMER_REASSIGNMENT_NOTIFICATION: "customer_reassignment_notification",
};

/**
 * Integration notes for Epic 6:
 *
 * 1. **Old Contractor Notification** (Epic 6, Story 6.3-6.4)
 *    - Trigger: When assignment status changes to "Cancelled" with reason "Reassigned"
 *    - Channels: SignalR (real-time) + Email
 *    - Priority: High (contractor needs to know immediately)
 *    - Implementation: Backend emits event on reassignment, frontend receives via SignalR
 *
 * 2. **New Contractor Notification** (Epic 6, Story 6.3-6.4)
 *    - Trigger: When new assignment is created
 *    - Channels: SignalR (real-time) + Email
 *    - Priority: High (contractor needs job details immediately)
 *    - Implementation: Same as initial assignment but with reassignment context
 *
 * 3. **Customer Notification** (Epic 6, Story 6.5-6.6)
 *    - Trigger: When old assignment is cancelled and new assignment is created
 *    - Channels: SignalR (real-time) + Email
 *    - Priority: Medium (customer transparency)
 *    - Implementation: Backend queues notification event for customer
 *
 * Frontend responsibilities:
 * - Display success toast with contractor name after reassignment
 * - Subscribe to NotificationContext for real-time updates
 * - Display in-app notification badges/banners when contractors/customers receive notifications
 * - Log reassignment events for analytics
 *
 * Backend responsibilities:
 * - Emit reassignment events (old contractor cancelled, new contractor assigned)
 * - Queue email notifications via SES/SNS
 * - Broadcast SignalR messages to affected parties
 * - Persist notification history for audit trail
 */

export type NotificationMessageType =
  | typeof REASSIGNMENT_NOTIFICATIONS.OLD_CONTRACTOR_REASSIGNMENT
  | typeof REASSIGNMENT_NOTIFICATIONS.NEW_CONTRACTOR_ASSIGNMENT
  | typeof REASSIGNMENT_NOTIFICATIONS.CUSTOMER_REASSIGNMENT_NOTIFICATION;

/**
 * Contractor Notification Types (Story 5.1, Story 5.5)
 * Used for contractor portal real-time notifications
 */
export type ContractorNotificationType =
  | "NewJobAssigned"
  | "JobReassigned"
  | "JobCancelled"
  | "JobStatusUpdated"
  | "ScheduleUpdated"
  | "RatingReceived";

/**
 * Dispatcher Notification Types
 * Used for dispatcher dashboard real-time notifications
 */
export type DispatcherNotificationType =
  | "NewJobCreated"
  | "JobAssigned"
  | "JobReassigned"
  | "JobStatusChanged"
  | "ContractorUnavailable"
  | "UrgentJobCreated";

/**
 * Customer Notification Types
 * Used for customer dashboard real-time notifications
 */
export type CustomerNotificationType =
  | "JobAssigned"
  | "JobInProgress"
  | "JobCompleted"
  | "JobCancelled"
  | "ContractorAssigned"
  | "JobReassigned"
  | "ScheduleUpdated";

/**
 * Union type for all notification types
 */
export type NotificationType =
  | ContractorNotificationType
  | DispatcherNotificationType
  | CustomerNotificationType;

export interface Notification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  jobId?: string;
  data?: Record<string, any>; // Additional context (location, time, etc.)
  createdAt: string; // ISO 8601 datetime
  isRead?: boolean;
  read?: boolean; // Legacy field for backward compatibility
  expiresAt?: string; // Optional expiration
}
