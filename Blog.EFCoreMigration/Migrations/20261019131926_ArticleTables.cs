using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.EFCoreMigration.Migrations
{
    /// <inheritdoc />
    public partial class ArticleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArticleOptions_ArticleId",
                table: "ArticleOptions");

            migrationBuilder.DropIndex(
                name: "IX_ArticleComment_RootId_CreateTime",
                table: "ArticleComment");

            migrationBuilder.DropColumn(
                name: "DislikeCount",
                table: "ArticleOptions");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "ArticleOptions");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "ArticleOptions");

            migrationBuilder.AlterTable(
                name: "ArticleOptions",
                comment: "文章选项信息表",
                oldComment: "文章统计选项表")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ArticleOptions",
                type: "bigint",
                nullable: false,
                comment: "文章选项信息Id",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "文章统计选项Id")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<bool>(
                name: "DisLiked",
                table: "ArticleOptions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                comment: "是否点踩");

            migrationBuilder.AddColumn<bool>(
                name: "HasCommentedOn",
                table: "ArticleOptions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Liked",
                table: "ArticleOptions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                comment: "是否点赞");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ArticleOptions",
                type: "int",
                maxLength: 64,
                nullable: false,
                defaultValue: 0,
                comment: "用户Id");

            migrationBuilder.AddColumn<bool>(
                name: "Viewed",
                table: "ArticleOptions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                comment: "是否查看过");

            migrationBuilder.AlterColumn<long>(
                name: "RootId",
                table: "ArticleComment",
                type: "bigint",
                nullable: false,
                defaultValue: -1L,
                comment: "根ID（指向根评论的 Id，根评论为 -1），防止嵌套过深往往只有一个根评论",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: -1L,
                oldComment: "根ID（指向根评论的 Id，根评论为 -1）");

            migrationBuilder.AddColumn<long>(
                name: "CommentToId",
                table: "ArticleComment",
                type: "bigint",
                nullable: true,
                comment: "除根评论外的评论Id，根评论此项为空");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ArticleComment",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "发起者");

            migrationBuilder.AddColumn<int>(
                name: "DislikeCount",
                table: "Article",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "点踩数");

            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "Article",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "点赞数");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Article",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "查看数");

            migrationBuilder.CreateTable(
                name: "ArticleCommentOptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "文章评论选项信息Id")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CommentId = table.Column<long>(type: "bigint", nullable: false, comment: "文章评论Id"),
                    UserId = table.Column<int>(type: "int", nullable: false, comment: "用户Id"),
                    Liked = table.Column<bool>(type: "tinyint(1)", nullable: false, comment: "是否点赞"),
                    Disliked = table.Column<bool>(type: "tinyint(1)", nullable: false, comment: "是否点踩")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleCommentOptions", x => x.Id);
                },
                comment: "文章评论选项信息表")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "Index_ArticleOptions_ArticleId_UserId",
                table: "ArticleOptions",
                columns: new[] { "ArticleId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleComment_UserId",
                table: "ArticleComment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleCommentOptions_CommentId",
                table: "ArticleCommentOptions",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleCommentOptions_UserId",
                table: "ArticleCommentOptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleCommentOptions");

            migrationBuilder.DropIndex(
                name: "Index_ArticleOptions_ArticleId_UserId",
                table: "ArticleOptions");

            migrationBuilder.DropIndex(
                name: "IX_ArticleComment_UserId",
                table: "ArticleComment");

            migrationBuilder.DropColumn(
                name: "DisLiked",
                table: "ArticleOptions");

            migrationBuilder.DropColumn(
                name: "HasCommentedOn",
                table: "ArticleOptions");

            migrationBuilder.DropColumn(
                name: "Liked",
                table: "ArticleOptions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ArticleOptions");

            migrationBuilder.DropColumn(
                name: "Viewed",
                table: "ArticleOptions");

            migrationBuilder.DropColumn(
                name: "CommentToId",
                table: "ArticleComment");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ArticleComment");

            migrationBuilder.DropColumn(
                name: "DislikeCount",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Article");

            migrationBuilder.AlterTable(
                name: "ArticleOptions",
                comment: "文章统计选项表",
                oldComment: "文章选项信息表")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ArticleOptions",
                type: "bigint",
                nullable: false,
                comment: "文章统计选项Id",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "文章选项信息Id")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "DislikeCount",
                table: "ArticleOptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "点踩数");

            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "ArticleOptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "点赞数");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "ArticleOptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "查看数");

            migrationBuilder.AlterColumn<long>(
                name: "RootId",
                table: "ArticleComment",
                type: "bigint",
                nullable: false,
                defaultValue: -1L,
                comment: "根ID（指向根评论的 Id，根评论为 -1）",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: -1L,
                oldComment: "根ID（指向根评论的 Id，根评论为 -1），防止嵌套过深往往只有一个根评论");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleOptions_ArticleId",
                table: "ArticleOptions",
                column: "ArticleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleComment_RootId_CreateTime",
                table: "ArticleComment",
                columns: new[] { "RootId", "CreateTime" });
        }
    }
}
