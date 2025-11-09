/**
 * Contractor Type Definitions
 * Shared interfaces for contractor recommendation and profile data
 */

export type TradeType =
  | "Flooring"
  | "HVAC"
  | "Plumbing"
  | "Electrical"
  | "Other";
export type SortField = "rank" | "rating" | "distance" | "travelTime";

/**
 * TimeSlot represents a contractor's available time window
 */
export interface TimeSlot {
  startTime: string; // ISO 8601 datetime string
  endTime: string; // ISO 8601 datetime string
}

/**
 * RecommendedContractor is returned from the recommendations API
 * Contains ranking, scoring, and availability information
 */
export interface RecommendedContractor {
  contractorId: string;
  name: string;
  rank: number; // 1-5
  score: number; // 0-1 (decimal score from scoring algorithm)
  avgRating: number | null; // Average rating from reviews, null if no reviews
  reviewCount: number; // Number of reviews received
  distance: number; // Distance in miles
  travelTime: number; // Travel time in minutes
  tradeType: TradeType;
  availableTimeSlots: TimeSlot[];
}

/**
 * RecommendationRequest is sent to the backend API
 * Contains job details and optional filters
 */
export interface RecommendationRequest {
  jobId: string;
  jobType: TradeType;
  location: string;
  desiredDateTime: string; // ISO 8601 datetime string
  contractor_list_only?: boolean; // Optional: filter to dispatcher's curated list
}

/**
 * Metadata about the recommendations response
 */
export interface RecommendationMetadata {
  totalAvailable: number;
  requestTime: string; // ISO 8601 datetime string
}

/**
 * RecommendationResponse is returned from the recommendations API
 * Contains the list of recommendations and metadata
 */
export interface RecommendationResponse {
  data: RecommendedContractor[];
  metadata: RecommendationMetadata;
}

/**
 * State management type for useRecommendations hook
 */
export interface RecommendationsState {
  recommendations: RecommendedContractor[];
  loading: boolean;
  error: string | null;
  sortBy: SortField;
}

/**
 * Contractor represents an available contractor (for list management)
 */
export interface Contractor {
  id: string;
  name: string;
  rating: number | null; // Average rating (null if no reviews)
  reviewCount: number;
  location: string;
  tradeType: TradeType;
  isActive: boolean;
  inDispatcherList?: boolean; // Computed: whether contractor is in current dispatcher's list
}

/**
 * Paginated contractors response
 */
export interface PaginatedContractorsResponse {
  contractors: Contractor[];
  total: number;
  hasMore: boolean;
}
