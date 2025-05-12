using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RideWild.Migrations
{
    /// <inheritdoc />
    public partial class CreateLogTableAndDeletedLogErrorsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Usa un blocco SQL per eliminare LogError solo se esiste
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'LogError', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [LogError];
                END;
            ");

            // Usa un blocco SQL per creare Logs solo se non esiste
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Logs', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Logs] (
                        [Id] int NOT NULL IDENTITY,
                        [Message] nvarchar(max) NOT NULL,
                        [MessageTemplate] nvarchar(max) NOT NULL,
                        [Level] nvarchar(128) NULL,
                        [TimeStamp] datetimeoffset NOT NULL,
                        [Exception] nvarchar(max) NULL,
                        [Properties] nvarchar(max) NULL,
                        [LogEvent] nvarchar(max) NULL,
                        CONSTRAINT [PK_Logs] PRIMARY KEY ([Id])
                    );
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Usa un blocco SQL per eliminare Logs solo se esiste
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Logs', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [Logs];
                END;
            ");

            // Usa un blocco SQL per ricreare LogError solo se non esiste
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'LogError', N'U') IS NULL
                BEGIN
                    CREATE TABLE [LogError] (
                        [Id] int NOT NULL IDENTITY,
                        [ActionName] nvarchar(50) NOT NULL,
                        [ClassName] nvarchar(255) NOT NULL,
                        [Line] int NOT NULL,
                        [Message] text NOT NULL,
                        [MethodName] nvarchar(50) NOT NULL,
                        [Time] datetime NOT NULL,
                        CONSTRAINT [logerror_id_primary] PRIMARY KEY ([Id])
                    );
                END;
            ");
        }
    }
}