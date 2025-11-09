/**
 * useJobSubmission Hook
 * Manages job submission form state, validation, and API integration
 */

import { useState, useCallback } from "react";
import { customerService } from "@/services/customerService";
import {
  JobSubmissionFormData,
  CreateJobRequest,
  FormValidationErrors,
  JobSubmissionState,
} from "@/types/Customer";
import { JobType } from "@/types/Job";

const initialFormData: JobSubmissionFormData = {
  jobType: "",
  location: "",
  description: "",
  desiredDateTime: "",
};

const initialState: JobSubmissionState = {
  formData: initialFormData,
  loading: false,
  error: null,
  success: false,
  submittedJobId: null,
  validationErrors: {},
};

/**
 * Custom hook for managing job submission form
 * @returns Object with form state, handlers, and submission logic
 */
export const useJobSubmission = () => {
  const [state, setState] = useState<JobSubmissionState>(initialState);

  /**
   * Validate a single field
   */
  const validateField = useCallback(
    (
      fieldName: keyof JobSubmissionFormData,
      value: string
    ): string | undefined => {
      switch (fieldName) {
        case "jobType":
          if (!value) {
            return "Please select a job type";
          }
          return undefined;

        case "location":
          if (!value || value.trim().length === 0) {
            return "Location is required";
          }
          if (value.trim().length < 5) {
            return "Location must be at least 5 characters";
          }
          return undefined;

        case "desiredDateTime":
          if (!value) {
            return "Please select a date and time";
          }
          const selectedDate = new Date(value);
          const now = new Date();
          if (selectedDate <= now) {
            return "Date and time must be in the future";
          }
          return undefined;

        case "description":
          if (value && value.length > 1000) {
            return "Description cannot exceed 1000 characters";
          }
          return undefined;

        default:
          return undefined;
      }
    },
    []
  );

  /**
   * Validate all form fields
   */
  const validateForm = useCallback((): FormValidationErrors => {
    const errors: FormValidationErrors = {};

    const jobTypeError = validateField("jobType", state.formData.jobType);
    if (jobTypeError) errors.jobType = jobTypeError;

    const locationError = validateField("location", state.formData.location);
    if (locationError) errors.location = locationError;

    const dateTimeError = validateField(
      "desiredDateTime",
      state.formData.desiredDateTime
    );
    if (dateTimeError) errors.desiredDateTime = dateTimeError;

    const descriptionError = validateField(
      "description",
      state.formData.description
    );
    if (descriptionError) errors.description = descriptionError;

    return errors;
  }, [state.formData, validateField]);

  /**
   * Handle form field changes
   */
  const handleChange = useCallback(
    (
      e: React.ChangeEvent<
        HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
      >
    ) => {
      const { name, value } = e.target;
      const fieldName = name as keyof JobSubmissionFormData;

      setState((prev) => ({
        ...prev,
        formData: {
          ...prev.formData,
          [fieldName]: value,
        },
        // Clear field error when user starts typing
        validationErrors: {
          ...prev.validationErrors,
          [fieldName]: undefined,
        },
      }));
    },
    []
  );

  /**
   * Handle form submission
   */
  const handleSubmit = useCallback(
    async (e: React.FormEvent<HTMLFormElement>) => {
      e.preventDefault();

      // Validate form before submission
      const errors = validateForm();
      if (Object.keys(errors).length > 0) {
        setState((prev) => ({
          ...prev,
          validationErrors: errors,
          error: "Please correct the errors above",
        }));
        return;
      }

      setState((prev) => ({
        ...prev,
        loading: true,
        error: null,
      }));

      try {
        const request: CreateJobRequest = {
          jobType: state.formData.jobType as JobType,
          location: state.formData.location,
          description: state.formData.description,
          desiredDateTime: state.formData.desiredDateTime,
        };

        const response = await customerService.submitJob(request);

        setState((prev) => ({
          ...prev,
          loading: false,
          success: true,
          submittedJobId: response.id,
          formData: initialFormData,
          validationErrors: {},
          error: null,
        }));
      } catch (err) {
        const errorMessage =
          err instanceof Error
            ? err.message
            : "Failed to submit job. Please try again.";

        setState((prev) => ({
          ...prev,
          loading: false,
          error: errorMessage,
        }));
      }
    },
    [state.formData, validateForm]
  );

  /**
   * Reset form to initial state
   */
  const resetForm = useCallback(() => {
    setState(initialState);
  }, []);

  /**
   * Clear error message
   */
  const clearError = useCallback(() => {
    setState((prev) => ({
      ...prev,
      error: null,
    }));
  }, []);

  /**
   * Clear success state
   */
  const clearSuccess = useCallback(() => {
    setState((prev) => ({
      ...prev,
      success: false,
      submittedJobId: null,
    }));
  }, []);

  return {
    formData: state.formData,
    loading: state.loading,
    error: state.error,
    success: state.success,
    submittedJobId: state.submittedJobId,
    validationErrors: state.validationErrors,
    handleChange,
    handleSubmit,
    resetForm,
    clearError,
    clearSuccess,
  };
};
