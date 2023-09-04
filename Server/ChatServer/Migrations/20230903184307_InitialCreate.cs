using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Channel",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channel", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "FileUploads",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    filename = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    path = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    size = table.Column<long>(type: "bigint", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUploads", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    password = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    file_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                    table.ForeignKey(
                        name: "FK__Users__file_id__38996AB5",
                        column: x => x.file_id,
                        principalTable: "FileUploads",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ChannelMembership",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    channel_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelMembership", x => x.id);
                    table.ForeignKey(
                        name: "FK__ChannelMe__chann__412EB0B6",
                        column: x => x.channel_id,
                        principalTable: "Channel",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__ChannelMe__user___403A8C7D",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    content = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    sender_status = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    receiver_status = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    send_time = table.Column<DateTime>(type: "datetime", nullable: true),
                    channel_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.id);
                    table.ForeignKey(
                        name: "FK__Message__channel__47DBAE45",
                        column: x => x.channel_id,
                        principalTable: "Channel",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__Message__user_id__3B75D760",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    message_id = table.Column<int>(type: "int", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.id);
                    table.ForeignKey(
                        name: "FK__Notificat__messa__440B1D61",
                        column: x => x.message_id,
                        principalTable: "Message",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__Notificat__user___44FF419A",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelMembership_channel_id",
                table: "ChannelMembership",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelMembership_user_id",
                table: "ChannelMembership",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Message_channel_id",
                table: "Message",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_Message_user_id",
                table: "Message",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_message_id",
                table: "Notification",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_user_id",
                table: "Notification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_file_id",
                table: "Users",
                column: "file_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelMembership");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "Channel");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "FileUploads");
        }
    }
}
