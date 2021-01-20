﻿CREATE TABLE "Files" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(1000) NOT NULL,
    "ContentType" character varying(255) NULL,
    "Size" bigint NOT NULL,
    "Created" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_Files" PRIMARY KEY ("Id")
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

CREATE TABLE "EventAttachments" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "EventId" integer NOT NULL,
    "FileId" integer NOT NULL,
    CONSTRAINT "PK_EventAttachments" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_EventAttachments_EventId_FileId" UNIQUE ("EventId", "FileId"),
    CONSTRAINT "FK_EventAttachments_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_EventAttachments_Files_FileId" FOREIGN KEY ("FileId") REFERENCES "Files" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_EventAttachments_FileId" ON "EventAttachments" ("FileId");
CREATE INDEX "IX_RewardAttachments_FileId" ON "RewardAttachments" ("FileId");