IF COL_LENGTH('dbo.TripLeg', 'RoomId') IS NULL
BEGIN
    ALTER TABLE dbo.TripLeg ADD RoomId INT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TripLeg_Room')
BEGIN
    ALTER TABLE dbo.TripLeg
        ADD CONSTRAINT FK_TripLeg_Room
            FOREIGN KEY (RoomId) REFERENCES dbo.Room(RoomId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_TripLeg_UnitNeedsResort')
BEGIN
    ALTER TABLE dbo.TripLeg
        ADD CONSTRAINT CK_TripLeg_UnitNeedsResort
            CHECK (RoomId IS NULL OR AccommodationId IS NOT NULL);
END;
GO
