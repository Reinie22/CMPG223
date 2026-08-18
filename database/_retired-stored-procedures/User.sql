/* =============================================================================
   Stored procedures - AppUser
   CMPG213 Group 6

   Covers logging in and the Maintain > Users and Access screens.

   SECURITY NOTE
   -------------
   The database never sees a password. It stores a hash, which is a one way
   scramble of the password. When somebody logs in, the C# hashes what they
   typed and compares it to the stored hash. That is why sp_User_GetByEmail
   returns the hash: the comparison happens in C#, not in SQL.

   Never write a procedure that takes a password and does the comparison in
   SQL. It would mean sending the real password to the database, and it would
   not work anyway, because the same password hashes to a different value every
   time it is hashed.
   ============================================================================= */

IF OBJECT_ID('dbo.sp_User_GetByEmail','P')   IS NOT NULL DROP PROCEDURE dbo.sp_User_GetByEmail;
IF OBJECT_ID('dbo.sp_User_GetAll','P')       IS NOT NULL DROP PROCEDURE dbo.sp_User_GetAll;
IF OBJECT_ID('dbo.sp_User_GetById','P')      IS NOT NULL DROP PROCEDURE dbo.sp_User_GetById;
IF OBJECT_ID('dbo.sp_User_Insert','P')       IS NOT NULL DROP PROCEDURE dbo.sp_User_Insert;
IF OBJECT_ID('dbo.sp_User_Update','P')       IS NOT NULL DROP PROCEDURE dbo.sp_User_Update;
IF OBJECT_ID('dbo.sp_User_SetPassword','P')  IS NOT NULL DROP PROCEDURE dbo.sp_User_SetPassword;
IF OBJECT_ID('dbo.sp_User_SetActive','P')    IS NOT NULL DROP PROCEDURE dbo.sp_User_SetActive;
GO


/* -----------------------------------------------------------------------------
   Used by the login page. Returns the password hash, so this is the only
   procedure that does.

   Email is matched case insensitively, which is how SQL Server compares text by
   default. Somebody typing Admin@nkanyezitours.co.za still gets in.
   ----------------------------------------------------------------------------- */
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


/* -----------------------------------------------------------------------------
   The user list screen. Both filters are optional.
   The password hash is deliberately NOT returned here.
   ----------------------------------------------------------------------------- */
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


/* -----------------------------------------------------------------------------
   Creates a user. Used both by the Administrator adding staff and by a client
   registering themselves, which is why the role is a parameter. The C# decides
   which role is allowed: registration always passes Client.

   The hash is worked out in C# before this is called.
   ----------------------------------------------------------------------------- */
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


/* -----------------------------------------------------------------------------
   Updates the details. The password is changed separately by sp_User_SetPassword,
   so that a routine edit cannot wipe somebody's password by accident.
   ----------------------------------------------------------------------------- */
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


/* -----------------------------------------------------------------------------
   Users are never deleted. A user who has captured a booking or an expense is
   referenced by those records, and deleting them would break the trail of who
   did what. Deactivating stops them logging in, which is the point.
   ----------------------------------------------------------------------------- */
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
