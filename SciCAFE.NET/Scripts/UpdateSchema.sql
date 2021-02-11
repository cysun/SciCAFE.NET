ALTER TABLE "Events" ADD COLUMN "LengthHours" integer NOT NULL DEFAULT 1;
ALTER TABLE "Events" ADD COLUMN "LengthMinutes" integer NOT NULL DEFAULT 0;

UPDATE "Events" SET "LengthHours" = FLOOR(EXTRACT(EPOCH FROM ("EndTime"-"StartTime"))/60/60);
UPDATE "Events" SET "LengthMinutes" = CAST(EXTRACT(EPOCH FROM ("EndTime"-"StartTime"))/60 as INTEGER)%60;

ALTER TABLE "Events" DROP COLUMN "EndTime";
