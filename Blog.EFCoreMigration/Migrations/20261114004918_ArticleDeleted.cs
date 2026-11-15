using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.EFCoreMigration.Migrations
{
    /// <inheritdoc />
    public partial class ArticleDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FansCount",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "粉丝数/被关在数",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0,
                oldComment: "粉丝数/被关注数");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Article",
                type: "TINYINT(1)",
                nullable: false,
                comment: "删除标识",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FansCount",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "粉丝数/被关注数",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0,
                oldComment: "粉丝数/被关在数");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Article",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "TINYINT(1)",
                oldComment: "删除标识");
        }
    }
}
