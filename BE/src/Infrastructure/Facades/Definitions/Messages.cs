using Humanizer;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace HuyHieuDang.Infrastructure.Facades.Definitions;

public static partial class Messages
{
    public const char Delimiter = '.';
    public const string Prefix = "Mes";
    public const string Success = "Successfully";
    public const string Fail = "Failed";

    [DisplayName("Middleware")]
    public static class Middleware
    {
        public const string IPAddressForbidden = "Mes.Middleware.IPAddress.Forbidden";
    }

    /// <summary>
    /// Khóa dùng chung cho mọi endpoint, không thuộc thực thể nào.
    /// </summary>
    [DisplayName("Common")]
    public static class Common
    {
        /// <summary>
        /// Tham số truy vấn hoặc thân yêu cầu sai kiểu, khung ASP.NET không đọc nổi (mục 1.5).
        /// Không kèm tên tham số hay giá trị người dùng nhập.
        /// </summary>
        public const string InvalidParameter = "Mes.Common.Invalid.Parameter";
    }
}

public enum MessagesType
{
    /// <summary>
    /// The resource is not accessible
    /// </summary>
    NotAllowed,

    /// <summary>
    /// The resource is blocked.
    /// </summary>
    Blocked,

    /// <summary>
    /// The resource has already been used.
    /// </summary>
    WasUsed,

    /// <summary>
    /// The resource could not be found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The value is repeated.
    /// </summary>
    Repeated,

    /// <summary>
    /// The value is invalid.
    /// </summary>
    Invalid,

    /// <summary>
    /// The value must be empty.
    /// </summary>
    MustBeEmpty,

    /// <summary>
    /// The value is required.
    /// </summary>
    Required,

    /// <summary>
    /// The value is too long.
    /// </summary>
    OverLength,

    /// <summary>
    /// The value is not long enough.
    /// </summary>
    NotEnoughLength,

    /// <summary>
    /// The value cannot be all whitespace characters.
    /// </summary>
    NotWhiteSpace,

    /// <summary>
    /// The value cannot contain special characters.
    /// </summary>
    NotSpecialCharacter,

    /// <summary>
    /// The resource already exists.
    /// </summary>
    AlreadyExist,

    /// <summary>
    /// The resource has expired.
    /// </summary>
    Expired,
}

public static class Messages<T>
{
    public static string NotAllowed()
       => Action(MessagesType.NotAllowed);

    public static string Blocked()
       => Action(MessagesType.Blocked);

    public static string WasUsed()
       => Action(MessagesType.WasUsed);

    public static string WasUsed(string propertyName)
        => Action(MessagesType.WasUsed, propertyName);

    public static string NotFound()
        => Action(MessagesType.NotFound);

    public static string NotFound<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        => Action(MessagesType.NotFound, propertySelector);

    public static string NotFound(string propertyName)
        => Action(MessagesType.NotFound, propertyName);

    public static string Repeated<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        => Action(MessagesType.Repeated, propertySelector);

    public static string Repeated(string propertyName)
        => Action(MessagesType.Repeated, propertyName);

    public static string Invalid<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        => Action(MessagesType.Invalid, propertySelector);

    public static string Invalid(string propertyName)
        => Action(MessagesType.Invalid, propertyName);

    public static string MustBeEmpty<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        => Action(MessagesType.MustBeEmpty, propertySelector);

    public static string MustBeEmpty(string propertyName)
        => Action(MessagesType.MustBeEmpty, propertyName);

    public static string Required<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        => Action(MessagesType.Required, propertySelector);

    public static string Required(string propertyName)
        => Action(MessagesType.Required, propertyName);

    public static string OverLength<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        => Action(MessagesType.OverLength, propertySelector);

    public static string OverLength(string propertyName)
        => Action(MessagesType.OverLength, propertyName);

    public static string NotEnoughLength<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        => Action(MessagesType.NotEnoughLength, propertySelector);

    public static string NotEnoughLength(string propertyName)
         => Action(MessagesType.NotEnoughLength, propertyName);

    public static string NotWhiteSpace<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        => Action(MessagesType.NotWhiteSpace, propertySelector);

    public static string NotWhiteSpace(string propertyName)
        => Action(MessagesType.NotWhiteSpace, propertyName);

    public static string NotSpecialCharacter<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        => Action(MessagesType.NotSpecialCharacter, propertySelector);

    public static string NotSpecialCharacter(string propertyName)
        => Action(MessagesType.NotSpecialCharacter, propertyName);

    public static string AlreadyExist<TProperty>(Expression<Func<T, TProperty>> propertySelector)
         => Action(MessagesType.AlreadyExist, propertySelector);

    public static string AlreadyExist(string propertyName)
         => Action(MessagesType.AlreadyExist, propertyName);

    public static string Expired<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        => Action(MessagesType.Expired, propertySelector);

    public static string Expired(string propertyName)
       => Action(MessagesType.Expired, propertyName);

    public static string Expired() => Action(MessagesType.Expired);

    public static string Create(bool success = true)
          => Action(nameof(Create), success);

    public static string Create(string other, bool success = true)
          => Action(GetAction(nameof(Create), other), success);

    public static string Create<TProperty>(Expression<Func<T, TProperty>> propertySelector, bool success = true)
            => Action(GetAction(nameof(Create), propertySelector.GetPropertyFromExpression().Name), success);

    public static string Update(bool success = true)
        => Action(nameof(Update), success);

    public static string Update(string other, bool success = true)
        => Action(GetAction(nameof(Update), other), success);

    public static string Update<TProperty>(Expression<Func<T, TProperty>> propertySelector, bool success = true)
        => Action(GetAction(nameof(Update), propertySelector.GetPropertyFromExpression().Name), success);

    public static string View(string suffix, bool success = true)
        => Action(GetAction(nameof(View), suffix), success);

    public static string Detail(bool success = true)
        => View(nameof(Detail), success);

    public static string List(bool success = true)
        => View(nameof(List), success);

    public static string Search(bool success = true)
        => Action(nameof(Search), success);

    public static string Delete(bool success = true)
        => Action(nameof(Delete), success);

    public static string Delete(string other, bool success = true)
        => Action(GetAction(nameof(Delete), other), success);

    public static string Import(bool success = true)
        => Action(nameof(Import), success);

    public static string Import(string other, bool success = true)
        => Action(GetAction(nameof(Import), other), success);

    public static string SendMail(bool success = true)
        => Action(nameof(SendMail), success);

    public static string Action(string action) => GetMessageBase().Append(action).ToString();

    public static string Action(MessagesType type) => GetMessageBase().Append(type.ToString()).ToString();

    public static string Action(string action, bool success)
          => GetMessageBase().Append(action).Append(Messages.Delimiter).Append(success ? Messages.Success : Messages.Fail).ToString();

    public static string Action(MessagesType type, string? propertyName)
    {
        StringBuilder sb = GetMessageBase().Append(type.ToString());
        if (!string.IsNullOrEmpty(propertyName))
        {
            sb.Append(Messages.Delimiter).Append(propertyName);
        }

        return sb.ToString();
    }

    private static string Action<TProperty>(MessagesType type, Expression<Func<T, TProperty>> propertySelector)
        => GetMessageBase().Append(type.ToString()).Append(Messages.Delimiter).Append(propertySelector.GetPropertyFromExpression().Name).ToString();

    private static string GetAction(string action, string? other) => other == null ? action : action + other.Pascalize();

    private static StringBuilder GetMessageBase()
    {
        Type type = typeof(T);
        MessageDisplayAttribute? nameAttribute = type.GetCustomAttribute<MessageDisplayAttribute>();
        string moduleName = nameAttribute is null ? type.Name : nameAttribute.Name;
        return new StringBuilder(Messages.Prefix)
            .Append(Messages.Delimiter)
            .Append(moduleName)
            .Append(Messages.Delimiter);
    }
}