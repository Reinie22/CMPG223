using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

/// <summary>
/// Everything the system can do to user accounts.
///
/// Data only. Whether a login is correct is decided by AuthService, not here.
/// This class just fetches and saves rows.
///
/// One thing to notice: the password hash is only ever selected by
/// GetByEmailForLogin. Every other read returns it as an empty string, so a
/// hash cannot leak onto a screen or into a hidden form field by accident.
/// </summary>
public class UserRepository
{
    private readonly Db _db;

    public UserRepository(Db db)
    {
        _db = db;
    }

    /// <summary>
    /// The safe column list: everything except the password hash.
    /// '' AS PasswordHash keeps the shape identical for the Map method, so one
    /// mapping serves both the safe reads and the login read.
    /// </summary>
    private const string SelectSql = @"
        SELECT      u.UserId, u.RoleId, r.RoleName, u.Name, u.Surname, u.Email,
                    u.ContactNumber, u.IsActive, u.CreatedDate,
                    '' AS PasswordHash
        FROM        dbo.AppUser AS u
        INNER JOIN  dbo.Role    AS r ON r.RoleId = u.RoleId";

    // -------------------------------------------------------------------------
    // Reading
    // -------------------------------------------------------------------------

    /// <summary>
    /// Finds a user by email address, INCLUDING the password hash.
    ///
    /// Only AuthService should call this. Everything else uses GetById, which
    /// does not return the hash.
    ///
    /// The email is a parameter, which is what makes the login safe. Gluing it
    /// into the string would let anyone type  x' OR '1'='1  into the email box
    /// and be signed in as the first user in the table. See Db.cs.
    /// </summary>
    public AppUser? GetByEmailForLogin(string email)
        => _db.QuerySingle(@"
        SELECT      u.*, r.RoleName
        FROM        dbo.AppUser AS u
        INNER JOIN  dbo.Role    AS r ON r.RoleId = u.RoleId
        WHERE       u.Email = @Email;",
        Map,
        Db.P("@Email", email.Trim()));

    /// <summary>All users, optionally filtered by role. For the admin screen.</summary>
    public List<AppUser> GetAll(int? roleId = null, bool includeInactive = false)
        => _db.Query(SelectSql + @"
        WHERE   (@IncludeInactive = 1 OR u.IsActive = 1)
          AND   (@RoleId IS NULL OR u.RoleId = @RoleId)
        ORDER BY u.Surname, u.Name;",
        Map,
        Db.P("@RoleId",          roleId),
        Db.P("@IncludeInactive", includeInactive));

    /// <summary>Search the user list by name, surname or email.</summary>
    public List<AppUser> Search(string? text = null, int? roleId = null,
                                bool includeInactive = false)
        => _db.Query(SelectSql + @"
        WHERE   (@IncludeInactive = 1 OR u.IsActive = 1)
          AND   (@RoleId IS NULL OR u.RoleId = @RoleId)
          AND   (@Text   IS NULL
                 OR u.Name    LIKE @Text ESCAPE '\'
                 OR u.Surname LIKE @Text ESCAPE '\'
                 OR u.Email   LIKE @Text ESCAPE '\')
        ORDER BY u.Surname, u.Name;",
        Map,
        Db.Like("@Text", text),
        Db.P("@RoleId",          roleId),
        Db.P("@IncludeInactive", includeInactive));

    public AppUser? GetById(int userId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   u.UserId = @UserId;",
        Map,
        Db.P("@UserId", userId));

    /// <summary>Everyone in one role. The agent and client dropdowns use this.</summary>
    public List<AppUser> GetByRole(string roleName)
        => _db.Query(SelectSql + @"
        WHERE   u.IsActive = 1
          AND   r.RoleName = @RoleName
        ORDER BY u.Surname, u.Name;",
        Map,
        Db.P("@RoleName", roleName));

    /// <summary>The travel agents, for the dropdown that assigns an agent to a trip.</summary>
    public List<AppUser> GetTravelAgents() => GetByRole(RoleNames.TravelAgent);

    /// <summary>The clients, for the dropdown that starts a new trip.</summary>
    public List<AppUser> GetClients() => GetByRole(RoleNames.Client);

    /// <summary>
    /// True when the email address is already taken.
    ///
    /// The database has a UNIQUE constraint on Email as well, and that is the
    /// one that actually guarantees it. This check exists so the page can show
    /// a friendly message instead of letting a SqlException reach the user.
    /// Never rely on the check alone: two people can register in the same
    /// second, and only the constraint catches that.
    /// </summary>
    public bool EmailExists(string email, int? excludeUserId = null)
        => _db.Scalar<int>(@"
        SELECT  COUNT(*)
        FROM    dbo.AppUser
        WHERE   Email = @Email
          AND   (@ExcludeId IS NULL OR UserId <> @ExcludeId);",
        Db.P("@Email",     email.Trim()),
        Db.P("@ExcludeId", excludeUserId)) > 0;

    // -------------------------------------------------------------------------
    // Writing
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates an account and returns the new id.
    ///
    /// The password arrives in plain text and is hashed on the line below, so
    /// the plain text never leaves this method and never reaches the database.
    /// </summary>
    public int Create(AppUser user, string plainPassword)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.AppUser (RoleId,  Name,  Surname,  Email,  PasswordHash,  ContactNumber)
        VALUES                  (@RoleId, @Name, @Surname, @Email, @PasswordHash, @ContactNumber);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        Db.P("@RoleId",        user.RoleId),
        Db.P("@Name",          user.Name.Trim()),
        Db.P("@Surname",       user.Surname.Trim()),
        Db.P("@Email",         user.Email.Trim()),
        Db.P("@PasswordHash",  PasswordHasher.Hash(plainPassword)),
        Db.P("@ContactNumber", user.ContactNumber));

    /// <summary>
    /// Registers a client from the public sign-up page.
    ///
    /// The role is looked up inside the SQL rather than passed in, so this
    /// method can only ever create a Client. If the page supplied the role, a
    /// crafted form post could sign somebody up as an Administrator.
    /// </summary>
    public int RegisterClient(AppUser user, string plainPassword)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.AppUser (RoleId, Name, Surname, Email, PasswordHash, ContactNumber)
        SELECT      RoleId, @Name, @Surname, @Email, @PasswordHash, @ContactNumber
        FROM        dbo.Role
        WHERE       RoleName = 'Client';

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        Db.P("@Name",          user.Name.Trim()),
        Db.P("@Surname",       user.Surname.Trim()),
        Db.P("@Email",         user.Email.Trim()),
        Db.P("@PasswordHash",  PasswordHasher.Hash(plainPassword)),
        Db.P("@ContactNumber", user.ContactNumber));

    /// <summary>Saves the details. Deliberately does not touch the password.</summary>
    public void Update(AppUser user)
        => _db.Execute(@"
        UPDATE  dbo.AppUser
        SET     RoleId        = @RoleId,
                Name          = @Name,
                Surname       = @Surname,
                Email         = @Email,
                ContactNumber = @ContactNumber
        WHERE   UserId = @UserId;",
        Db.P("@UserId",        user.UserId),
        Db.P("@RoleId",        user.RoleId),
        Db.P("@Name",          user.Name.Trim()),
        Db.P("@Surname",       user.Surname.Trim()),
        Db.P("@Email",         user.Email.Trim()),
        Db.P("@ContactNumber", user.ContactNumber));

    /// <summary>Sets a new password, hashing it on the way in.</summary>
    public void SetPassword(int userId, string plainPassword)
        => _db.Execute(@"
        UPDATE  dbo.AppUser
        SET     PasswordHash = @PasswordHash
        WHERE   UserId = @UserId;",
        Db.P("@UserId",       userId),
        Db.P("@PasswordHash", PasswordHasher.Hash(plainPassword)));

    /// <summary>
    /// Turns an account on or off.
    ///
    /// Users are never deleted. Bookings and expenses record who captured them,
    /// and that trail has to survive somebody leaving the company - which is
    /// also why there is no Delete method on this class at all.
    /// </summary>
    public void SetActive(int userId, bool isActive)
        => _db.Execute(@"
        UPDATE  dbo.AppUser
        SET     IsActive = @IsActive
        WHERE   UserId = @UserId;",
        Db.P("@UserId",   userId),
        Db.P("@IsActive", isActive));

    // -------------------------------------------------------------------------

    private static AppUser Map(SqlDataReader r) => new AppUser
    {
        UserId        = (int)r["UserId"],
        RoleId        = (int)r["RoleId"],
        RoleName      = (string)r["RoleName"],
        Name          = (string)r["Name"],
        Surname       = (string)r["Surname"],
        Email         = (string)r["Email"],
        PasswordHash  = Db.GetNullableString(r, "PasswordHash") ?? "",
        ContactNumber = Db.GetNullableString(r, "ContactNumber"),
        IsActive      = (bool)r["IsActive"],
        CreatedDate   = (DateTime)r["CreatedDate"]
    };
}
