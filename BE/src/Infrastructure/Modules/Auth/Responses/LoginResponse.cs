namespace HuyHieuDang.Infrastructure.Modules.Auth.Responses;

/// <summary>
/// Phần <c>data</c> của <c>POST /api/Auth/Login</c>.
/// </summary>
public class LoginResponse : SessionResponse
{
    /// <summary>
    /// Chuỗi JWT gắn vào header <c>Authorization</c> của mọi lời gọi sau.
    /// </summary>
    public string AccessToken { get; set; } = default!;

    /// <summary>
    /// Luôn là <c>Bearer</c>.
    /// </summary>
    public string TokenType { get; set; } = TokenTypes.Bearer;
}
