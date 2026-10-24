using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.EFCoreMigration.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cover",
                table: "Article",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                comment: "文章封面")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProjectAuditLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "日志Id")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActionName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, comment: "Action方法名")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExecutionDuration = table.Column<int>(type: "int", nullable: false, comment: "执行耗时（毫秒）"),
                    ExecutionTime = table.Column<DateTime>(type: "datetime", nullable: false, comment: "执行时间"),
                    ActionType = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false, comment: "请求方式")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionDesc = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, comment: "Action描述内容")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServiceName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, comment: "服务/控制器名")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParameterJson = table.Column<string>(type: "json", nullable: true, comment: "请求参数Json")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Route = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, comment: "请求路由")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RemoteIpAddr = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, comment: "远程请求IP")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Exception = table.Column<string>(type: "longtext", nullable: true, comment: "异常信息")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAuditLog", x => x.Id);
                },
                comment: "API审计日志表")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAuditLog_ActionName",
                table: "ProjectAuditLog",
                column: "ActionName");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAuditLog_ExecutionTime",
                table: "ProjectAuditLog",
                column: "ExecutionTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectAuditLog");

            migrationBuilder.DropColumn(
                name: "Cover",
                table: "Article");
        }
    }
}
