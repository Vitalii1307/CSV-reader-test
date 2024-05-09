# CSV-reader-test

You can run this project locally

Here is a script to create a database and tables:
`-- Create the database
CREATE DATABASE etl_db;
GO

-- Use the database
USE etl_db;
GO

-- Create the CabTrips table
CREATE TABLE CabTrips (
    Id INT PRIMARY KEY IDENTITY,
    PickupDateTime DATETIME NOT NULL,
    DropoffDateTime DATETIME NOT NULL,
    PassengerCount INT NOT NULL,
    TripDistance FLOAT NOT NULL,
    StoreAndFwdFlag NVARCHAR(3) NOT NULL,
    PULocationID INT NOT NULL,
    DOLocationID INT NOT NULL,
    FareAmount DECIMAL(10, 2) NOT NULL,
    TipAmount DECIMAL(10, 2) NOT NULL
);`

The number of rows in the table is visible immediately after launching the application in the console:
![image](https://github.com/Vitalii1307/CSV-reader-test/assets/70515154/74f8f6f3-fc41-4519-87a1-dcecb03b0fd2)
