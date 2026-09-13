IF OBJECT_ID('dbo.TripLeg','U') IS NOT NULL
BEGIN
    PRINT 'dbo.TripLeg already exists - nothing created.';
END
ELSE
BEGIN
    CREATE TABLE dbo.TripLeg (
        TripLegId       INT IDENTITY(1,1)   NOT NULL,
        ItineraryId     INT                 NOT NULL,

        LegNumber       INT                 NOT NULL,

        Destination     NVARCHAR(120)       NULL,

        StartDate       DATE                NOT NULL,
        EndDate         DATE                NOT NULL,

        AccommodationId INT                 NULL,

        Guests          INT                 NOT NULL
            CONSTRAINT DF_TripLeg_Guests DEFAULT (2),

        ClientNote      NVARCHAR(500)       NULL,

        CreatedDate     DATETIME2(0)        NOT NULL
            CONSTRAINT DF_TripLeg_Created DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_TripLeg PRIMARY KEY (TripLegId),

        CONSTRAINT FK_TripLeg_Itinerary FOREIGN KEY (ItineraryId)
            REFERENCES dbo.Itinerary (ItineraryId) ON DELETE CASCADE,

        CONSTRAINT FK_TripLeg_Accommodation FOREIGN KEY (AccommodationId)
            REFERENCES dbo.Accommodation (AccommodationId),

        CONSTRAINT UQ_TripLeg_Number UNIQUE (ItineraryId, LegNumber),

        CONSTRAINT CK_TripLeg_Dates  CHECK (EndDate > StartDate),
        CONSTRAINT CK_TripLeg_Number CHECK (LegNumber > 0),
        CONSTRAINT CK_TripLeg_Guests CHECK (Guests > 0)
    );

    CREATE INDEX IX_TripLeg_Itinerary ON dbo.TripLeg (ItineraryId, LegNumber);

    PRINT 'Created dbo.TripLeg.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.TripRequestItem') AND name = 'TripLegId')
BEGIN

    ALTER TABLE dbo.TripRequestItem ADD TripLegId INT NULL;

    PRINT 'Added dbo.TripRequestItem.TripLegId.';
END
ELSE
    PRINT 'dbo.TripRequestItem.TripLegId already exists.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TripReq_TripLeg')
BEGIN

    ALTER TABLE dbo.TripRequestItem
        ADD CONSTRAINT FK_TripReq_TripLeg FOREIGN KEY (TripLegId)
            REFERENCES dbo.TripLeg (TripLegId);

    PRINT 'Added FK_TripReq_TripLeg.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_TripReq_TripLeg' AND object_id = OBJECT_ID('dbo.TripRequestItem'))
BEGIN
    CREATE INDEX IX_TripReq_TripLeg ON dbo.TripRequestItem (TripLegId);
    PRINT 'Added IX_TripReq_TripLeg.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Accommodation') AND name = 'ImageUrl')
BEGIN
    ALTER TABLE dbo.Accommodation ADD ImageUrl NVARCHAR(300) NULL;
    PRINT 'Added dbo.Accommodation.ImageUrl.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Attraction') AND name = 'ImageUrl')
BEGIN
    ALTER TABLE dbo.Attraction ADD ImageUrl NVARCHAR(300) NULL;
    PRINT 'Added dbo.Attraction.ImageUrl.';
END
GO

SELECT (SELECT COUNT(*) FROM dbo.TripLeg)         AS Legs,
       (SELECT COUNT(*) FROM dbo.TripRequestItem) AS RequestItems;
GO
