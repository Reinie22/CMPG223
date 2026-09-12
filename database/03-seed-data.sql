/* =============================================================================
   Nkanyezi Tours Information System
   03 - Seed data (test data)
   CMPG213 Group 6

   Run this AFTER 01-schema.sql.

   WHY THIS FILE EXISTS
   --------------------
   All eight of us must be looking at the same data. If everybody types in their
   own test guest houses, then a bug on your machine cannot be reproduced on
   mine, and screenshots in the report will not match. Run this file and we all
   have an identical database.

   It is safe to run again. It clears the data first and starts the numbering
   from 1, so the ids below stay stable no matter how many times you run it.

   PASSWORDS
   ---------
   Every seeded user has the password: Password123
   The hash below is a real BCrypt hash of that password, so you can log in with
   any of these accounts once the login page is built.
   ============================================================================= */

SET NOCOUNT ON;

/* -----------------------------------------------------------------------------
   Clear existing data, children first, and reset the numbering.
   ----------------------------------------------------------------------------- */
DELETE FROM dbo.TripExpense;
DELETE FROM dbo.AttractionBooking;
DELETE FROM dbo.RoomBooking;
DELETE FROM dbo.RouteBooking;
DELETE FROM dbo.VehicleBooking;
DELETE FROM dbo.Itinerary;
DELETE FROM dbo.TimeSlot;
DELETE FROM dbo.Attraction;
DELETE FROM dbo.Room;
DELETE FROM dbo.Accommodation;
DELETE FROM dbo.Route;
DELETE FROM dbo.Vehicle;
DELETE FROM dbo.ExchangeRate;
DELETE FROM dbo.AppUser;

/* -----------------------------------------------------------------------------
   Reset the numbering so that the first row in every table gets id 1.

   READ THIS BEFORE CHANGING IT. It looks over-complicated and it is not.

   DBCC CHECKIDENT behaves in two different ways depending on whether the table
   has EVER held a row:

     - table has never held a row   RESEED, N  ->  the next row gets id N
     - table has held rows before   RESEED, N  ->  the next row gets id N + 1

   So a plain "RESEED, 0" gives id 1 on a database that has been seeded before,
   but id 0 on a brand new one. Because 01-schema.sql drops and recreates every
   table, a fresh machine always hits the second case: the first accommodation
   would get id 0, and every hardcoded AccommodationId below would point at the
   wrong property or at nothing at all. The failure looks like
   "The INSERT statement conflicted with the FOREIGN KEY constraint".

   Worse, running this file a second time would appear to fix it, which is how a
   bug like this survives to the demonstration.

   The block below asks each table which case it is in (sys.identity_columns
   .last_value is NULL for a table that has never held a row) and reseeds
   accordingly, so the first id is always 1 either way.

   Only the tables this file clears are listed. The lookup tables from
   01-schema.sql are deliberately left alone - reseeding those while they still
   hold rows would hand out ids that already exist.
   ----------------------------------------------------------------------------- */
DECLARE @tables TABLE (TableName SYSNAME);

INSERT INTO @tables (TableName) VALUES
    ('TripExpense'), ('AttractionBooking'), ('RoomBooking'), ('RouteBooking'),
    ('VehicleBooking'), ('Itinerary'), ('TimeSlot'), ('Attraction'),
    ('Room'), ('Accommodation'), ('Route'), ('Vehicle'),
    ('ExchangeRate'), ('AppUser');

DECLARE @sql NVARCHAR(MAX) = N'';

SELECT  @sql = @sql
        + N'DBCC CHECKIDENT (''dbo.' + t.TableName + N''', RESEED, '
        + CASE WHEN ic.last_value IS NULL THEN N'1' ELSE N'0' END
        + N') WITH NO_INFOMSGS;' + CHAR(13)
FROM        @tables            AS t
INNER JOIN  sys.identity_columns AS ic
        ON  ic.object_id = OBJECT_ID(N'dbo.' + t.TableName);

EXEC sp_executesql @sql;
GO


/* =============================================================================
   USERS
   Password for every account below is: Password123
   ============================================================================= */
/* A real BCrypt hash of the word Password123, generated at work factor 11,
   which is the same factor Data/PasswordHasher.cs uses. Every seeded account
   shares it, so you can sign in as any of them while testing. */
DECLARE @pwd NVARCHAR(255) = '$2a$11$eBJhj5Nyg043bBr1RTOhFuqyW5WSyZypKXlhxfJGUkr4ZLOHE0pSq';

INSERT INTO dbo.AppUser (RoleId, Name, Surname, Email, PasswordHash, ContactNumber) VALUES
-- Administrator (1)
((SELECT RoleId FROM dbo.Role WHERE RoleName='Administrator'),
    'Vincent','Govender','admin@nkanyezitours.co.za',@pwd,'0114567890'),
-- Supervisor (2)
((SELECT RoleId FROM dbo.Role WHERE RoleName='Supervisor'),
    'Thandiwe','Mokoena','supervisor@nkanyezitours.co.za',@pwd,'0114567891'),
-- Travel agents (3, 4, 5)
((SELECT RoleId FROM dbo.Role WHERE RoleName='Travel Agent'),
    'Precious','Moeketsi','precious@nkanyezitours.co.za',@pwd,'0821234567'),
((SELECT RoleId FROM dbo.Role WHERE RoleName='Travel Agent'),
    'Liam','Du Preez','liam@nkanyezitours.co.za',@pwd,'0837654321'),
((SELECT RoleId FROM dbo.Role WHERE RoleName='Travel Agent'),
    'Naledi','Khumalo','naledi@nkanyezitours.co.za',@pwd,'0729876543'),
-- Clients (6, 7, 8, 9)
((SELECT RoleId FROM dbo.Role WHERE RoleName='Client'),
    'Johan','Botha','johan.botha@gmail.com',@pwd,'0825551234'),
((SELECT RoleId FROM dbo.Role WHERE RoleName='Client'),
    'Sarah','Nkosi','sarah.nkosi@outlook.com',@pwd,'0834442345'),
((SELECT RoleId FROM dbo.Role WHERE RoleName='Client'),
    'Michael','Chen','m.chen@example.com',@pwd,'0713334567'),
((SELECT RoleId FROM dbo.Role WHERE RoleName='Client'),
    'Emma','Williams','emma.w@example.co.uk',@pwd,'0784445678');
GO


/* =============================================================================
   ACCOMMODATION AND ROOMS
   ============================================================================= */
INSERT INTO dbo.Accommodation (Name, Description, Type, Address, City, ProvinceId, StarRating, PricePerNight, Capacity, Amenities) VALUES
('Nkanyezi Lodge Sandton','Business lodge close to the Sandton business district.','Lodge','15 Rivonia Road','Johannesburg',
    (SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='Gauteng'),4,1450.00,40,'Wi-Fi, Pool, Restaurant, Secure parking'),
('Table View Guest House','Guest house with views of Table Mountain.','Guest House','8 Beach Road','Cape Town',
    (SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='Western Cape'),4,1850.00,24,'Wi-Fi, Breakfast, Sea view, Parking'),
('Drakensberg Mountain Retreat','Quiet retreat in the northern Drakensberg.','Lodge','Cathedral Peak Road','Winterton',
    (SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='KwaZulu-Natal'),3,980.00,30,'Wi-Fi, Hiking trails, Restaurant'),
('Kruger Gate Safari Camp','Tented camp at the Paul Kruger gate.','Camp','Paul Kruger Gate','Skukuza',
    (SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='Mpumalanga'),4,2200.00,20,'Game drives, Restaurant, Pool'),
('Garden Route Beach Hotel','Beachfront hotel in Plettenberg Bay.','Hotel','1 Beacon Way','Plettenberg Bay',
    (SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='Western Cape'),5,3100.00,60,'Wi-Fi, Spa, Pool, Two restaurants'),
('Kalahari Desert Camp','Remote desert camp with dark skies for stargazing.','Camp','Askham Road','Askham',
    (SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='Northern Cape'),3,1100.00,16,'Stargazing deck, Restaurant, Guided walks');
GO

INSERT INTO dbo.Room (AccommodationId, RoomNumber, RoomType, Beds, Size, Bathroom, AvailabilityStatus) VALUES
-- Nkanyezi Lodge Sandton (1)
(1,'101','Single',1,'22 sqm',1,1),
(1,'102','Double',2,'30 sqm',1,1),
(1,'103','Double',2,'30 sqm',1,1),
(1,'201','Family',4,'48 sqm',1,1),
-- Table View Guest House (2)
(2,'A1','Double',2,'28 sqm',1,1),
(2,'A2','Double',2,'28 sqm',1,1),
(2,'B1','Suite',2,'45 sqm',1,1),
-- Drakensberg Mountain Retreat (3)
(3,'C1','Double',2,'26 sqm',1,1),
(3,'C2','Family',4,'40 sqm',1,1),
(3,'C3','Single',1,'18 sqm',0,1),
-- Kruger Gate Safari Camp (4)
(4,'T1','Tent',2,'20 sqm',1,1),
(4,'T2','Tent',2,'20 sqm',1,1),
(4,'T3','Family tent',4,'35 sqm',1,1),
-- Garden Route Beach Hotel (5)
(5,'301','Double',2,'32 sqm',1,1),
(5,'302','Sea facing',2,'38 sqm',1,1),
(5,'401','Suite',4,'65 sqm',1,1),
-- Kalahari Desert Camp (6)
(6,'D1','Chalet',2,'24 sqm',1,1),
(6,'D2','Chalet',2,'24 sqm',1,1);
GO


/* =============================================================================
   ATTRACTIONS AND TIME SLOTS

   Quantity is the number of people the attraction can take in one time slot.
   ============================================================================= */
INSERT INTO dbo.Attraction (Name, CategoryId, Address, City, ProvinceId, Description, Rate, Quantity, UnitOfMeasure, DurationMinutes, OperatingTimeStart, OperatingTimeEnd, BestOperatingSeason) VALUES
('Table Mountain Cableway',(SELECT CategoryId FROM dbo.AttractionCategory WHERE CategoryName='Sightseeing'),
    'Tafelberg Road','Cape Town',(SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='Western Cape'),
    'Return cable car to the top of Table Mountain.',420.00,60,'Per person',120,'08:00','18:00','Summer'),
('Robben Island Tour',(SELECT CategoryId FROM dbo.AttractionCategory WHERE CategoryName='Historical'),
    'V and A Waterfront','Cape Town',(SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='Western Cape'),
    'Ferry and guided tour of the island and the prison.',600.00,40,'Per person',210,'09:00','15:00','All year'),
('Kruger Sunrise Game Drive',(SELECT CategoryId FROM dbo.AttractionCategory WHERE CategoryName='Wildlife'),
    'Paul Kruger Gate','Skukuza',(SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='Mpumalanga'),
    'Open vehicle game drive with a guide.',950.00,10,'Per person',240,'05:30','10:00','Winter'),
('Soweto Cultural Tour',(SELECT CategoryId FROM dbo.AttractionCategory WHERE CategoryName='Cultural'),
    'Vilakazi Street','Johannesburg',(SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='Gauteng'),
    'Walking tour of Vilakazi Street and the Hector Pieterson Museum.',550.00,20,'Per person',180,'09:00','16:00','All year'),
('Drakensberg Guided Hike',(SELECT CategoryId FROM dbo.AttractionCategory WHERE CategoryName='Adventure'),
    'Cathedral Peak','Winterton',(SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='KwaZulu-Natal'),
    'Guided day hike to the Rainbow Gorge.',380.00,15,'Per person',300,'06:00','15:00','Autumn'),
('Cango Caves Adventure Tour',(SELECT CategoryId FROM dbo.AttractionCategory WHERE CategoryName='Nature'),
    'Cango Valley','Oudtshoorn',(SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='Western Cape'),
    'Guided tour through the limestone caves.',280.00,25,'Per person',90,'09:00','16:00','All year'),
('Bloukrans Bridge Bungee',(SELECT CategoryId FROM dbo.AttractionCategory WHERE CategoryName='Adventure'),
    'N2 Bloukrans Bridge','Nature''s Valley',(SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='Western Cape'),
    'The 216 metre bungee jump from the Bloukrans Bridge.',1750.00,8,'Per person',60,'09:00','17:00','Summer'),
('Kalahari Stargazing Evening',(SELECT CategoryId FROM dbo.AttractionCategory WHERE CategoryName='Nature'),
    'Askham Road','Askham',(SELECT ProvinceId FROM dbo.Province WHERE ProvinceName='Northern Cape'),
    'Guided evening under some of the darkest skies in the country.',320.00,12,'Per person',120,'19:00','23:00','Winter');
GO

INSERT INTO dbo.TimeSlot (AttractionId, TimeStart, TimeEnd) VALUES
(1,'08:00','10:00'),(1,'11:00','13:00'),(1,'14:00','16:00'),(1,'16:00','18:00'),
(2,'09:00','12:30'),(2,'11:00','14:30'),
(3,'05:30','09:30'),
(4,'09:00','12:00'),(4,'13:00','16:00'),
(5,'06:00','11:00'),
(6,'09:00','10:30'),(6,'11:00','12:30'),(6,'14:00','15:30'),
(7,'09:00','10:00'),(7,'12:00','13:00'),(7,'15:00','16:00'),
(8,'19:00','21:00'),(8,'21:00','23:00');
GO


/* =============================================================================
   VEHICLES (car rental)
   ============================================================================= */
INSERT INTO dbo.Vehicle (VehicleDescription, Make, Model, Colour, RegistrationNumber, Capacity, Location, PricePerDay, IncludedKmPerDay, RatePerExtraKm) VALUES
('Compact hatchback, manual','Volkswagen','Polo Vivo','White','CA 123 456',5,'Cape Town',450.00,200,2.50),
('Compact hatchback, automatic','Toyota','Starlet','Silver','GP 789 012',5,'Johannesburg',520.00,200,2.50),
('Family sedan','Toyota','Corolla','Grey','GP 345 678',5,'Johannesburg',680.00,200,3.00),
('Seven seater people carrier','Volkswagen','Kombi','White','CA 901 234',7,'Cape Town',1250.00,200,4.50),
('Four wheel drive, suited to game reserves','Ford','Ranger','Blue','MP 567 890',5,'Nelspruit',1450.00,200,5.00),
('Luxury sedan','Mercedes-Benz','C-Class','Black','GP 111 222',5,'Johannesburg',1800.00,200,6.00);
GO

INSERT INTO dbo.VehicleMaintenance
       (VehicleId, MaintenanceType, StartDate, EndDate, Odometer, EstimatedCost,
        ActualCost, Status, Notes, CompletedDate, CreatedDate, CreatedBy) VALUES
(5,'60 000 km service','2026-07-01','2026-07-02',44820,4200.00,3980.00,'Completed',
 'Oil, filters, brakes and safety inspection.','2026-07-02T15:20:00','2026-06-20T09:00:00',1),
(2,'Annual service','2026-09-22','2026-09-23',31200,3500.00,NULL,'Scheduled',
 'Confirm workshop booking one week before.',NULL,'2026-08-18T10:15:00',2),
(4,'Tyre replacement','2026-11-28','2026-11-28',88900,7600.00,NULL,'Scheduled',
 'Replace all four tyres after the November rental.',NULL,'2026-08-18T10:25:00',2);
GO


/* =============================================================================
   ROUTES (flights and bus shuttles)

   The fares are indicative. The agent captures the fare actually paid when the
   booking is made.
   ============================================================================= */
INSERT INTO dbo.Route (RouteType, Carrier, RouteCode, DepartureCity, ArrivalCity, DepartureTime, ArrivalTime, DurationMinutes, IndicativeFarePerSeat, SeatCapacity) VALUES
('Flight','South African Airways','SA 322','Johannesburg','Cape Town','07:00','09:10',130,1850.00,180),
('Flight','South African Airways','SA 323','Cape Town','Johannesburg','10:00','12:05',125,1850.00,180),
('Flight','FlySafair','FA 212','Johannesburg','Durban','06:30','07:40',70,1150.00,186),
('Flight','Airlink','4Z 128','Johannesburg','Skukuza','09:30','10:45',75,2650.00,74),
('Flight','Emirates','EK 764','Johannesburg','New York','13:30','21:45',1095,14500.00,300),
('Flight','British Airways','BA 56','Johannesburg','London','19:15','06:05',655,11200.00,280),
('Flight','Qantas','QF 64','Johannesburg','Sydney','16:45','12:30',830,16800.00,250),
('Shuttle','Nkanyezi Shuttle','NS 01','Cape Town','Stellenbosch','08:00','09:00',60,180.00,22),
('Shuttle','Nkanyezi Shuttle','NS 02','Johannesburg','Pretoria','07:30','08:30',60,150.00,22),
('Shuttle','Baz Bus','BZ 10','Cape Town','Plettenberg Bay','07:00','15:30',510,650.00,36);
GO


/* =============================================================================
   EXCHANGE RATES
   ============================================================================= */
INSERT INTO dbo.ExchangeRate (CurrencyCode, CurrencyName, RateToZar, EffectiveFrom, CapturedBy) VALUES
('USD','US Dollar',18.450000,'2026-08-01',1),
('EUR','Euro',20.120000,'2026-08-01',1),
('GBP','British Pound',23.780000,'2026-08-01',1),
('AUD','Australian Dollar',12.050000,'2026-08-01',1),
('USD','US Dollar',18.620000,'2026-08-10',1),
('EUR','Euro',20.310000,'2026-08-10',1);
GO


/* =============================================================================
   ITINERARIES

   Four trips, deliberately different from each other so that every screen has
   something interesting to show:

     1  Confirmed local trip, agent assigned, bookings of every type
     2  Draft trip built by a client in the portal, NO AGENT yet
     3  Completed trip with expenses captured and a car already returned
     4  Confirmed international trip, over budget on purpose
   ============================================================================= */
INSERT INTO dbo.Itinerary (ClientId, AgentId, TripName, StartDate, EndDate, TotalBudget, TripComments, Status, CreatedDate) VALUES
(6, 3, 'Cape Town Long Weekend','2026-09-11','2026-09-14', 25000.00,
    'Client prefers a sea facing room. Phoned on 5 August to confirm the cableway booking.','Confirmed','2026-08-05T09:15:00'),
(7, NULL,'Drakensberg Hiking Trip','2026-10-02','2026-10-05', 12000.00,
    'Submitted through the website. Needs an agent to check availability and call the client back.','Draft','2026-08-09T20:42:00'),
(8, 4, 'Kruger Safari Week','2026-07-06','2026-07-11', 40000.00,
    'Trip completed. Client asked for the invoice to be emailed to his company.','Completed','2026-06-14T11:30:00'),
(9, 5, 'South Africa Grand Tour','2026-11-14','2026-11-28', 85000.00,
    'International client flying in from London. Wants everything booked before arrival.','Confirmed','2026-08-08T14:05:00');
GO


/* =============================================================================
   BOOKINGS
   ============================================================================= */

/* --- Rooms. One row per room, as agreed. ------------------------------------ */
INSERT INTO dbo.RoomBooking (RoomId, ItineraryId, CheckInDate, CheckOutDate, TotalCost, Status, CreatedDate, CreatedBy) VALUES
-- Trip 1, one sea facing room at Table View for 3 nights
(7, 1,'2026-09-11','2026-09-14', 5550.00,'Confirmed','2026-08-05T09:20:00',3),
-- Trip 3, two tents at Kruger for 5 nights (two rows, one per tent)
(11,3,'2026-07-06','2026-07-11',11000.00,'Confirmed','2026-06-14T11:35:00',4),
(12,3,'2026-07-06','2026-07-11',11000.00,'Confirmed','2026-06-14T11:35:00',4),
-- Trip 4, a suite in Plettenberg Bay and a room in Sandton
(16,4,'2026-11-20','2026-11-24',12400.00,'Confirmed','2026-08-08T14:20:00',5),
(4, 4,'2026-11-14','2026-11-17', 4350.00,'Confirmed','2026-08-08T14:25:00',5);
GO

/* --- Attractions. Quantity is the number of people. ------------------------- */
INSERT INTO dbo.AttractionBooking (AttractionId, ItineraryId, TimeSlotId, BookingDate, Quantity, TotalCost, Status, CreatedDate, CreatedBy) VALUES
-- Trip 1: cableway and Robben Island for two people
(1,1,1,'2026-09-12',2, 840.00,'Confirmed','2026-08-05T09:30:00',3),
(2,1,5,'2026-09-13',2,1200.00,'Confirmed','2026-08-05T09:32:00',3),
-- Trip 3: two game drives for four people
(3,3,7,'2026-07-07',4,3800.00,'Confirmed','2026-06-14T11:40:00',4),
(3,3,7,'2026-07-09',4,3800.00,'Confirmed','2026-06-14T11:41:00',4),
-- Trip 4: Soweto, Cango Caves and a bungee jump
(4,4,8,'2026-11-15',2,1100.00,'Confirmed','2026-08-08T14:30:00',5),
(6,4,11,'2026-11-22',2, 560.00,'Confirmed','2026-08-08T14:32:00',5),
(7,4,14,'2026-11-23',1,1750.00,'Confirmed','2026-08-08T14:33:00',5);
GO

/* --- Cars. Trip 3 has been returned, so it has both odometer readings. ------ */
INSERT INTO dbo.VehicleBooking (VehicleId, ItineraryId, StartDate, EndDate, RatePerDay, IncludedKmPerDay, RatePerExtraKm, OdometerOut, OdometerIn, Status, CreatedDate, CreatedBy) VALUES
-- Trip 1: hatchback in Cape Town, booked but not collected yet
(1,1,'2026-09-11','2026-09-14',450.00,200,2.50,NULL,NULL,'Confirmed','2026-08-05T09:40:00',3),
-- Trip 3: 4x4 at Kruger, returned. 6 days, 1200km included, 1580km driven,
-- so 380 extra kilometres at R5.00 = R1900 on top of R8700 base.
(5,3,'2026-07-06','2026-07-11',1450.00,200,5.00,45200,46780,'Returned','2026-06-14T11:45:00',4),
-- Trip 4: people carrier, collected but not yet returned
(4,4,'2026-11-20','2026-11-24',1250.00,200,4.50,88400,NULL,'Collected','2026-08-08T14:40:00',5);
GO

/* --- Flights and shuttles --------------------------------------------------- */
INSERT INTO dbo.RouteBooking (RouteId, ItineraryId, TravelDate, Seats, FarePerSeat, BookingReference, Status, CreatedDate, CreatedBy) VALUES
-- Trip 1: Johannesburg to Cape Town and back, fare slightly above the estimate
(1,1,'2026-09-11',2,1920.00,'SA-QK7T2X','Confirmed','2026-08-05T09:45:00',3),
(2,1,'2026-09-14',2,1780.00,'SA-QK7T2X','Confirmed','2026-08-05T09:46:00',3),
-- Trip 3: flight into Skukuza
(4,3,'2026-07-06',4,2590.00,'4Z-MB88K1','Confirmed','2026-06-14T11:50:00',4),
-- Trip 4: London to Johannesburg, and a shuttle to Stellenbosch
(6,4,'2026-11-14',2,11850.00,'BA-RT99PL','Confirmed','2026-08-08T14:45:00',5),
(8,4,'2026-11-21',2,180.00,'NS-2211','Confirmed','2026-08-08T14:47:00',5);
GO


/* =============================================================================
   TRIP EXPENSES

   Mostly on trip 3, which is the completed one, so that the trip cost summary
   has something real to add up. Trip 4 has an expense captured by the client
   themselves, in a foreign currency.
   ============================================================================= */
INSERT INTO dbo.TripExpense (ItineraryId, ExpenseCategoryId, Description, Amount, AmountOriginal, CurrencyCode, ExpenseDate, ReceiptNumber, CapturedBy) VALUES
(3,(SELECT ExpenseCategoryId FROM dbo.ExpenseCategory WHERE CategoryName='Meals'),
    'Dinner at Cattle Baron, Skukuza', 1240.00,NULL,'ZAR','2026-07-06','R-88201',4),
(3,(SELECT ExpenseCategoryId FROM dbo.ExpenseCategory WHERE CategoryName='Fuel'),
    'Fuel, Malelane filling station',    980.00,NULL,'ZAR','2026-07-08','R-88455',4),
(3,(SELECT ExpenseCategoryId FROM dbo.ExpenseCategory WHERE CategoryName='Entrance fees'),
    'Kruger conservation fees, 4 people',1760.00,NULL,'ZAR','2026-07-06','R-88198',4),
(3,(SELECT ExpenseCategoryId FROM dbo.ExpenseCategory WHERE CategoryName='Shopping'),
    'Curios at Skukuza rest camp',        640.00,NULL,'ZAR','2026-07-10',NULL,8),
(3,(SELECT ExpenseCategoryId FROM dbo.ExpenseCategory WHERE CategoryName='Meals'),
    'Lunch at Lower Sabie',               720.00,NULL,'ZAR','2026-07-09','R-88610',4),
-- Captured by the client, in pounds, converted at the rate for the day
(4,(SELECT ExpenseCategoryId FROM dbo.ExpenseCategory WHERE CategoryName='Other'),
    'Travel insurance bought in London', 2853.60,120.00,'GBP','2026-08-09',NULL,9);
GO


/* =============================================================================
   CHECK IT WORKED
   ============================================================================= */
PRINT '';
PRINT 'Seed data loaded. Row counts:';
SELECT 'AppUser' AS TableName, COUNT(*) AS Rows FROM dbo.AppUser
UNION ALL SELECT 'Accommodation', COUNT(*) FROM dbo.Accommodation
UNION ALL SELECT 'Room',          COUNT(*) FROM dbo.Room
UNION ALL SELECT 'Attraction',    COUNT(*) FROM dbo.Attraction
UNION ALL SELECT 'TimeSlot',      COUNT(*) FROM dbo.TimeSlot
UNION ALL SELECT 'Vehicle',       COUNT(*) FROM dbo.Vehicle
UNION ALL SELECT 'Route',         COUNT(*) FROM dbo.Route
UNION ALL SELECT 'ExchangeRate',  COUNT(*) FROM dbo.ExchangeRate
UNION ALL SELECT 'Itinerary',     COUNT(*) FROM dbo.Itinerary
UNION ALL SELECT 'RoomBooking',   COUNT(*) FROM dbo.RoomBooking
UNION ALL SELECT 'AttractionBooking', COUNT(*) FROM dbo.AttractionBooking
UNION ALL SELECT 'VehicleBooking',COUNT(*) FROM dbo.VehicleBooking
UNION ALL SELECT 'RouteBooking',  COUNT(*) FROM dbo.RouteBooking
UNION ALL SELECT 'TripExpense',   COUNT(*) FROM dbo.TripExpense;

/* The returned car should show 1580 km driven, 1200 included, 380 extra,
   R8700 base, R1900 extra, R10600 total. If it does, the calculated columns
   are working. */
PRINT '';
PRINT 'Car rental calculation check (expect 1580 km, 380 extra, R10600 total):';
SELECT VehicleBookingId, TotalDays, KmDriven, KmIncluded, ExtraKm, BaseCost, ExtraKmCost, TotalCost
FROM dbo.VehicleBooking WHERE Status = 'Returned';
GO
