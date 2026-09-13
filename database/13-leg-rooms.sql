IF OBJECT_ID('dbo.TripLegRoom', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TripLegRoom (
        TripLegId    INT          NOT NULL,
        RoomId       INT          NOT NULL,
        CreatedDate  DATETIME2(0) NOT NULL CONSTRAINT DF_TripLegRoom_Created DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_TripLegRoom         PRIMARY KEY (TripLegId, RoomId),
        CONSTRAINT FK_TripLegRoom_TripLeg FOREIGN KEY (TripLegId)
            REFERENCES dbo.TripLeg (TripLegId) ON DELETE CASCADE,
        CONSTRAINT FK_TripLegRoom_Room    FOREIGN KEY (RoomId)
            REFERENCES dbo.Room (RoomId)
    );

    CREATE INDEX IX_TripLegRoom_RoomId ON dbo.TripLegRoom (RoomId);
END;
GO

IF COL_LENGTH('dbo.TripLeg', 'RoomId') IS NOT NULL
    EXEC(N'
        INSERT INTO dbo.TripLegRoom (TripLegId, RoomId)
        SELECT  l.TripLegId, l.RoomId
        FROM    dbo.TripLeg AS l
        WHERE   l.RoomId IS NOT NULL
          AND   NOT EXISTS (SELECT 1 FROM dbo.TripLegRoom AS x
                            WHERE x.TripLegId = l.TripLegId AND x.RoomId = l.RoomId);');
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_TripLeg_UnitNeedsResort')
    ALTER TABLE dbo.TripLeg DROP CONSTRAINT CK_TripLeg_UnitNeedsResort;
GO

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TripLeg_Room')
    ALTER TABLE dbo.TripLeg DROP CONSTRAINT FK_TripLeg_Room;
GO

IF COL_LENGTH('dbo.TripLeg', 'RoomId') IS NOT NULL
    ALTER TABLE dbo.TripLeg DROP COLUMN RoomId;
GO
