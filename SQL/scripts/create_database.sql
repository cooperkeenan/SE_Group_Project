USE master;
-- Drop any existing database
IF DB_ID('enviromonitor') IS NOT NULL
BEGIN
    ALTER DATABASE enviromonitor
    SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE enviromonitor;
END
GO

-- Create a new instance of the database
CREATE DATABASE enviromonitor;
GO

USE enviromonitor;
GO

CREATE LOGIN enviromonitor_app WITH PASSWORD='${DB_PASSWORD}';
CREATE USER enviromonitor_app for login enviromonitor_app;
GRANT CONTROL ON DATABASE::enviromonitor to enviromonitor_app;
GO

-- Create tables
CREATE TABLE users (
    id INT PRIMARY KEY,
    username VARCHAR(31) UNIQUE,
    fullname VARCHAR(63),
    password_hash VARCHAR(255)
);
GO

CREATE TABLE permissions (
    id INT PRIMARY KEY,
    permission_name VARCHAR(32) UNIQUE,
    permission_description TEXT
);
GO

CREATE TABLE user_permissions (
    user_id INT,
    permission_id INT,

    PRIMARY KEY (user_id, permission_id),
    FOREIGN KEY (user_id) REFERENCES users(id),
    FOREIGN KEY (permission_id) REFERENCES permissions(id)
);
GO


CREATE TABLE metadata (
    category VARCHAR(32),
    quantity VARCHAR(64),
    symbol VARCHAR(8),
    unit VARCHAR(16),
    unit_description VARCHAR(64),
    measurement_frequency VARCHAR(32),
    safe_level DECIMAL(4,1),
    reference VARCHAR(255),
    sensor VARCHAR(16),
    data_url VARCHAR(255),

    PRIMARY KEY (category, quantity)
);


CREATE TABLE air_quality_records (
    record_datetime DATETIME PRIMARY KEY,
    record_time TIME,
    nitrogen_dioxide FLOAT,
    sulphur_dioxide FLOAT,
    pm25 FLOAT,
    pm10 FLOAT
);

CREATE TABLE water_quality_records (
    record_datetime DATETIME PRIMARY KEY,
    nitrate FLOAT,
    nitrite FLOAT,
    phosphate FLOAT,
    ec FLOAT
);

CREATE TABLE weather_records (
    record_datetime DATETIME PRIMARY KEY,
    temperature FLOAT,
    relative_humidity TINYINT,
    wind_speed FLOAT,
    wind_direction SMALLINT
);
GO