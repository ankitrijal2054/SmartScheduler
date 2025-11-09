/**
 * DateTimePicker Component
 * Date and time picker for job desired date/time
 */

import React, { useMemo } from "react";

interface DateTimePickerProps {
  value: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  error?: string;
  disabled?: boolean;
}

export const DateTimePicker: React.FC<DateTimePickerProps> = ({
  value,
  onChange,
  error,
  disabled,
}) => {
  // Calculate minimum datetime (now + 1 hour) for validation
  const minDateTime = useMemo(() => {
    const now = new Date();
    // Add 1 hour buffer
    now.setHours(now.getHours() + 1);
    // Format as ISO string for datetime-local input
    return now.toISOString().slice(0, 16);
  }, []);

  return (
    <div className="space-y-2">
      <label
        htmlFor="desiredDateTime"
        className="block text-sm font-medium text-gray-700"
      >
        Desired Date & Time <span className="text-red-500">*</span>
      </label>
      <input
        id="desiredDateTime"
        type="datetime-local"
        name="desiredDateTime"
        value={value}
        onChange={onChange}
        disabled={disabled}
        min={minDateTime}
        aria-label="Select desired date and time"
        aria-describedby={error ? "desiredDateTime-error" : undefined}
        className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed ${
          error ? "border-red-500 focus:ring-red-500" : "border-gray-300"
        }`}
      />
      <p className="text-xs text-gray-500">
        Schedule at least 1 hour in advance
      </p>
      {error && (
        <p
          id="desiredDateTime-error"
          className="text-sm text-red-500 font-medium"
          role="alert"
        >
          {error}
        </p>
      )}
    </div>
  );
};
