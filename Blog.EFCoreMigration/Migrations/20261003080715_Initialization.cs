using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.EFCoreMigration.Migrations
{
    /// <inheritdoc />
    public partial class Initialization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Article",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, comment: "文章Id")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false, comment: "标题")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AuthorId = table.Column<int>(type: "int", nullable: false, comment: "作者Id"),
                    Content = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "文章最终内容指向源")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Abstract = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, comment: "摘要")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UploadTime = table.Column<DateTime>(type: "datetime", nullable: true, comment: "正式上传时间"),
                    IsDraft = table.Column<bool>(type: "tinyint(1)", nullable: false, comment: "是否为草稿"),
                    UpdateTime = table.Column<DateTime>(type: "datetime", nullable: true, comment: "更新时间"),
                    CreateTime = table.Column<DateTime>(type: "datetime", nullable: true, comment: "创建时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Article", x => x.Id);
                },
                comment: "文章表")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArticleComment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "文章评论Id")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false, comment: "内容")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: true, comment: "创建时间"),
                    LikeCount = table.Column<int>(type: "int", nullable: false, comment: "喜欢数"),
                    DislikeCount = table.Column<int>(type: "int", nullable: false, comment: "讨厌数"),
                    RootId = table.Column<long>(type: "bigint", nullable: false, defaultValue: -1L, comment: "根ID（指向根评论的 Id，根评论为 -1）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleComment", x => x.Id);
                },
                comment: "文章评论表")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArticleOptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "文章统计选项Id")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ViewCount = table.Column<int>(type: "int", nullable: false, comment: "查看数"),
                    LikeCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "点赞数"),
                    DislikeCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "点踩数"),
                    ArticleId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, comment: "文章Id")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleOptions", x => x.Id);
                },
                comment: "文章统计选项表")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArticleSource",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, comment: "文件名")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ArticleId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, comment: "文章Id")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Url = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false, comment: "源地址/其他资源链接")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsHyperLink = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false, comment: "是否是超链接"),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: true, comment: "创建时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleSource", x => x.Id);
                },
                comment: "文章内资源表")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false, comment: "用户Id")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "邮箱")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false, comment: "密码")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nickname = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, comment: "昵称")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Avatar = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "头像")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Gender = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "性别"),
                    SimpleIntroduction = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false, comment: "简介")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BirthDay = table.Column<DateOnly>(type: "date", nullable: true, comment: "生日"),
                    Region = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, comment: "地区")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreateTime = table.Column<DateTime>(type: "datetime", nullable: true, comment: "创建时间"),
                    LastLoginTime = table.Column<DateTime>(type: "datetime", nullable: true, comment: "上次登录时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                },
                comment: "用户表")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IDX_Article_AuthorId",
                table: "Article",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IDX_Article_CreateTime",
                table: "Article",
                column: "CreateTime");

            migrationBuilder.CreateIndex(
                name: "IDX_Article_IsDraft",
                table: "Article",
                column: "IsDraft");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleComment_RootId",
                table: "ArticleComment",
                column: "RootId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleComment_RootId_CreateTime",
                table: "ArticleComment",
                columns: new[] { "RootId", "CreateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleOptions_ArticleId",
                table: "ArticleOptions",
                column: "ArticleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleSource_ArticleId",
                table: "ArticleSource",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleSourceCreateTime",
                table: "ArticleSource",
                column: "CreateTime");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Article");

            migrationBuilder.DropTable(
                name: "ArticleComment");

            migrationBuilder.DropTable(
                name: "ArticleOptions");

            migrationBuilder.DropTable(
                name: "ArticleSource");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
