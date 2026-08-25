/* =============================================================================
   Nkanyezi Tours Information System
   01 - Database schema
   CMPG213 Group 6

   Target: Microsoft SQL Server (Express is fine)

   HOW TO RUN THIS
   ---------------
   1. Open SQL Server Management Studio and connect to your local server.
   2. Open this file (File > Open > File).
   3. Press Execute (F5).
   4. It will drop and recreate everything, so it is safe to run again if you
      make a mistake. Anything you typed into the tables yourself will be lost.

   RULES FOR THE TEAM
   ------------------
   - Never change tables using the SSMS designer. Change THIS file, commit it,
     and tell everyone to re-run it. Otherwise everybody's database drifts
     apart and nothing works on anyone else's machine.
   - Tables are created in dependency order. A table that points at another
     table has to be created after it, which is why the order looks odd.

   STATUS
   ------
   All eight open decisions have been settled by the group. Nothing here is
   pending. The decisions were:

     1. All three transport types are in: car rentals, flights and bus
        shuttles. Flights and shuttles are both a journey between two cities
        priced per seat, so they share one Route table with a type column
        rather than having a module each.
     2. Set price per day covering 200km per day, then a rate per extra km.
        Kilometres are measured from the odometer readings taken when the car
        goes out and when it comes back.
     3. Vehicle stock location and maintenance schedules are out of scope.
     4. Records with bookings against them are deactivated, never deleted.
     5. Itineraries carry trip dates, but bookings are not restricted to them.
     6. Itineraries carry a status: Draft, Confirmed, Cancelled, Completed.
     7. Agents and clients may both capture expenses. There is no approval step.
     8. Bookings and expenses stay separate and are added together only on the
        trip summary.
     9. Not every client has an agent, so AgentId on Itinerary allows NULL.
    10. One booking row per room. A client taking three rooms gets three rows.
    11. The system records what a trip costs. It does not track payments,
        deposits or outstanding balances.
    12. Four reports are being built: revenue and profit analysis, seasonal
        trends and hotspots, trip cost, and agent performance. Agent performance
        counts only trips that have an agent assigned, since a client can build
        a trip in the portal before anyone picks it up.
    13. Exchange rates are captured by the Administrator, not fetched from an
        online service.
    14. Quantity on an attraction is the number of people it can take in one
        time slot. The column keeps the name used in the Phase 2 document, and
        the screens spell out what it means. A booking must name a slot, since
        capacity is counted per slot.
    15. Route fares are indicative prices captured by the Administrator. The
        agent overrides them with the fare actually paid when the booking is
        made. No airline pricing API is used.
    16. The ERD's single Transport entity is now Vehicle and Route, with a
        booking table each.
   ============================================================================= */


/* -----------------------------------------------------------------------------
   DROP EVERYTHING FIRST (children before parents)
   ----------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ExchangeRate','U')       IS NOT NULL DROP TABLE dbo.ExchangeRate;
IF OBJECT_ID('dbo.TripExpense','U')        IS NOT NULL DROP TABLE dbo.TripExpense;
IF OBJECT_ID('dbo.AttractionBooking','U')  IS NOT NULL DROP TABLE dbo.AttractionBooking;
IF OBJECT_ID('dbo.RoomBooking','U')        IS NOT NULL DROP TABLE dbo.RoomBooking;
IF OBJECT_ID('dbo.RouteBooking','U')       IS NOT NULL DROP TABLE dbo.RouteBooking;
IF OBJECT_ID('dbo.VehicleBooking','U')     IS NOT NULL DROP TABLE dbo.VehicleBooking;
IF OBJECT_ID('dbo.Itinerary','U')          IS NOT NULL DROP TABLE dbo.Itinerary;
IF OBJECT_ID('dbo.TimeSlot','U')           IS NOT NULL DROP TABLE dbo.TimeSlot;
IF OBJECT_ID('dbo.Attraction','U')         IS NOT NULL DROP TABLE dbo.Attraction;
IF OBJECT_ID('dbo.Room','U')               IS NOT NULL DROP TABLE dbo.Room;
IF OBJECT_ID('dbo.Accommodation','U')      IS NOT NULL DROP TABLE dbo.Accommodation;
IF OBJECT_ID('dbo.Route','U')              IS NOT NULL DROP TABLE dbo.Route;
IF OBJECT_ID('dbo.Vehicle','U')            IS NOT NULL DROP TABLE dbo.Vehicle;
IF OBJECT_ID('dbo.AppUser','U')            IS NOT NULL DROP TABLE dbo.AppUser;
IF OBJECT_ID('dbo.ExpenseCategory','U')    IS NOT NULL DROP TABLE dbo.ExpenseCategory;
IF OBJECT_ID('dbo.AttractionCategory','U') IS NOT NULL DROP TABLE dbo.AttractionCategory;
IF OBJECT_ID('dbo.Province','U')           IS NOT NULL DROP TABLE dbo.Province;
IF OBJECT_ID('dbo.Role','U')               IS NOT NULL DROP TABLE dbo.Role;
GO


/* =============================================================================
   SECTION 1 - LOOKUP TABLES

   These exist because Phase 2 says category and province are "selected from a
   list". A lookup table is how you enforce that in a database. It stops one
   agent typing "Gauteng" and another typing "GP", which would break every
   report that groups by province.
   ============================================================================= */

CREATE TABLE dbo.Role (
    RoleId      INT IDENTITY(1,1)   NOT NULL,
    RoleName    NVARCHAR(30)        NOT NULL,

    CONSTRAINT PK_Role      PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Role_Name UNIQUE (RoleName)
);
GO

CREATE TABLE dbo.Province (
    ProvinceId      INT IDENTITY(1,1)   NOT NULL,
    ProvinceName    NVARCHAR(50)        NOT NULL,

    CONSTRAINT PK_Province      PRIMARY KEY (ProvinceId),
    CONSTRAINT UQ_Province_Name UNIQUE (ProvinceName)
);
GO

CREATE TABLE dbo.AttractionCategory (
    CategoryId      INT IDENTITY(1,1)   NOT NULL,
    CategoryName    NVARCHAR(50)        NOT NULL,

    CONSTRAINT PK_AttractionCategory      PRIMARY KEY (CategoryId),
    CONSTRAINT UQ_AttractionCategory_Name UNIQUE (CategoryName)
);
GO

CREATE TABLE dbo.ExpenseCategory (
    ExpenseCategoryId   INT IDENTITY(1,1)   NOT NULL,
    CategoryName        NVARCHAR(50)        NOT NULL,

    CONSTRAINT PK_ExpenseCategory      PRIMARY KEY (ExpenseCategoryId),
    CONSTRAINT UQ_ExpenseCategory_Name UNIQUE (CategoryName)
);
GO


/* =============================================================================
   SECTION 2 - USERS AND ACCESS

   Correction 2.4 from the design corrections document: the ERD had no way to
   tell an Administrator from a Client, so role based access was impossible.

   The table is called AppUser and not User because USER is a reserved word in
   SQL Server. Using it would force us to write [User] with brackets everywhere.
   ============================================================================= */

CREATE TABLE dbo.AppUser (
    UserId          INT IDENTITY(1,1)   NOT NULL,
    RoleId          INT                 NOT NULL,
    Name            NVARCHAR(60)        NOT NULL,
    Surname         NVARCHAR(60)        NOT NULL,
    Email           NVARCHAR(150)       NOT NULL,
    -- Correction 4: never store the password as it was typed. The C# code
    -- hashes it with BCrypt before it ever reaches the database.
    PasswordHash    NVARCHAR(255)       NOT NULL,
    ContactNumber   NVARCHAR(20)        NULL,
    IsActive        BIT                 NOT NULL CONSTRAINT DF_AppUser_IsActive  DEFAULT (1),
    CreatedDate     DATETIME2(0)        NOT NULL CONSTRAINT DF_AppUser_Created   DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_AppUser        PRIMARY KEY (UserId),
    CONSTRAINT UQ_AppUser_Email  UNIQUE (Email),
    CONSTRAINT FK_AppUser_Role   FOREIGN KEY (RoleId) REFERENCES dbo.Role (RoleId)
);
GO

CREATE INDEX IX_AppUser_RoleId ON dbo.AppUser (RoleId);
GO


/* =============================================================================
   SECTION 3 - THINGS WE SELL

   Accommodation, Room, Attraction, TimeSlot, Vehicle and Route.
   Fields come from the Phase 2 requirement tables.

   Corrections applied here:
     2.2  Accommodation no longer points back at Room.
     2.3  Attraction no longer points back at TimeSlot or AttractionBooking.
     3.3  Province and Category are now foreign keys, not free text.
   ============================================================================= */

CREATE TABLE dbo.Accommodation (
    AccommodationId INT IDENTITY(1,1)   NOT NULL,
    Name            NVARCHAR(120)       NOT NULL,
    Description     NVARCHAR(1000)      NULL,
    Type            NVARCHAR(50)        NULL,       -- hotel, guest house, lodge
    Address         NVARCHAR(200)       NULL,
    City            NVARCHAR(80)        NULL,
    ProvinceId      INT                 NULL,
    StarRating      TINYINT             NULL,
    PricePerNight   DECIMAL(10,2)       NOT NULL,
    Capacity        INT                 NOT NULL,
    Amenities       NVARCHAR(500)       NULL,
    IsActive        BIT                 NOT NULL CONSTRAINT DF_Accom_IsActive DEFAULT (1),
    CreatedDate     DATETIME2(0)        NOT NULL CONSTRAINT DF_Accom_Created  DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Accommodation          PRIMARY KEY (AccommodationId),
    CONSTRAINT FK_Accommodation_Province FOREIGN KEY (ProvinceId) REFERENCES dbo.Province (ProvinceId),
    -- Phase 2: "PricePerNight, StarRating and Capacity are numeric and greater
    -- than zero". These CHECK constraints enforce that in the database, so the
    -- rule holds even if somebody forgets to validate it in the C# code.
    CONSTRAINT CK_Accom_Price    CHECK (PricePerNight > 0),
    CONSTRAINT CK_Accom_Capacity CHECK (Capacity > 0),
    CONSTRAINT CK_Accom_Stars    CHECK (StarRating BETWEEN 1 AND 5)
);
GO

CREATE TABLE dbo.Room (
    RoomId              INT IDENTITY(1,1)   NOT NULL,
    AccommodationId     INT                 NOT NULL,
    RoomNumber          NVARCHAR(20)        NOT NULL,
    RoomType            NVARCHAR(50)        NULL,
    Beds                INT                 NOT NULL,
    Size                NVARCHAR(30)        NULL,
    Bathroom            BIT                 NOT NULL CONSTRAINT DF_Room_Bathroom DEFAULT (1),
    AvailabilityStatus  BIT                 NOT NULL CONSTRAINT DF_Room_Available DEFAULT (1),

    CONSTRAINT PK_Room               PRIMARY KEY (RoomId),
    CONSTRAINT FK_Room_Accommodation FOREIGN KEY (AccommodationId)
        REFERENCES dbo.Accommodation (AccommodationId) ON DELETE CASCADE,
    -- The same room number cannot appear twice in the same guest house.
    CONSTRAINT UQ_Room_Number        UNIQUE (AccommodationId, RoomNumber),
    CONSTRAINT CK_Room_Beds          CHECK (Beds > 0)
);
GO

CREATE INDEX IX_Room_AccommodationId ON dbo.Room (AccommodationId);
GO

CREATE TABLE dbo.Attraction (
    AttractionId        INT IDENTITY(1,1)   NOT NULL,
    Name                NVARCHAR(120)       NOT NULL,
    CategoryId          INT                 NULL,
    Address             NVARCHAR(200)       NULL,
    City                NVARCHAR(80)        NULL,
    ProvinceId          INT                 NULL,
    Description         NVARCHAR(1000)      NULL,
    Rate                DECIMAL(10,2)       NOT NULL,
    -- Quantity means the number of people the attraction can take in a single
    -- time slot. The name is kept as it appears in the Phase 2 requirement
    -- table, so the document and the database match. The screens label it
    -- clearly for the user, for example "Capacity per time slot (people)".
    -- The availability check adds up the people already booked for one
    -- attraction, on one date, in one slot, and compares that to this number.
    Quantity            INT                 NOT NULL,
    UnitOfMeasure       NVARCHAR(30)        NULL,       -- per person, per group
    DurationMinutes     INT                 NULL,
    OperatingTimeStart  TIME(0)             NULL,
    OperatingTimeEnd    TIME(0)             NULL,
    BestOperatingSeason NVARCHAR(50)        NULL,
    IsActive            BIT                 NOT NULL CONSTRAINT DF_Attr_IsActive DEFAULT (1),
    CreatedDate         DATETIME2(0)        NOT NULL CONSTRAINT DF_Attr_Created  DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Attraction          PRIMARY KEY (AttractionId),
    CONSTRAINT FK_Attraction_Category FOREIGN KEY (CategoryId) REFERENCES dbo.AttractionCategory (CategoryId),
    CONSTRAINT FK_Attraction_Province FOREIGN KEY (ProvinceId) REFERENCES dbo.Province (ProvinceId),
    CONSTRAINT CK_Attr_Rate     CHECK (Rate > 0),
    CONSTRAINT CK_Attr_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_Attr_Times    CHECK (OperatingTimeEnd > OperatingTimeStart)
);
GO

CREATE INDEX IX_Attraction_CategoryId ON dbo.Attraction (CategoryId);
CREATE INDEX IX_Attraction_ProvinceId ON dbo.Attraction (ProvinceId);
GO

CREATE TABLE dbo.TimeSlot (
    TimeSlotId      INT IDENTITY(1,1)   NOT NULL,
    AttractionId    INT                 NOT NULL,
    TimeStart       TIME(0)             NOT NULL,
    TimeEnd         TIME(0)             NOT NULL,

    CONSTRAINT PK_TimeSlot            PRIMARY KEY (TimeSlotId),
    CONSTRAINT FK_TimeSlot_Attraction FOREIGN KEY (AttractionId)
        REFERENCES dbo.Attraction (AttractionId) ON DELETE CASCADE,
    CONSTRAINT UQ_TimeSlot            UNIQUE (AttractionId, TimeStart),
    CONSTRAINT CK_TimeSlot_Order      CHECK (TimeEnd > TimeStart)
);
GO

CREATE INDEX IX_TimeSlot_AttractionId ON dbo.TimeSlot (AttractionId);
GO

/* TRANSPORT IS SPLIT IN TWO.

   The single Transport entity in the ERD has become two tables, because the
   three kinds of transport do not behave alike:

     Vehicle  - car rentals. Priced per day, includes 200km per day, and the
                final cost is only known once the odometer comes back.
     Route    - flights and bus shuttles. Both are a journey from one city to
                another at a set time, priced per seat. They share one table
                with a type column, because apart from the carrier there is no
                difference between them worth a second set of screens.

   Trying to hold all three in one table would leave the odometer columns empty
   on every flight and the departure time empty on every car.

   Vehicle stock location and maintenance schedules remain out of scope. The
   Location column stays because a client has to collect the car somewhere, but
   nothing reports on it. */
CREATE TABLE dbo.Vehicle (
    VehicleId           INT IDENTITY(1,1)   NOT NULL,
    VehicleDescription  NVARCHAR(200)       NULL,
    Make                NVARCHAR(50)        NULL,
    Model               NVARCHAR(50)        NULL,
    Colour              NVARCHAR(30)        NULL,
    RegistrationNumber  NVARCHAR(20)        NOT NULL,
    Capacity            INT                 NULL,       -- passengers
    Location            NVARCHAR(80)        NULL,       -- collection branch
    -- Pricing. The set price covers IncludedKmPerDay kilometres for each day of
    -- the rental. Anything driven above that is charged at RatePerExtraKm.
    PricePerDay         DECIMAL(10,2)       NOT NULL,
    IncludedKmPerDay    INT                 NOT NULL CONSTRAINT DF_Vehicle_InclKm DEFAULT (200),
    RatePerExtraKm      DECIMAL(10,2)       NOT NULL,
    IsActive            BIT                 NOT NULL CONSTRAINT DF_Vehicle_IsActive DEFAULT (1),
    CreatedDate         DATETIME2(0)        NOT NULL CONSTRAINT DF_Vehicle_Created  DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Vehicle         PRIMARY KEY (VehicleId),
    CONSTRAINT UQ_Vehicle_Reg     UNIQUE (RegistrationNumber),
    CONSTRAINT CK_Vehicle_Price   CHECK (PricePerDay > 0),
    CONSTRAINT CK_Vehicle_InclKm  CHECK (IncludedKmPerDay > 0),
    CONSTRAINT CK_Vehicle_ExtraKm CHECK (RatePerExtraKm >= 0)
);
GO


/* Flights and bus shuttles.

   IndicativeFarePerSeat is what the Administrator captures as a guide price, so
   that a client browsing the portal sees roughly what a Johannesburg to New York
   flight costs. It is an estimate, not a quote. When the agent actually books
   with the airline or the bus company, the real fare is captured on the booking
   and it is that figure the client is charged.

   The group considered pulling live fares from an airline API and decided
   against it: the free tiers return test prices rather than real ones, it needs
   an internet connection during the demonstration, and it puts an external
   dependency in the middle of a module that is being marked on its database
   work. */
CREATE TABLE dbo.Route (
    RouteId                 INT IDENTITY(1,1)   NOT NULL,
    RouteType               NVARCHAR(10)        NOT NULL,   -- Flight or Shuttle
    Carrier                 NVARCHAR(80)        NOT NULL,   -- airline or bus company
    RouteCode               NVARCHAR(20)        NULL,       -- flight number or service number
    DepartureCity           NVARCHAR(80)        NOT NULL,
    ArrivalCity             NVARCHAR(80)        NOT NULL,
    DepartureTime           TIME(0)             NULL,
    ArrivalTime             TIME(0)             NULL,
    DurationMinutes         INT                 NULL,
    IndicativeFarePerSeat   DECIMAL(10,2)       NOT NULL,
    SeatCapacity            INT                 NOT NULL,
    IsActive                BIT                 NOT NULL CONSTRAINT DF_Route_IsActive DEFAULT (1),
    CreatedDate             DATETIME2(0)        NOT NULL CONSTRAINT DF_Route_Created  DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Route         PRIMARY KEY (RouteId),
    CONSTRAINT CK_Route_Type    CHECK (RouteType IN ('Flight','Shuttle')),
    CONSTRAINT CK_Route_Fare    CHECK (IndicativeFarePerSeat > 0),
    CONSTRAINT CK_Route_Capacity CHECK (SeatCapacity > 0),
    -- A route has to go somewhere else.
    CONSTRAINT CK_Route_Cities  CHECK (DepartureCity <> ArrivalCity)
);
GO

CREATE INDEX IX_Route_Type_Cities ON dbo.Route (RouteType, DepartureCity, ArrivalCity);
GO


/* =============================================================================
   SECTION 4 - ITINERARIES AND BOOKINGS

   This is where most of the ERD corrections land.

     2.1  Itinerary no longer holds the three booking foreign keys. The bookings
          point at the itinerary instead, so one trip can hold as many hotels,
          vehicles and attractions as the client wants.
     2.5  UserId became ClientId and AgentId. The client is who the trip is for,
          the agent is who is responsible for it.
     2.6  VehicleBooking now has ItineraryId like the other booking tables.
     3.2  CreatedDate added everywhere, for the revenue and seasonal reports.
   ============================================================================= */

/* Decisions 5 and 6 settled by the group:
     - The trip dates are recorded, but they do NOT restrict what can be booked.
       A booking is free to fall outside them. The dates are there for reporting
       and for the client's own reference, which is why they allow NULL and why
       there is no constraint tying the bookings to them.
     - Status is used, so a client can build a trip in the portal and hand it to
       an agent to confirm. */
CREATE TABLE dbo.Itinerary (
    ItineraryId     INT IDENTITY(1,1)   NOT NULL,
    ClientId        INT                 NOT NULL,
    -- Nullable on purpose. A client who builds their own trip through the
    -- portal has no agent until one picks it up, so this stays empty until then.
    AgentId         INT                 NULL,
    TripName        NVARCHAR(120)       NOT NULL,
    StartDate       DATE                NULL,
    EndDate         DATE                NULL,
    TotalBudget     DECIMAL(12,2)       NOT NULL,
    TripComments    NVARCHAR(MAX)       NULL,   -- replaces the agents' notebooks
    Status          NVARCHAR(20)        NOT NULL CONSTRAINT DF_Itin_Status  DEFAULT ('Draft'),
    CreatedDate     DATETIME2(0)        NOT NULL CONSTRAINT DF_Itin_Created DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Itinerary        PRIMARY KEY (ItineraryId),
    CONSTRAINT FK_Itinerary_Client FOREIGN KEY (ClientId) REFERENCES dbo.AppUser (UserId),
    CONSTRAINT FK_Itinerary_Agent  FOREIGN KEY (AgentId)  REFERENCES dbo.AppUser (UserId),
    CONSTRAINT CK_Itin_Dates       CHECK (EndDate IS NULL OR StartDate IS NULL OR EndDate >= StartDate),
    CONSTRAINT CK_Itin_Budget      CHECK (TotalBudget > 0),
    CONSTRAINT CK_Itin_Status      CHECK (Status IN ('Draft','Confirmed','Cancelled','Completed'))
);
GO

CREATE INDEX IX_Itinerary_ClientId ON dbo.Itinerary (ClientId);
CREATE INDEX IX_Itinerary_AgentId  ON dbo.Itinerary (AgentId);
GO

CREATE TABLE dbo.RoomBooking (
    RoomBookingId   INT IDENTITY(1,1)   NOT NULL,
    RoomId          INT                 NOT NULL,
    ItineraryId     INT                 NOT NULL,
    -- One row per room. A client taking three rooms produces three rows, each
    -- naming the exact room. There is no Quantity column, because a booking
    -- that points at one room while claiming three of them cannot tell you
    -- which rooms are actually occupied.
    CheckInDate     DATE                NOT NULL,
    CheckOutDate    DATE                NOT NULL,
    TotalCost       DECIMAL(12,2)       NOT NULL,
    Status          NVARCHAR(20)        NOT NULL CONSTRAINT DF_RoomBk_Status  DEFAULT ('Confirmed'),
    CreatedDate     DATETIME2(0)        NOT NULL CONSTRAINT DF_RoomBk_Created DEFAULT (SYSDATETIME()),
    CreatedBy       INT                 NULL,

    CONSTRAINT PK_RoomBooking           PRIMARY KEY (RoomBookingId),
    CONSTRAINT FK_RoomBooking_Room      FOREIGN KEY (RoomId)      REFERENCES dbo.Room (RoomId),
    CONSTRAINT FK_RoomBooking_Itinerary FOREIGN KEY (ItineraryId) REFERENCES dbo.Itinerary (ItineraryId) ON DELETE CASCADE,
    CONSTRAINT FK_RoomBooking_User      FOREIGN KEY (CreatedBy)   REFERENCES dbo.AppUser (UserId),
    CONSTRAINT CK_RoomBk_Dates          CHECK (CheckOutDate > CheckInDate),
    CONSTRAINT CK_RoomBk_Cost           CHECK (TotalCost >= 0)
);
GO

/* Used by the date clash check in the booking process. Without this index the
   availability query gets slower as bookings build up, and Phase 2 promises
   queries under 2.5 seconds. */
CREATE INDEX IX_RoomBooking_Room_Dates ON dbo.RoomBooking (RoomId, CheckInDate, CheckOutDate);
CREATE INDEX IX_RoomBooking_Itinerary  ON dbo.RoomBooking (ItineraryId);
GO

CREATE TABLE dbo.AttractionBooking (
    AttractionBookingId INT IDENTITY(1,1)   NOT NULL,
    AttractionId        INT                 NOT NULL,
    ItineraryId         INT                 NOT NULL,
    -- Correction 4: now a real foreign key, and required. Capacity is counted
    -- per slot, so a booking with no slot could not be capacity checked.
    TimeSlotId          INT                 NOT NULL,
    BookingDate         DATE                NOT NULL,
    Quantity            INT                 NOT NULL,   -- number of people
    TotalCost           DECIMAL(12,2)       NOT NULL,
    Status              NVARCHAR(20)        NOT NULL CONSTRAINT DF_AttrBk_Status  DEFAULT ('Confirmed'),
    CreatedDate         DATETIME2(0)        NOT NULL CONSTRAINT DF_AttrBk_Created DEFAULT (SYSDATETIME()),
    CreatedBy           INT                 NULL,

    CONSTRAINT PK_AttractionBooking           PRIMARY KEY (AttractionBookingId),
    CONSTRAINT FK_AttrBooking_Attraction      FOREIGN KEY (AttractionId) REFERENCES dbo.Attraction (AttractionId),
    CONSTRAINT FK_AttrBooking_Itinerary       FOREIGN KEY (ItineraryId)  REFERENCES dbo.Itinerary (ItineraryId) ON DELETE CASCADE,
    CONSTRAINT FK_AttrBooking_TimeSlot        FOREIGN KEY (TimeSlotId)   REFERENCES dbo.TimeSlot (TimeSlotId),
    CONSTRAINT FK_AttrBooking_User            FOREIGN KEY (CreatedBy)    REFERENCES dbo.AppUser (UserId),
    CONSTRAINT CK_AttrBk_Qty                  CHECK (Quantity > 0),
    CONSTRAINT CK_AttrBk_Cost                 CHECK (TotalCost >= 0)
);
GO

/* Supports the availability check: how many people are already booked for this
   attraction, on this date, in this slot. */
CREATE INDEX IX_AttrBooking_Attraction_Date ON dbo.AttractionBooking (AttractionId, BookingDate, TimeSlotId);
CREATE INDEX IX_AttrBooking_Itinerary       ON dbo.AttractionBooking (ItineraryId);
GO

/* Car rental booking, including the kilometre billing agreed by the group.

   Why the three rate columns are copied onto the booking:
   the price of a car will change over time. If the booking only pointed at the
   Vehicle record, then raising a car's daily price next month would silently
   change what every past client was charged, and the revenue report would stop
   matching the invoices. Copying the rates onto the booking freezes them at the
   moment of hire. This is how real billing systems work.

   The costs are calculated columns. SQL Server works them out from the other
   columns in the row, so nobody can save a booking with the wrong total. Extra
   kilometres only apply once the car is returned and the closing odometer is
   captured, which is why the calculation checks for NULL first. */
CREATE TABLE dbo.VehicleBooking (
    VehicleBookingId    INT IDENTITY(1,1)   NOT NULL,
    VehicleId           INT                 NOT NULL,
    ItineraryId         INT                 NOT NULL,   -- correction 2.6
    StartDate           DATE                NOT NULL,
    EndDate             DATE                NOT NULL,
    TotalDays           AS (DATEDIFF(DAY, StartDate, EndDate) + 1),

    -- Rates copied from the Vehicle when the booking is made
    RatePerDay          DECIMAL(10,2)       NOT NULL,
    IncludedKmPerDay    INT                 NOT NULL,
    RatePerExtraKm      DECIMAL(10,2)       NOT NULL,

    -- Odometer readings, captured when the car goes out and comes back
    OdometerOut         INT                 NULL,
    OdometerIn          INT                 NULL,
    KmDriven            AS (OdometerIn - OdometerOut),
    KmIncluded          AS ((DATEDIFF(DAY, StartDate, EndDate) + 1) * IncludedKmPerDay),
    ExtraKm             AS (CASE
                              WHEN OdometerIn IS NULL OR OdometerOut IS NULL THEN 0
                              WHEN (OdometerIn - OdometerOut)
                                   > ((DATEDIFF(DAY, StartDate, EndDate) + 1) * IncludedKmPerDay)
                                THEN (OdometerIn - OdometerOut)
                                     - ((DATEDIFF(DAY, StartDate, EndDate) + 1) * IncludedKmPerDay)
                              ELSE 0
                            END),

    BaseCost            AS ((DATEDIFF(DAY, StartDate, EndDate) + 1) * RatePerDay),
    ExtraKmCost         AS ((CASE
                              WHEN OdometerIn IS NULL OR OdometerOut IS NULL THEN 0
                              WHEN (OdometerIn - OdometerOut)
                                   > ((DATEDIFF(DAY, StartDate, EndDate) + 1) * IncludedKmPerDay)
                                THEN (OdometerIn - OdometerOut)
                                     - ((DATEDIFF(DAY, StartDate, EndDate) + 1) * IncludedKmPerDay)
                              ELSE 0
                            END) * RatePerExtraKm),
    TotalCost           AS (((DATEDIFF(DAY, StartDate, EndDate) + 1) * RatePerDay)
                            + ((CASE
                              WHEN OdometerIn IS NULL OR OdometerOut IS NULL THEN 0
                              WHEN (OdometerIn - OdometerOut)
                                   > ((DATEDIFF(DAY, StartDate, EndDate) + 1) * IncludedKmPerDay)
                                THEN (OdometerIn - OdometerOut)
                                     - ((DATEDIFF(DAY, StartDate, EndDate) + 1) * IncludedKmPerDay)
                              ELSE 0
                            END) * RatePerExtraKm)),

    -- Collected means the car is out. Returned means the odometer is captured
    -- and the final cost is settled.
    Status              NVARCHAR(20)        NOT NULL CONSTRAINT DF_TransBk_Status  DEFAULT ('Confirmed'),
    CreatedDate         DATETIME2(0)        NOT NULL CONSTRAINT DF_TransBk_Created DEFAULT (SYSDATETIME()),
    CreatedBy           INT                 NULL,

    CONSTRAINT PK_VehicleBooking            PRIMARY KEY (VehicleBookingId),
    CONSTRAINT FK_VehBooking_Vehicle        FOREIGN KEY (VehicleId)   REFERENCES dbo.Vehicle (VehicleId),
    CONSTRAINT FK_VehBooking_Itinerary      FOREIGN KEY (ItineraryId) REFERENCES dbo.Itinerary (ItineraryId) ON DELETE CASCADE,
    CONSTRAINT FK_VehBooking_User           FOREIGN KEY (CreatedBy)   REFERENCES dbo.AppUser (UserId),
    CONSTRAINT CK_VehBk_Dates               CHECK (EndDate >= StartDate),
    CONSTRAINT CK_VehBk_Rate                CHECK (RatePerDay > 0),
    -- A car cannot come back with fewer kilometres than it left with.
    CONSTRAINT CK_VehBk_Odo                 CHECK (OdometerIn IS NULL OR OdometerOut IS NULL
                                                    OR OdometerIn >= OdometerOut),
    CONSTRAINT CK_VehBk_Status              CHECK (Status IN ('Confirmed','Collected','Returned','Cancelled'))
);
GO

CREATE INDEX IX_VehBooking_Vehicle_Dates ON dbo.VehicleBooking (VehicleId, StartDate, EndDate);
CREATE INDEX IX_VehBooking_Itinerary     ON dbo.VehicleBooking (ItineraryId);
GO


/* Flight and shuttle booking.

   The cost is seats multiplied by the fare and is known the moment the booking
   is made. Availability is counted against the route's seat capacity for each
   travel date. BookingReference is the Nkanyezi reference generated only when
   the booking is confirmed.

   FarePerSeat is copied from the route as a starting point and the agent
   corrects it to the fare actually paid, so the itinerary total reflects real
   money rather than the guide price. */
CREATE TABLE dbo.RouteBooking (
    RouteBookingId      INT IDENTITY(1,1)   NOT NULL,
    RouteId             INT                 NOT NULL,
    ItineraryId         INT                 NOT NULL,
    TravelDate          DATE                NOT NULL,
    Seats               INT                 NOT NULL,
    FarePerSeat         DECIMAL(10,2)       NOT NULL,
    TotalCost           AS (Seats * FarePerSeat),
    BookingReference    NVARCHAR(30)        NULL,
    Status              NVARCHAR(20)        NOT NULL CONSTRAINT DF_RouteBk_Status  DEFAULT ('Confirmed'),
    CreatedDate         DATETIME2(0)        NOT NULL CONSTRAINT DF_RouteBk_Created DEFAULT (SYSDATETIME()),
    CreatedBy           INT                 NULL,

    CONSTRAINT PK_RouteBooking           PRIMARY KEY (RouteBookingId),
    CONSTRAINT FK_RouteBooking_Route     FOREIGN KEY (RouteId)     REFERENCES dbo.Route (RouteId),
    CONSTRAINT FK_RouteBooking_Itinerary FOREIGN KEY (ItineraryId) REFERENCES dbo.Itinerary (ItineraryId) ON DELETE CASCADE,
    CONSTRAINT FK_RouteBooking_User      FOREIGN KEY (CreatedBy)   REFERENCES dbo.AppUser (UserId),
    CONSTRAINT CK_RouteBk_Seats          CHECK (Seats > 0),
    CONSTRAINT CK_RouteBk_Fare           CHECK (FarePerSeat > 0),
    CONSTRAINT CK_RouteBk_Status         CHECK (Status IN ('Confirmed','Cancelled'))
);
GO

CREATE INDEX IX_RouteBooking_Route_Date ON dbo.RouteBooking (RouteId, TravelDate);
CREATE INDEX IX_RouteBooking_Itinerary  ON dbo.RouteBooking (ItineraryId);
GO


/* =============================================================================
   SECTION 5 - TRIP EXPENSES

   New table, correction 3.1. "Manage trip expenses" is in the scope and the use
   case diagram but had no entity in the ERD.

   Decisions 7 and 8 settled by the group:
     - Agents and clients may both capture expenses, and there is no approval
       step, so there is no verification column. CapturedBy still records who
       entered each line, which is enough to see where a figure came from.
     - Expenses and bookings stay in separate tables. Bookings are what the
       agency sold, expenses are what the client spent along the way. They are
       added together only on the trip summary screen. Storing them together
       would double count revenue in the reports.

   AmountOriginal and CurrencyCode support the currency conversion function
   promised to international clients in the project scope.
   ============================================================================= */

CREATE TABLE dbo.TripExpense (
    ExpenseId           INT IDENTITY(1,1)   NOT NULL,
    ItineraryId         INT                 NOT NULL,
    ExpenseCategoryId   INT                 NOT NULL,
    Description         NVARCHAR(300)       NOT NULL,
    Amount              DECIMAL(12,2)       NOT NULL,   -- always in Rand
    AmountOriginal      DECIMAL(12,2)       NULL,       -- as the client paid it
    CurrencyCode        NCHAR(3)            NOT NULL CONSTRAINT DF_Exp_Currency DEFAULT ('ZAR'),
    ExpenseDate         DATE                NOT NULL,
    ReceiptNumber       NVARCHAR(50)        NULL,
    CapturedBy          INT                 NOT NULL,   -- agent or client, no approval step
    CreatedDate         DATETIME2(0)        NOT NULL CONSTRAINT DF_Exp_Created  DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_TripExpense           PRIMARY KEY (ExpenseId),
    CONSTRAINT FK_Expense_Itinerary     FOREIGN KEY (ItineraryId)       REFERENCES dbo.Itinerary (ItineraryId) ON DELETE CASCADE,
    CONSTRAINT FK_Expense_Category      FOREIGN KEY (ExpenseCategoryId) REFERENCES dbo.ExpenseCategory (ExpenseCategoryId),
    CONSTRAINT FK_Expense_User          FOREIGN KEY (CapturedBy)        REFERENCES dbo.AppUser (UserId),
    CONSTRAINT CK_Expense_Amount        CHECK (Amount > 0)
);
GO

CREATE INDEX IX_TripExpense_Itinerary ON dbo.TripExpense (ItineraryId);
GO


/* =============================================================================
   SECTION 5b - EXCHANGE RATES

   The project scope promises international clients a currency conversion
   calculator. Rates are captured by the Administrator rather than fetched from
   an online service, so the system keeps working without an internet connection
   and there is nothing to pay for.

   Rates are stored with the date they applied from, and the newest rate on or
   before a given date is the one used. Old rows are kept so that a conversion
   done last month can still be explained.
   ============================================================================= */

CREATE TABLE dbo.ExchangeRate (
    ExchangeRateId  INT IDENTITY(1,1)   NOT NULL,
    CurrencyCode    NCHAR(3)            NOT NULL,   -- USD, EUR, GBP
    CurrencyName    NVARCHAR(50)        NOT NULL,
    RateToZar       DECIMAL(12,6)       NOT NULL,   -- 1 unit of this currency = this many Rand
    EffectiveFrom   DATE                NOT NULL,
    CapturedBy      INT                 NULL,
    CreatedDate     DATETIME2(0)        NOT NULL CONSTRAINT DF_Rate_Created DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_ExchangeRate      PRIMARY KEY (ExchangeRateId),
    CONSTRAINT FK_ExchangeRate_User FOREIGN KEY (CapturedBy) REFERENCES dbo.AppUser (UserId),
    -- Only one rate per currency per day.
    CONSTRAINT UQ_ExchangeRate      UNIQUE (CurrencyCode, EffectiveFrom),
    CONSTRAINT CK_ExchangeRate_Rate CHECK (RateToZar > 0)
);
GO


/* =============================================================================
   SECTION 6 - LOOKUP VALUES

   These belong with the schema rather than the test data, because the system
   cannot run without them.
   ============================================================================= */

INSERT INTO dbo.Role (RoleName) VALUES
    ('Administrator'), ('Supervisor'), ('Travel Agent'), ('Client');

INSERT INTO dbo.Province (ProvinceName) VALUES
    ('Eastern Cape'), ('Free State'), ('Gauteng'), ('KwaZulu-Natal'),
    ('Limpopo'), ('Mpumalanga'), ('Northern Cape'), ('North West'), ('Western Cape');

INSERT INTO dbo.AttractionCategory (CategoryName) VALUES
    ('Adventure'), ('Cultural'), ('Wildlife'), ('Historical'),
    ('Nature'), ('Entertainment'), ('Sightseeing');

INSERT INTO dbo.ExpenseCategory (CategoryName) VALUES
    ('Meals'), ('Fuel'), ('Entrance fees'), ('Shopping'), ('Other');
GO

PRINT 'Schema created. 18 tables. Now run 03-seed-data.sql for test data.';
GO
