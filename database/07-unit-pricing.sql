IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Room') AND name = 'PricePerNight')
BEGIN

    ALTER TABLE dbo.Room ADD PricePerNight DECIMAL(10,2) NULL;
    PRINT 'Added dbo.Room.PricePerNight.';
END
ELSE
    PRINT 'dbo.Room.PricePerNight already exists.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Room') AND name = 'Capacity')
BEGIN
    ALTER TABLE dbo.Room ADD Capacity INT NULL;
    PRINT 'Added dbo.Room.Capacity.';
END
ELSE
    PRINT 'dbo.Room.Capacity already exists.';
GO

UPDATE  r
SET     r.PricePerNight = a.PricePerNight
FROM    dbo.Room          AS r
JOIN    dbo.Accommodation AS a ON a.AccommodationId = r.AccommodationId
WHERE   r.PricePerNight IS NULL;
GO

UPDATE  dbo.Room
SET     Capacity = Beds
WHERE   Capacity IS NULL;
GO

UPDATE dbo.Room SET PricePerNight = 1.00 WHERE PricePerNight IS NULL OR PricePerNight <= 0;
UPDATE dbo.Room SET Capacity      = 1    WHERE Capacity      IS NULL OR Capacity      <= 0;
GO

IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.Room')
             AND name = 'PricePerNight' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.Room ALTER COLUMN PricePerNight DECIMAL(10,2) NOT NULL;
    PRINT 'Room.PricePerNight set to NOT NULL.';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.Room')
             AND name = 'Capacity' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.Room ALTER COLUMN Capacity INT NOT NULL;
    PRINT 'Room.Capacity set to NOT NULL.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE parent_object_id = OBJECT_ID('dbo.Room') AND name = 'CK_Room_Price')
BEGIN
    ALTER TABLE dbo.Room ADD CONSTRAINT CK_Room_Price CHECK (PricePerNight > 0);
    PRINT 'Added CK_Room_Price.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE parent_object_id = OBJECT_ID('dbo.Room') AND name = 'CK_Room_Capacity')
BEGIN
    ALTER TABLE dbo.Room ADD CONSTRAINT CK_Room_Capacity CHECK (Capacity > 0);
    PRINT 'Added CK_Room_Capacity.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Accom_PriceDead')
BEGIN
    ALTER TABLE dbo.Accommodation
        ADD CONSTRAINT DF_Accom_PriceDead DEFAULT (0) FOR PricePerNight;
    PRINT 'Added DF_Accom_PriceDead (column is now unused).';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Accom_CapacityDead')
BEGIN
    ALTER TABLE dbo.Accommodation
        ADD CONSTRAINT DF_Accom_CapacityDead DEFAULT (0) FOR Capacity;
    PRINT 'Added DF_Accom_CapacityDead (column is now unused).';
END
GO

SELECT      a.Name                        AS Resort,
            COUNT(r.RoomId)               AS Units,
            MIN(r.PricePerNight)          AS FromPerNight,
            MAX(r.PricePerNight)          AS ToPerNight,
            SUM(r.Capacity)               AS SleepsInTotal
FROM        dbo.Accommodation AS a
LEFT JOIN   dbo.Room          AS r ON r.AccommodationId = a.AccommodationId
GROUP BY    a.Name
ORDER BY    a.Name;
GO
