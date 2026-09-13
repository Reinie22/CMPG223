IF OBJECT_ID('dbo.TripRequestItem','U') IS NOT NULL
BEGIN
    PRINT 'dbo.TripRequestItem already exists - nothing created.';
END
ELSE
BEGIN
    CREATE TABLE dbo.TripRequestItem (
        TripRequestItemId   INT IDENTITY(1,1)   NOT NULL,
        ItineraryId         INT                 NOT NULL,

        ItemType            NVARCHAR(20)        NOT NULL,
        AccommodationId     INT                 NULL,
        AttractionId        INT                 NULL,

        StartDate           DATE                NULL,
        EndDate             DATE                NULL,

        Guests              INT                 NOT NULL
            CONSTRAINT DF_TripReq_Guests DEFAULT (1),

        ClientNote          NVARCHAR(500)       NULL,

        Status              NVARCHAR(20)        NOT NULL
            CONSTRAINT DF_TripReq_Status DEFAULT ('Requested'),

        AgentNote           NVARCHAR(500)       NULL,

        CreatedDate         DATETIME2(0)        NOT NULL
            CONSTRAINT DF_TripReq_Created DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_TripRequestItem PRIMARY KEY (TripRequestItemId),

        CONSTRAINT FK_TripReq_Itinerary FOREIGN KEY (ItineraryId)
            REFERENCES dbo.Itinerary (ItineraryId) ON DELETE CASCADE,

        CONSTRAINT FK_TripReq_Accommodation FOREIGN KEY (AccommodationId)
            REFERENCES dbo.Accommodation (AccommodationId),

        CONSTRAINT FK_TripReq_Attraction FOREIGN KEY (AttractionId)
            REFERENCES dbo.Attraction (AttractionId),

        CONSTRAINT CK_TripReq_Type   CHECK (ItemType IN ('Accommodation','Attraction')),
        CONSTRAINT CK_TripReq_Status CHECK (Status   IN ('Requested','Approved','Declined')),

        CONSTRAINT CK_TripReq_OneItem CHECK (
              (ItemType = 'Accommodation' AND AccommodationId IS NOT NULL AND AttractionId    IS NULL)
           OR (ItemType = 'Attraction'    AND AttractionId    IS NOT NULL AND AccommodationId IS NULL)),

        CONSTRAINT CK_TripReq_Dates  CHECK (EndDate IS NULL OR StartDate IS NULL OR EndDate >= StartDate),
        CONSTRAINT CK_TripReq_Guests CHECK (Guests > 0)
    );

    CREATE INDEX IX_TripReq_Itinerary ON dbo.TripRequestItem (ItineraryId);
    CREATE INDEX IX_TripReq_Status    ON dbo.TripRequestItem (Status);

    PRINT 'Created dbo.TripRequestItem.';
END
GO

SELECT COUNT(*) AS RequestRows FROM dbo.TripRequestItem;
GO
