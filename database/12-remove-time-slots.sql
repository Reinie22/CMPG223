IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = 'IX_AttrBooking_Attraction_Date'
             AND object_id = OBJECT_ID('dbo.AttractionBooking'))
    DROP INDEX IX_AttrBooking_Attraction_Date ON dbo.AttractionBooking;
GO

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AttrBooking_TimeSlot')
    ALTER TABLE dbo.AttractionBooking DROP CONSTRAINT FK_AttrBooking_TimeSlot;
GO

IF COL_LENGTH('dbo.AttractionBooking', 'TimeSlotId') IS NOT NULL
    ALTER TABLE dbo.AttractionBooking DROP COLUMN TimeSlotId;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_AttrBooking_Attraction_Date'
                 AND object_id = OBJECT_ID('dbo.AttractionBooking'))
    CREATE INDEX IX_AttrBooking_Attraction_Date ON dbo.AttractionBooking (AttractionId, BookingDate);
GO

IF OBJECT_ID('dbo.TimeSlot', 'U') IS NOT NULL
    DROP TABLE dbo.TimeSlot;
GO
