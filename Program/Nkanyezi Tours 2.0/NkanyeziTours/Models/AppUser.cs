namespace Nkanyezi.Models;

public class AppUser
{
    public int UserId { get; set; }

    public int RoleId { get; set; }
    public string RoleName { get; set; } = "";

    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public string Email { get; set; } = "";
    public string? ContactNumber { get; set; }

    public string PasswordHash { get; set; } = "";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }

    public string FullName => $"{Name} {Surname}".Trim();

    public bool IsAdministrator => RoleName == RoleNames.Administrator;
    public bool IsSupervisor    => RoleName == RoleNames.Supervisor;
    public bool IsAgent         => RoleName == RoleNames.TravelAgent;
    public bool IsClient        => RoleName == RoleNames.Client;

    public bool IsStaff => !IsClient;
}

public static class RoleNames
{
    public const string Administrator = "Administrator";
    public const string Supervisor    = "Supervisor";
    public const string TravelAgent   = "Travel Agent";
    public const string Client        = "Client";

    public const string StaffOnly = Administrator + "," + Supervisor + "," + TravelAgent;

    public const string ManagementOnly = Administrator + "," + Supervisor;
}
