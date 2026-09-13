IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.TripLeg') AND name = 'ProvinceId')
BEGIN

    ALTER TABLE dbo.TripLeg ADD ProvinceId INT NULL;
    PRINT 'Added dbo.TripLeg.ProvinceId.';
END
ELSE
    PRINT 'dbo.TripLeg.ProvinceId already exists.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TripLeg_Province')
BEGIN
    ALTER TABLE dbo.TripLeg
        ADD CONSTRAINT FK_TripLeg_Province FOREIGN KEY (ProvinceId)
            REFERENCES dbo.Province (ProvinceId);

    PRINT 'Added FK_TripLeg_Province.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Vehicle') AND name = 'ProvinceId')
BEGIN
    ALTER TABLE dbo.Vehicle ADD ProvinceId INT NULL;
    PRINT 'Added dbo.Vehicle.ProvinceId.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Vehicle_Province')
BEGIN
    ALTER TABLE dbo.Vehicle
        ADD CONSTRAINT FK_Vehicle_Province FOREIGN KEY (ProvinceId)
            REFERENCES dbo.Province (ProvinceId);

    PRINT 'Added FK_Vehicle_Province.';
END
GO

UPDATE  v
SET     v.ProvinceId = p.ProvinceId
FROM    dbo.Vehicle  AS v
JOIN    dbo.Province AS p
     ON (v.Location = 'Cape Town'    AND p.ProvinceName = 'Western Cape')
     OR (v.Location = 'Johannesburg' AND p.ProvinceName = 'Gauteng')
     OR (v.Location = 'Pretoria'     AND p.ProvinceName = 'Gauteng')
     OR (v.Location = 'Durban'       AND p.ProvinceName = 'KwaZulu-Natal')
     OR (v.Location = 'Bloemfontein' AND p.ProvinceName = 'Free State')
     OR (v.Location = 'Port Elizabeth' AND p.ProvinceName = 'Eastern Cape')
WHERE   v.ProvinceId IS NULL;
GO

SELECT      'Vehicles' AS Thing,
            COUNT(*)                                        AS Total,
            SUM(CASE WHEN ProvinceId IS NULL THEN 1 ELSE 0 END) AS WithoutRegion
FROM        dbo.Vehicle
UNION ALL
SELECT      'Legs',
            COUNT(*),
            SUM(CASE WHEN ProvinceId IS NULL THEN 1 ELSE 0 END)
FROM        dbo.TripLeg;
GO
