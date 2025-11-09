/**
 * NotificationContext
 * Global state for managing notifications for all user roles
 * Provides notification list, add, dismiss, and clear all functionality
 */

import React, {
  createContext,
  useReducer,
  useCallback,
  useEffect,
  useState,
} from "react";
import { Notification, NotificationType } from "@/types/NotificationMessages";
import { config } from "@/utils/config";

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

// Get storage key based on user role
const getStorageKey = (role?: string): string => {
  const roleKey = role?.toLowerCase() || "user";
  return `${roleKey}_notifications`;
};

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [notifications, dispatch] = useReducer(notificationReducer, []);
  const [storageKey, setStorageKey] = useState<string>("user_notifications");

  // Update storage key when user role changes
  useEffect(() => {
    const updateStorageKey = () => {
      const token = localStorage.getItem(config.auth.jwtStorageKey);
      if (token) {
        try {
          // Decode JWT to get role
          const payload = JSON.parse(atob(token.split(".")[1]));
          const role =
            payload.role ||
            payload[
              "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            ];
          setStorageKey(getStorageKey(role));
        } catch {
          // Fallback to default
          setStorageKey("user_notifications");
        }
      } else {
        setStorageKey("user_notifications");
      }
    };

    updateStorageKey();

    // Listen for storage changes (when user logs in/out)
    const handleStorageChange = () => {
      updateStorageKey();
    };

    window.addEventListener("storage", handleStorageChange);
    // Also check periodically for auth changes
    const interval = setInterval(updateStorageKey, 1000);

    return () => {
      window.removeEventListener("storage", handleStorageChange);
      clearInterval(interval);
    };
  }, []);

  // Load notifications from localStorage on mount or when storage key changes
  useEffect(() => {
    const stored = localStorage.getItem(storageKey);
    if (stored) {
      try {
        const parsed = JSON.parse(stored) as Notification[];
        dispatch({ type: "LOAD_FROM_STORAGE", payload: parsed });
      } catch (err) {
        console.error("Failed to load notifications from storage:", err);
      }
    } else {
      // Clear notifications if switching to a different role
      dispatch({ type: "CLEAR_ALL" });
    }
  }, [storageKey]);

  // Persist notifications to localStorage whenever they change
  useEffect(() => {
    localStorage.setItem(storageKey, JSON.stringify(notifications));
  }, [notifications, storageKey]);

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
