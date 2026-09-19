using System.Text;

namespace HuyHieuDang.Infrastructure.Facades.Common.Extensions;

public static class RamdomExtentions
{
    public const string Symbols = "!@#$%^&*()_-";
    public const string FullSymbols = @"!""#$%&'()*+,-./:;<=>?@[\]^_`{|}~";
    public static readonly Random Random = new(Environment.TickCount);

    public enum CharacterType
    {
        /// <summary>
        /// Chữ hoa
        /// </summary>
        Upper = 1,

        /// <summary>
        /// Chữ thường
        /// </summary>
        Lower = 2,

        /// <summary>
        /// Ký tự đặc biệt
        /// </summary>
        Symbol = 3,

        /// <summary>
        /// Chữ số
        /// </summary>
        Digit = 4,
    }

    public static string RandomString(int length, string chars)
    {
        StringBuilder build = new();
        build.Clear();
        for (int i = 0; i < length; i++)
        {
            build.Append(chars[Random.Next(0, chars.Length)]);
        }

        return build.ToString();
    }

    public static string RandomDigit(int lenght = 1)
    {
        if (lenght == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new();
        for (int i = 0; i < lenght; i++)
        {
            builder.Append(Convert.ToChar((int)Math.Floor((10 * Random.NextDouble()) + 48)));
        }

        return builder.ToString();
    }

    public static string RandomUpperCase(int lenght = 1)
    {
        if (lenght == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new();
        for (int i = 0; i < lenght; i++)
        {
            builder.Append(Convert.ToChar((int)Math.Floor((26 * Random.NextDouble()) + 65)));
        }

        return builder.ToString();
    }

    public static string RandomLowwerCase(int lenght = 1)
        => RandomUpperCase(lenght).ToLower();

    public static string RandomSymbols(int lenght = 1, string symbols = Symbols)
    {
        if (lenght == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new();
        for (int i = 0; i < lenght; i++)
        {
            builder.Append(symbols[Random.Next(symbols.Length)]);
        }

        return builder.ToString();
    }

    public static string RandomAlphabetOrSymbols(int lenght = 1, string symbols = Symbols, params CharacterType[] types)
    {
        if (lenght == 0)
        {
            return string.Empty;
        }

        if (!types.Any())
        {
            throw new ArgumentException("The type of character when perform random cant not be emty");
        }

        StringBuilder builder = new();
        foreach (int value in RandomRangeNotRepeat(lenght, 0, types.Length))
        {
            builder.Append(types[value] switch
            {
                CharacterType.Digit => RandomDigit(),
                CharacterType.Upper => RandomUpperCase(),
                CharacterType.Lower => RandomLowwerCase(),
                CharacterType.Symbol => RandomSymbols(symbols: symbols),
                _ => throw new NotImplementedException(),
            });
        }

        return builder.ToString();
    }

    /// <summary>
    /// <returns>Integer that is greater than or equal to min value and less than max value.</returns>
    /// </summary>
    private static IEnumerable<int> RandomRangeNotRepeat(int lenght, int minValue = 0, int maxValue = int.MaxValue)
    {
        IEnumerable<int> numer = Enumerable.Range(minValue, maxValue - minValue).OrderBy(_ => Random.Next());
        int numLenght = numer.Count();

        if (numLenght >= lenght)
        {
            return numer.Take(lenght);
        }

        return numer.Take(numLenght).Concat(RandomRangeNotRepeat(lenght - numLenght, minValue, maxValue));
    }
}