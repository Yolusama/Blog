using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.EFCoreMigration.Migrations
{
    /// <inheritdoc />
    public partial class ArticleTablesStructureAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArticleComment_ArticleId",
                table: "ArticleComment");

            migrationBuilder.DropIndex(
                name: "IX_ArticleComment_UserId",
                table: "ArticleComment");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "ArticleOptions",
                type: "datetime(6)",
                nullable: true,
                comment: "创建时间");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "ArticleOptions",
                type: "datetime(6)",
                nullable: true,
                comment: "更新时间");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "ArticleCommentOptions",
                type: "datetime",
                nullable: true,
                comment: "创建时间");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "ArticleCommentOptions",
                type: "datetime",
                nullable: true,
                comment: "修改时间");

            migrationBuilder.AddColumn<DateTime>(
                name: "PostTime",
                table: "ArticleComment",
                type: "datetime(6)",
                nullable: true,
                comment: "发表时间");

            migrationBuilder.CreateIndex(
                name: "Index_ArticleOptions_CreateTime",
                table: "ArticleOptions",
                column: "CreateTime");

            migrationBuilder.CreateIndex(
                name: "Index_ArticleCommentOptions_CreateTime",
                table: "ArticleCommentOptions",
                column: "CreateTime");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleComment_Article",
                table: "ArticleComment",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleComment_PostTime",
                table: "ArticleComment",
                column: "PostTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Index_ArticleOptions_CreateTime",
                table: "ArticleOptions");

            migrationBuilder.DropIndex(
                name: "Index_ArticleCommentOptions_CreateTime",
                table: "ArticleCommentOptions");

            migrationBuilder.DropIndex(
                name: "IX_ArticleComment_Article",
                table: "ArticleComment");

            migrationBuilder.DropIndex(
                name: "IX_ArticleComment_PostTime",
                table: "ArticleComment");

            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "ArticleOptions");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "ArticleOptions");

            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "ArticleCommentOptions");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "ArticleCommentOptions");

            migrationBuilder.DropColumn(
                name: "PostTime",
                table: "ArticleComment");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleComment_ArticleId",
                table: "ArticleComment",
                column: "ArticleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleComment_UserId",
                table: "ArticleComment",
                column: "UserId");
        }
    }
}
