/* Non-destructive upgrade for databases created before transport maintenance
   was added. Existing bookings and other records are preserved. */
IF OBJECT_ID('dbo.VehicleMaintenance','U') IS NULL
BEGIN
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
END;
GO
