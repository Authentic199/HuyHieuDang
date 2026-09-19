namespace HuyHieuDang.Infrastructure.Modules.AwardPeriods.Requests;

/// <summary>
/// Thân yêu cầu của <c>PUT /api/AwardPeriods/{id}</c> (UC-32). Gửi đủ cả năm trường.
/// </summary>
public class UpdateAwardPeriodRequest : AwardPeriodRequest
{
}

/// <summary>
/// Ràng buộc giống thêm mới; trùng tên bỏ qua chính đợt đang sửa, việc đó do service làm.
/// </summary>
public class UpdateAwardPeriodRequestValidator : AwardPeriodRequestValidator<UpdateAwardPeriodRequest>
{
}
