ALTER TABLE "AspNetUsers" ADD COLUMN "Cin" character varying(255) NULL;
ALTER TABLE "AspNetUsers" ADD COLUMN "Major" character varying(255) NULL;

CREATE TABLE "UserPrograms" (
    "UserId" text NOT NULL,
    "ProgramId" integer NOT NULL,
    CONSTRAINT "PK_UserPrograms" PRIMARY KEY ("UserId", "ProgramId"),
    CONSTRAINT "FK_UserPrograms_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserPrograms_Programs_ProgramId" FOREIGN KEY ("ProgramId") REFERENCES "Programs" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_UserPrograms_ProgramId" ON "UserPrograms" ("ProgramId");
