/**
 * JobTypeSelect Component
 * Dropdown selector for job type (Flooring, HVAC, Plumbing, Electrical, Other)
 */

import React from "react";

interface JobTypeSelectProps {
  value: string;
  onChange: (e: React.ChangeEvent<HTMLSelectElement>) => void;
  error?: string;
  disabled?: boolean;
}

const JOB_TYPES = ["Flooring", "HVAC", "Plumbing", "Electrical", "Other"];

export const JobTypeSelect: React.FC<JobTypeSelectProps> = ({
  value,
  onChange,
  error,
  disabled,
}) => {
  return (
    <div className="space-y-2">
      <label
        htmlFor="jobType"
        className="block text-sm font-medium text-gray-700"
      >
        Job Type <span className="text-red-500">*</span>
      </label>
      <select
        id="jobType"
        name="jobType"
        value={value}
        onChange={onChange}
        disabled={disabled}
        aria-label="Select job type"
        aria-describedby={error ? "jobType-error" : undefined}
        className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed ${
          error ? "border-red-500 focus:ring-red-500" : "border-gray-300"
        }`}
      >
        <option value="">Select a job type...</option>
        {JOB_TYPES.map((type) => (
          <option key={type} value={type}>
            {type}
          </option>
        ))}
      </select>
      {error && (
        <p
          id="jobType-error"
          className="text-sm text-red-500 font-medium"
          role="alert"
        >
          {error}
        </p>
      )}
    </div>
  );
};
