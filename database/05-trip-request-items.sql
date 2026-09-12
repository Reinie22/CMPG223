/* =============================================================================
   05 - Client trip requests
   =============================================================================
   Lets a client put accommodation and attractions onto their own draft trip,
   for a travel agent to approve.

   WHY THIS IS A NEW TABLE AND NOT A NEW BOOKING STATUS
   ----------------------------------------------------
   The obvious approach is to add 'Requested' to RoomBooking, AttractionBooking
   and friends, and let the agent flip it to 'Confirmed'. That breaks two things
   that currently work:

   1. Capacity. Every availability query subtracts bookings that are not
      'Cancelled' - see RoomRepository.GetAvailable, AttractionRepository
      .GetAvailability and RouteRepository.GetAvailabilityByType. A 'Requested'
      row would therefore hold a real room or a real seat before anybody agreed
      to it, so two clients could each be told they have the last room.

   2. Money. The booking tables store what the client is actually charged, and
      TotalCost is a computed column. A client picking a hotel does not know the
      rate the agency will charge, so there is nothing honest to put in it yet.

   So a request is NOT a booking. It is the client saying what they would like.
   When the agent approves one, the agent creates the real booking through the
   normal repository - which checks capacity and captures the real price - and
   this row is marked 'Approved' as a record of where it came from.

   Safe to run more than once.
   ============================================================================= */

IF OBJECT_ID('dbo.TripRequestItem','U') IS NOT NULL
BEGIN
    PRINT 'dbo.TripRequestItem already exists - nothing created.';
END
ELSE
BEGIN
    CREATE TABLE dbo.TripRequestItem (
        TripRequestItemId   INT IDENTITY(1,1)   NOT NULL,
        ItineraryId         INT                 NOT NULL,

        -- 'Accommodation' or 'Attraction'. Which one decides which of the two
        -- id columns below is filled in.
        ItemType            NVARCHAR(20)        NOT NULL,
        AccommodationId     INT                 NULL,
        AttractionId        INT                 NULL,

        -- Check-in and check-out for accommodation. For an attraction only
        -- StartDate is used, as the visit date.
        StartDate           DATE                NULL,
        EndDate             DATE                NULL,

        Guests              INT                 NOT NULL
            CONSTRAINT DF_TripReq_Guests DEFAULT (1),

        ClientNote          NVARCHAR(500)       NULL,

        -- 'Requested' until an agent decides. 'Approved' once the agent has
        -- made the real booking, 'Declined' if it cannot be done.
        Status              NVARCHAR(20)        NOT NULL
            CONSTRAINT DF_TripReq_Status DEFAULT ('Requested'),

        -- Why the agent declined, so the client is not left guessing.
        AgentNote           NVARCHAR(500)       NULL,

        CreatedDate         DATETIME2(0)        NOT NULL
            CONSTRAINT DF_TripReq_Created DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_TripRequestItem PRIMARY KEY (TripRequestItemId),

        -- Cascade matches the booking tables: deleting a draft trip takes its
        -- requests with it.
        CONSTRAINT FK_TripReq_Itinerary FOREIGN KEY (ItineraryId)
            REFERENCES dbo.Itinerary (ItineraryId) ON DELETE CASCADE,

        CONSTRAINT FK_TripReq_Accommodation FOREIGN KEY (AccommodationId)
            REFERENCES dbo.Accommodation (AccommodationId),

        CONSTRAINT FK_TripReq_Attraction FOREIGN KEY (AttractionId)
            REFERENCES dbo.Attraction (AttractionId),

        CONSTRAINT CK_TripReq_Type   CHECK (ItemType IN ('Accommodation','Attraction')),
        CONSTRAINT CK_TripReq_Status CHECK (Status   IN ('Requested','Approved','Declined')),

        -- Exactly one of the two ids, matching ItemType. Without this a row
        -- could claim to be an attraction while pointing at a hotel.
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
