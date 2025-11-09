/**
 * StarRatingInput Component
 * Interactive star rating selector with keyboard support
 * Allows users to click or use arrow keys to select 1-5 star rating
 */

import React, { useState, useCallback } from "react";

interface StarRatingInputProps {
  rating: number;
  onRatingChange: (rating: number) => void;
  disabled?: boolean;
}

export const StarRatingInput: React.FC<StarRatingInputProps> = ({
  rating,
  onRatingChange,
  disabled = false,
}) => {
  const [hoverRating, setHoverRating] = useState<number>(0);

  const handleStarClick = useCallback(
    (value: number) => {
      if (!disabled) {
        onRatingChange(value);
      }
    },
    [onRatingChange, disabled]
  );

  const handleStarKeyDown = useCallback(
    (e: React.KeyboardEvent, index: number) => {
      if (disabled) return;

      switch (e.key) {
        case "ArrowRight":
          e.preventDefault();
          // Move to next star if not at max
          if (index < 4) {
            onRatingChange(index + 2); // index is 0-4, so +2 gives 2-5
          }
          break;
        case "ArrowLeft":
          e.preventDefault();
          // Move to previous star if not at min
          if (index > 0) {
            onRatingChange(index); // index is 0-4, so value is 1-4
          }
          break;
        case "Delete":
        case "Backspace":
          e.preventDefault();
          onRatingChange(0);
          break;
        case "Enter":
          e.preventDefault();
          // Already selected, do nothing
          break;
        default:
          break;
      }
    },
    [onRatingChange, disabled]
  );

  const displayRating = hoverRating || rating;

  return (
    <div className="flex flex-col gap-2">
      {/* Star rating display */}
      <div
        className="flex gap-1 items-center"
        role="radiogroup"
        aria-label="Job rating"
        aria-required="true"
      >
        {[1, 2, 3, 4, 5].map((starValue, index) => (
          <button
            key={starValue}
            onClick={() => handleStarClick(starValue)}
            onKeyDown={(e) => handleStarKeyDown(e, index)}
            onMouseEnter={() => !disabled && setHoverRating(starValue)}
            onMouseLeave={() => setHoverRating(0)}
            aria-label={`Rate ${starValue} stars`}
            aria-checked={rating === starValue}
            role="radio"
            tabIndex={rating === starValue ? 0 : -1}
            disabled={disabled}
            className={`transition-all duration-150 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 rounded ${
              disabled ? "cursor-not-allowed opacity-50" : "cursor-pointer"
            }`}
          >
            <span
              className={`text-4xl ${
                starValue <= displayRating
                  ? "text-yellow-400 drop-shadow-md"
                  : "text-gray-300"
              } transition-all duration-100`}
            >
              ★
            </span>
          </button>
        ))}
      </div>

      {/* Rating label display */}
      {displayRating > 0 && (
        <p className="text-sm text-gray-600 font-medium">
          {displayRating === 1 ? "Poor" : ""}
          {displayRating === 2 ? "Fair" : ""}
          {displayRating === 3 ? "Good" : ""}
          {displayRating === 4 ? "Very Good" : ""}
          {displayRating === 5 ? "Excellent" : ""}
        </p>
      )}

      {/* Clear button */}
      {rating > 0 && (
        <button
          onClick={() => onRatingChange(0)}
          onKeyDown={(e) => {
            if (e.key === "Delete" || e.key === "Backspace") {
              e.preventDefault();
              onRatingChange(0);
            }
          }}
          disabled={disabled}
          className={`text-sm text-blue-600 hover:text-blue-800 hover:underline font-medium transition-colors ${
            disabled ? "text-gray-400 cursor-not-allowed" : ""
          }`}
          aria-label="Clear rating"
        >
          Clear
        </button>
      )}
    </div>
  );
};


