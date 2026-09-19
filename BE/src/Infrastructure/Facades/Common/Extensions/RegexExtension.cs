using System.Text.RegularExpressions;

namespace HuyHieuDang.Infrastructure.Facades.Common.Extensions;

public static partial class RegexExtension
{
    public static readonly Regex NistPassword = NistPasswordRegex();
    public static readonly Regex Whitespace = WhiteSpaceRegex();
    public static readonly Regex VnPhoneNumber = VnPhoneNumberRegex();
    private const string TextToReplace = nameof(TextToReplace);
    private const string Seperator = ";";
    private static readonly string SpecialCharacters = $"[^a-zA-Z0-9{TextToReplace}]+";
    private static readonly string EqualSeperator = $@"{TextToReplace}(\s*)=(\s*)[^;]+{Seperator}";

    public static string ReplaceWhitespace(this string input, string replacement)
    {
        return Whitespace.Replace(input, replacement);
    }

    public static string ReplaceSpecialCharacters(this string input, string replacement, string? acceptCharacters = null)
    {
        string template = SpecialCharacters.Replace(TextToReplace, acceptCharacters, StringComparison.Ordinal);
        return new Regex(template).Replace(input, replacement);
    }

    public static string ReplaceByEqualSeperator(this string input, string replacement, params string[] props)
    {
        foreach (string key in props)
        {
            string pattern = EqualSeperator.Replace(TextToReplace, key, StringComparison.OrdinalIgnoreCase);
            input = Regex.Replace(input, pattern, replacement);
        }

        return input;
    }

    /// <summary>
    /// Checks whether the given string is a valid NIST password based on specific criteria.
    /// </summary>
    /// <param name="password">The input string to check for NIST password validity.</param>
    /// <returns>true if the string is a valid NIST password; otherwise, false.</returns>
    public static bool IsNISTPassword(this string password)
    {
        return NistPassword.IsMatch(password);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhiteSpaceRegex();

    [GeneratedRegex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")]
    private static partial Regex NistPasswordRegex();

    [GeneratedRegex(@"^((((\+?)84)(0{0,1})|0)(3|5|7|8|9)\d{8})$")]
    private static partial Regex VnPhoneNumberRegex();
}