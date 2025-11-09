/**
 * ContractorRoutes Component
 * Defines all routes for the contractor portal
 * Can be integrated into main App.tsx routing structure
 */

import { Route } from "react-router-dom";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { ContractorDashboard } from "./ContractorDashboard";

export const contractorRoutes = (
  <>
    <Route
      path="/contractor/dashboard"
      element={
        <ProtectedRoute requiredRole="Contractor">
          <ContractorDashboard />
        </ProtectedRoute>
      }
    />
  </>
);



