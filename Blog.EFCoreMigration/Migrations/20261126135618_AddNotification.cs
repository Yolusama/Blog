using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.EFCoreMigration.Migrations
{
    /// <inheritdoc />
    public partial class AddNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, comment: "消息内容")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SenderId = table.Column<int>(type: "int", nullable: true, comment: "发送方Id"),
                    ReceiverId = table.Column<int>(type: "int", nullable: false, comment: "接收方Id"),
                    IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false, comment: "是否已读"),
                    Type = table.Column<int>(type: "int", nullable: false, comment: "通知消息类性：1.评论回复@，2.收到点赞/点踩，3.用户消息传输，4.文章被收藏"),
                    CreateTime = table.Column<DateTime>(type: "datetime", nullable: true, comment: "创建时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                },
                comment: "通知信息表")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "Index_Notification_ReceiverId",
                table: "Notification",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "Index_Notification_SenderId",
                table: "Notification",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "Index_Notification_Type",
                table: "Notification",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notification");
        }
    }
}
