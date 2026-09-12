/* =============================================================================
   FIX: Route.SeatCapacity is missing on databases created before it was added
   =============================================================================
   01-schema.sql declares SeatCapacity on dbo.Route, but databases that were
   created from an earlier version of that file do not have the column. The
   application references it in the SELECT list, the INSERT, the UPDATE and the
   seats-available calculation, so EVERY route query fails with

       Invalid column name 'SeatCapacity'.

   The booking pages catch that and show "Could not load routes", which is why
   it looks like a page problem rather than a database one.

   This script is safe to run more than once, and safe to run on a database
   that is already correct - it checks before it changes anything.
   ============================================================================= */

USE NkanyeziTours;
GO

/* --- 1. Add the column, nullable for now so existing rows are allowed ------ */
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Route') AND name = 'SeatCapacity')
BEGIN
    ALTER TABLE dbo.Route ADD SeatCapacity INT NULL;
    PRINT 'Added dbo.Route.SeatCapacity.';
END
ELSE
    PRINT 'dbo.Route.SeatCapacity already exists - nothing added.';
GO

/* --- 2. Fill in the capacities from 03-seed-data.sql ----------------------- */
/* Matched on RouteCode rather than RouteId, because identity values differ
   between machines depending on how often the data has been reloaded.        */
UPDATE dbo.Route SET SeatCapacity = v.Seats
FROM dbo.Route r
JOIN (VALUES
        ('SA 322', 180),
        ('SA 323', 180),
        ('FA 212', 186),
        ('4Z 128',  74),
        ('EK 764', 300),
        ('BA 56',  280),
        ('QF 64',  250),
        ('NS 01',   22),
        ('NS 02',   22),
        ('BZ 10',   36)
     ) AS v (Code, Seats) ON v.Code = r.RouteCode
WHERE r.SeatCapacity IS NULL;
GO

/* --- 3. Anything still empty is a route somebody added by hand ------------- */
/* A typical domestic aeroplane against a typical shuttle bus. The Administrator
   can correct these on the Maintain screen; they only need to be sensible and
   greater than zero so the NOT NULL and the CHECK below can go on.           */
UPDATE dbo.Route
SET    SeatCapacity = CASE WHEN RouteType = 'Flight' THEN 180 ELSE 22 END
WHERE  SeatCapacity IS NULL;
GO

/* --- 4. Now lock it down to match 01-schema.sql ---------------------------- */
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.Route')
             AND name = 'SeatCapacity' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.Route ALTER COLUMN SeatCapacity INT NOT NULL;
    PRINT 'SeatCapacity set to NOT NULL.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE parent_object_id = OBJECT_ID('dbo.Route')
                 AND name = 'CK_Route_Capacity')
BEGIN
    ALTER TABLE dbo.Route
        ADD CONSTRAINT CK_Route_Capacity CHECK (SeatCapacity > 0);
    PRINT 'Added CK_Route_Capacity.';
END
GO

/* --- 5. Show the result --------------------------------------------------- */
SELECT   RouteType, Carrier, RouteCode,
         DepartureCity + ' to ' + ArrivalCity AS Journey,
         SeatCapacity
FROM     dbo.Route
ORDER BY RouteType, Carrier, RouteCode;
GO
