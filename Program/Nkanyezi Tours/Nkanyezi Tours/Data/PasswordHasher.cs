namespace Nkanyezi.Data;

/// <summary>
/// Turns a password into something safe to store, and checks one later.
///
/// WHY WE DO NOT STORE PASSWORDS
/// -----------------------------
/// If the database is ever seen by somebody who should not see it, plain text
/// passwords hand over every account at once, including the ones people reuse
/// on their email. A hash cannot be turned back into the password.
///
/// BCrypt is used because it is deliberately slow, which makes guessing
/// millions of passwords impractical, and because it puts a random salt inside
/// every hash automatically. That is why hashing the same password twice gives
/// two different strings, and why you must use Verify to compare rather than
/// hashing the attempt and testing the two strings for equality.
/// </summary>
public static class PasswordHasher
{
    /// <summary>
    /// Work factor. Higher is slower and safer. 11 takes roughly a quarter of a
    /// second, which nobody notices when logging in but is painful for anyone
    /// trying to guess.
    /// </summary>
    private const int WorkFactor = 11;

    /// <summary>Scrambles a password for storing.</summary>
    public static string Hash(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(plainPassword));

        return BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);
    }

    /// <summary>
    /// Checks a typed password against a stored hash. Returns false rather than
    /// throwing when the stored hash is missing or malformed, so that a broken
    /// row cannot crash the login page.
    /// </summary>
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
            // The stored value is not a valid BCrypt hash, so it cannot match.
            return false;
        }
    }
}
