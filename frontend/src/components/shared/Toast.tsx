/**
 * Toast Component
 * Notification system for displaying success and error messages
 */

import React, { useEffect, useState } from "react";

export type ToastType = "success" | "error" | "info" | "warning";

interface ToastProps {
  id: string;
  message: string;
  type: ToastType;
  onClose: (id: string) => void;
  duration?: number; // milliseconds, 0 = never auto-close
}

/**
 * Individual Toast notification
 */
export const Toast: React.FC<ToastProps> = ({
  id,
  message,
  type,
  onClose,
  duration = 3000,
}) => {
  useEffect(() => {
    if (duration > 0) {
      const timer = setTimeout(() => {
        onClose(id);
      }, duration);
      return () => clearTimeout(timer);
    }
  }, [id, onClose, duration]);

  const bgColor = {
    success: "bg-green-500",
    error: "bg-red-500",
    info: "bg-blue-500",
    warning: "bg-yellow-500",
  }[type];

  const icon = {
    success: "✓",
    error: "✕",
    info: "ℹ",
    warning: "⚠",
  }[type];

  return (
    <div
      className={`${bgColor} pointer-events-auto flex gap-3 rounded-lg px-4 py-3 text-white shadow-lg transition-all animate-in slide-in-from-bottom-2 duration-200`}
      role="alert"
      aria-live="polite"
      aria-atomic="true"
    >
      <span className="font-bold text-lg">{icon}</span>
      <p className="flex-1 text-sm font-medium">{message}</p>
      <button
        onClick={() => onClose(id)}
        className="text-white hover:opacity-75"
        aria-label="Close notification"
        type="button"
      >
        ✕
      </button>
    </div>
  );
};

interface ToastContainerProps {
  toasts: Array<{
    id: string;
    message: string;
    type: ToastType;
  }>;
  onRemove: (id: string) => void;
}

/**
 * ToastContainer - manages multiple toasts
 */
export const ToastContainer: React.FC<ToastContainerProps> = ({
  toasts,
  onRemove,
}) => {
  return (
    <div
      className="fixed bottom-4 right-4 z-[9999] flex flex-col gap-2 pointer-events-none"
      role="region"
      aria-label="Notifications"
      aria-live="polite"
    >
      {toasts.map((toast) => (
        <div key={toast.id} className="pointer-events-auto">
          <Toast
            id={toast.id}
            message={toast.message}
            type={toast.type}
            onClose={onRemove}
            duration={toast.type === "error" ? 5000 : 3000}
          />
        </div>
      ))}
    </div>
  );
};

/**
 * useToast hook for managing toasts
 */
export const useToast = () => {
  const [toasts, setToasts] = useState<
    Array<{
      id: string;
      message: string;
      type: ToastType;
    }>
  >([]);

  const addToast = (message: string, type: ToastType = "info") => {
    const id = `${Date.now()}-${Math.random()}`;
    setToasts((prev) => [...prev, { id, message, type }]);
    return id;
  };

  const removeToast = (id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  };

  const success = (message: string) => addToast(message, "success");
  const error = (message: string) => addToast(message, "error");
  const info = (message: string) => addToast(message, "info");
  const warning = (message: string) => addToast(message, "warning");

  return {
    toasts,
    addToast,
    removeToast,
    success,
    error,
    info,
    warning,
  };
};
