using System.Text;
using Microsoft.Extensions.Primitives;

namespace HuyHieuDang.Infrastructure.Facades.Common.Extensions;

public static class PathExtension
{
    public static readonly char[] PathSeparators = new[] { '/', '\\' };

    /// <summary>
    /// Combines two path parts.
    /// </summary>
    public static string Combine(string path, string other)
    {
        if (string.IsNullOrWhiteSpace(other))
        {
            return path;
        }

        if (other.StartsWith('/') || other.StartsWith('\\'))
        {
            // "other" is already an app-rooted path. Return it as-is.
            return other;
        }

        int index = path.LastIndexOfAny(PathSeparators);

        if (index != path.Length - 1)
        {
            // If the first ends in a trailing slash e.g. "/Home/", assume it's a directory.
            return path + "/" + other;
        }
        else
        {
            return string.Concat(path.AsSpan(0, index + 1), other);
        }
    }

    /// <summary>
    /// Combines multiple path parts.
    /// </summary>
    public static string Combine(string path, params string[] others)
    {
        string result = path;

        for (int i = 0; i < others.Length; i++)
        {
            result = Combine(result, others[i]);
        }

        return result;
    }
}