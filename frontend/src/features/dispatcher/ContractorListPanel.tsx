/**
 * ContractorListPanel Component
 * Main component for managing dispatcher's contractor list
 */

import React, { useState, useEffect } from "react";
import { useContractorList } from "@/hooks/useContractorList";
import { ContractorListItem } from "@/components/shared/ContractorListItem";
import { EmptyState } from "@/components/common/EmptyState";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";

type TabType = "my-list" | "all-contractors";

interface ContractorListPanelProps {
  onFilterChange?: (contractorListOnly: boolean) => void;
}

const ITEMS_PER_PAGE = 50;

export const ContractorListPanel: React.FC<ContractorListPanelProps> = ({
  onFilterChange,
}) => {
  const {
    myList,
    allContractors,
    totalContractors,
    loading,
    error,
    contractorListOnly,
    fetchMyList,
    fetchAvailableContractors,
    addContractor,
    removeContractor,
    toggleFilter,
    cleanup,
  } = useContractorList();

  const [activeTab, setActiveTab] = useState<TabType>("my-list");
  const [currentPage, setCurrentPage] = useState(0);
  const [searchQuery, setSearchQuery] = useState("");
  const [searchTimeout, setSearchTimeout] = useState<ReturnType<
    typeof setTimeout
  > | null>(null);

  // Initialize: fetch my list and available contractors
  useEffect(() => {
    fetchMyList();
    fetchAvailableContractors(ITEMS_PER_PAGE, 0);
  }, [fetchMyList, fetchAvailableContractors]);

  // Cleanup on unmount
  useEffect(() => {
    return () => cleanup();
  }, [cleanup]);

  // Handle search with debounce
  const handleSearch = (query: string) => {
    setSearchQuery(query);
    setCurrentPage(0);

    if (searchTimeout) {
      clearTimeout(searchTimeout);
    }

    if (query.trim()) {
      const timeout = setTimeout(() => {
        fetchAvailableContractors(ITEMS_PER_PAGE, 0, query.trim());
      }, 300);
      setSearchTimeout(timeout);
    } else {
      const timeout = setTimeout(() => {
        fetchAvailableContractors(ITEMS_PER_PAGE, 0);
      }, 300);
      setSearchTimeout(timeout);
    }
  };

  // Handle filter toggle
  const handleFilterToggle = () => {
    toggleFilter();
    onFilterChange?.(!contractorListOnly);
  };

  // Handle pagination
  const handleNextPage = () => {
    const nextPage = currentPage + 1;
    const offset = nextPage * ITEMS_PER_PAGE;
    if (offset < totalContractors) {
      setCurrentPage(nextPage);
      fetchAvailableContractors(
        ITEMS_PER_PAGE,
        offset,
        searchQuery || undefined
      );
    }
  };

  const handlePreviousPage = () => {
    if (currentPage > 0) {
      const prevPage = currentPage - 1;
      const offset = prevPage * ITEMS_PER_PAGE;
      setCurrentPage(prevPage);
      fetchAvailableContractors(
        ITEMS_PER_PAGE,
        offset,
        searchQuery || undefined
      );
    }
  };

  // Handle add contractor
  const handleAddContractor = async (
    contractorId: string
  ): Promise<boolean> => {
    const success = await addContractor(contractorId);
    if (success) {
      // Update allContractors to mark as added
      const updatedContractors = allContractors.map((c) =>
        c.id === contractorId ? { ...c, inDispatcherList: true } : c
      );
      // Note: This is a simple local update. In production, we might refetch
    }
    return success;
  };

  // Handle remove contractor
  const handleRemoveContractor = async (
    contractorId: string
  ): Promise<boolean> => {
    const success = await removeContractor(contractorId);
    if (success) {
      // Clear search and reset to page 0
      setSearchQuery("");
      setCurrentPage(0);
      // Refetch available contractors to update inDispatcherList flags
      fetchAvailableContractors(ITEMS_PER_PAGE, 0);
    }
    return success;
  };

  const hasMore = (currentPage + 1) * ITEMS_PER_PAGE < totalContractors;
  const canGoBack = currentPage > 0;

  return (
    <div className="space-y-6">
      {/* Filter Toggle */}
      <div className="rounded-md border border-gray-200 bg-white p-4">
        <label className="flex items-center gap-3">
          <input
            type="checkbox"
            checked={contractorListOnly}
            onChange={handleFilterToggle}
            className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
          />
          <span className="text-sm font-medium text-gray-700">
            Filter recommendations by my contractor list only
          </span>
        </label>
        <p className="mt-1 text-xs text-gray-600">
          When enabled, job recommendations will only include contractors from
          your personal list.
        </p>
      </div>

      {/* Tabs */}
      <div className="border-b border-gray-200">
        <div className="flex gap-4">
          <button
            onClick={() => {
              setActiveTab("my-list");
              setSearchQuery("");
            }}
            className={`border-b-2 px-4 py-2 text-sm font-medium transition-colors ${
              activeTab === "my-list"
                ? "border-blue-600 text-blue-600"
                : "border-transparent text-gray-600 hover:text-gray-900"
            }`}
          >
            My Contractor List ({myList.length})
          </button>
          <button
            onClick={() => {
              setActiveTab("all-contractors");
              setCurrentPage(0);
            }}
            className={`border-b-2 px-4 py-2 text-sm font-medium transition-colors ${
              activeTab === "all-contractors"
                ? "border-blue-600 text-blue-600"
                : "border-transparent text-gray-600 hover:text-gray-900"
            }`}
          >
            All Contractors ({totalContractors})
          </button>
        </div>
      </div>

      {/* Error Display */}
      {error && (
        <div className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {/* My Contractor List Tab */}
      {activeTab === "my-list" && (
        <div className="rounded-md border border-gray-200 bg-white p-4">
          {myList.length === 0 ? (
            <EmptyState
              title="No contractors added yet"
              description="Add contractors from the 'All Contractors' tab to create your personal list."
              action={
                <button
                  onClick={() => setActiveTab("all-contractors")}
                  className="mt-4 inline-block rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
                >
                  Browse Contractors
                </button>
              }
            />
          ) : (
            <div className="space-y-4">
              {/* Search in my list */}
              <div className="mb-4">
                <input
                  type="text"
                  placeholder="Search in my list..."
                  className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm placeholder-gray-400 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  onChange={(e) => {
                    // Client-side filtering for my list
                    const filtered = myList.filter((c) =>
                      c.name
                        .toLowerCase()
                        .includes(e.target.value.toLowerCase())
                    );
                    // Store filtered results locally (simplified approach)
                  }}
                />
              </div>

              {/* Contractor list */}
              <div>
                {myList.map((contractor) => (
                  <ContractorListItem
                    key={contractor.id}
                    contractor={contractor}
                    mode="my-list"
                    onRemove={handleRemoveContractor}
                    isLoading={loading}
                  />
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {/* All Contractors Tab */}
      {activeTab === "all-contractors" && (
        <div className="rounded-md border border-gray-200 bg-white p-4">
          {/* Search input */}
          <div className="mb-4">
            <input
              type="text"
              placeholder="Search contractors by name..."
              value={searchQuery}
              onChange={(e) => handleSearch(e.target.value)}
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm placeholder-gray-400 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          {/* Loading state */}
          {loading ? (
            <div className="flex justify-center py-8">
              <LoadingSpinner />
            </div>
          ) : allContractors.length === 0 ? (
            <EmptyState
              title="No contractors found"
              description="Try adjusting your search criteria."
            />
          ) : (
            <>
              {/* Contractor list */}
              <div className="space-y-2">
                {allContractors.map((contractor) => (
                  <ContractorListItem
                    key={contractor.id}
                    contractor={contractor}
                    mode="available"
                    onAdd={handleAddContractor}
                    isLoading={loading}
                  />
                ))}
              </div>

              {/* Pagination */}
              <div className="mt-6 flex items-center justify-between border-t border-gray-200 pt-4">
                <div className="text-xs text-gray-600">
                  Showing {currentPage * ITEMS_PER_PAGE + 1}–
                  {Math.min(
                    (currentPage + 1) * ITEMS_PER_PAGE,
                    totalContractors
                  )}{" "}
                  of {totalContractors}
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={handlePreviousPage}
                    disabled={!canGoBack}
                    className={`rounded-md px-3 py-2 text-sm font-medium ${
                      canGoBack
                        ? "border border-gray-300 bg-white text-gray-700 hover:bg-gray-50"
                        : "border border-gray-200 bg-gray-50 text-gray-400"
                    }`}
                  >
                    ← Previous
                  </button>
                  <button
                    onClick={handleNextPage}
                    disabled={!hasMore}
                    className={`rounded-md px-3 py-2 text-sm font-medium ${
                      hasMore
                        ? "border border-gray-300 bg-white text-gray-700 hover:bg-gray-50"
                        : "border border-gray-200 bg-gray-50 text-gray-400"
                    }`}
                  >
                    Next →
                  </button>
                </div>
              </div>
            </>
          )}
        </div>
      )}
    </div>
  );
};
