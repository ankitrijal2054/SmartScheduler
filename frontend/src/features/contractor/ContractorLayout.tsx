/**
 * ContractorLayout Component
 * Main layout wrapper for contractor portal with header, sidebar, and notification badge
 */

import React from "react";
import { DashboardHeader } from "@/components/DashboardHeader";

interface ContractorLayoutProps {
  children: React.ReactNode;
}

export const ContractorLayout: React.FC<ContractorLayoutProps> = ({
  children,
}) => {
  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <DashboardHeader
        title="Contractor Dashboard"
        subtitle="Manage your jobs and track your performance"
        showNotificationBadge={true}
      />

      {/* Main Content */}
      <main className="px-6 py-8">{children}</main>
    </div>
  );
};
