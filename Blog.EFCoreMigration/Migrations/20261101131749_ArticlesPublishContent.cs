using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.EFCoreMigration.Migrations
{
    /// <inheritdoc />
    public partial class ArticlesPublishContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "ArticleSource",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "源地址/其他资源链接",
                oldClrType: typeof(string),
                oldType: "varchar(2000)",
                oldMaxLength: 2000,
                oldComment: "源地址/其他资源链接")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<bool>(
                name: "HasCommentedOn",
                table: "ArticleOptions",
                type: "tinyint(1)",
                nullable: false,
                comment: "是否已评论过该文章",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AddColumn<bool>(
                name: "HasCollected",
                table: "ArticleOptions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                comment: "是否已收藏该文章");

            migrationBuilder.AddColumn<int>(
                name: "CollectCount",
                table: "Article",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "收藏数");

            migrationBuilder.AddColumn<string>(
                name: "CreationDeclaration",
                table: "Article",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                comment: "创作声明")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PublishStatus",
                table: "Article",
                type: "int",
                nullable: true,
                comment: "发布状态");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishTime",
                table: "Article",
                type: "datetime",
                nullable: true,
                comment: "发布时间");

            migrationBuilder.AddColumn<bool>(
                name: "SavedWhenInit",
                table: "Article",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                comment: "初始化保存,在手动保存后失效");

            migrationBuilder.CreateIndex(
                name: "IDX_Article_PublishStatus",
                table: "Article",
                column: "PublishStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IDX_Article_PublishStatus",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "HasCollected",
                table: "ArticleOptions");

            migrationBuilder.DropColumn(
                name: "CollectCount",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "CreationDeclaration",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "PublishStatus",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "PublishTime",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "SavedWhenInit",
                table: "Article");

            migrationBuilder.UpdateData(
                table: "ArticleSource",
                keyColumn: "Url",
                keyValue: null,
                column: "Url",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "ArticleSource",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: false,
                comment: "源地址/其他资源链接",
                oldClrType: typeof(string),
                oldType: "varchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true,
                oldComment: "源地址/其他资源链接")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<bool>(
                name: "HasCommentedOn",
                table: "ArticleOptions",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldComment: "是否已评论过该文章");
        }
    }
}
