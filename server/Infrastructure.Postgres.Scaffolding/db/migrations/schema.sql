DROP SCHEMA IF EXISTS meetyourplants CASCADE;
CREATE SCHEMA IF NOT EXISTS meetyourplants;
SET search_path TO meetyourplants;

CREATE TABLE IF NOT EXISTS meetyourplants."User" (
                                               UserId UUID PRIMARY KEY,
                                               FirstName VARCHAR(100),
    LastName VARCHAR(100),
    Email VARCHAR(255) UNIQUE NOT NULL,
    Birthday DATE,
    Country VARCHAR(100),
    Role VARCHAR(50),
    Salt VARCHAR(255) NOT NULL,
    Hash VARCHAR(255) NOT NULL
    );

CREATE TABLE IF NOT EXISTS meetyourplants.Weather (
                                                UserId UUID PRIMARY KEY REFERENCES meetyourplants."User"(UserId) ON DELETE CASCADE,
    City VARCHAR(100),
    Country VARCHAR(100)
    );

CREATE TABLE IF NOT EXISTS meetyourplants.UserSettings (
                                                     UserId UUID PRIMARY KEY REFERENCES meetyourplants."User"(UserId) ON DELETE CASCADE,
    Celsius BOOLEAN,
    DarkTheme BOOLEAN,
    ConfirmDialog BOOLEAN,
    SecretMode BOOLEAN,
    WaitTime VARCHAR(50)
    );

CREATE TABLE IF NOT EXISTS meetyourplants.Plant (
                                              PlantID UUID PRIMARY KEY,
                                              Planted DATE,
                                              PlantName VARCHAR(100),
    PlantType VARCHAR(100),
    PlantNotes TEXT,
    LastWatered DATE,
    WaterEvery INT,
    IsDead BOOLEAN
    );

CREATE TABLE IF NOT EXISTS meetyourplants.UserPlant (
                                                  UserID UUID REFERENCES meetyourplants."User"(UserId) ON DELETE CASCADE,
    PlantID UUID REFERENCES meetyourplants.Plant(PlantID) ON DELETE CASCADE,
    PRIMARY KEY (UserID, PlantID)
    );

CREATE TABLE IF NOT EXISTS meetyourplants.Alert (
                                              AlertID UUID PRIMARY KEY,
                                              AlertUserId UUID REFERENCES meetyourplants."User"(UserId) ON DELETE CASCADE,
    AlertName VARCHAR(100),
    AlertDesc TEXT,
    AlertTime TIMESTAMP,
    AlertPlant UUID REFERENCES meetyourplants.Plant(PlantID) ON DELETE SET NULL
    );

CREATE TABLE IF NOT EXISTS meetyourplants.SensorHistory (
                                                HistoryId UUID REFERENCES meetyourplants."User"(UserId) ON DELETE CASCADE,
    DeviceId VARCHAR(100),
    Temperature INT,
    Humidity INT,
    AirPressure INT,
    AirQuality INT,
    Time TIMESTAMP,
    PRIMARY KEY (HistoryId, Time)
    );
