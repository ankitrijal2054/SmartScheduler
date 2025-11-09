/**
 * LocationAutocomplete Component
 * Text input for job location/address
 * (MVP version: simple text input; future: integrate Google Maps autocomplete)
 */

import React from "react";

interface LocationAutocompleteProps {
  value: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  error?: string;
  disabled?: boolean;
}

export const LocationAutocomplete: React.FC<LocationAutocompleteProps> = ({
  value,
  onChange,
  error,
  disabled,
}) => {
  return (
    <div className="space-y-2">
      <label
        htmlFor="location"
        className="block text-sm font-medium text-gray-700"
      >
        Location/Address <span className="text-red-500">*</span>
      </label>
      <input
        id="location"
        type="text"
        name="location"
        value={value}
        onChange={onChange}
        disabled={disabled}
        placeholder="e.g., 123 Main St, Springfield, IL"
        aria-label="Enter job location"
        aria-describedby={error ? "location-error" : undefined}
        className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed ${
          error ? "border-red-500 focus:ring-red-500" : "border-gray-300"
        }`}
      />
      {error && (
        <p
          id="location-error"
          className="text-sm text-red-500 font-medium"
          role="alert"
        >
          {error}
        </p>
      )}
    </div>
  );
};
