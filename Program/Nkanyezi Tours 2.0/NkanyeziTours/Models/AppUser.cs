namespace Nkanyezi.Models;

/// <summary>
/// One row of the AppUser table: a person who can log in.
///
/// Called AppUser rather than User because User is a reserved word in SQL
/// Server, and because ASP.NET already has a User property on every page. Using
/// the same name would be confusing in both places at once.
/// </summary>
public class AppUser
{
    public int UserId { get; set; }

    public int RoleId { get; set; }
    public string RoleName { get; set; } = "";

    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public string Email { get; set; } = "";
    public string? ContactNumber { get; set; }

    /// <summary>
    /// The scrambled password. Only ever filled in by sp_User_GetByEmail during
    /// login. Every other procedure returns it empty on purpose, so a hash
    /// cannot leak out onto a screen by accident.
    ///
    /// Never display this, never log it, never put it in a hidden form field.
    /// </summary>
    public string PasswordHash { get; set; } = "";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }

    public string FullName => $"{Name} {Surname}".Trim();

    // Convenience checks for the screens, so that pages do not compare loose
    // strings like "Administrator" all over the place and misspell one of them.
    public bool IsAdministrator => RoleName == RoleNames.Administrator;
    public bool IsSupervisor    => RoleName == RoleNames.Supervisor;
    public bool IsAgent         => RoleName == RoleNames.TravelAgent;
    public bool IsClient        => RoleName == RoleNames.Client;

    /// <summary>True for anyone who works for the agency, as opposed to a client.</summary>
    public bool IsStaff => !IsClient;
}

/// <summary>
/// The four role names, spelled once.
///
/// Use these constants everywhere instead of typing the words. A misspelled
/// role string does not fail to compile, it just silently denies access, and
/// that is a miserable bug to find.
/// </summary>
public static class RoleNames
{
    public const string Administrator = "Administrator";
    public const string Supervisor    = "Supervisor";
    public const string TravelAgent   = "Travel Agent";
    public const string Client        = "Client";

    /// <summary>Everyone who works for the agency.</summary>
    public const string StaffOnly = Administrator + "," + Supervisor + "," + TravelAgent;

    /// <summary>Management, for the reports.</summary>
    public const string ManagementOnly = Administrator + "," + Supervisor;
}
