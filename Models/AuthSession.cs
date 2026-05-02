namespace BimAiAssistant.Models;

public sealed class AuthSession
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string UserName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
}
