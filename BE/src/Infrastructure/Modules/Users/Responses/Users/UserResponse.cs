using AutoMapper;
using HuyHieuDang.Core.Bases;
using HuyHieuDang.Infrastructure.Facades.Identity.Base;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;

namespace HuyHieuDang.Infrastructure.Modules.Users.Responses.Users;

public abstract class UserResponse : BaseEntity<Guid>
{
    /// <summary>
    /// Tên người dùng
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Địa chỉ email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Tên đầy đủ
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Ngày sinh
    /// </summary>
    public DateTimeOffset? DayOfBirth { get; set; }

    /// <summary>
    /// Trạng thái hoạt động
    /// </summary>
    public OperationStatus Status { get; set; }

    /// <summary>
    /// Định danh vai trò
    /// </summary>
    public Guid? RoleId { get; set; }
}

public class UserResponseProfile : Profile
{
    public UserResponseProfile()
    {
        CreateMap<User, UserResponse>();
    }
}