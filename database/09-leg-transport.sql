IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.TripRequestItem') AND name = 'VehicleId')
BEGIN
    ALTER TABLE dbo.TripRequestItem ADD VehicleId INT NULL;
    PRINT 'Added dbo.TripRequestItem.VehicleId.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.TripRequestItem') AND name = 'RouteId')
BEGIN
    ALTER TABLE dbo.TripRequestItem ADD RouteId INT NULL;
    PRINT 'Added dbo.TripRequestItem.RouteId.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TripReq_Vehicle')
BEGIN
    ALTER TABLE dbo.TripRequestItem
        ADD CONSTRAINT FK_TripReq_Vehicle FOREIGN KEY (VehicleId)
            REFERENCES dbo.Vehicle (VehicleId);
    PRINT 'Added FK_TripReq_Vehicle.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TripReq_Route')
BEGIN
    ALTER TABLE dbo.TripRequestItem
        ADD CONSTRAINT FK_TripReq_Route FOREIGN KEY (RouteId)
            REFERENCES dbo.Route (RouteId);
    PRINT 'Added FK_TripReq_Route.';
END
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints
           WHERE parent_object_id = OBJECT_ID('dbo.TripRequestItem')
             AND name = 'CK_TripReq_Type')
BEGIN
    ALTER TABLE dbo.TripRequestItem DROP CONSTRAINT CK_TripReq_Type;
    PRINT 'Dropped the old CK_TripReq_Type.';
END
GO

ALTER TABLE dbo.TripRequestItem
    ADD CONSTRAINT CK_TripReq_Type
        CHECK (ItemType IN ('Accommodation','Attraction','Vehicle','Route'));
GO
PRINT 'Added CK_TripReq_Type with Vehicle and Route.';
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints
           WHERE parent_object_id = OBJECT_ID('dbo.TripRequestItem')
             AND name = 'CK_TripReq_OneItem')
BEGIN
    ALTER TABLE dbo.TripRequestItem DROP CONSTRAINT CK_TripReq_OneItem;
    PRINT 'Dropped the old CK_TripReq_OneItem.';
END
GO

ALTER TABLE dbo.TripRequestItem
    ADD CONSTRAINT CK_TripReq_OneItem CHECK (
          (ItemType = 'Accommodation'
               AND AccommodationId IS NOT NULL
               AND AttractionId IS NULL AND VehicleId IS NULL AND RouteId IS NULL)
       OR (ItemType = 'Attraction'
               AND AttractionId IS NOT NULL
               AND AccommodationId IS NULL AND VehicleId IS NULL AND RouteId IS NULL)
       OR (ItemType = 'Vehicle'
               AND VehicleId IS NOT NULL
               AND AccommodationId IS NULL AND AttractionId IS NULL AND RouteId IS NULL)
       OR (ItemType = 'Route'
               AND RouteId IS NOT NULL
               AND AccommodationId IS NULL AND AttractionId IS NULL AND VehicleId IS NULL));
GO
PRINT 'Added CK_TripReq_OneItem covering all four types.';
GO

SELECT      ItemType, COUNT(*) AS Rows
FROM        dbo.TripRequestItem
GROUP BY    ItemType
ORDER BY    ItemType;
GO
