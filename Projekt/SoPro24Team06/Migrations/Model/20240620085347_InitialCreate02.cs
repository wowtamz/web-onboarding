using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoPro24Team06.Migrations.Model
{
    /// <inheritdoc />
    public partial class InitialCreate02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUser",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DueTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    Days = table.Column<int>(type: "INTEGER", nullable: false),
                    Weeks = table.Column<int>(type: "INTEGER", nullable: false),
                    Months = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DueTimes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Instructions = table.Column<string>(type: "TEXT", nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AssigneeType = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ProcessId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Instructions = table.Column<string>(type: "TEXT", nullable: true),
                    DueInId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssigneeType = table.Column<int>(type: "INTEGER", nullable: false),
                    ProcessTemplateId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentTemplates_DueTimes_DueInId",
                        column: x => x.DueInId,
                        principalTable: "DueTimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    AssignmentTemplateId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_AssignmentTemplates_AssignmentTemplateId",
                        column: x => x.AssignmentTemplateId,
                        principalTable: "AssignmentTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contracts_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    AssignmentTemplateId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_AssignmentTemplates_AssignmentTemplateId",
                        column: x => x.AssignmentTemplateId,
                        principalTable: "AssignmentTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Departments_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Processes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WorkerOfReferenceId = table.Column<string>(type: "TEXT", nullable: false),
                    SupervisorId = table.Column<string>(type: "TEXT", nullable: false),
                    ContractOfRefWorkerId = table.Column<int>(type: "INTEGER", nullable: false),
                    DepartmentOfRefWorkerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Processes_ApplicationUser_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Processes_ApplicationUser_WorkerOfReferenceId",
                        column: x => x.WorkerOfReferenceId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Processes_Contracts_ContractOfRefWorkerId",
                        column: x => x.ContractOfRefWorkerId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Processes_Departments_DepartmentOfRefWorkerId",
                        column: x => x.DepartmentOfRefWorkerId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ContractOfRefWorkerId = table.Column<int>(type: "INTEGER", nullable: false),
                    DepartmentOfRefWorkerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessTemplates_Contracts_ContractOfRefWorkerId",
                        column: x => x.ContractOfRefWorkerId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessTemplates_Departments_DepartmentOfRefWorkerId",
                        column: x => x.DepartmentOfRefWorkerId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdentityRole",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    AssignmentTemplateId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProcessTemplateId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityRole_AssignmentTemplates_AssignmentTemplateId",
                        column: x => x.AssignmentTemplateId,
                        principalTable: "AssignmentTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IdentityRole_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IdentityRole_ProcessTemplates_ProcessTemplateId",
                        column: x => x.ProcessTemplateId,
                        principalTable: "ProcessTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ProcessId",
                table: "Assignments",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentTemplates_DueInId",
                table: "AssignmentTemplates",
                column: "DueInId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentTemplates_ProcessTemplateId",
                table: "AssignmentTemplates",
                column: "ProcessTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_AssignmentId",
                table: "Contracts",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_AssignmentTemplateId",
                table: "Contracts",
                column: "AssignmentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_AssignmentId",
                table: "Departments",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_AssignmentTemplateId",
                table: "Departments",
                column: "AssignmentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityRole_AssignmentId",
                table: "IdentityRole",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityRole_AssignmentTemplateId",
                table: "IdentityRole",
                column: "AssignmentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityRole_ProcessTemplateId",
                table: "IdentityRole",
                column: "ProcessTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_ContractOfRefWorkerId",
                table: "Processes",
                column: "ContractOfRefWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_DepartmentOfRefWorkerId",
                table: "Processes",
                column: "DepartmentOfRefWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_SupervisorId",
                table: "Processes",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_WorkerOfReferenceId",
                table: "Processes",
                column: "WorkerOfReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessTemplates_ContractOfRefWorkerId",
                table: "ProcessTemplates",
                column: "ContractOfRefWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessTemplates_DepartmentOfRefWorkerId",
                table: "ProcessTemplates",
                column: "DepartmentOfRefWorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Processes_ProcessId",
                table: "Assignments",
                column: "ProcessId",
                principalTable: "Processes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentTemplates_ProcessTemplates_ProcessTemplateId",
                table: "AssignmentTemplates",
                column: "ProcessTemplateId",
                principalTable: "ProcessTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Processes_ProcessId",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentTemplates_DueTimes_DueInId",
                table: "AssignmentTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentTemplates_ProcessTemplates_ProcessTemplateId",
                table: "AssignmentTemplates");

            migrationBuilder.DropTable(
                name: "IdentityRole");

            migrationBuilder.DropTable(
                name: "Processes");

            migrationBuilder.DropTable(
                name: "ApplicationUser");

            migrationBuilder.DropTable(
                name: "DueTimes");

            migrationBuilder.DropTable(
                name: "ProcessTemplates");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "AssignmentTemplates");

            migrationBuilder.DropTable(
                name: "Assignments");
        }
    }
}
