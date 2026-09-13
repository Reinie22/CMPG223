using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class UserRepository
{
    private readonly Db _db;

    public UserRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      u.UserId, u.RoleId, r.RoleName, u.Name, u.Surname, u.Email,
                    u.ContactNumber, u.IsActive, u.CreatedDate,
                    '' AS PasswordHash
        FROM        dbo.AppUser AS u
        INNER JOIN  dbo.Role    AS r ON r.RoleId = u.RoleId";

    public AppUser? GetByEmailForLogin(string email)
        => _db.QuerySingle(@"
        SELECT      u.*, r.RoleName
        FROM        dbo.AppUser AS u
        INNER JOIN  dbo.Role    AS r ON r.RoleId = u.RoleId
        WHERE       u.Email = @Email;",
        Map,
        Db.P("@Email", email.Trim()));

    public List<AppUser> GetAll(int? roleId = null, bool includeInactive = false)
        => _db.Query(SelectSql + @"
        WHERE   (@IncludeInactive = 1 OR u.IsActive = 1)
          AND   (@RoleId IS NULL OR u.RoleId = @RoleId)
        ORDER BY u.Surname, u.Name;",
        Map,
        Db.P("@RoleId",          roleId),
        Db.P("@IncludeInactive", includeInactive));

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

    public List<AppUser> GetByRole(string roleName)
        => _db.Query(SelectSql + @"
        WHERE   u.IsActive = 1
          AND   r.RoleName = @RoleName
        ORDER BY u.Surname, u.Name;",
        Map,
        Db.P("@RoleName", roleName));

    public List<AppUser> GetTravelAgents() => GetByRole(RoleNames.TravelAgent);

    public List<AppUser> GetClients() => GetByRole(RoleNames.Client);

    public bool EmailExists(string email, int? excludeUserId = null)
        => _db.Scalar<int>(@"
        SELECT  COUNT(*)
        FROM    dbo.AppUser
        WHERE   Email = @Email
          AND   (@ExcludeId IS NULL OR UserId <> @ExcludeId);",
        Db.P("@Email",     email.Trim()),
        Db.P("@ExcludeId", excludeUserId)) > 0;

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

    public void SetPassword(int userId, string plainPassword)
        => _db.Execute(@"
        UPDATE  dbo.AppUser
        SET     PasswordHash = @PasswordHash
        WHERE   UserId = @UserId;",
        Db.P("@UserId",       userId),
        Db.P("@PasswordHash", PasswordHasher.Hash(plainPassword)));

    public void SetActive(int userId, bool isActive)
        => _db.Execute(@"
        UPDATE  dbo.AppUser
        SET     IsActive = @IsActive
        WHERE   UserId = @UserId;",
        Db.P("@UserId",   userId),
        Db.P("@IsActive", isActive));

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
