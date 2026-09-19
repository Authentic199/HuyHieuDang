namespace HuyHieuDang.Infrastructure.Modules.AwardPeriods.Requests;

/// <summary>
/// Thân yêu cầu của <c>POST /api/AwardPeriods</c> (UC-31).
/// </summary>
public class CreateAwardPeriodRequest : AwardPeriodRequest
{
}

/// <summary>
/// Ràng buộc của thêm mới; trùng tên do service kiểm tra.
/// </summary>
public class CreateAwardPeriodRequestValidator : AwardPeriodRequestValidator<CreateAwardPeriodRequest>
{
}
