IF OBJECT_ID('dbo.sp_User_GetByEmail','P')   IS NOT NULL DROP PROCEDURE dbo.sp_User_GetByEmail;
IF OBJECT_ID('dbo.sp_User_GetAll','P')       IS NOT NULL DROP PROCEDURE dbo.sp_User_GetAll;
IF OBJECT_ID('dbo.sp_User_GetById','P')      IS NOT NULL DROP PROCEDURE dbo.sp_User_GetById;
IF OBJECT_ID('dbo.sp_User_Insert','P')       IS NOT NULL DROP PROCEDURE dbo.sp_User_Insert;
IF OBJECT_ID('dbo.sp_User_Update','P')       IS NOT NULL DROP PROCEDURE dbo.sp_User_Update;
IF OBJECT_ID('dbo.sp_User_SetPassword','P')  IS NOT NULL DROP PROCEDURE dbo.sp_User_SetPassword;
IF OBJECT_ID('dbo.sp_User_SetActive','P')    IS NOT NULL DROP PROCEDURE dbo.sp_User_SetActive;
GO

CREATE PROCEDURE dbo.sp_User_GetByEmail
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  u.UserId, u.RoleId, r.RoleName, u.Name, u.Surname, u.Email,
            u.PasswordHash, u.ContactNumber, u.IsActive, u.CreatedDate
    FROM        dbo.AppUser u
    INNER JOIN  dbo.Role    r ON r.RoleId = u.RoleId
    WHERE   u.Email = @Email;
END
GO

CREATE PROCEDURE dbo.sp_User_GetAll
    @RoleId          INT = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  u.UserId, u.RoleId, r.RoleName, u.Name, u.Surname, u.Email,
            CAST('' AS NVARCHAR(255)) AS PasswordHash,
            u.ContactNumber, u.IsActive, u.CreatedDate
    FROM        dbo.AppUser u
    INNER JOIN  dbo.Role    r ON r.RoleId = u.RoleId
    WHERE  (@RoleId IS NULL OR u.RoleId = @RoleId)
      AND  (@IncludeInactive = 1 OR u.IsActive = 1)
    ORDER BY r.RoleId, u.Surname, u.Name;
END
GO

CREATE PROCEDURE dbo.sp_User_GetById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  u.UserId, u.RoleId, r.RoleName, u.Name, u.Surname, u.Email,
            CAST('' AS NVARCHAR(255)) AS PasswordHash,
            u.ContactNumber, u.IsActive, u.CreatedDate
    FROM        dbo.AppUser u
    INNER JOIN  dbo.Role    r ON r.RoleId = u.RoleId
    WHERE   u.UserId = @UserId;
END
GO

CREATE PROCEDURE dbo.sp_User_Insert
    @RoleId        INT,
    @Name          NVARCHAR(60),
    @Surname       NVARCHAR(60),
    @Email         NVARCHAR(150),
    @PasswordHash  NVARCHAR(255),
    @ContactNumber NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.AppUser WHERE Email = @Email)
    BEGIN
        RAISERROR('An account with this email address already exists.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.AppUser (RoleId, Name, Surname, Email, PasswordHash, ContactNumber)
    VALUES (@RoleId, @Name, @Surname, @Email, @PasswordHash, @ContactNumber);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewId;
END
GO

CREATE PROCEDURE dbo.sp_User_Update
    @UserId        INT,
    @RoleId        INT,
    @Name          NVARCHAR(60),
    @Surname       NVARCHAR(60),
    @Email         NVARCHAR(150),
    @ContactNumber NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.AppUser WHERE UserId = @UserId)
    BEGIN
        RAISERROR('User not found.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.AppUser WHERE Email = @Email AND UserId <> @UserId)
    BEGIN
        RAISERROR('Another account already uses this email address.', 16, 1);
        RETURN;
    END

    UPDATE dbo.AppUser
    SET RoleId        = @RoleId,
        Name          = @Name,
        Surname       = @Surname,
        Email         = @Email,
        ContactNumber = @ContactNumber
    WHERE UserId = @UserId;
END
GO

CREATE PROCEDURE dbo.sp_User_SetPassword
    @UserId       INT,
    @PasswordHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.AppUser SET PasswordHash = @PasswordHash WHERE UserId = @UserId;
END
GO

CREATE PROCEDURE dbo.sp_User_SetActive
    @UserId   INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.AppUser SET IsActive = @IsActive WHERE UserId = @UserId;
END
GO

PRINT 'User procedures created.';
GO
