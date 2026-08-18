/* =============================================================================
   Stored procedures - Attraction
   CMPG213 Group 6

   This file is the pattern for every other entity. Copy it, replace the table
   and column names, and you have the procedures for accommodation, vehicles,
   routes and the rest.

   NAMING
   ------
   sp_<Entity>_<Action>       sp_Attraction_GetAll, sp_Attraction_Insert

   RULES
   -----
   - Every procedure starts with SET NOCOUNT ON. Without it ADO.NET receives
     extra "3 rows affected" messages that confuse the reader.
   - Insert returns the new id with SCOPE_IDENTITY so the C# code knows what was
     created.
   - Nothing is ever deleted outright where bookings exist. Delete deactivates.
   - Parameters are named after the columns so nobody has to guess.
   ============================================================================= */

IF OBJECT_ID('dbo.sp_Attraction_GetAll','P')     IS NOT NULL DROP PROCEDURE dbo.sp_Attraction_GetAll;
IF OBJECT_ID('dbo.sp_Attraction_GetById','P')    IS NOT NULL DROP PROCEDURE dbo.sp_Attraction_GetById;
IF OBJECT_ID('dbo.sp_Attraction_Search','P')     IS NOT NULL DROP PROCEDURE dbo.sp_Attraction_Search;
IF OBJECT_ID('dbo.sp_Attraction_Insert','P')     IS NOT NULL DROP PROCEDURE dbo.sp_Attraction_Insert;
IF OBJECT_ID('dbo.sp_Attraction_Update','P')     IS NOT NULL DROP PROCEDURE dbo.sp_Attraction_Update;
IF OBJECT_ID('dbo.sp_Attraction_Delete','P')     IS NOT NULL DROP PROCEDURE dbo.sp_Attraction_Delete;
IF OBJECT_ID('dbo.sp_Attraction_Availability','P') IS NOT NULL DROP PROCEDURE dbo.sp_Attraction_Availability;
GO


/* -----------------------------------------------------------------------------
   Every active attraction, with the category and province names joined in so
   the list screen does not have to look them up separately.
   ----------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_Attraction_GetAll
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  a.AttractionId, a.Name, a.CategoryId, c.CategoryName,
            a.Address, a.City, a.ProvinceId, p.ProvinceName,
            a.Description, a.Rate, a.Quantity, a.UnitOfMeasure,
            a.DurationMinutes, a.OperatingTimeStart, a.OperatingTimeEnd,
            a.BestOperatingSeason, a.IsActive, a.CreatedDate
    FROM        dbo.Attraction         a
    LEFT JOIN   dbo.AttractionCategory c ON c.CategoryId = a.CategoryId
    LEFT JOIN   dbo.Province           p ON p.ProvinceId = a.ProvinceId
    WHERE   (@IncludeInactive = 1 OR a.IsActive = 1)
    ORDER BY a.Name;
END
GO


/* -----------------------------------------------------------------------------
   One attraction. Returns no rows if the id does not exist, which the C# turns
   into null.
   ----------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_Attraction_GetById
    @AttractionId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  a.AttractionId, a.Name, a.CategoryId, c.CategoryName,
            a.Address, a.City, a.ProvinceId, p.ProvinceName,
            a.Description, a.Rate, a.Quantity, a.UnitOfMeasure,
            a.DurationMinutes, a.OperatingTimeStart, a.OperatingTimeEnd,
            a.BestOperatingSeason, a.IsActive, a.CreatedDate
    FROM        dbo.Attraction         a
    LEFT JOIN   dbo.AttractionCategory c ON c.CategoryId = a.CategoryId
    LEFT JOIN   dbo.Province           p ON p.ProvinceId = a.ProvinceId
    WHERE   a.AttractionId = @AttractionId;
END
GO


/* -----------------------------------------------------------------------------
   Search for the list screen. Every parameter is optional: pass NULL to ignore
   that filter. This is why the WHERE clause tests for NULL first.
   ----------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_Attraction_Search
    @Name       NVARCHAR(120) = NULL,
    @CategoryId INT           = NULL,
    @ProvinceId INT           = NULL,
    @MaxRate    DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  a.AttractionId, a.Name, a.CategoryId, c.CategoryName,
            a.Address, a.City, a.ProvinceId, p.ProvinceName,
            a.Description, a.Rate, a.Quantity, a.UnitOfMeasure,
            a.DurationMinutes, a.OperatingTimeStart, a.OperatingTimeEnd,
            a.BestOperatingSeason, a.IsActive, a.CreatedDate
    FROM        dbo.Attraction         a
    LEFT JOIN   dbo.AttractionCategory c ON c.CategoryId = a.CategoryId
    LEFT JOIN   dbo.Province           p ON p.ProvinceId = a.ProvinceId
    WHERE   a.IsActive = 1
      AND  (@Name       IS NULL OR a.Name LIKE '%' + @Name + '%')
      AND  (@CategoryId IS NULL OR a.CategoryId = @CategoryId)
      AND  (@ProvinceId IS NULL OR a.ProvinceId = @ProvinceId)
      AND  (@MaxRate    IS NULL OR a.Rate      <= @MaxRate)
    ORDER BY a.Name;
END
GO


/* -----------------------------------------------------------------------------
   Insert. Returns the new id.

   The validation rules from Phase 2 that the database can enforce are already
   CHECK constraints on the table, so they are not repeated here. What this
   procedure adds is the duplicate name check, which a constraint cannot express
   politely.
   ----------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_Attraction_Insert
    @Name                NVARCHAR(120),
    @CategoryId          INT,
    @Address             NVARCHAR(200),
    @City                NVARCHAR(80),
    @ProvinceId          INT,
    @Description         NVARCHAR(1000),
    @Rate                DECIMAL(10,2),
    @Quantity            INT,
    @UnitOfMeasure       NVARCHAR(30),
    @DurationMinutes     INT,
    @OperatingTimeStart  TIME(0),
    @OperatingTimeEnd    TIME(0),
    @BestOperatingSeason NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Attraction WHERE Name = @Name AND IsActive = 1)
    BEGIN
        RAISERROR('An active attraction with this name already exists.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Attraction
        (Name, CategoryId, Address, City, ProvinceId, Description, Rate, Quantity,
         UnitOfMeasure, DurationMinutes, OperatingTimeStart, OperatingTimeEnd,
         BestOperatingSeason)
    VALUES
        (@Name, @CategoryId, @Address, @City, @ProvinceId, @Description, @Rate, @Quantity,
         @UnitOfMeasure, @DurationMinutes, @OperatingTimeStart, @OperatingTimeEnd,
         @BestOperatingSeason);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewId;
END
GO


/* -----------------------------------------------------------------------------
   Update.
   ----------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_Attraction_Update
    @AttractionId        INT,
    @Name                NVARCHAR(120),
    @CategoryId          INT,
    @Address             NVARCHAR(200),
    @City                NVARCHAR(80),
    @ProvinceId          INT,
    @Description         NVARCHAR(1000),
    @Rate                DECIMAL(10,2),
    @Quantity            INT,
    @UnitOfMeasure       NVARCHAR(30),
    @DurationMinutes     INT,
    @OperatingTimeStart  TIME(0),
    @OperatingTimeEnd    TIME(0),
    @BestOperatingSeason NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Attraction WHERE AttractionId = @AttractionId)
    BEGIN
        RAISERROR('Attraction not found.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Attraction
               WHERE Name = @Name AND AttractionId <> @AttractionId AND IsActive = 1)
    BEGIN
        RAISERROR('Another active attraction already uses this name.', 16, 1);
        RETURN;
    END

    UPDATE dbo.Attraction
    SET Name                = @Name,
        CategoryId          = @CategoryId,
        Address             = @Address,
        City                = @City,
        ProvinceId          = @ProvinceId,
        Description         = @Description,
        Rate                = @Rate,
        Quantity            = @Quantity,
        UnitOfMeasure       = @UnitOfMeasure,
        DurationMinutes     = @DurationMinutes,
        OperatingTimeStart  = @OperatingTimeStart,
        OperatingTimeEnd    = @OperatingTimeEnd,
        BestOperatingSeason = @BestOperatingSeason
    WHERE AttractionId = @AttractionId;
END
GO


/* -----------------------------------------------------------------------------
   Delete, which follows the rule agreed by the group: an attraction that has
   been booked is deactivated so that historic bookings and reports still work.
   Only an attraction nobody has ever booked is removed for real.

   @WasDeactivated tells the C# which of the two happened, so the screen can say
   so instead of guessing.
   ----------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_Attraction_Delete
    @AttractionId    INT,
    @WasDeactivated  BIT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Attraction WHERE AttractionId = @AttractionId)
    BEGIN
        RAISERROR('Attraction not found.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.AttractionBooking WHERE AttractionId = @AttractionId)
    BEGIN
        UPDATE dbo.Attraction SET IsActive = 0 WHERE AttractionId = @AttractionId;
        SET @WasDeactivated = 1;
    END
    ELSE
    BEGIN
        DELETE FROM dbo.TimeSlot   WHERE AttractionId = @AttractionId;
        DELETE FROM dbo.Attraction WHERE AttractionId = @AttractionId;
        SET @WasDeactivated = 0;
    END
END
GO


/* -----------------------------------------------------------------------------
   Availability for the booking screen.

   Capacity is counted per slot, so this returns every slot for the attraction
   on the chosen date with the number of people already booked and the number
   still free. The booking page uses this to fill the dropdown and to refuse a
   booking that does not fit.
   ----------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_Attraction_Availability
    @AttractionId INT,
    @BookingDate  DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  ts.TimeSlotId,
            ts.TimeStart,
            ts.TimeEnd,
            a.Quantity                                   AS Capacity,
            ISNULL(SUM(ab.Quantity), 0)                  AS AlreadyBooked,
            a.Quantity - ISNULL(SUM(ab.Quantity), 0)     AS SpacesLeft
    FROM        dbo.TimeSlot   ts
    INNER JOIN  dbo.Attraction a  ON a.AttractionId = ts.AttractionId
    LEFT JOIN   dbo.AttractionBooking ab
            ON  ab.TimeSlotId  = ts.TimeSlotId
            AND ab.BookingDate = @BookingDate
            AND ab.Status      = 'Confirmed'
    WHERE   ts.AttractionId = @AttractionId
    GROUP BY ts.TimeSlotId, ts.TimeStart, ts.TimeEnd, a.Quantity
    ORDER BY ts.TimeStart;
END
GO

PRINT 'Attraction procedures created.';
GO
