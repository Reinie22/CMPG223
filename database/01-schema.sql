IF OBJECT_ID('dbo.ExchangeRate','U')       IS NOT NULL DROP TABLE dbo.ExchangeRate;
IF OBJECT_ID('dbo.TripLegRoom','U')        IS NOT NULL DROP TABLE dbo.TripLegRoom;
IF OBJECT_ID('dbo.TripRequestItem','U')    IS NOT NULL DROP TABLE dbo.TripRequestItem;
IF OBJECT_ID('dbo.TripLeg','U')            IS NOT NULL DROP TABLE dbo.TripLeg;
IF OBJECT_ID('dbo.TimeSlot','U')           IS NOT NULL DROP TABLE dbo.TimeSlot;
IF OBJECT_ID('dbo.TripExpense','U')        IS NOT NULL DROP TABLE dbo.TripExpense;
IF OBJECT_ID('dbo.AttractionBooking','U')  IS NOT NULL DROP TABLE dbo.AttractionBooking;
IF OBJECT_ID('dbo.RoomBooking','U')        IS NOT NULL DROP TABLE dbo.RoomBooking;
IF OBJECT_ID('dbo.RouteBooking','U')       IS NOT NULL DROP TABLE dbo.RouteBooking;
IF OBJECT_ID('dbo.VehicleBooking','U')     IS NOT NULL DROP TABLE dbo.VehicleBooking;
IF OBJECT_ID('dbo.Itinerary','U')          IS NOT NULL DROP TABLE dbo.Itinerary;
IF OBJECT_ID('dbo.Attraction','U')         IS NOT NULL DROP TABLE dbo.Attraction;
IF OBJECT_ID('dbo.Room','U')               IS NOT NULL DROP TABLE dbo.Room;
IF OBJECT_ID('dbo.Accommodation','U')      IS NOT NULL DROP TABLE dbo.Accommodation;
IF OBJECT_ID('dbo.Route','U')              IS NOT NULL DROP TABLE dbo.Route;
IF OBJECT_ID('dbo.VehicleMaintenance','U') IS NOT NULL DROP TABLE dbo.VehicleMaintenance;
IF OBJECT_ID('dbo.Vehicle','U')            IS NOT NULL DROP TABLE dbo.Vehicle;
IF OBJECT_ID('dbo.AppUser','U')            IS NOT NULL DROP TABLE dbo.AppUser;
IF OBJECT_ID('dbo.ExpenseCategory','U')    IS NOT NULL DROP TABLE dbo.ExpenseCategory;
IF OBJECT_ID('dbo.AttractionCategory','U') IS NOT NULL DROP TABLE dbo.AttractionCategory;
IF OBJECT_ID('dbo.Province','U')           IS NOT NULL DROP TABLE dbo.Province;
IF OBJECT_ID('dbo.Role','U')               IS NOT NULL DROP TABLE dbo.Role;
GO

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

CREATE TABLE dbo.AppUser (
    UserId          INT IDENTITY(1,1)   NOT NULL,
    RoleId          INT                 NOT NULL,
    Name            NVARCHAR(60)        NOT NULL,
    Surname         NVARCHAR(60)        NOT NULL,
    Email           NVARCHAR(150)       NOT NULL,
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

CREATE TABLE dbo.Accommodation (
    AccommodationId INT IDENTITY(1,1)   NOT NULL,
    Name            NVARCHAR(120)       NOT NULL,
    Description     NVARCHAR(1000)      NULL,
    Type            NVARCHAR(50)        NULL,
    Address         NVARCHAR(200)       NULL,
    City            NVARCHAR(80)        NULL,
    ProvinceId      INT                 NULL,
    StarRating      TINYINT             NULL,
    PricePerNight   DECIMAL(10,2)       NOT NULL CONSTRAINT DF_Accom_PriceDead    DEFAULT (0),
    Capacity        INT                 NOT NULL CONSTRAINT DF_Accom_CapacityDead DEFAULT (0),
    Amenities       NVARCHAR(500)       NULL,
    IsActive        BIT                 NOT NULL CONSTRAINT DF_Accom_IsActive DEFAULT (1),
    CreatedDate     DATETIME2(0)        NOT NULL CONSTRAINT DF_Accom_Created  DEFAULT (SYSDATETIME()),
    ImageUrl        NVARCHAR(300)       NULL,

    CONSTRAINT PK_Accommodation          PRIMARY KEY (AccommodationId),
    CONSTRAINT FK_Accommodation_Province FOREIGN KEY (ProvinceId) REFERENCES dbo.Province (ProvinceId),
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
    PricePerNight       DECIMAL(10,2)       NOT NULL,
    Capacity            INT                 NOT NULL,

    CONSTRAINT PK_Room               PRIMARY KEY (RoomId),
    CONSTRAINT FK_Room_Accommodation FOREIGN KEY (AccommodationId)
        REFERENCES dbo.Accommodation (AccommodationId) ON DELETE CASCADE,
    CONSTRAINT UQ_Room_Number        UNIQUE (AccommodationId, RoomNumber),
    CONSTRAINT CK_Room_Beds          CHECK (Beds > 0),
    CONSTRAINT CK_Room_Price         CHECK (PricePerNight > 0),
    CONSTRAINT CK_Room_Capacity      CHECK (Capacity > 0)
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
    Quantity            INT                 NOT NULL,
    UnitOfMeasure       NVARCHAR(30)        NULL,
    DurationMinutes     INT                 NULL,
    OperatingTimeStart  TIME(0)             NULL,
    OperatingTimeEnd    TIME(0)             NULL,
    BestOperatingSeason NVARCHAR(50)        NULL,
    IsActive            BIT                 NOT NULL CONSTRAINT DF_Attr_IsActive DEFAULT (1),
    CreatedDate         DATETIME2(0)        NOT NULL CONSTRAINT DF_Attr_Created  DEFAULT (SYSDATETIME()),
    ImageUrl            NVARCHAR(300)       NULL,

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

CREATE TABLE dbo.Vehicle (
    VehicleId           INT IDENTITY(1,1)   NOT NULL,
    VehicleDescription  NVARCHAR(200)       NULL,
    Make                NVARCHAR(50)        NULL,
    Model               NVARCHAR(50)        NULL,
    Colour              NVARCHAR(30)        NULL,
    RegistrationNumber  NVARCHAR(20)        NOT NULL,
    Capacity            INT                 NULL,
    Location            NVARCHAR(80)        NULL,
    PricePerDay         DECIMAL(10,2)       NOT NULL,
    IncludedKmPerDay    INT                 NOT NULL CONSTRAINT DF_Vehicle_InclKm DEFAULT (200),
    RatePerExtraKm      DECIMAL(10,2)       NOT NULL,
    IsActive            BIT                 NOT NULL CONSTRAINT DF_Vehicle_IsActive DEFAULT (1),
    CreatedDate         DATETIME2(0)        NOT NULL CONSTRAINT DF_Vehicle_Created  DEFAULT (SYSDATETIME()),
    ProvinceId          INT                 NULL,

    CONSTRAINT PK_Vehicle         PRIMARY KEY (VehicleId),
    CONSTRAINT UQ_Vehicle_Reg     UNIQUE (RegistrationNumber),
    CONSTRAINT FK_Vehicle_Province FOREIGN KEY (ProvinceId) REFERENCES dbo.Province (ProvinceId),
    CONSTRAINT CK_Vehicle_Price   CHECK (PricePerDay > 0),
    CONSTRAINT CK_Vehicle_InclKm  CHECK (IncludedKmPerDay > 0),
    CONSTRAINT CK_Vehicle_ExtraKm CHECK (RatePerExtraKm >= 0)
);
GO

CREATE TABLE dbo.VehicleMaintenance (
    VehicleMaintenanceId INT IDENTITY(1,1) NOT NULL,
    VehicleId            INT               NOT NULL,
    MaintenanceType      NVARCHAR(100)     NOT NULL,
    StartDate            DATE              NOT NULL,
    EndDate              DATE              NOT NULL,
    Odometer             INT               NULL,
    EstimatedCost        DECIMAL(10,2)     NULL,
    ActualCost           DECIMAL(10,2)     NULL,
    Status               NVARCHAR(20)      NOT NULL CONSTRAINT DF_VehicleMaintenance_Status DEFAULT ('Scheduled'),
    Notes                NVARCHAR(500)     NULL,
    CompletedDate        DATETIME2(0)      NULL,
    CreatedDate          DATETIME2(0)      NOT NULL CONSTRAINT DF_VehicleMaintenance_Created DEFAULT (SYSDATETIME()),
    CreatedBy            INT               NULL,

    CONSTRAINT PK_VehicleMaintenance PRIMARY KEY (VehicleMaintenanceId),
    CONSTRAINT FK_VehicleMaintenance_Vehicle FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicle(VehicleId),
    CONSTRAINT FK_VehicleMaintenance_User FOREIGN KEY (CreatedBy) REFERENCES dbo.AppUser(UserId),
    CONSTRAINT CK_VehicleMaintenance_Dates CHECK (EndDate >= StartDate),
    CONSTRAINT CK_VehicleMaintenance_Odometer CHECK (Odometer IS NULL OR Odometer >= 0),
    CONSTRAINT CK_VehicleMaintenance_EstimatedCost CHECK (EstimatedCost IS NULL OR EstimatedCost >= 0),
    CONSTRAINT CK_VehicleMaintenance_ActualCost CHECK (ActualCost IS NULL OR ActualCost >= 0),
    CONSTRAINT CK_VehicleMaintenance_Status CHECK (Status IN ('Scheduled','In progress','Completed','Cancelled'))
);
GO

CREATE TABLE dbo.Route (
    RouteId                 INT IDENTITY(1,1)   NOT NULL,
    RouteType               NVARCHAR(10)        NOT NULL,
    Carrier                 NVARCHAR(80)        NOT NULL,
    RouteCode               NVARCHAR(20)        NULL,
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
    CONSTRAINT CK_Route_Cities  CHECK (DepartureCity <> ArrivalCity)
);
GO

CREATE INDEX IX_Route_Type_Cities ON dbo.Route (RouteType, DepartureCity, ArrivalCity);
GO

CREATE TABLE dbo.Itinerary (
    ItineraryId     INT IDENTITY(1,1)   NOT NULL,
    ClientId        INT                 NOT NULL,
    AgentId         INT                 NULL,
    TripName        NVARCHAR(120)       NOT NULL,
    StartDate       DATE                NULL,
    EndDate         DATE                NULL,
    TotalBudget     DECIMAL(12,2)       NOT NULL,
    TripComments    NVARCHAR(MAX)       NULL,
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

CREATE INDEX IX_RoomBooking_Room_Dates ON dbo.RoomBooking (RoomId, CheckInDate, CheckOutDate);
CREATE INDEX IX_RoomBooking_Itinerary  ON dbo.RoomBooking (ItineraryId);
GO

CREATE TABLE dbo.AttractionBooking (
    AttractionBookingId INT IDENTITY(1,1)   NOT NULL,
    AttractionId        INT                 NOT NULL,
    ItineraryId         INT                 NOT NULL,
    BookingDate         DATE                NOT NULL,
    Quantity            INT                 NOT NULL,
    TotalCost           DECIMAL(12,2)       NOT NULL,
    Status              NVARCHAR(20)        NOT NULL CONSTRAINT DF_AttrBk_Status  DEFAULT ('Confirmed'),
    CreatedDate         DATETIME2(0)        NOT NULL CONSTRAINT DF_AttrBk_Created DEFAULT (SYSDATETIME()),
    CreatedBy           INT                 NULL,

    CONSTRAINT PK_AttractionBooking           PRIMARY KEY (AttractionBookingId),
    CONSTRAINT FK_AttrBooking_Attraction      FOREIGN KEY (AttractionId) REFERENCES dbo.Attraction (AttractionId),
    CONSTRAINT FK_AttrBooking_Itinerary       FOREIGN KEY (ItineraryId)  REFERENCES dbo.Itinerary (ItineraryId) ON DELETE CASCADE,
    CONSTRAINT FK_AttrBooking_User            FOREIGN KEY (CreatedBy)    REFERENCES dbo.AppUser (UserId),
    CONSTRAINT CK_AttrBk_Qty                  CHECK (Quantity > 0),
    CONSTRAINT CK_AttrBk_Cost                 CHECK (TotalCost >= 0)
);
GO

CREATE INDEX IX_AttrBooking_Attraction_Date ON dbo.AttractionBooking (AttractionId, BookingDate);
CREATE INDEX IX_AttrBooking_Itinerary       ON dbo.AttractionBooking (ItineraryId);
GO

CREATE TABLE dbo.VehicleBooking (
    VehicleBookingId    INT IDENTITY(1,1)   NOT NULL,
    VehicleId           INT                 NOT NULL,
    ItineraryId         INT                 NOT NULL,
    StartDate           DATE                NOT NULL,
    EndDate             DATE                NOT NULL,
    TotalDays           AS (DATEDIFF(DAY, StartDate, EndDate) + 1),
    RatePerDay          DECIMAL(10,2)       NOT NULL,
    IncludedKmPerDay    INT                 NOT NULL,
    RatePerExtraKm      DECIMAL(10,2)       NOT NULL,
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

    Status              NVARCHAR(20)        NOT NULL CONSTRAINT DF_TransBk_Status  DEFAULT ('Confirmed'),
    CreatedDate         DATETIME2(0)        NOT NULL CONSTRAINT DF_TransBk_Created DEFAULT (SYSDATETIME()),
    CreatedBy           INT                 NULL,

    CONSTRAINT PK_VehicleBooking            PRIMARY KEY (VehicleBookingId),
    CONSTRAINT FK_VehBooking_Vehicle        FOREIGN KEY (VehicleId)   REFERENCES dbo.Vehicle (VehicleId),
    CONSTRAINT FK_VehBooking_Itinerary      FOREIGN KEY (ItineraryId) REFERENCES dbo.Itinerary (ItineraryId) ON DELETE CASCADE,
    CONSTRAINT FK_VehBooking_User           FOREIGN KEY (CreatedBy)   REFERENCES dbo.AppUser (UserId),
    CONSTRAINT CK_VehBk_Dates               CHECK (EndDate >= StartDate),
    CONSTRAINT CK_VehBk_Rate                CHECK (RatePerDay > 0),
    CONSTRAINT CK_VehBk_Odo                 CHECK (OdometerIn IS NULL OR OdometerOut IS NULL
                                                    OR OdometerIn >= OdometerOut),
    CONSTRAINT CK_VehBk_Status              CHECK (Status IN ('Confirmed','Collected','Returned','Cancelled'))
);
GO

CREATE INDEX IX_VehBooking_Vehicle_Dates ON dbo.VehicleBooking (VehicleId, StartDate, EndDate);
CREATE INDEX IX_VehBooking_Itinerary     ON dbo.VehicleBooking (ItineraryId);
GO

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

CREATE TABLE dbo.TripExpense (
    ExpenseId           INT IDENTITY(1,1)   NOT NULL,
    ItineraryId         INT                 NOT NULL,
    ExpenseCategoryId   INT                 NOT NULL,
    Description         NVARCHAR(300)       NOT NULL,
    Amount              DECIMAL(12,2)       NOT NULL,
    AmountOriginal      DECIMAL(12,2)       NULL,
    CurrencyCode        NCHAR(3)            NOT NULL CONSTRAINT DF_Exp_Currency DEFAULT ('ZAR'),
    ExpenseDate         DATE                NOT NULL,
    ReceiptNumber       NVARCHAR(50)        NULL,
    CapturedBy          INT                 NOT NULL,
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
CREATE TABLE dbo.TripLeg (
    TripLegId       INT IDENTITY(1,1)   NOT NULL,
    ItineraryId     INT                 NOT NULL,
    LegNumber       INT                 NOT NULL,
    Destination     NVARCHAR(120)       NULL,
    StartDate       DATE                NOT NULL,
    EndDate         DATE                NOT NULL,
    AccommodationId INT                 NULL,
    Guests          INT                 NOT NULL CONSTRAINT DF_TripLeg_Guests  DEFAULT (2),
    ClientNote      NVARCHAR(500)       NULL,
    CreatedDate     DATETIME2(0)        NOT NULL CONSTRAINT DF_TripLeg_Created DEFAULT (SYSDATETIME()),
    ProvinceId      INT                 NULL,

    CONSTRAINT PK_TripLeg               PRIMARY KEY (TripLegId),
    CONSTRAINT FK_TripLeg_Itinerary     FOREIGN KEY (ItineraryId)
        REFERENCES dbo.Itinerary (ItineraryId) ON DELETE CASCADE,
    CONSTRAINT FK_TripLeg_Accommodation FOREIGN KEY (AccommodationId) REFERENCES dbo.Accommodation (AccommodationId),
    CONSTRAINT FK_TripLeg_Province      FOREIGN KEY (ProvinceId)      REFERENCES dbo.Province (ProvinceId),
    CONSTRAINT UQ_TripLeg_Number        UNIQUE (ItineraryId, LegNumber),
    CONSTRAINT CK_TripLeg_Dates         CHECK (EndDate > StartDate),
    CONSTRAINT CK_TripLeg_Number        CHECK (LegNumber > 0),
    CONSTRAINT CK_TripLeg_Guests        CHECK (Guests > 0)
);
GO

CREATE INDEX IX_TripLeg_Itinerary ON dbo.TripLeg (ItineraryId, LegNumber);
GO

CREATE TABLE dbo.TripLegRoom (
    TripLegId       INT                 NOT NULL,
    RoomId          INT                 NOT NULL,
    CreatedDate     DATETIME2(0)        NOT NULL CONSTRAINT DF_TripLegRoom_Created DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_TripLegRoom         PRIMARY KEY (TripLegId, RoomId),
    CONSTRAINT FK_TripLegRoom_TripLeg FOREIGN KEY (TripLegId)
        REFERENCES dbo.TripLeg (TripLegId) ON DELETE CASCADE,
    CONSTRAINT FK_TripLegRoom_Room    FOREIGN KEY (RoomId) REFERENCES dbo.Room (RoomId)
);
GO

CREATE INDEX IX_TripLegRoom_RoomId ON dbo.TripLegRoom (RoomId);
GO

CREATE TABLE dbo.TripRequestItem (
    TripRequestItemId   INT IDENTITY(1,1)   NOT NULL,
    ItineraryId         INT                 NOT NULL,
    ItemType            NVARCHAR(20)        NOT NULL,
    AccommodationId     INT                 NULL,
    AttractionId        INT                 NULL,
    StartDate           DATE                NULL,
    EndDate             DATE                NULL,
    Guests              INT                 NOT NULL CONSTRAINT DF_TripReq_Guests  DEFAULT (1),
    ClientNote          NVARCHAR(500)       NULL,
    Status              NVARCHAR(20)        NOT NULL CONSTRAINT DF_TripReq_Status  DEFAULT ('Requested'),
    AgentNote           NVARCHAR(500)       NULL,
    CreatedDate         DATETIME2(0)        NOT NULL CONSTRAINT DF_TripReq_Created DEFAULT (SYSDATETIME()),
    TripLegId           INT                 NULL,
    VehicleId           INT                 NULL,
    RouteId             INT                 NULL,

    CONSTRAINT PK_TripRequestItem       PRIMARY KEY (TripRequestItemId),
    CONSTRAINT FK_TripReq_Itinerary     FOREIGN KEY (ItineraryId)
        REFERENCES dbo.Itinerary (ItineraryId) ON DELETE CASCADE,
    CONSTRAINT FK_TripReq_Accommodation FOREIGN KEY (AccommodationId) REFERENCES dbo.Accommodation (AccommodationId),
    CONSTRAINT FK_TripReq_Attraction    FOREIGN KEY (AttractionId)    REFERENCES dbo.Attraction (AttractionId),
    CONSTRAINT FK_TripReq_TripLeg       FOREIGN KEY (TripLegId)       REFERENCES dbo.TripLeg (TripLegId),
    CONSTRAINT FK_TripReq_Vehicle       FOREIGN KEY (VehicleId)       REFERENCES dbo.Vehicle (VehicleId),
    CONSTRAINT FK_TripReq_Route         FOREIGN KEY (RouteId)         REFERENCES dbo.Route (RouteId),
    CONSTRAINT CK_TripReq_Type          CHECK (ItemType IN ('Accommodation','Attraction','Vehicle','Route')),
    CONSTRAINT CK_TripReq_Status        CHECK (Status   IN ('Requested','Approved','Declined')),
    CONSTRAINT CK_TripReq_OneItem       CHECK (
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
               AND AccommodationId IS NULL AND AttractionId IS NULL AND VehicleId IS NULL)),
    CONSTRAINT CK_TripReq_Dates         CHECK (EndDate IS NULL OR StartDate IS NULL OR EndDate >= StartDate),
    CONSTRAINT CK_TripReq_Guests        CHECK (Guests > 0)
);
GO

CREATE INDEX IX_TripReq_Itinerary ON dbo.TripRequestItem (ItineraryId);
CREATE INDEX IX_TripReq_Status    ON dbo.TripRequestItem (Status);
CREATE INDEX IX_TripReq_TripLeg   ON dbo.TripRequestItem (TripLegId);
GO

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

PRINT 'Schema created. 20 tables. Now run 03-seed-data.sql for test data.';
GO
