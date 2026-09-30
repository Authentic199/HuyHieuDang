using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Notes;

namespace HuyHieuDang.Infrastructure.UnitTests;

/// <summary>
/// QT12 — quy tắc ghi chú và ngày ghi. Luật nằm ở đúng một chỗ nên bộ kiểm thử này phủ cho cả
/// ba đường ghi: <c>POST</c>, <c>PUT</c> và <c>PUT /Note</c>.
/// </summary>
public class PartyMemberNoteTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 30, 14, 5, 0, TimeSpan.FromHours(7));

    private static readonly DateTimeOffset Earlier =
        new(2026, 1, 2, 8, 0, 0, TimeSpan.FromHours(7));

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("\r\n\t ", null)]
    [InlineData("  Sức khỏe yếu  ", "Sức khỏe yếu")]
    [InlineData("Dòng 1\nDòng 2", "Dòng 1\nDòng 2")]
    public void Normalize_ShouldTrimAndTurnBlankIntoNull(string? raw, string? expected)
        => Assert.Equal(expected, PartyMemberNote.Normalize(raw));

    [Fact]
    public void IsWithinMaxLength_ShouldCountAfterTrimming()
    {
        string exactly = new('a', PartyMemberNote.MaxLength);

        Assert.True(PartyMemberNote.IsWithinMaxLength(null));
        Assert.True(PartyMemberNote.IsWithinMaxLength("   "));
        Assert.True(PartyMemberNote.IsWithinMaxLength(exactly));
        Assert.True(PartyMemberNote.IsWithinMaxLength("   " + exactly + "   "));
        Assert.False(PartyMemberNote.IsWithinMaxLength(exactly + "a"));
    }

    [Fact]
    public void Apply_ShouldStampWhenNoteAppears()
    {
        PartyMember entity = new();

        PartyMemberNote.Apply(entity, "  Ghi chú mới  ", Now);

        Assert.Equal("Ghi chú mới", entity.Note);
        Assert.Equal(Now, entity.NoteUpdatedAt);
    }

    [Fact]
    public void Apply_ShouldKeepStampWhenContentIsUnchanged()
    {
        PartyMember entity = WithNote("Ghi chú cũ", Earlier);

        // Cùng nội dung, chỉ khác khoảng trắng hai đầu: coi như không đổi.
        PartyMemberNote.Apply(entity, "   Ghi chú cũ   ", Now);

        Assert.Equal("Ghi chú cũ", entity.Note);
        Assert.Equal(Earlier, entity.NoteUpdatedAt);
    }

    [Fact]
    public void Apply_ShouldStampWhenContentChanges()
    {
        PartyMember entity = WithNote("Ghi chú cũ", Earlier);

        PartyMemberNote.Apply(entity, "Ghi chú mới", Now);

        Assert.Equal("Ghi chú mới", entity.Note);
        Assert.Equal(Now, entity.NoteUpdatedAt);
    }

    [Fact]
    public void Apply_ShouldClearBothFieldsWhenNoteIsRemoved()
    {
        PartyMember entity = WithNote("Ghi chú cũ", Earlier);

        PartyMemberNote.Apply(entity, "   ", Now);

        Assert.Null(entity.Note);
        Assert.Null(entity.NoteUpdatedAt);
    }

    [Fact]
    public void Apply_ShouldLeaveEmptyNoteUntouched()
    {
        PartyMember entity = new();

        PartyMemberNote.Apply(entity, null, Now);

        Assert.Null(entity.Note);
        Assert.Null(entity.NoteUpdatedAt);
    }

    /// <summary>
    /// Bản ghi đã có sẵn ghi chú và ngày ghi, để thử các nhánh "đổi" và "không đổi".
    /// </summary>
    /// <param name="note">Nội dung ghi chú đang lưu.</param>
    /// <param name="stampedAt">Ngày ghi đang lưu.</param>
    /// <returns>Bản ghi đảng viên.</returns>
    private static PartyMember WithNote(string note, DateTimeOffset stampedAt)
        => new() { Note = note, NoteUpdatedAt = stampedAt };
}
