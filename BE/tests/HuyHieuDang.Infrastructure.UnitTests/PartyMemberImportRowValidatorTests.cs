using HuyHieuDang.Infrastructure.Modules.PartyMembers.Enums;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Models;

namespace HuyHieuDang.Infrastructure.UnitTests;

/// <summary>
/// Sáu mã lỗi cấp dòng của mục 4.2 hợp đồng API, cộng các ca chuẩn hóa OQ-4, OQ-5, OQ-6
/// và hai ca biên OQ-1, OQ-10. "Hôm nay" cố định ở T0 = 19/09/2026 như bộ fixture.
/// </summary>
public class PartyMemberImportRowValidatorTests
{
    private static readonly DateOnly Today = new(2026, 9, 19);

    [Fact(DisplayName = "Dòng đủ bốn ô hợp lệ thì không có lỗi và được chuẩn hóa")]
    public void Validate_ValidRow_ReturnsNormalizedRow()
    {
        IReadOnlyList<ImportRowErrorResponse> errors = Validate(
            "Nguyễn Hợp Lệ Một", "12/03/1974", "Nam", "01/10/1996", out ImportValidRowResponse? valid);

        Assert.Empty(errors);
        Assert.NotNull(valid);
        Assert.Equal("Nguyễn Hợp Lệ Một", valid!.FullName);
        Assert.Equal(new DateOnly(1974, 3, 12), valid.DateOfBirth);
        Assert.Equal("Male", valid.Gender);
        Assert.Equal(Gender.Male, valid.ToGender());
        Assert.Equal(new DateOnly(1996, 10, 1), valid.OfficialAdmissionDate);
    }

    [Fact(DisplayName = "Ngày sinh và giới tính bỏ trống vẫn hợp lệ, trả null")]
    public void Validate_OptionalCellsEmpty_IsValid()
    {
        IReadOnlyList<ImportRowErrorResponse> errors = Validate(
            "Lê Hợp Lệ Ba", string.Empty, string.Empty, "15/01/1996", out ImportValidRowResponse? valid);

        Assert.Empty(errors);
        Assert.Null(valid!.DateOfBirth);
        Assert.Null(valid.Gender);
        Assert.Null(valid.ToGender());
    }

    [Fact(DisplayName = "Ngày chính thức đúng bằng hôm nay là hợp lệ")]
    public void Validate_AdmissionDateEqualsToday_IsValid()
    {
        IReadOnlyList<ImportRowErrorResponse> errors = Validate(
            "Lưu Đúng Hôm Nay", "04/04/2000", "Nữ", "19/09/2026", out ImportValidRowResponse? valid);

        Assert.Empty(errors);
        Assert.Equal("Female", valid!.Gender);
    }

    [Fact(DisplayName = "Thiếu họ tên → MissingFullName")]
    public void Validate_MissingFullName()
        => AssertSingleError(
            Validate(string.Empty, "12/03/1974", "Nam", "01/10/1996", out _),
            ImportErrorCodes.MissingFullName,
            ImportFields.FullName);

    [Fact(DisplayName = "Thiếu ngày chính thức → MissingOfficialAdmissionDate, không kèm lỗi định dạng")]
    public void Validate_MissingOfficialAdmissionDate()
        => AssertSingleError(
            Validate("Nguyễn Thiếu Ngày", "12/03/1974", "Nam", string.Empty, out _),
            ImportErrorCodes.MissingOfficialAdmissionDate,
            ImportFields.OfficialAdmissionDate);

    [Fact(DisplayName = "Ngày chính thức dạng yyyy-MM-dd → InvalidDateFormat")]
    public void Validate_InvalidAdmissionDateFormat()
        => AssertSingleError(
            Validate("Trần Sai Định Dạng", "12/03/1974", "Nữ", "1996-10-01", out _),
            ImportErrorCodes.InvalidDateFormat,
            ImportFields.OfficialAdmissionDate);

    [Fact(DisplayName = "Ngày chính thức ở tương lai → FutureOfficialAdmissionDate")]
    public void Validate_FutureAdmissionDate()
        => AssertSingleError(
            Validate("Lê Ngày Tương Lai", "12/03/1974", "Nam", "20/09/2026", out _),
            ImportErrorCodes.FutureOfficialAdmissionDate,
            ImportFields.OfficialAdmissionDate);

    [Fact(DisplayName = "Giới tính lạ → InvalidGender")]
    public void Validate_InvalidGender()
        => AssertSingleError(
            Validate("Phạm Giới Tính Lạ", "12/03/1974", "Khác", "01/10/1996", out _),
            ImportErrorCodes.InvalidGender,
            ImportFields.Gender);

    [Fact(DisplayName = "Ngày sinh sau ngày chính thức → BirthDateAfterAdmissionDate")]
    public void Validate_BirthDateAfterAdmissionDate()
        => AssertSingleError(
            Validate("Hoàng Sinh Sau", "02/01/1997", "Nam", "01/10/1996", out _),
            ImportErrorCodes.BirthDateAfterAdmissionDate,
            ImportFields.DateOfBirth);

    [Fact(DisplayName = "OQ-10: ngày sinh bằng đúng ngày chính thức vẫn là lỗi")]
    public void Validate_BirthDateEqualsAdmissionDate_IsError()
        => AssertSingleError(
            Validate("Trùng Ngày", "01/10/1996", "Nam", "01/10/1996", out _),
            ImportErrorCodes.BirthDateAfterAdmissionDate,
            ImportFields.DateOfBirth);

    [Fact(DisplayName = "OQ-1: ngày sinh 31/02/1974 không có thật → InvalidDateFormat")]
    public void Validate_NonExistentBirthDate()
        => AssertSingleError(
            Validate("Vũ Ngày Sinh Sai", "31/02/1974", "Nữ", "01/10/1996", out _),
            ImportErrorCodes.InvalidDateFormat,
            ImportFields.DateOfBirth);

    [Fact(DisplayName = "OQ-2: một dòng nhiều lỗi trả đủ mọi lý do, đúng thứ tự bảng mã lỗi")]
    public void Validate_ManyErrors_ReturnsAllInTableOrder()
    {
        IReadOnlyList<ImportRowErrorResponse> errors =
            Validate(string.Empty, "31/02/1974", "Khác", string.Empty, out ImportValidRowResponse? valid);

        Assert.Null(valid);
        Assert.Equal(
            new[]
            {
                ImportErrorCodes.MissingFullName,
                ImportErrorCodes.MissingOfficialAdmissionDate,
                ImportErrorCodes.InvalidDateFormat,
                ImportErrorCodes.InvalidGender,
            },
            errors.Select(x => x.ErrorCode));
    }

    [Theory(DisplayName = "OQ-5: giới tính không phân biệt hoa thường")]
    [InlineData("nam", "Male")]
    [InlineData("NAM", "Male")]
    [InlineData("Nữ", "Female")]
    [InlineData("nữ", "Female")]
    [InlineData("NỮ", "Female")]
    public void Validate_GenderIsCaseInsensitive(string cell, string expected)
    {
        IReadOnlyList<ImportRowErrorResponse> errors =
            Validate("Trần Giới Tính", "05/07/1973", cell, "07/11/1996", out ImportValidRowResponse? valid);

        Assert.Empty(errors);
        Assert.Equal(expected, valid!.Gender);
    }

    [Theory(DisplayName = "OQ-6: ngày nhận cả một chữ số lẫn hai chữ số")]
    [InlineData("9/2/1975", "1/10/1996")]
    [InlineData("09/02/1975", "01/10/1996")]
    [InlineData("9/02/1975", "01/10/1996")]
    public void Validate_DateAcceptsSingleDigitParts(string birth, string admission)
    {
        IReadOnlyList<ImportRowErrorResponse> errors =
            Validate("Phạm Ngày Một Chữ Số", birth, "Nữ", admission, out ImportValidRowResponse? valid);

        Assert.Empty(errors);
        Assert.Equal(new DateOnly(1975, 2, 9), valid!.DateOfBirth);
        Assert.Equal(new DateOnly(1996, 10, 1), valid.OfficialAdmissionDate);
    }

    [Fact(DisplayName = "29/02 năm nhuận là ngày có thật, không bị coi là sai định dạng")]
    public void Validate_LeapDay_IsValid()
    {
        IReadOnlyList<ImportRowErrorResponse> errors =
            Validate("Ngô Văn Khánh", "29/02/1976", "Nam", "29/02/1996", out ImportValidRowResponse? valid);

        Assert.Empty(errors);
        Assert.Equal(new DateOnly(1996, 2, 29), valid!.OfficialAdmissionDate);
    }

    [Fact(DisplayName = "29/02 năm không nhuận là ngày không có thật → InvalidDateFormat")]
    public void Validate_LeapDayInCommonYear_IsError()
        => AssertSingleError(
            Validate("Ngày Không Có Thật", "12/03/1974", "Nam", "29/02/1997", out _),
            ImportErrorCodes.InvalidDateFormat,
            ImportFields.OfficialAdmissionDate);

    private static IReadOnlyList<ImportRowErrorResponse> Validate(
        string fullName, string birth, string gender, string admission, out ImportValidRowResponse? valid)
        => PartyMemberImportRowValidator.Validate(
            new ImportRawRow(2, fullName, birth, gender, admission), Today, out valid);

    private static void AssertSingleError(
        IReadOnlyList<ImportRowErrorResponse> errors, string expectedCode, string expectedField)
    {
        ImportRowErrorResponse error = Assert.Single(errors);
        Assert.Equal(expectedCode, error.ErrorCode);
        Assert.Equal(expectedField, error.Field);
    }
}
