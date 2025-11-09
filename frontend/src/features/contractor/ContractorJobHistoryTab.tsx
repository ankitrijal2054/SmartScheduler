/**
 * ContractorJobHistoryTab Component
 * Displays contractor's job history with filtering and pagination
 * Used in Story 5.4 - Contractor Rating & Earnings History
 */

import React, { useState } from "react";
import { useJobHistory } from "@/hooks/useJobHistory";
import { JobHistoryTable } from "./JobHistoryTable";
import { JobHistoryFilterOptions } from "@/types/ContractorProfile";
import { Calendar } from "lucide-react";

/**
 * Component to display contractor's job history tab
 * Includes date filter controls and pagination
 */
export const ContractorJobHistoryTab: React.FC = () => {
  const {
    jobs,
    totalCount,
    loading,
    currentPage,
    pageSize,
    goToPage,
    setFilters,
  } = useJobHistory(20);

  const [startDate, setStartDate] = useState<string>("");
  const [endDate, setEndDate] = useState<string>("");
  const [hasAppliedFilters, setHasAppliedFilters] = useState(false);

  /**
   * Apply date filters
   */
  const handleApplyFilters = async () => {
    const filters: JobHistoryFilterOptions = {};

    if (startDate) {
      filters.startDate = new Date(startDate).toISOString();
    }

    if (endDate) {
      filters.endDate = new Date(endDate).toISOString();
    }

    await setFilters(Object.keys(filters).length > 0 ? filters : undefined);
    setHasAppliedFilters(true);
  };

  /**
   * Clear filters
   */
  const handleClearFilters = async () => {
    setStartDate("");
    setEndDate("");
    await setFilters(undefined);
    setHasAppliedFilters(false);
  };

  return (
    <div className="space-y-6">
      {/* Filter Section */}
      <div className="bg-white rounded-lg shadow-md p-6 border border-gray-200">
        <div className="flex items-center gap-2 mb-4">
          <Calendar size={20} className="text-gray-600" />
          <h3 className="text-lg font-semibold text-gray-900">
            Filter Job History
          </h3>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
          {/* Start Date */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Start Date (Optional)
            </label>
            <input
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
            />
          </div>

          {/* End Date */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              End Date (Optional)
            </label>
            <input
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
            />
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex gap-3">
          <button
            onClick={handleApplyFilters}
            disabled={loading}
            className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            Apply Filters
          </button>

          {hasAppliedFilters && (
            <button
              onClick={handleClearFilters}
              disabled={loading}
              className="px-6 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              Clear Filters
            </button>
          )}
        </div>
      </div>

      {/* Job History Table */}
      <JobHistoryTable
        jobs={jobs}
        totalCount={totalCount}
        currentPage={currentPage}
        pageSize={pageSize}
        loading={loading}
        onPageChange={goToPage}
      />
    </div>
  );
};
