DROP SCHEMA IF EXISTS meetyourplants CASCADE;
CREATE SCHEMA IF NOT EXISTS meetyourplants;
SET search_path TO meetyourplants;

CREATE TABLE IF NOT EXISTS meetyourplants."User" (
                                                     "UserId" UUID PRIMARY KEY,
                                                     "FirstName" VARCHAR(100),
                                                     "LastName" VARCHAR(100),
                                                     "Email" VARCHAR(255) UNIQUE NOT NULL,
                                                     "Birthday" DATE,
                                                     "Country" VARCHAR(100),
                                                     "Role" VARCHAR(50),
                                                     "Salt" VARCHAR(255) NOT NULL,
                                                     "Hash" VARCHAR(255) NOT NULL
);

CREATE TABLE IF NOT EXISTS meetyourplants."Weather" (
                                                        "UserId" UUID PRIMARY KEY REFERENCES meetyourplants."User"("UserId") ON DELETE CASCADE,
                                                        "City" VARCHAR(100),
                                                        "Country" VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS meetyourplants."UserSettings" (
                                                             "UserId" UUID PRIMARY KEY REFERENCES meetyourplants."User"("UserId") ON DELETE CASCADE,
                                                             "Celsius" BOOLEAN,
                                                             "DarkTheme" BOOLEAN,
                                                             "ConfirmDialog" BOOLEAN,
                                                             "SecretMode" BOOLEAN
);

CREATE TABLE IF NOT EXISTS meetyourplants."Plant" (
                                                      "PlantId" UUID PRIMARY KEY,
                                                      "Planted" DATE,
                                                      "PlantName" VARCHAR(100),
                                                      "PlantType" VARCHAR(100),
                                                      "PlantNotes" TEXT,
                                                      "LastWatered" DATE,
                                                      "WaterEvery" INT,
                                                      "IsDead" BOOLEAN
);

CREATE TABLE IF NOT EXISTS meetyourplants."UserPlant" (
                                                          "UserId" UUID REFERENCES meetyourplants."User"("UserId") ON DELETE CASCADE,
                                                          "PlantId" UUID REFERENCES meetyourplants."Plant"("PlantId") ON DELETE CASCADE,
                                                          PRIMARY KEY ("UserId", "PlantId")
);

CREATE TABLE IF NOT EXISTS meetyourplants."Alert" (
                                                      "AlertId" UUID PRIMARY KEY,
                                                      "AlertUserId" UUID REFERENCES meetyourplants."User"("UserId") ON DELETE CASCADE,
                                                      "AlertName" VARCHAR(100),
                                                      "AlertDesc" TEXT,
                                                      "AlertTime" TIMESTAMP,
                                                      "AlertPlant" UUId REFERENCES meetyourplants."Plant"("PlantId") ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS meetyourplants."UserDevice" (
                                                           "DeviceId" UUID PRIMARY KEY,
                                                           "UserId" UUID REFERENCES meetyourplants."User"("UserId") ON DELETE CASCADE,
                                                           "DeviceName" VARCHAR(100),
                                                           "DeviceDescription" TEXT,
                                                           "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                                           "WaitTime" VARCHAR(50)
);


CREATE TABLE IF NOT EXISTS meetyourplants."SensorHistory" (
                                                              "SensorHistoryId" UUID PRIMARY KEY,
                                                              "DeviceId" UUID REFERENCES meetyourplants."UserDevice"("DeviceId") ON DELETE CASCADE,
                                                              "Temperature" FLOAT,
                                                              "Humidity" FLOAT,
                                                              "AirPressure" FLOAT,
                                                              "AirQuality" INT,
                                                              "Time" TIMESTAMP
);

CREATE TABLE IF NOT EXISTS meetyourplants."EmailList"
(
    "Id" SERIAL PRIMARY KEY,
    "Email" VARCHAR(255) UNIQUE NOT NULL
);


