using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace elbanna.Migrations
{
    /// <inheritdoc />
    public partial class AddLastUpdateUserNameToDaily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "acc_CostCenter",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    costCenter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    building = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    floor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    floorUnit = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_CostCenter", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "acc_CreditAccount",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    creditAcc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_CreditAccount", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "acc_Daily",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    processDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    costcenterId = table.Column<int>(type: "int", nullable: true),
                    costcenter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dealerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dealer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    discount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    net = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    fromAcc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    toAcc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    invoiceCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isReviewed = table.Column<bool>(type: "bit", nullable: true),
                    isAllowed = table.Column<bool>(type: "bit", nullable: true),
                    insertUserId = table.Column<int>(type: "int", nullable: true),
                    insertDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    lastUpdateUserId = table.Column<int>(type: "int", nullable: true),
                    lastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_Daily", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "acc_Dealer",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dealer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nationalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isBank = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_Dealer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "acc_invoiceCode",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoiceCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    invoiceType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acc_invoiceCode", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_invoice",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoiceCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    invoiceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    costcenter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    costcenterId = table.Column<int>(type: "int", nullable: true),
                    balance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    factor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    factorValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    net = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    payFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    payForId = table.Column<int>(type: "int", nullable: true),
                    isRefund = table.Column<bool>(type: "bit", nullable: true),
                    isReviewed = table.Column<bool>(type: "bit", nullable: true),
                    insertDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    lastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    lastUpdateUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_con_invoice", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_payFor",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    payFor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    factor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    factorValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_con_payFor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hr_user",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    job = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    lastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    jobId = table.Column<int>(type: "int", nullable: true),
                    allowSaveDays = table.Column<int>(type: "int", nullable: true),
                    allowShowDays = table.Column<int>(type: "int", nullable: true),
                    allowShowOtherData = table.Column<bool>(type: "bit", nullable: true),
                    allowFutureSaveDays = table.Column<int>(type: "int", nullable: true),
                    islogged = table.Column<bool>(type: "bit", nullable: true),
                    canReview = table.Column<bool>(type: "bit", nullable: true),
                    canPaid = table.Column<bool>(type: "bit", nullable: true),
                    canUpdateCustody = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ic_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ic_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pr_purchase",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    itemId = table.Column<int>(type: "int", nullable: true),
                    dealer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dealerId = table.Column<int>(type: "int", nullable: true),
                    costcenterId = table.Column<int>(type: "int", nullable: true),
                    processDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    qty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    unitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    isreviewed = table.Column<bool>(type: "bit", nullable: true),
                    insertUserId = table.Column<int>(type: "int", nullable: true),
                    insertDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    lastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    lastUpdateUserId = table.Column<int>(type: "int", nullable: true),
                    invoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pr_purchase", x => x.id);
                    table.ForeignKey(
                        name: "FK_pr_purchase_acc_CostCenter_costcenterId",
                        column: x => x.costcenterId,
                        principalTable: "acc_CostCenter",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_pr_purchase_costcenterId",
                table: "pr_purchase",
                column: "costcenterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "acc_CreditAccount");

            migrationBuilder.DropTable(
                name: "acc_Daily");

            migrationBuilder.DropTable(
                name: "acc_Dealer");

            migrationBuilder.DropTable(
                name: "acc_invoiceCode");

            migrationBuilder.DropTable(
                name: "con_invoice");

            migrationBuilder.DropTable(
                name: "con_payFor");

            migrationBuilder.DropTable(
                name: "hr_user");

            migrationBuilder.DropTable(
                name: "ic_item");

            migrationBuilder.DropTable(
                name: "pr_purchase");

            migrationBuilder.DropTable(
                name: "acc_CostCenter");
        }
    }
}
