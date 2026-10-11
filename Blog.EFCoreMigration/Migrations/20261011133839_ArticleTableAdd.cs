using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.EFCoreMigration.Migrations
{
    /// <inheritdoc />
    public partial class ArticleTableAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SimpleIntroduction",
                table: "User",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "简介",
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldComment: "简介")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "User",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true,
                comment: "地区",
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldMaxLength: 128,
                oldComment: "地区")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ArticleId",
                table: "ArticleComment",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                comment: "文章Id")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Article",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                comment: "文章最终内容指向源",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldComment: "文章最终内容指向源")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Abstract",
                table: "Article",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true,
                comment: "摘要",
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldMaxLength: 128,
                oldComment: "摘要")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Article",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleComment_ArticleId",
                table: "ArticleComment",
                column: "ArticleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArticleComment_ArticleId",
                table: "ArticleComment");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ArticleId",
                table: "ArticleComment");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Article");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "SimpleIntroduction",
                keyValue: null,
                column: "SimpleIntroduction",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "SimpleIntroduction",
                table: "User",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                comment: "简介",
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "简介")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Region",
                keyValue: null,
                column: "Region",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "User",
                type: "varchar(128)",
                maxLength: 128,
                nullable: false,
                comment: "地区",
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldMaxLength: 128,
                oldNullable: true,
                oldComment: "地区")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Article",
                keyColumn: "Content",
                keyValue: null,
                column: "Content",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Article",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                comment: "文章最终内容指向源",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "文章最终内容指向源")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Article",
                keyColumn: "Abstract",
                keyValue: null,
                column: "Abstract",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Abstract",
                table: "Article",
                type: "varchar(128)",
                maxLength: 128,
                nullable: false,
                comment: "摘要",
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldMaxLength: 128,
                oldNullable: true,
                oldComment: "摘要")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
