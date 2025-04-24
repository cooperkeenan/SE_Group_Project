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

CREATE LOGIN enviromonitor_app WITH PASSWORD='Password123!';
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
