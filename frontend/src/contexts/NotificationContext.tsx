/**
 * NotificationContext
 * Global state for managing contractor notifications
 * Provides notification list, add, dismiss, and clear all functionality
 */

import React, {
  createContext,
  useReducer,
  useCallback,
  useEffect,
} from "react";
import { Notification, NotificationType } from "@/types/NotificationMessages";

export interface NotificationContextType {
  notifications: Notification[];
  addNotification: (
    message: string,
    type: NotificationType,
    jobId?: string,
    title?: string
  ) => void;
  dismissNotification: (id: string) => void;
  clearAll: () => void;
}

export const NotificationContext = createContext<
  NotificationContextType | undefined
>(undefined);

type NotificationAction =
  | { type: "ADD"; payload: Notification }
  | { type: "DISMISS"; payload: string }
  | { type: "CLEAR_ALL" }
  | { type: "LOAD_FROM_STORAGE"; payload: Notification[] };

const notificationReducer = (
  state: Notification[],
  action: NotificationAction
): Notification[] => {
  switch (action.type) {
    case "ADD":
      // Keep max 20 notifications in memory
      return [action.payload, ...state].slice(0, 20);
    case "DISMISS":
      return state.filter((n) => n.id !== action.payload);
    case "CLEAR_ALL":
      return [];
    case "LOAD_FROM_STORAGE":
      return action.payload;
    default:
      return state;
  }
};

const STORAGE_KEY = "contractor_notifications";

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [notifications, dispatch] = useReducer(notificationReducer, []);

  // Load notifications from localStorage on mount
  useEffect(() => {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      try {
        const parsed = JSON.parse(stored) as Notification[];
        dispatch({ type: "LOAD_FROM_STORAGE", payload: parsed });
      } catch (err) {
        console.error("Failed to load notifications from storage:", err);
      }
    }
  }, []);

  // Persist notifications to localStorage whenever they change
  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(notifications));
  }, [notifications]);

  const addNotification = useCallback(
    (
      message: string,
      type: NotificationType,
      jobId?: string,
      title?: string
    ) => {
      const notification: Notification = {
        id: `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
        type,
        title: title || type,
        message,
        jobId,
        createdAt: new Date().toISOString(),
        isRead: false,
      };
      dispatch({ type: "ADD", payload: notification });
    },
    []
  );

  const dismissNotification = useCallback((id: string) => {
    dispatch({ type: "DISMISS", payload: id });
  }, []);

  const clearAll = useCallback(() => {
    dispatch({ type: "CLEAR_ALL" });
  }, []);

  const value: NotificationContextType = {
    notifications,
    addNotification,
    dismissNotification,
    clearAll,
  };

  return (
    <NotificationContext.Provider value={value}>
      {children}
    </NotificationContext.Provider>
  );
};
