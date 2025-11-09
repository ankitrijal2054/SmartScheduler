import React, { useState, useMemo } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "@/hooks/useAuthContext";
import { UserRole } from "@/types/Auth";
import { SignupRequest } from "@/services/authService";

/**
 * Sign-Up Page Component
 * Allows new users to create an account with a role and profile information
 */
export const SignupPage: React.FC = () => {
  const navigate = useNavigate();
  const { signup, isLoading, error } = useAuth();

  const [formData, setFormData] = useState({
    email: "",
    password: "",
    confirmPassword: "",
    role: "" as UserRole | "",
    name: "",
    phoneNumber: "",
    location: "",
    tradeType: "" as
      | "Flooring"
      | "HVAC"
      | "Plumbing"
      | "Electrical"
      | "Other"
      | "",
    workingHoursStart: "",
    workingHoursEnd: "",
    termsAccepted: false,
  });
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});

  const roleOptions = [
    {
      value: "Dispatcher" as UserRole,
      label: "Dispatcher",
      description: "Manage contractors and assign jobs",
    },
    {
      value: "Customer" as UserRole,
      label: "Customer",
      description: "Submit jobs and track progress",
    },
    {
      value: "Contractor" as UserRole,
      label: "Contractor",
      description: "Accept jobs and build your rating",
    },
  ];

  const tradeTypeOptions = [
    { value: "Flooring", label: "Flooring" },
    { value: "HVAC", label: "HVAC" },
    { value: "Plumbing", label: "Plumbing" },
    { value: "Electrical", label: "Electrical" },
    { value: "Other", label: "Other" },
  ];

  const passwordStrength = useMemo(() => {
    const pwd = formData.password;
    let strength = 0;
    if (pwd.length >= 8) strength++;
    if (/[A-Z]/.test(pwd)) strength++;
    if (/[0-9]/.test(pwd)) strength++;
    if (/[^A-Za-z0-9]/.test(pwd)) strength++;

    return {
      score: strength,
      label:
        strength === 0
          ? ""
          : strength <= 1
          ? "Weak"
          : strength <= 2
          ? "Fair"
          : "Strong",
      color:
        strength === 0
          ? "bg-gray-200"
          : strength <= 1
          ? "bg-red-400"
          : strength <= 2
          ? "bg-yellow-400"
          : "bg-green-400",
    };
  }, [formData.password]);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;

    setFormData((prev) => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));

    // Clear error for this field on change
    setFormErrors((prev) => ({
      ...prev,
      [name]: "",
    }));
  };

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};

    if (!formData.email.trim()) {
      errors.email = "Email is required";
    } else {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(formData.email)) {
        errors.email = "Email must be a valid email address";
      }
    }

    if (!formData.password) {
      errors.password = "Password is required";
    } else if (formData.password.length < 8) {
      errors.password = "Password must be at least 8 characters";
    }

    if (!formData.confirmPassword) {
      errors.confirmPassword = "Confirm password is required";
    } else if (formData.password !== formData.confirmPassword) {
      errors.confirmPassword = "Passwords do not match";
    }

    if (!formData.role) {
      errors.role = "Please select a role";
    }

    if (!formData.name.trim()) {
      errors.name = "Name is required";
    } else if (formData.name.trim().length < 2) {
      errors.name = "Name must be at least 2 characters";
    }

    // Contractor-specific validations
    if (formData.role === "Contractor") {
      if (!formData.location.trim()) {
        errors.location = "Location is required for contractors";
      }
      if (!formData.tradeType) {
        errors.tradeType = "Trade type is required";
      }
      if (!formData.workingHoursStart) {
        errors.workingHoursStart = "Working hours start time is required";
      }
      if (!formData.workingHoursEnd) {
        errors.workingHoursEnd = "Working hours end time is required";
      }
      if (formData.workingHoursStart && formData.workingHoursEnd) {
        const start = formData.workingHoursStart.split(":").map(Number);
        const end = formData.workingHoursEnd.split(":").map(Number);
        const startTime = start[0] * 60 + start[1];
        const endTime = end[0] * 60 + end[1];
        if (endTime <= startTime) {
          errors.workingHoursEnd = "End time must be after start time";
        }
      }
    }

    if (!formData.termsAccepted) {
      errors.termsAccepted = "You must agree to the terms of service";
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (!validateForm()) return;

    try {
      const signupRequest: SignupRequest = {
        email: formData.email,
        password: formData.password,
        role: formData.role as "Dispatcher" | "Customer" | "Contractor",
        name: formData.name.trim(),
        phoneNumber: formData.phoneNumber.trim() || undefined,
        location: formData.location.trim() || undefined,
        tradeType: formData.tradeType || undefined,
        workingHoursStart: formData.workingHoursStart || undefined,
        workingHoursEnd: formData.workingHoursEnd || undefined,
      };

      await signup(signupRequest);
      // Signup successful - redirect to dashboard (handled by auth context)
      navigate("/");
    } catch (err) {
      console.error("Signup failed:", err);
    }
  };

  const isContractor = formData.role === "Contractor";

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 to-blue-100 flex items-center justify-center p-4 py-8">
      <div className="w-full max-w-md">
        {/* Card Container */}
        <div className="bg-white rounded-lg shadow-xl p-8">
          {/* Logo & Branding */}
          <div className="text-center mb-8">
            <div className="inline-flex items-center justify-center w-12 h-12 bg-indigo-600 rounded-lg mb-4">
              <span className="text-white text-xl font-bold">SS</span>
            </div>
            <h1 className="text-3xl font-bold text-gray-900">SmartScheduler</h1>
            <p className="text-gray-500 text-sm mt-2">Create your account</p>
          </div>

          {/* API Error Message */}
          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-red-700 text-sm">{error}</p>
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-5">
            {/* Email Field */}
            <div>
              <label
                htmlFor="email"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Email Address
              </label>
              <input
                type="email"
                id="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                disabled={isLoading}
                placeholder="you@example.com"
                className={`w-full px-4 py-2 border rounded-lg outline-none transition disabled:bg-gray-100 disabled:cursor-not-allowed ${
                  formErrors.email
                    ? "border-red-500 focus:ring-2 focus:ring-red-500"
                    : "border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                }`}
              />
              {formErrors.email && (
                <p className="text-red-600 text-xs mt-1">{formErrors.email}</p>
              )}
            </div>

            {/* Password Field */}
            <div>
              <label
                htmlFor="password"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Password
              </label>
              <input
                type="password"
                id="password"
                name="password"
                value={formData.password}
                onChange={handleChange}
                disabled={isLoading}
                placeholder="••••••••"
                className={`w-full px-4 py-2 border rounded-lg outline-none transition disabled:bg-gray-100 disabled:cursor-not-allowed ${
                  formErrors.password
                    ? "border-red-500 focus:ring-2 focus:ring-red-500"
                    : "border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                }`}
              />
              {formData.password && (
                <div className="mt-2">
                  <div className="flex items-center gap-2">
                    <div className="h-2 flex-1 bg-gray-200 rounded-full overflow-hidden">
                      <div
                        className={`h-full ${passwordStrength.color} transition-all`}
                        style={{
                          width: `${(passwordStrength.score / 4) * 100}%`,
                        }}
                      />
                    </div>
                    {passwordStrength.label && (
                      <span className="text-xs font-semibold text-gray-600">
                        {passwordStrength.label}
                      </span>
                    )}
                  </div>
                </div>
              )}
              {formErrors.password && (
                <p className="text-red-600 text-xs mt-1">
                  {formErrors.password}
                </p>
              )}
            </div>

            {/* Confirm Password Field */}
            <div>
              <label
                htmlFor="confirmPassword"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Confirm Password
              </label>
              <input
                type="password"
                id="confirmPassword"
                name="confirmPassword"
                value={formData.confirmPassword}
                onChange={handleChange}
                disabled={isLoading}
                placeholder="••••••••"
                className={`w-full px-4 py-2 border rounded-lg outline-none transition disabled:bg-gray-100 disabled:cursor-not-allowed ${
                  formErrors.confirmPassword
                    ? "border-red-500 focus:ring-2 focus:ring-red-500"
                    : "border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                }`}
              />
              {formErrors.confirmPassword && (
                <p className="text-red-600 text-xs mt-1">
                  {formErrors.confirmPassword}
                </p>
              )}
            </div>

            {/* Name Field */}
            <div>
              <label
                htmlFor="name"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Full Name
              </label>
              <input
                type="text"
                id="name"
                name="name"
                value={formData.name}
                onChange={handleChange}
                disabled={isLoading}
                placeholder="John Doe"
                className={`w-full px-4 py-2 border rounded-lg outline-none transition disabled:bg-gray-100 disabled:cursor-not-allowed ${
                  formErrors.name
                    ? "border-red-500 focus:ring-2 focus:ring-red-500"
                    : "border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                }`}
              />
              {formErrors.name && (
                <p className="text-red-600 text-xs mt-1">{formErrors.name}</p>
              )}
            </div>

            {/* Phone Number Field */}
            <div>
              <label
                htmlFor="phoneNumber"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Phone Number{" "}
                {!isContractor && (
                  <span className="text-gray-400">(Optional)</span>
                )}
              </label>
              <input
                type="tel"
                id="phoneNumber"
                name="phoneNumber"
                value={formData.phoneNumber}
                onChange={handleChange}
                disabled={isLoading}
                placeholder="+1 (555) 123-4567"
                className={`w-full px-4 py-2 border rounded-lg outline-none transition disabled:bg-gray-100 disabled:cursor-not-allowed ${
                  formErrors.phoneNumber
                    ? "border-red-500 focus:ring-2 focus:ring-red-500"
                    : "border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                }`}
              />
              {formErrors.phoneNumber && (
                <p className="text-red-600 text-xs mt-1">
                  {formErrors.phoneNumber}
                </p>
              )}
            </div>

            {/* Role Selection */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-3">
                Role
              </label>
              <div className="space-y-2">
                {roleOptions.map((option) => (
                  <label
                    key={option.value}
                    className={`flex items-start p-3 border rounded-lg cursor-pointer transition ${
                      formData.role === option.value
                        ? "border-indigo-500 bg-indigo-50"
                        : "border-gray-300 hover:bg-gray-50"
                    }`}
                  >
                    <input
                      type="radio"
                      name="role"
                      value={option.value}
                      checked={formData.role === option.value}
                      onChange={handleChange}
                      disabled={isLoading}
                      className="mt-1 w-4 h-4"
                    />
                    <div className="ml-3">
                      <p className="font-semibold text-gray-900">
                        {option.label}
                      </p>
                      <p className="text-xs text-gray-600">
                        {option.description}
                      </p>
                    </div>
                  </label>
                ))}
              </div>
              {formErrors.role && (
                <p className="text-red-600 text-xs mt-1">{formErrors.role}</p>
              )}
            </div>

            {/* Contractor-specific fields */}
            {isContractor && (
              <>
                {/* Location Field */}
                <div>
                  <label
                    htmlFor="location"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Location Address <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    id="location"
                    name="location"
                    value={formData.location}
                    onChange={handleChange}
                    disabled={isLoading}
                    placeholder="123 Main St, City, State ZIP"
                    className={`w-full px-4 py-2 border rounded-lg outline-none transition disabled:bg-gray-100 disabled:cursor-not-allowed ${
                      formErrors.location
                        ? "border-red-500 focus:ring-2 focus:ring-red-500"
                        : "border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                    }`}
                  />
                  {formErrors.location && (
                    <p className="text-red-600 text-xs mt-1">
                      {formErrors.location}
                    </p>
                  )}
                </div>

                {/* Trade Type Field */}
                <div>
                  <label
                    htmlFor="tradeType"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Trade Type <span className="text-red-500">*</span>
                  </label>
                  <select
                    id="tradeType"
                    name="tradeType"
                    value={formData.tradeType}
                    onChange={handleChange}
                    disabled={isLoading}
                    className={`w-full px-4 py-2 border rounded-lg outline-none transition disabled:bg-gray-100 disabled:cursor-not-allowed ${
                      formErrors.tradeType
                        ? "border-red-500 focus:ring-2 focus:ring-red-500"
                        : "border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                    }`}
                  >
                    <option value="">Select a trade type</option>
                    {tradeTypeOptions.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                  {formErrors.tradeType && (
                    <p className="text-red-600 text-xs mt-1">
                      {formErrors.tradeType}
                    </p>
                  )}
                </div>

                {/* Working Hours */}
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label
                      htmlFor="workingHoursStart"
                      className="block text-sm font-medium text-gray-700 mb-1"
                    >
                      Work Start Time <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="time"
                      id="workingHoursStart"
                      name="workingHoursStart"
                      value={formData.workingHoursStart}
                      onChange={handleChange}
                      disabled={isLoading}
                      className={`w-full px-4 py-2 border rounded-lg outline-none transition disabled:bg-gray-100 disabled:cursor-not-allowed ${
                        formErrors.workingHoursStart
                          ? "border-red-500 focus:ring-2 focus:ring-red-500"
                          : "border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                      }`}
                    />
                    {formErrors.workingHoursStart && (
                      <p className="text-red-600 text-xs mt-1">
                        {formErrors.workingHoursStart}
                      </p>
                    )}
                  </div>
                  <div>
                    <label
                      htmlFor="workingHoursEnd"
                      className="block text-sm font-medium text-gray-700 mb-1"
                    >
                      Work End Time <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="time"
                      id="workingHoursEnd"
                      name="workingHoursEnd"
                      value={formData.workingHoursEnd}
                      onChange={handleChange}
                      disabled={isLoading}
                      className={`w-full px-4 py-2 border rounded-lg outline-none transition disabled:bg-gray-100 disabled:cursor-not-allowed ${
                        formErrors.workingHoursEnd
                          ? "border-red-500 focus:ring-2 focus:ring-red-500"
                          : "border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                      }`}
                    />
                    {formErrors.workingHoursEnd && (
                      <p className="text-red-600 text-xs mt-1">
                        {formErrors.workingHoursEnd}
                      </p>
                    )}
                  </div>
                </div>
              </>
            )}

            {/* Location Field for Customer/Dispatcher (optional) */}
            {!isContractor && formData.role && (
              <div>
                <label
                  htmlFor="location"
                  className="block text-sm font-medium text-gray-700 mb-1"
                >
                  Location <span className="text-gray-400">(Optional)</span>
                </label>
                <input
                  type="text"
                  id="location"
                  name="location"
                  value={formData.location}
                  onChange={handleChange}
                  disabled={isLoading}
                  placeholder="123 Main St, City, State ZIP"
                  className={`w-full px-4 py-2 border rounded-lg outline-none transition disabled:bg-gray-100 disabled:cursor-not-allowed ${
                    formErrors.location
                      ? "border-red-500 focus:ring-2 focus:ring-red-500"
                      : "border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  }`}
                />
                {formErrors.location && (
                  <p className="text-red-600 text-xs mt-1">
                    {formErrors.location}
                  </p>
                )}
              </div>
            )}

            {/* Terms Checkbox */}
            <div className="flex items-start gap-3">
              <input
                type="checkbox"
                id="termsAccepted"
                name="termsAccepted"
                checked={formData.termsAccepted}
                onChange={handleChange}
                disabled={isLoading}
                className="mt-1 w-4 h-4 rounded border-gray-300"
              />
              <label htmlFor="termsAccepted" className="text-sm text-gray-700">
                I agree to the{" "}
                <a
                  href="#"
                  className="text-indigo-600 hover:text-indigo-700 font-semibold"
                >
                  Terms of Service
                </a>
              </label>
            </div>
            {formErrors.termsAccepted && (
              <p className="text-red-600 text-xs">{formErrors.termsAccepted}</p>
            )}

            {/* Create Account Button */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 text-white font-semibold py-2 px-4 rounded-lg transition disabled:cursor-not-allowed flex items-center justify-center gap-2"
            >
              {isLoading ? (
                <>
                  <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                  Creating account...
                </>
              ) : (
                "Create Account"
              )}
            </button>
          </form>

          {/* Divider */}
          <div className="relative my-6">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-300" />
            </div>
          </div>

          {/* Sign In Link */}
          <div className="text-center text-sm">
            <p className="text-gray-600">
              Already have an account?{" "}
              <Link
                to="/login"
                className="text-indigo-600 hover:text-indigo-700 font-semibold"
              >
                Sign in
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
