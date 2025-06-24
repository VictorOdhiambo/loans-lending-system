using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loan_application_service.Migrations
{
    /// <inheritdoc />
    public partial class loanupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanCharge_LoanProduct_LoanProductProductId",
                table: "LoanCharge");

            migrationBuilder.RenameColumn(
                name: "LoanProductProductId",
                table: "LoanCharge",
                newName: "LoanProductId");

            migrationBuilder.RenameIndex(
                name: "IX_LoanCharge_LoanProductProductId",
                table: "LoanCharge",
                newName: "IX_LoanCharge_LoanProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanCharge_LoanProduct_LoanProductId",
                table: "LoanCharge",
                column: "LoanProductId",
                principalTable: "LoanProduct",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanCharge_LoanProduct_LoanProductId",
                table: "LoanCharge");

            migrationBuilder.RenameColumn(
                name: "LoanProductId",
                table: "LoanCharge",
                newName: "LoanProductProductId");

            migrationBuilder.RenameIndex(
                name: "IX_LoanCharge_LoanProductId",
                table: "LoanCharge",
                newName: "IX_LoanCharge_LoanProductProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanCharge_LoanProduct_LoanProductProductId",
                table: "LoanCharge",
                column: "LoanProductProductId",
                principalTable: "LoanProduct",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
