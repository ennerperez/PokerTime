﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PokerTime.Persistence.Migrations {
    public partial class InitialCreate : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "PredefinedParticipantColor",
                columns: table => new {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Color_R = table.Column<byte>(nullable: true),
                    Color_G = table.Column<byte>(nullable: true),
                    Color_B = table.Column<byte>(nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_PredefinedParticipantColor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Symbol",
                columns: table => new {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ValueAsNumber = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Symbol", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Estimation",
                columns: table => new {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserStoryId = table.Column<int>(nullable: false),
                    ParticipantId = table.Column<int>(nullable: false),
                    SymbolId = table.Column<int>(nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Estimation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Estimation_Symbol_SymbolId",
                        column: x => x.SymbolId,
                        principalTable: "Symbol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Participant",
                columns: table => new {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Color_R = table.Column<byte>(nullable: true),
                    Color_G = table.Column<byte>(nullable: true),
                    Color_B = table.Column<byte>(nullable: true),
                    SessionId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    IsFacilitator = table.Column<bool>(nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Participant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserStory",
                columns: table => new {
                    Id = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_UserStory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Session",
                columns: table => new {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrlId_StringId = table.Column<string>(unicode: false, maxLength: 32, nullable: true),
                    CurrentStage = table.Column<int>(nullable: false),
                    HashedPassphrase = table.Column<string>(unicode: false, fixedLength: true, maxLength: 64, nullable: true),
                    CurrentUserStoryId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(maxLength: 256, nullable: false),
                    FacilitatorHashedPassphrase = table.Column<string>(unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    CreationTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Session", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Session_UserStory_CurrentUserStoryId",
                        column: x => x.CurrentUserStoryId,
                        principalTable: "UserStory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Estimation_ParticipantId",
                table: "Estimation",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Estimation_SymbolId",
                table: "Estimation",
                column: "SymbolId");

            migrationBuilder.CreateIndex(
                name: "IX_Estimation_UserStoryId",
                table: "Estimation",
                column: "UserStoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Participant_SessionId",
                table: "Participant",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_CurrentUserStoryId",
                table: "Session",
                column: "CurrentUserStoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_UrlId_StringId",
                table: "Session",
                column: "UrlId_StringId",
                unique: true,
                filter: "[UrlId_StringId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Estimation_Participant_ParticipantId",
                table: "Estimation",
                column: "ParticipantId",
                principalTable: "Participant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Estimation_UserStory_UserStoryId",
                table: "Estimation",
                column: "UserStoryId",
                principalTable: "UserStory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Participant_Session_SessionId",
                table: "Participant",
                column: "SessionId",
                principalTable: "Session",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserStory_Session_Id",
                table: "UserStory",
                column: "Id",
                principalTable: "Session",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_UserStory_CurrentUserStoryId",
                table: "Session");

            migrationBuilder.DropTable(
                name: "Estimation");

            migrationBuilder.DropTable(
                name: "PredefinedParticipantColor");

            migrationBuilder.DropTable(
                name: "Participant");

            migrationBuilder.DropTable(
                name: "Symbol");

            migrationBuilder.DropTable(
                name: "UserStory");

            migrationBuilder.DropTable(
                name: "Session");
        }
    }
}
