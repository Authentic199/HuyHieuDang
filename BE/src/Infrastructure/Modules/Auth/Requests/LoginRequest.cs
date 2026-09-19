using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;

namespace HuyHieuDang.Infrastructure.Modules.Auth.Requests;

/// <summary>
/// Thân yêu cầu của <c>POST /api/Auth/Login</c>.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Tên đăng nhập.
    /// </summary>
    public string Username { get; set; } = default!;

    /// <summary>
    /// Mật khẩu ở dạng chữ thường.
    /// </summary>
    public string Password { get; set; } = default!;
}

/// <summary>
/// Chỉ kiểm tra hai trường có mặt; sai tài khoản hay sai mật khẩu đều do service trả lời
/// bằng một thông điệp chung, không phân biệt (UC-00).
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(Messages<User>.Required(x => x.Username));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(Messages<User>.Required(x => x.Password));
    }
}
