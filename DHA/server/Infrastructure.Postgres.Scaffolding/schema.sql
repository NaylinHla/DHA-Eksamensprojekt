DROP SCHEMA IF EXISTS botanica CASCADE;
CREATE SCHEMA IF NOT EXISTS botanica;
SET search_path TO botanica;

CREATE TABLE IF NOT EXISTS botanica."User" (
                                               UserId UUID PRIMARY KEY,
                                               FirstName VARCHAR(100),
    LastName VARCHAR(100),
    Email VARCHAR(255) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL,
    Birthday DATE,
    Country VARCHAR(100)
    );

CREATE TABLE IF NOT EXISTS botanica.Weather (
                                                UserId UUID PRIMARY KEY REFERENCES botanica."User"(UserId) ON DELETE CASCADE,
    City VARCHAR(100),
    Country VARCHAR(100)
    );

CREATE TABLE IF NOT EXISTS botanica.UserSettings (
                                                     UserId UUID PRIMARY KEY REFERENCES botanica."User"(UserId) ON DELETE CASCADE,
    Celsius BOOLEAN,
    DarkTheme BOOLEAN,
    ConfirmDialog BOOLEAN,
    SecretMode BOOLEAN,
    WaitTime VARCHAR(50)
    );

CREATE TABLE IF NOT EXISTS botanica.Plant (
                                              PlantID UUID PRIMARY KEY,
                                              Planted DATE,
                                              PlantName VARCHAR(100),
    PlantType VARCHAR(100),
    PlantNotes TEXT,
    LastWatered DATE,
    WaterEvery INT,
    IsDead BOOLEAN
    );

CREATE TABLE IF NOT EXISTS botanica.UserPlant (
                                                  UserID UUID REFERENCES botanica."User"(UserId) ON DELETE CASCADE,
    PlantID UUID REFERENCES botanica.Plant(PlantID) ON DELETE CASCADE,
    PRIMARY KEY (UserID, PlantID)
    );

CREATE TABLE IF NOT EXISTS botanica.Alert (
                                              AlertID UUID PRIMARY KEY,
                                              AlertUserId UUID REFERENCES botanica."User"(UserId) ON DELETE CASCADE,
    AlertName VARCHAR(100),
    AlertDesc TEXT,
    AlertTime TIMESTAMP,
    AlertPlant UUID REFERENCES botanica.Plant(PlantID) ON DELETE SET NULL
    );

CREATE TABLE IF NOT EXISTS botanica.SensorHistory (
                                                HistoryId UUID REFERENCES botanica."User"(UserId) ON DELETE CASCADE,
    DeviceId VARCHAR(100),
    Temperature INT,
    Humidity INT,
    AirPressure INT,
    AirQuality INT,
    Time TIMESTAMP,
    PRIMARY KEY (HistoryId, Time)
    );
