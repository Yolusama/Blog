using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.EFCoreMigration.Migrations
{
    /// <inheritdoc />
    public partial class AddLogBrowserInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientInfo",
                table: "ProjectAuditLog",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                comment: "客户端请求信息（请求头User-Agent)")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientInfo",
                table: "ProjectAuditLog");
        }
    }
}
