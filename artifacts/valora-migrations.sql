CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE recurrence_rules (
    "Id" uuid NOT NULL,
    kind character varying(16) NOT NULL,
    description character varying(300) NOT NULL,
    start_month integer NOT NULL,
    end_month integer,
    CONSTRAINT "PK_recurrence_rules" PRIMARY KEY ("Id")
);

CREATE TABLE recurrence_rule_versions (
    "Id" uuid NOT NULL,
    rule_id uuid NOT NULL,
    effective_from_month integer NOT NULL,
    kind character varying(16) NOT NULL,
    description character varying(300) NOT NULL,
    status character varying(16) NOT NULL,
    value_mode character varying(24) NOT NULL,
    default_amount numeric(29,2),
    CONSTRAINT "PK_recurrence_rule_versions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_recurrence_rule_versions_recurrence_rules_rule_id" FOREIGN KEY (rule_id) REFERENCES recurrence_rules ("Id") ON DELETE CASCADE
);

CREATE TABLE entries (
    "Id" uuid NOT NULL,
    month_key integer NOT NULL,
    kind character varying(16) NOT NULL,
    description character varying(300) NOT NULL,
    amount numeric(29,2),
    status character varying(24) NOT NULL,
    recurrence_rule_id uuid,
    recurrence_rule_version_id uuid,
    note character varying(2000),
    CONSTRAINT "PK_entries" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_entries_recurrence_rule_versions_recurrence_rule_version_id" FOREIGN KEY (recurrence_rule_version_id) REFERENCES recurrence_rule_versions ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_entries_recurrence_rules_recurrence_rule_id" FOREIGN KEY (recurrence_rule_id) REFERENCES recurrence_rules ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_entries_month_key" ON entries (month_key);

CREATE UNIQUE INDEX "IX_entries_recurrence_rule_id_month_key" ON entries (recurrence_rule_id, month_key) WHERE "recurrence_rule_id" IS NOT NULL;

CREATE INDEX "IX_entries_recurrence_rule_version_id" ON entries (recurrence_rule_version_id);

CREATE UNIQUE INDEX "IX_recurrence_rule_versions_rule_id_effective_from_month" ON recurrence_rule_versions (rule_id, effective_from_month);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261111021831_InitialCreate', '10.0.14');

COMMIT;

START TRANSACTION;
ALTER TABLE recurrence_rule_versions ALTER COLUMN default_amount TYPE numeric(31,2);

ALTER TABLE entries ALTER COLUMN amount TYPE numeric(31,2);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261111022634_UseDecimalMaxPrecision', '10.0.14');

COMMIT;

