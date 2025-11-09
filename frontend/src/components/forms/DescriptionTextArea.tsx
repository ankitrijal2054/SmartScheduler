/**
 * DescriptionTextArea Component
 * Multi-line text area for job description
 */

import React from "react";

interface DescriptionTextAreaProps {
  value: string;
  onChange: (e: React.ChangeEvent<HTMLTextAreaElement>) => void;
  error?: string;
  disabled?: boolean;
}

const MAX_DESCRIPTION_LENGTH = 1000;

export const DescriptionTextArea: React.FC<DescriptionTextAreaProps> = ({
  value,
  onChange,
  error,
  disabled,
}) => {
  const charCount = value.length;
  const isNearLimit = charCount > MAX_DESCRIPTION_LENGTH * 0.8;

  return (
    <div className="space-y-2">
      <label
        htmlFor="description"
        className="block text-sm font-medium text-gray-700"
      >
        Job Description{" "}
        <span className="text-gray-500 font-normal">(Optional)</span>
      </label>
      <textarea
        id="description"
        name="description"
        value={value}
        onChange={onChange}
        disabled={disabled}
        placeholder="e.g., 3-room hardwood flooring installation, existing carpet removal needed"
        maxLength={MAX_DESCRIPTION_LENGTH}
        rows={4}
        aria-label="Enter job description"
        aria-describedby={error ? "description-error" : "description-hint"}
        className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed resize-none ${
          error ? "border-red-500 focus:ring-red-500" : "border-gray-300"
        }`}
      />
      <div className="flex justify-between items-center">
        <p id="description-hint" className="text-xs text-gray-500">
          Provide details about the job to help contractors understand what's
          needed
        </p>
        <p
          className={`text-xs font-medium ${
            isNearLimit ? "text-orange-600" : "text-gray-500"
          }`}
          role="status"
          aria-live="polite"
        >
          {charCount}/{MAX_DESCRIPTION_LENGTH}
        </p>
      </div>
      {error && (
        <p
          id="description-error"
          className="text-sm text-red-500 font-medium"
          role="alert"
        >
          {error}
        </p>
      )}
    </div>
  );
};
