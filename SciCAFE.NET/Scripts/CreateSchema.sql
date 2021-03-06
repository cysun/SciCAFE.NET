﻿CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "AspNetRoles" (
    "Id" text NOT NULL,
    "Name" character varying(256) NULL,
    "NormalizedName" character varying(256) NULL,
    "ConcurrencyStamp" text NULL,
    CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
);

CREATE TABLE "AspNetUsers" (
    "Id" text NOT NULL,
    "FirstName" character varying(255) NOT NULL,
    "LastName" character varying(255) NOT NULL,
    "IsAdministrator" boolean NOT NULL,
    "IsEventOrganizer" boolean NOT NULL,
    "IsEventReviewer" boolean NOT NULL,
    "IsRewardProvider" boolean NOT NULL,
    "IsRewardReviewer" boolean NOT NULL,
    "Cin" character varying(255) NULL,
    "Major" character varying(255) NULL,
    "UserName" character varying(256) NULL,
    "NormalizedUserName" character varying(256) NULL,
    "Email" character varying(256) NULL,
    "NormalizedEmail" character varying(256) NULL,
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text NULL,
    "SecurityStamp" text NULL,
    "ConcurrencyStamp" text NULL,
    "PhoneNumber" text NULL,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone NULL,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
);

CREATE TABLE "Categories" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(255) NOT NULL,
    "AdditionalInfo" character varying(255) NULL,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
);

CREATE TABLE "Files" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(1000) NOT NULL,
    "ContentType" character varying(255) NULL,
    "Size" bigint NOT NULL,
    "Created" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_Files" PRIMARY KEY ("Id")
);

CREATE TABLE "Programs" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(255) NOT NULL,
    "ShortName" character varying(50) NOT NULL,
    "Description" text NULL,
    "Website" character varying(255) NULL,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT "PK_Programs" PRIMARY KEY ("Id")
);

CREATE TABLE "Themes" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(255) NOT NULL,
    "Description" text NULL,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT "PK_Themes" PRIMARY KEY ("Id")
);

CREATE TABLE "AspNetRoleClaims" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "RoleId" text NOT NULL,
    "ClaimType" text NULL,
    "ClaimValue" text NULL,
    CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserClaims" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "UserId" text NOT NULL,
    "ClaimType" text NULL,
    "ClaimValue" text NULL,
    CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text NULL,
    "UserId" text NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserRoles" (
    "UserId" text NOT NULL,
    "RoleId" text NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
    "UserId" text NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Rewards" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(255) NOT NULL,
    "Description" text NULL,
    "NumOfEventsToQualify" integer NOT NULL DEFAULT 1,
    "CreatorId" text NULL,
    "SubmitDate" timestamp without time zone NULL,
    "ExpireDate" timestamp without time zone NULL,
    "Review_IsApproved" boolean NULL,
    "Review_Comments" text NULL,
    "Review_Timestamp" timestamp without time zone NULL,
    "Review_ReviewerId" text NULL,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Rewards" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Rewards_AspNetUsers_CreatorId" FOREIGN KEY ("CreatorId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Rewards_AspNetUsers_Review_ReviewerId" FOREIGN KEY ("Review_ReviewerId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Events" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(255) NOT NULL,
    "Location" character varying(255) NULL,
    "Description" text NOT NULL,
    "StartTime" timestamp without time zone NOT NULL,
    "LengthHours" integer NOT NULL,
    "LengthMinutes" integer NOT NULL,
    "CategoryId" integer NULL,
    "TargetAudience" character varying(255) NULL,
    "CoreCompetency" character varying(255) NULL,
    "CreatorId" text NULL,
    "SubmitDate" timestamp without time zone NULL,
    "Review_IsApproved" boolean NULL,
    "Review_Comments" text NULL,
    "Review_Timestamp" timestamp without time zone NULL,
    "Review_ReviewerId" text NULL,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Events" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Events_AspNetUsers_CreatorId" FOREIGN KEY ("CreatorId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Events_AspNetUsers_Review_ReviewerId" FOREIGN KEY ("Review_ReviewerId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Events_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "UserPrograms" (
    "UserId" text NOT NULL,
    "ProgramId" integer NOT NULL,
    CONSTRAINT "PK_UserPrograms" PRIMARY KEY ("UserId", "ProgramId"),
    CONSTRAINT "FK_UserPrograms_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserPrograms_Programs_ProgramId" FOREIGN KEY ("ProgramId") REFERENCES "Programs" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RewardAttachments" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "RewardId" integer NOT NULL,
    "FileId" integer NOT NULL,
    CONSTRAINT "PK_RewardAttachments" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_RewardAttachments_RewardId_FileId" UNIQUE ("RewardId", "FileId"),
    CONSTRAINT "FK_RewardAttachments_Files_FileId" FOREIGN KEY ("FileId") REFERENCES "Files" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RewardAttachments_Rewards_RewardId" FOREIGN KEY ("RewardId") REFERENCES "Rewards" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Attendances" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "EventId" integer NOT NULL,
    "AttendeeId" text NOT NULL,
    CONSTRAINT "PK_Attendances" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_Attendances_EventId_AttendeeId" UNIQUE ("EventId", "AttendeeId"),
    CONSTRAINT "FK_Attendances_AspNetUsers_AttendeeId" FOREIGN KEY ("AttendeeId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Attendances_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE
);

CREATE TABLE "EventAttachments" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "EventId" integer NOT NULL,
    "FileId" integer NOT NULL,
    CONSTRAINT "PK_EventAttachments" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_EventAttachments_EventId_FileId" UNIQUE ("EventId", "FileId"),
    CONSTRAINT "FK_EventAttachments_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_EventAttachments_Files_FileId" FOREIGN KEY ("FileId") REFERENCES "Files" ("Id") ON DELETE CASCADE
);

CREATE TABLE "EventPrograms" (
    "EventId" integer NOT NULL,
    "ProgramId" integer NOT NULL,
    CONSTRAINT "PK_EventPrograms" PRIMARY KEY ("EventId", "ProgramId"),
    CONSTRAINT "FK_EventPrograms_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_EventPrograms_Programs_ProgramId" FOREIGN KEY ("ProgramId") REFERENCES "Programs" ("Id") ON DELETE CASCADE
);

CREATE TABLE "EventThemes" (
    "EventId" integer NOT NULL,
    "ThemeId" integer NOT NULL,
    CONSTRAINT "PK_EventThemes" PRIMARY KEY ("EventId", "ThemeId"),
    CONSTRAINT "FK_EventThemes_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_EventThemes_Themes_ThemeId" FOREIGN KEY ("ThemeId") REFERENCES "Themes" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RewardEvents" (
    "RewardId" integer NOT NULL,
    "EventId" integer NOT NULL,
    CONSTRAINT "PK_RewardEvents" PRIMARY KEY ("RewardId", "EventId"),
    CONSTRAINT "FK_RewardEvents_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RewardEvents_Rewards_RewardId" FOREIGN KEY ("RewardId") REFERENCES "Rewards" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

CREATE INDEX "IX_Attendances_AttendeeId" ON "Attendances" ("AttendeeId");

CREATE UNIQUE INDEX "IX_Categories_Name" ON "Categories" ("Name");

CREATE INDEX "IX_EventAttachments_FileId" ON "EventAttachments" ("FileId");

CREATE INDEX "IX_EventPrograms_ProgramId" ON "EventPrograms" ("ProgramId");

CREATE INDEX "IX_Events_CategoryId" ON "Events" ("CategoryId");

CREATE INDEX "IX_Events_CreatorId" ON "Events" ("CreatorId");

CREATE INDEX "IX_Events_Name" ON "Events" ("Name");

CREATE INDEX "IX_Events_Review_ReviewerId" ON "Events" ("Review_ReviewerId");

CREATE INDEX "IX_EventThemes_ThemeId" ON "EventThemes" ("ThemeId");

CREATE INDEX "IX_RewardAttachments_FileId" ON "RewardAttachments" ("FileId");

CREATE INDEX "IX_RewardEvents_EventId" ON "RewardEvents" ("EventId");

CREATE INDEX "IX_Rewards_CreatorId" ON "Rewards" ("CreatorId");

CREATE INDEX "IX_Rewards_Review_ReviewerId" ON "Rewards" ("Review_ReviewerId");

CREATE UNIQUE INDEX "IX_Themes_Name" ON "Themes" ("Name");

CREATE INDEX "IX_UserPrograms_ProgramId" ON "UserPrograms" ("ProgramId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210302182215_InitialSchema', '5.0.2');

COMMIT;

