using HuyHieuDang.Infrastructure.Facades.Definitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HuyHieuDang.Infrastructure.Facades.Validations;

/// <summary>
/// Dựng phản hồi 400 cho mọi lỗi ở tầng đọc tham số và xác thực dữ liệu của ASP.NET.
/// <para>
/// Hợp đồng API mục 1.5 chốt Backend chỉ trả <b>khóa</b>; chữ tiếng Anh mặc định của khung
/// (<c>The value '...' is not valid for Year.</c>) từng hiện thẳng lên banner người dùng và còn
/// dội lại nguyên giá trị người dùng nhập. Vì vậy: giữ nguyên khóa của FluentValidation khi có,
/// còn mọi thông điệp không phải khóa đều quy về <see cref="Messages.Common.InvalidParameter"/>.
/// </para>
/// </summary>
public static class ModelStateErrorFactory
{
    private static readonly string MessageKeyPrefix = Messages.Prefix + Messages.Delimiter;

    /// <summary>
    /// Khóa thông điệp tương ứng với một bộ lỗi model state.
    /// </summary>
    /// <param name="modelState">Bộ lỗi do khung dựng.</param>
    /// <returns>Khóa đầu tiên của FluentValidation, hoặc khóa chung cho lỗi ép kiểu.</returns>
    public static string ResolveMessage(ModelStateDictionary? modelState)
    {
        string? key = modelState?
            .Where(entry => entry.Value?.ValidationState is ModelValidationState.Invalid)
            .SelectMany(entry => entry.Value!.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(IsMessageKey);

        return key ?? Messages.Common.InvalidParameter;
    }

    /// <summary>
    /// Phản hồi 400 dạng A của hợp đồng API (mục 1.4): đúng một trường <c>message</c>.
    /// </summary>
    /// <param name="context">Ngữ cảnh hành động hiện tại.</param>
    /// <returns>Kết quả 400 mang khóa thông điệp.</returns>
    public static IActionResult Build(ActionContext context)
        => new BadRequestObjectResult(new { message = ResolveMessage(context?.ModelState) });

    private static bool IsMessageKey(string? message)
        => message?.StartsWith(MessageKeyPrefix, StringComparison.Ordinal) == true;
}
