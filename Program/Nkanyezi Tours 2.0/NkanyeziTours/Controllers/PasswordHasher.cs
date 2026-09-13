namespace Nkanyezi.Data;

public static class PasswordHasher
{
    private const int WorkFactor = 11;

    public static string Hash(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(plainPassword));

        return BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);
    }

    public static bool Verify(string plainPassword, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
        }
        catch
        {
            return false;
        }
    }
}
