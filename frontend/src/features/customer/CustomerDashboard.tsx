/**
 * Customer Dashboard Component
 * Main dashboard for customers to view their jobs and submit new ones
 */

import React from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/hooks/useAuthContext";
import { useJobs } from "@/hooks/useJobs";
import { useCustomerNotifications } from "@/hooks/useCustomerNotifications";
import { DashboardHeader } from "@/components/DashboardHeader";
import { CustomerJobCard } from "./CustomerJobCard";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";

export const CustomerDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const { jobs, loading, error, pagination, setPage } = useJobs();

  // Initialize customer notifications
  useCustomerNotifications();

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <DashboardHeader
        title="Customer Dashboard"
        subtitle="Manage your jobs and track their progress"
        showNotificationBadge={true}
      />

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-6 py-8">
        {/* Quick Actions */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
          {/* Submit New Job Card */}
          <button
            onClick={() => navigate("/customer/submit-job")}
            className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow text-left group"
          >
            <div className="flex items-center justify-between mb-4">
              <div className="w-12 h-12 bg-indigo-100 rounded-lg flex items-center justify-center group-hover:bg-indigo-200 transition-colors">
                <svg
                  className="w-6 h-6 text-indigo-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 4v16m8-8H4"
                  />
                </svg>
              </div>
            </div>
            <h3 className="text-xl font-semibold text-gray-900 mb-2">
              Submit New Job
            </h3>
            <p className="text-gray-600 text-sm">
              Create a new job request for contractors
            </p>
          </button>

          {/* View My Jobs Card */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center justify-between mb-4">
              <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                <svg
                  className="w-6 h-6 text-blue-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                  />
                </svg>
              </div>
            </div>
            <h3 className="text-xl font-semibold text-gray-900 mb-2">
              My Jobs
            </h3>
            <p className="text-gray-600 text-sm">
              {loading
                ? "Loading..."
                : `${jobs.length} ${
                    jobs.length === 1 ? "job" : "jobs"
                  } submitted`}
            </p>
          </div>

          {/* Help Card */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center justify-between mb-4">
              <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                <svg
                  className="w-6 h-6 text-green-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
              </div>
            </div>
            <h3 className="text-xl font-semibold text-gray-900 mb-2">
              Need Help?
            </h3>
            <p className="text-gray-600 text-sm">
              Get assistance with using SmartScheduler
            </p>
          </div>
        </div>

        {/* My Jobs Section */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-8">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-2xl font-semibold text-gray-900">My Jobs</h2>
            <button
              onClick={() => navigate("/customer/submit-job")}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors text-sm font-medium"
            >
              Submit New Job
            </button>
          </div>

          {loading ? (
            <div className="flex justify-center py-12">
              <LoadingSpinner />
            </div>
          ) : error ? (
            <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-800">
              <p className="font-semibold">Error loading jobs</p>
              <p className="text-sm">{error}</p>
            </div>
          ) : !jobs || jobs.length === 0 ? (
            <div className="text-center py-12 text-gray-500">
              <p className="text-lg mb-2">No jobs yet</p>
              <p className="text-sm mb-4">
                Submit your first job to get started!
              </p>
              <button
                onClick={() => navigate("/customer/submit-job")}
                className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors font-semibold"
              >
                Submit your first job →
              </button>
            </div>
          ) : (
            <>
              <div className="space-y-3">
                {jobs.map((job) => (
                  <CustomerJobCard key={job.id} job={job} />
                ))}
              </div>

              {/* Pagination Controls */}
              {pagination && pagination.totalPages > 1 && (
                <div className="flex items-center justify-between mt-6 pt-6 border-t border-gray-200">
                  <div className="text-sm text-gray-600">
                    Page{" "}
                    <span className="font-semibold">{pagination.page}</span> of{" "}
                    <span className="font-semibold">
                      {pagination.totalPages}
                    </span>{" "}
                    (<span className="font-semibold">{pagination.total}</span>{" "}
                    total)
                  </div>

                  <div className="flex gap-2">
                    <button
                      onClick={() => setPage(pagination.page - 1)}
                      disabled={pagination.page === 1}
                      className="rounded-md bg-gray-200 px-4 py-2 text-sm font-medium text-gray-900 transition-colors hover:bg-gray-300 disabled:cursor-not-allowed disabled:opacity-50"
                      aria-label="Previous page"
                    >
                      ← Previous
                    </button>
                    <button
                      onClick={() => setPage(pagination.page + 1)}
                      disabled={pagination.page >= pagination.totalPages}
                      className="rounded-md bg-gray-200 px-4 py-2 text-sm font-medium text-gray-900 transition-colors hover:bg-gray-300 disabled:cursor-not-allowed disabled:opacity-50"
                      aria-label="Next page"
                    >
                      Next →
                    </button>
                  </div>
                </div>
              )}
            </>
          )}
        </div>

        {/* Recent Activity Section - Show if there are jobs */}
        {jobs && jobs.length > 0 && (
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-2xl font-semibold text-gray-900 mb-4">
              Recent Activity
            </h2>
            <div className="text-center py-8 text-gray-500">
              <p>Activity tracking coming soon</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
