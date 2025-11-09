using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartScheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create composite index for querying completed assignments with pagination
            // Supports: GetAssignmentsByContractorAndStatusAsync queries
            // Used for: Contractor job history, filtering by status with chronological ordering
            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ContractorId_Status_CompletedAt",
                table: "Assignments",
                columns: new[] { "ContractorId", "Status", "CompletedAt" },
                descending: new[] { false, false, true });

            // Separate index for common filtering patterns (Completed status queries)
            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ContractorId_Status",
                table: "Assignments",
                columns: new[] { "ContractorId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assignments_ContractorId_Status_CompletedAt",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_ContractorId_Status",
                table: "Assignments");
        }
    }
}

