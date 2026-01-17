using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrisilPreOnboardingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreOnboardings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalCandidateId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CrisilOfferId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JoiningStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JoiningDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MobileCountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    AddressJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KycJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RawRequestJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreOnboardings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreOnboardings_ExternalCandidateId_CrisilOfferId",
                table: "PreOnboardings",
                columns: new[] { "ExternalCandidateId", "CrisilOfferId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreOnboardings");
        }
    }
}
