-- This schema is generated based on the current DBContext. Please check the class Seeder to see.
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'botanica') THEN
        CREATE SCHEMA botanica;
    END IF;
END $EF$;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'weatherstation') THEN
        CREATE SCHEMA weatherstation;
    END IF;
END $EF$;


CREATE TABLE weatherstation.devicelog (
    id text NOT NULL,
    deviceid text NOT NULL,
    value numeric NOT NULL,
    unit text NOT NULL,
    timestamp timestamp with time zone NOT NULL,
    CONSTRAINT devicelog_pkey PRIMARY KEY (id)
);


CREATE TABLE botanica."Plant" (
    "PlantID" uuid NOT NULL,
    "Planted" timestamp with time zone,
    "PlantName" text NOT NULL,
    "PlantType" text NOT NULL,
    "PlantNotes" text NOT NULL,
    "LastWatered" timestamp with time zone,
    "WaterEvery" integer,
    "IsDead" boolean NOT NULL,
    CONSTRAINT "PK_Plant" PRIMARY KEY ("PlantID")
);


CREATE TABLE botanica."User" (
    "UserId" uuid NOT NULL,
    "Hash" text NOT NULL,
    "Salt" text NOT NULL,
    "FirstName" text NOT NULL,
    "LastName" text NOT NULL,
    "Email" text NOT NULL,
    "Birthday" timestamp with time zone,
    "Country" text NOT NULL,
    "Role" text NOT NULL,
    CONSTRAINT "PK_User" PRIMARY KEY ("UserId")
);


CREATE TABLE botanica."Alert" (
    "AlertID" uuid NOT NULL,
    "AlertUserId" uuid NOT NULL,
    "AlertName" text NOT NULL,
    "AlertDesc" text NOT NULL,
    "AlertTime" timestamp with time zone NOT NULL,
    "AlertPlant" uuid,
    CONSTRAINT "PK_Alert" PRIMARY KEY ("AlertID"),
    CONSTRAINT "FK_Alert_Plant_AlertPlant" FOREIGN KEY ("AlertPlant") REFERENCES botanica."Plant" ("PlantID") ON DELETE SET NULL,
    CONSTRAINT "FK_Alert_User_AlertUserId" FOREIGN KEY ("AlertUserId") REFERENCES botanica."User" ("UserId") ON DELETE CASCADE
);


CREATE TABLE botanica."SensorHistory" (
    "HistoryId" uuid NOT NULL,
    "Time" timestamp with time zone NOT NULL,
    "DeviceId" text NOT NULL,
    "Temperature" integer NOT NULL,
    "Humidity" integer NOT NULL,
    "AirPressure" integer NOT NULL,
    "AirQuality" integer NOT NULL,
    CONSTRAINT "PK_SensorHistory" PRIMARY KEY ("HistoryId", "Time"),
    CONSTRAINT "FK_SensorHistory_User_HistoryId" FOREIGN KEY ("HistoryId") REFERENCES botanica."User" ("UserId") ON DELETE CASCADE
);


CREATE TABLE botanica."UserPlant" (
    "UserID" uuid NOT NULL,
    "PlantID" uuid NOT NULL,
    CONSTRAINT "PK_UserPlant" PRIMARY KEY ("UserID", "PlantID"),
    CONSTRAINT "FK_UserPlant_Plant_PlantID" FOREIGN KEY ("PlantID") REFERENCES botanica."Plant" ("PlantID") ON DELETE CASCADE,
    CONSTRAINT "FK_UserPlant_User_UserID" FOREIGN KEY ("UserID") REFERENCES botanica."User" ("UserId") ON DELETE CASCADE
);


CREATE TABLE botanica."UserSettings" (
    "UserId" uuid NOT NULL,
    "Celsius" boolean NOT NULL,
    "DarkTheme" boolean NOT NULL,
    "ConfirmDialog" boolean NOT NULL,
    "SecretMode" boolean NOT NULL,
    "WaitTime" text NOT NULL,
    CONSTRAINT "PK_UserSettings" PRIMARY KEY ("UserId"),
    CONSTRAINT "FK_UserSettings_User_UserId" FOREIGN KEY ("UserId") REFERENCES botanica."User" ("UserId") ON DELETE CASCADE
);


CREATE TABLE botanica."Weather" (
    "UserId" uuid NOT NULL,
    "City" text NOT NULL,
    "Country" text NOT NULL,
    CONSTRAINT "PK_Weather" PRIMARY KEY ("UserId"),
    CONSTRAINT "FK_Weather_User_UserId" FOREIGN KEY ("UserId") REFERENCES botanica."User" ("UserId") ON DELETE CASCADE
);


CREATE INDEX "IX_Alert_AlertPlant" ON botanica."Alert" ("AlertPlant");


CREATE INDEX "IX_Alert_AlertUserId" ON botanica."Alert" ("AlertUserId");


CREATE INDEX "IX_UserPlant_PlantID" ON botanica."UserPlant" ("PlantID");


