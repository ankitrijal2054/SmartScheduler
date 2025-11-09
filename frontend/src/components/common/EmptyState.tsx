/**
 * EmptyState Component
 * Empty state messaging and illustration
 */

import React from "react";

interface EmptyStateProps {
  title?: string;
  description?: string;
  icon?: React.ReactNode;
  action?: React.ReactNode;
}

export const EmptyState: React.FC<EmptyStateProps> = ({
  title = "No items found",
  description = "There are no items to display at this time.",
  icon,
  action,
}) => {
  return (
    <div className="flex flex-col items-center justify-center rounded-lg border border-gray-200 bg-gray-50 py-12 px-6 text-center">
      {icon && <div className="mb-4 text-4xl">{icon}</div>}
      <h3 className="mb-2 text-lg font-semibold text-gray-900">{title}</h3>
      <p className="text-gray-600">{description}</p>
      {action && <div className="mt-4">{action}</div>}
    </div>
  );
};
