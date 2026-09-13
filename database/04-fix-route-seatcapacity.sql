USE NkanyeziTours;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Route') AND name = 'SeatCapacity')
BEGIN
    ALTER TABLE dbo.Route ADD SeatCapacity INT NULL;
    PRINT 'Added dbo.Route.SeatCapacity.';
END
ELSE
    PRINT 'dbo.Route.SeatCapacity already exists - nothing added.';
GO

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

UPDATE dbo.Route
SET    SeatCapacity = CASE WHEN RouteType = 'Flight' THEN 180 ELSE 22 END
WHERE  SeatCapacity IS NULL;
GO

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

SELECT   RouteType, Carrier, RouteCode,
         DepartureCity + ' to ' + ArrivalCity AS Journey,
         SeatCapacity
FROM     dbo.Route
ORDER BY RouteType, Carrier, RouteCode;
GO
