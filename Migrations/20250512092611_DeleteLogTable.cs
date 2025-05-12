using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RideWild.Migrations
{
    /// <inheritdoc />
    public partial class DeleteLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Usa un blocco SQL per eliminare Logs solo se esiste
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Logs', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [Logs];
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Usa un blocco SQL per ricreare Logs solo se non esiste
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Logs', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Logs] (
                        [Id] int NOT NULL IDENTITY,
                        [Exception] nvarchar(max) NULL,
                        [Level] nvarchar(128) NULL,
                        [LogEvent] nvarchar(max) NULL,
                        [Message] nvarchar(max) NOT NULL,
                        [MessageTemplate] nvarchar(max) NOT NULL,
                        [Properties] nvarchar(max) NULL,
                        [TimeStamp] datetimeoffset NOT NULL,
                        CONSTRAINT [PK_Logs] PRIMARY KEY ([Id])
                    );
                END;
            ");
        }
    }
}