CREATE UNIQUE INDEX "UserEmailIndex" ON "AspNetUsers" (LOWER("Email") varchar_pattern_ops);
CREATE INDEX "UserFirstNameIndex" ON "AspNetUsers" (LOWER("FirstName") varchar_pattern_ops);
CREATE INDEX "UserLastNameIndex" ON "AspNetUsers" (LOWER("LastName") varchar_pattern_ops);
CREATE INDEX "UserFullNameIndex" ON "AspNetUsers" (LOWER("FirstName" || ' ' || "LastName") varchar_pattern_ops);

CREATE OR REPLACE FUNCTION "SearchUsersByPrefix"(prefix varchar) RETURNS SETOF "AspNetUsers" AS
$BODY$
BEGIN
    RETURN QUERY SELECT * FROM "AspNetUsers" WHERE
        LOWER("FirstName") LIKE prefix || '%' OR
        LOWER("LastName") LIKE prefix || '%' OR
        LOWER("FirstName" || ' ' || "LastName") LIKE prefix || '%' OR
        LOWER("Email") LIKE prefix || '%'
        ORDER BY "FirstName", "LastName";
    RETURN;
 END
$BODY$
LANGUAGE plpgsql;

CREATE INDEX "EventNameIndex" ON "Events" (LOWER("Name")  varchar_pattern_ops);

ALTER TABLE "Events" ADD COLUMN tsv tsvector;
CREATE INDEX "EventTsIndex" ON "Events" USING GIN(tsv);
CREATE TRIGGER "EventTsTrigger"
    BEFORE INSERT OR UPDATE ON "Events"
    FOR EACH ROW EXECUTE FUNCTION
    tsvector_update_trigger(tsv, 'pg_catalog.english', "Name");

CREATE OR REPLACE FUNCTION "SearchEvents"(q varchar) RETURNS SETOF "Events" AS
$BODY$
BEGIN
    RETURN QUERY SELECT * FROM "Events" WHERE
        "Review_IsApproved" = TRUE AND (LOWER("Name") LIKE q || '%' OR plainto_tsquery(q) @@ tsv)
        LIMIT 10;
    RETURN;
 END
$BODY$
LANGUAGE plpgsql;

INSERT INTO "Programs" ("Name", "ShortName", "Description", "Website") VALUES
    ('Louis Stokes Alliance for Minority Participation', 'LSAMP', '<p>California State University –
        Louis Stokes Alliance for Minority Participation (CSU-LSAMP) is a comprehensive, statewide program
        dedicated to broadening participation in science, technology, engineering, and mathematics (STEM) disciplines.</p>',
        'https://www.calstatela.edu/lsamp');
INSERT INTO "Programs" ("Name", "ShortName", "Description", "Website") VALUES
    ('Minority Access to Research Careers-Undergraduate Student Training for Academic Research', 'MARC-U*STAR',
        '<p>The Minority Access to Research Careers-Undergraduate Student Training in Academic Research (MARC-U*STAR)
        Program at Cal State LA is the premier undergraduate honors research training program on campus. Participation
        in MARC-U*STAR is an exceptional opportunity to enhance your academic and professional career through involvement
        in contemporary biomedical research under the guidance of faculty research directors.</p>',
        'https://www.calstatela.edu/centers/moreprograms/ustar/index.htm');
INSERT INTO "Programs" ("Name", "ShortName", "Description", "Website") VALUES
    ('Chemistry and Biochemistry Club at Cal State LA', 'Chemistry & Biochemistry Club', '<p>Chemistry & Biochemistry Club
        at Cal State LA are dedicated to helping all majors succeed both professionally and academically by offering tutoring,
        social events, outreach & volunteering opportunities, invited speakers, and workshops!</p>',
        'https://www.calstatela.edu/orgs/chemistryandbiochemistry');
INSERT INTO "Programs" ("Name", "ShortName", "Description", "Website") VALUES
    ('Cecilia I. Zurita-Lopez Laboratory', 'CZL Lab', '<p>Cecilia I. Zurita-Lopez Laboratory investigates protein arginine
        methyltransferases (PRMTs), enzymes that post-translationally modify (PTM) proteins by adding a methyl group to
        arginine residues.</p>',
        'https://www.calstatela.edu/faculty/zuritalopezlab');

INSERT INTO "Categories" ("Name", "AdditionalInfo") VALUES
    ('General Meeting', null);
INSERT INTO "Categories" ("Name", "AdditionalInfo") VALUES
    ('Guest Speaker', null);
INSERT INTO "Categories" ("Name", "AdditionalInfo") VALUES
    ('Information Session', 'seminar, colloquium, training, webinar, workshop');
INSERT INTO "Categories" ("Name", "AdditionalInfo") VALUES
    ('Conference', 'symposium, retreat, summit');
INSERT INTO "Categories" ("Name", "AdditionalInfo") VALUES
    ('Trip', 'tour, off-campus conference, hike');
INSERT INTO "Categories" ("Name", "AdditionalInfo") VALUES
    ('Fundraiser', null);
INSERT INTO "Categories" ("Name", "AdditionalInfo") VALUES
    ('Celebration', 'ceremony, banquet, festival, happy hour');
INSERT INTO "Categories" ("Name", "AdditionalInfo") VALUES
    ('Performance', 'dance, concert, recital, open mic');
INSERT INTO "Categories" ("Name", "AdditionalInfo") VALUES
    ('Competition', 'sporting event, tournament');
INSERT INTO "Categories" ("Name", "AdditionalInfo") VALUES
    ('Exhibit', 'exposition, film screening');
INSERT INTO "Categories" ("Name", "AdditionalInfo") VALUES
    ('Election', null);
INSERT INTO "Categories" ("Name", "AdditionalInfo") VALUES
    ('Other', null);

INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Community Building & Celebration', '<p>Recognizing our campus community''s rich diversity, these events help
        us to build community and engage in shared celebrations together.</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Leadership Development', '<p>Expand your leadership skills and talents to propel you as a leader in organizations,
        the classroom, and the community. Includes topics on mentoring others. Includes managing projects and time,
        setting goals, monitoring results.</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Service', '<p>Serve others and gain awareness of specific issues as you support and transform your community
        through these service opportunities.</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Golden Eagle Spirit', '<p>Learn how to soar like the Cal State LA Golden Eagles you are through these events
        and activities designed to raise campus spirit and pride! Examples include annual celebrations, department
        traditions, and campus anniversaries.</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Critical Dialogues', '<p>These events encourage students to engage in critical dialogue and ask questions to
        gain a deeper understanding of the experiences of others while potentially identifying root causes, consequences,
        and possible solutions to community issues.</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Music & Entertainment', '<p>Enrich your student experience with musical performances, talented entertainers, movie
        nights and night markets.</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Holistic Health & Wellness', '<p>Learn more about your role in enhancing your personal well-being and that of our
        community through unique collective and selfcare practices.</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Social Identity Exploration', '<p>Deepen the awareness and understanding of yourself. Explore who you are and how
        your multifaceted identities exist in different spaces. Includes events that celebrate your heritage.</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Recreation', '<p>Take time for a break and enjoy these recreational events and activities.</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Trips', '<p>Take a trip and explore what fun activities LA and the surrounding areas have in store for you!</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Scholarship', '<p>These events will make you smarter! Learn about a new topic related to your major or interest
        by attending presentations on original research. Includes topics that support content knowledge related to your
        discipline or end career goal.</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Scientific Identity Exploration', '<p>What are the characteristics of a scientist? These events will encourage
        you to review the scientific method, urge you to question current assumptions, highlight a specific research
        technique or methodology, expose you to the analysis of data. Also includes events that promote scientific
        literacy (critical reading of scientific literature).</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Academic and Social Integrity', '<p>What is the responsible thing to do? What is the ethical thing to do? These
        events include responsible conduct in social situations and in research, identifying conflicts of interest and
        ways to mitigate misconduct.</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Career Development', '<p>What careers are out there? What do I have to do to get a good job? Includes resume, CV
        and interview workshops; includes ‘how to apply to…’ workshops. Also includes events that expose you to career
        opportunities such as graduate and internship fairs, and fellowship / scholarship orientations. </p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Professional Development', '<p>How can I develop skills that will help me reach my career goal? Workplace etiquette,
        performance standards, cultural competence, teamwork. Organizational skills. Also includes written and oral
        communication, and quantitative skills development. Includes workshops on time management and procrastination.</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('College and Academic Success', '<p>Improve your GPA. Tips for academic success and exposure to resources offered by
        our campus to maximize your time here. Includes academic advisement and/or orientations to an affiliation such as
        a program, center or department. Includes FAFSA, thesis and new student workshops. Includes software tutorials,
        academic tutoring, and instructional workshops.</p>');
INSERT INTO "Themes" ("Name", "Description") VALUES
    ('Fundraisers', '<p>Support "locally grown" affiliations including student organizations or programs. Includes food
        drives, blood drives, and run/walk events.</p>');
