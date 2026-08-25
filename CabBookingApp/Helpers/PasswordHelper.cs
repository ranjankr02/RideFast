using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CabBookingApp.Helpers;

public static class PasswordHelper
{
    public static string CreateHash(string password)
    {
        byte[] salt   = RandomNumberGenerator.GetBytes(16);
        string saltB64 = Convert.ToBase64String(salt);
        return $"{saltB64}.{ComputeHash(password, salt)}";
    }

    public static bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('.', 2);
        if (parts.Length != 2) return false;
        byte[] salt = Convert.FromBase64String(parts[0]);
        string hash = ComputeHash(password, salt);
        return CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(hash),
            System.Text.Encoding.UTF8.GetBytes(parts[1]));
    }

    private static string ComputeHash(string password, byte[] salt) =>
        Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password:          password,
            salt:              salt,
            prf:               KeyDerivationPrf.HMACSHA256,
            iterationCount:    100_000,
            numBytesRequested: 32));
}
