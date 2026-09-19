namespace HuyHieuDang.Infrastructure.Facades.Definitions
{
    /// <summary>
    /// Data Types for MySQL database.
    /// </summary>
    public static class ColumnType
    {
        public static class String
        {
            /// <summary>
            /// A fixed-length (0-255, default 1) string that is always right-padded with spaces to the specified length when stored.
            /// </summary>
            public static string Char(int maxLength) =>
                $"{nameof(Char).ToLower()}({maxLength})";

            /// <summary>
            /// A variable-length (0-65,535) string, the effective maximum length is subject to the maximum row size.
            /// </summary>
            public static string VarChar(int maxLength = 1000) =>
                $"{nameof(VarChar).ToLower()}({maxLength})";

            /// <summary>
            /// A TEXT column with a maximum length of 255 (2^8 - 1) characters, stored with a one-byte prefix indicating the length of the value in bytes.
            /// </summary>
            public static string TinyText() =>
                $"{nameof(TinyText).ToLower()}";

            /// <summary>
            /// A TEXT column with a maximum length of 65,535 (2^16 - 1) characters, stored with a two-byte prefix indicating the length of the value in bytes.
            /// </summary>
            public static string Text() =>
                $"{nameof(Text).ToLower()}";

            /// <summary>
            /// A TEXT column with a maximum length of 16,777,215 (2^24 - 1) characters, stored with a three-byte prefix indicating the length of the value in bytes.
            /// </summary>
            public static string MediumText() =>
                $"{nameof(MediumText).ToLower()}";

            /// <summary>
            /// A TEXT column with a maximum length of 4,294,967,295 or 4GiB (2^32 - 1) characters, stored with a four-byte prefix indicating the length of the value in bytes.
            /// </summary>
            public static string LongText() =>
                $"{nameof(LongText).ToLower()}";

            /// <summary>
            /// An enumeration, chosen from the list of up to 65,535 values or the special '' error value.
            /// </summary>
            public static string Enum() =>
                $"{nameof(Enum).ToLower()}";

            /// <summary>
            /// A single value chosen from a set of up to 64 members.
            /// </summary>
            public static string Set() =>
                $"{nameof(Set).ToLower()}";
        }

        public static class Numeric
        {
            /// <summary>
            /// A 1-byte integer, signed range is -128 to 127, unsigned range is 0 to 255.
            /// </summary>
            public static string TinyInt(byte? m = default) =>
                m == null ? $"{nameof(TinyInt).ToLower()}" : $"{nameof(TinyInt).ToLower()}({m})";

            /// <summary>
            /// A 2-byte integer, signed range is -32,768 to 32,767, unsigned range is 0 to 65,535.
            /// </summary>
            public static string SmallInt(byte? m = default) =>
                m == null ? $"{nameof(SmallInt).ToLower()}" : $"{nameof(SmallInt).ToLower()}({m})";

            /// <summary>
            /// A 3-byte integer, signed range is -8,388,608 to 8,388,607, unsigned range is 0 to 16,777,215.
            /// </summary>
            public static string MediumInt(byte? m = default) =>
                m == null ? $"{nameof(MediumInt).ToLower()}" : $"{nameof(MediumInt).ToLower()}({m})";

            /// <summary>
            /// A 4-byte integer, signed range is -2,147,483,648 to 2,147,483,647, unsigned range is 0 to 4,294,967,295.
            /// </summary>
            public static string Int(byte? m = default) =>
                m == null ? $"{nameof(Int).ToLower()}" : $"{nameof(Int).ToLower()}({m})";

            /// <summary>
            /// An 8-byte integer, signed range is -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807, unsigned range is 0 to 18,446,744,073,709,551,615.
            /// </summary>
            public static string BigInt(byte? m = default) =>
                m == null ? $"{nameof(BigInt).ToLower()}" : $"{nameof(BigInt).ToLower()}({m})";

            /// <summary>
            /// A fixed-point number (M, D) - the maximum number of digits (M) is 65 (default 10), the maximum number of decimals (D) is 30 (default 0).
            /// </summary>
            public static string Decimal(byte m = 10, byte d = 0) =>
                $"{nameof(Decimal).ToLower()}({m},{d})";

            /// <summary>
            /// A small floating-point number, allowable values are -3.402823466E+38 to -1.175494351E-38, 0, and 1.175494351E-38 to 3.402823466E+38.
            /// </summary>
            public static string Float(byte? m = default, byte? d = default) =>
                m == null || d == null ? $"{nameof(Float).ToLower()}" : $"{nameof(Float).ToLower()}({m},{d})";

            /// <summary>
            /// A double-precision floating-point number, allowable values are -1.7976931348623157E+308 to -2.2250738585072014E-308, 0,
            /// and 2.2250738585072014E-308 to 1.7976931348623157E+308.
            /// </summary>
            public static string Double(byte? m = default, byte? d = default) =>
                m == null || d == null ? $"{nameof(Double).ToLower()}" : $"{nameof(Double).ToLower()}({m},{d})";

            /// <summary>
            /// Synonym for DOUBLE (exception: in REAL_AS_FLOAT SQL mode it is a synonym for FLOAT).
            /// </summary>
            public static string Real() =>
                $"{nameof(Real).ToLower()}";

            /// <summary>
            /// A bit-field type (M), storing M of bits per value (default is 1, maximum is 64).
            /// </summary>
            public static string Bit(byte m = 1) =>
                $"{nameof(Bit).ToLower()}({m})";

            /// <summary>
            /// A synonym for TINYINT(1), a value of zero is considered false, nonzero values are considered true.
            /// </summary>
            public static string Boolean() =>
                $"{nameof(Boolean).ToLower()}";

            /// <summary>
            /// An alias for BIGINT UNSIGNED NOT NULL AUTO_INCREMENT UNIQUE.
            /// </summary>
            public static string Serial() =>
                $"{nameof(Serial).ToLower()}";
        }

        public static class DateAndTime
        {
            /// <summary>
            /// A date, supported range is 1000-01-01 to 9999-12-31.
            /// </summary>
            public static string Date() =>
                $"{nameof(Date).ToLower()}";

            /// <summary>
            /// A date and time combination, supported range is 1000-01-01 00:00:00 to 9999-12-31 23:59:59.
            /// </summary>
            public static string DateTime() =>
                $"{nameof(DateTime).ToLower()}";

            /// <summary>
            /// A timestamp, range is 1970-01-01 00:00:01 UTC to 2038-01-09 03:14:07 UTC, stored as the number of seconds since the epoch (1970-01-01 00:00:00 UTC)
            /// </summary>
            public static string Timestamp() =>
                $"{nameof(Timestamp).ToLower()}";

            /// <summary>
            /// A time, range is -838:59:59 to 838:59:59
            /// </summary>
            public static string Time() =>
                $"{nameof(Time).ToLower()}";

            /// <summary>
            /// A year in four-digit (4, default) or two-digit (2) format, the allowable values are 70 (1970) to 69 (2069) or 1901 to 2155 and 0000
            /// </summary>
            public static string Year() =>
                $"{nameof(Year).ToLower()}";
        }
    }
}