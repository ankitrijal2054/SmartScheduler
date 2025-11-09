/**
 * JobSubmissionForm Component
 * Main form for customer job submission
 * Integrates all form field components and handles submission flow
 */

import React, { useEffect } from "react";
import { useJobSubmission } from "@/hooks/useJobSubmission";
import { JobTypeSelect } from "@/components/forms/JobTypeSelect";
import { LocationAutocomplete } from "@/components/forms/LocationAutocomplete";
import { DateTimePicker } from "@/components/forms/DateTimePicker";
import { DescriptionTextArea } from "@/components/forms/DescriptionTextArea";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { ToastContainer, useToast } from "@/components/shared/Toast";

interface JobSubmissionFormProps {
  onSuccess?: (jobId: string) => void;
}

export const JobSubmissionForm: React.FC<JobSubmissionFormProps> = ({
  onSuccess,
}) => {
  const {
    toasts,
    success: showSuccessToast,
    error: showErrorToast,
    removeToast,
  } = useToast();
  const {
    formData,
    loading,
    error,
    success,
    submittedJobId,
    validationErrors,
    handleChange,
    handleSubmit,
    clearError,
    clearSuccess,
  } = useJobSubmission();

  // Show success toast and call callback when form submission succeeds
  useEffect(() => {
    if (success && submittedJobId) {
      showSuccessToast("Job submitted! We're finding contractors now.");
      if (onSuccess) {
        const timer = setTimeout(() => {
          onSuccess(submittedJobId);
        }, 2000); // Wait 2 seconds for user to see success message
        return () => clearTimeout(timer);
      }
    }
  }, [success, submittedJobId]); // Only depend on submission state, not callbacks

  // Show error toast when submission fails
  useEffect(() => {
    if (error) {
      showErrorToast(error);
    }
  }, [error]); // Only depend on error state

  // Check if all required fields are filled for submit button state
  const isFormValid = !!(
    formData.jobType &&
    formData.location.trim() &&
    formData.desiredDateTime
  );

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-2xl mx-auto">
        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="text-3xl md:text-4xl font-bold text-gray-900 mb-2">
            Submit a Job
          </h1>
          <p className="text-gray-600">
            Tell us what you need and we'll connect you with qualified
            contractors
          </p>
        </div>

        {/* Form Card */}
        <div className="bg-white rounded-lg shadow-lg p-6 md:p-8">
          <form onSubmit={handleSubmit} noValidate className="space-y-6">
            {/* Job Type Select */}
            <JobTypeSelect
              value={formData.jobType}
              onChange={handleChange}
              error={validationErrors.jobType}
              disabled={loading}
            />

            {/* Location Input */}
            <LocationAutocomplete
              value={formData.location}
              onChange={handleChange}
              error={validationErrors.location}
              disabled={loading}
            />

            {/* DateTime Picker */}
            <DateTimePicker
              value={formData.desiredDateTime}
              onChange={handleChange}
              error={validationErrors.desiredDateTime}
              disabled={loading}
            />

            {/* Description TextArea */}
            <DescriptionTextArea
              value={formData.description}
              onChange={handleChange}
              error={validationErrors.description}
              disabled={loading}
            />

            {/* Submit Button */}
            <div className="pt-2">
              <button
                type="submit"
                disabled={loading || !isFormValid}
                className={`w-full py-3 px-4 rounded-lg font-semibold text-white transition-colors ${
                  loading || !isFormValid
                    ? "bg-gray-400 cursor-not-allowed"
                    : "bg-blue-600 hover:bg-blue-700 active:bg-blue-800"
                }`}
              >
                {loading ? "Submitting..." : "Submit Job"}
              </button>
            </div>

            {/* Form-level Error */}
            {error && (
              <div className="p-3 bg-red-50 border border-red-200 rounded-md">
                <p className="text-sm text-red-700">{error}</p>
              </div>
            )}
          </form>

          {/* Loading Overlay */}
          {loading && (
            <div className="absolute inset-0 bg-black bg-opacity-10 rounded-lg flex items-center justify-center">
              <LoadingSpinner />
            </div>
          )}
        </div>
      </div>

      {/* Toast Container */}
      <ToastContainer toasts={toasts} onRemove={removeToast} />
    </div>
  );
};
