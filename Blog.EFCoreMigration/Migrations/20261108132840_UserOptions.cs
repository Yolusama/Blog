using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.EFCoreMigration.Migrations
{
    /// <inheritdoc />
    public partial class UserOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_User_Email",
                table: "User",
                newName: "Index_User_Email");

            migrationBuilder.AddColumn<int>(
                name: "FansCount",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "粉丝数/被关在数");

            migrationBuilder.AddColumn<int>(
                name: "FollowingCount",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "关注数");

            migrationBuilder.CreateTable(
                name: "UserOptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, comment: "用户选项信息表Id")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", maxLength: 64, nullable: false, comment: "用户Id"),
                    ObjectId = table.Column<int>(type: "int", maxLength: 64, nullable: false, comment: "用户操作对象的用户Id"),
                    IsFollowing = table.Column<bool>(type: "TINYINT(1)", nullable: false, comment: "是否已关注")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOptions", x => x.Id);
                },
                comment: "用户选项信息表")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserOptions_ObjectId",
                table: "UserOptions",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOptions_UserId",
                table: "UserOptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOptions");

            migrationBuilder.DropColumn(
                name: "FansCount",
                table: "User");

            migrationBuilder.DropColumn(
                name: "FollowingCount",
                table: "User");

            migrationBuilder.RenameIndex(
                name: "Index_User_Email",
                table: "User",
                newName: "IX_User_Email");
        }
    }
}
