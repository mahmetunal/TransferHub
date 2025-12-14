using System.Security.Cryptography;
using System.Text;

namespace Shared.Common.Authentication;

public class User
{
    private string PasswordHash { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password + "TransferHub_Salt_2024");
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyPassword(string password)
    {
        return PasswordHash == HashPassword(password);
    }
}