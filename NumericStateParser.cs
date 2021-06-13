namespace InputProcessorApp
{
    using System.Text.RegularExpressions;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Parses strings into other primitive data types, and allows for certain formats such as
    /// binary and hexadecimal prefixes, as well as scientific notation. When parsing strings into booleans,
    /// custom states for strings can be added.
    /// </summary>
    public class NumericStateParser
    {
        public enum FormatStyle
        {
            SIFormat = 0,
            EUFormat = 1,
            ENFormat = 2
        }

        private FormatStyle _style;

        public FormatStyle Style
        {
            get => _style;
            set
            {
                switch (value)
                {
                    case FormatStyle.EUFormat:
                        _formatChars = new List<char>() {'_', ' ', '.'};
                        _decimalChars = new List<char>() {','};
                        break;
                    case FormatStyle.ENFormat:
                        _formatChars = new List<char>() {'_', ' ', ','};
                        _decimalChars = new List<char>() {'.'};
                        break;
                    case FormatStyle.SIFormat:
                        _formatChars = new List<char>() {'_', ' '};
                        _decimalChars = new List<char>() {'.', ','};
                        break;
                    default:
                        _formatChars = new List<char>() {'_', ' '};
                        _decimalChars = new List<char>() {'.', ','};
                        break;
                }

                _style = value;
            }
        }

        private List<char> _formatChars, _decimalChars;

        /// <summary>
        /// Set of arguments that are always processed as <c>false</c>.
        /// </summary>
        public static IEnumerable<string> FixedFalseArgs { get; } = new HashSet<string> {"false", "0", "no", "n", "f"};

        /// <summary>
        /// Set of arguments that are always processed as <c>true</c> when being parsed into <c>boolean</c>.
        /// </summary>
        public static IEnumerable<string> FixedTrueArgs { get; } = new HashSet<string> {"true", "1", "yes", "y", "t"};

        /// <summary>
        /// The string arguments that translate to <c>false</c> when being parsed into <c>boolean</c>.
        /// </summary>
        public HashSet<string> FalseArgs { get; }

        /// <summary>
        /// The string arguments that translate to <c>true</c> when being parsed into <c>boolean</c>.
        /// </summary>
        public HashSet<string> TrueArgs { get; }

        /// <summary>
        /// Default constructor for <see cref="NumericStateParser"/> class. Sets default arguments for
        /// <c>true</c> and <c>false</c> boolean parsing from strings, which can be modified for the parser.
        /// </summary>
        public NumericStateParser(FormatStyle style = default)
        {
            FalseArgs = new HashSet<string>(FixedFalseArgs);
            TrueArgs = new HashSet<string>(FixedTrueArgs);
            Style = style;
        }

        private static decimal DecPow(int exp)
        {
            decimal pow = 10M, ret = 1M;
            while (exp > 0)
            {
                if ((exp & 1) == 1)
                {
                    ret *= pow;
                }

                pow *= pow;
                exp >>= 1;
            }

            return ret;
        }

        private static double DoublePow(int exp)
        {
            long pow = 10, ret = 1;
            while (exp > 0)
            {
                if ((exp & 1) == 1)
                {
                    ret *= pow;
                }

                pow *= pow;
                exp >>= 1;
            }

            return ret;
        }

        public void AddFalseArgs(IEnumerable<string> args) => FalseArgs.UnionWith(args);

        public void AddTrueArgs(IEnumerable<string> args) => TrueArgs.UnionWith(args);

        /// <summary>
        /// Removes all non-fixed and custom false arguments in the given set from FalseArgs.
        /// </summary>
        /// <param name="args">The string arguments to remove.</param>
        public void RemoveFalseArgs(IEnumerable<string> args)
        {
            FalseArgs.ExceptWith(args);
            FalseArgs.UnionWith(FixedFalseArgs);
        }

        private static int CharVal(char c)
        {
            if (c is (< '0' or > '9') and (< 'A' or > 'Z') and (< 'a' or > 'z')) return -1;
            return c switch
            {
                <= '9' => c - '0',
                <= 'Z' => 10 + (c - 'A'),
                _ => 36 + (c - 'a')
            };
        }

        public int ParseSigned32Bit(string parse)
        {
            int ret = 0, pow = 0;
            if (Regex.IsMatch(parse, "^0[xX]."))
            {
                return ParseInt(parse[2..].ToUpperInvariant(), 16);
            }

            if (Regex.IsMatch(parse, "^0[bB].?[01]+")) parse = parse[2..];

            for (var i = 0; i < parse.Length; i++)
            {
                var c = parse[i];
                if (c is '0' or '1') pow++;
                else if (_formatChars.Contains(c)) continue;
                else
                {
                    throw new ArgumentException($"The given number is illegal: \'{parse}\' at index {i}.");
                }

                if (pow > 32)
                    throw new ArgumentException("The binary string must be 32 bits long or be written in " +
                                                "hexadecimal format.", nameof(parse));
                ret <<= 1;
                ret |= c - '0';
            }

            return ret;
        }

        public long ParseSigned64Bit(string parse)
        {
            long ret = 0, pow = 0;
            if (Regex.IsMatch(parse, "^0[xX]."))
            {
                return ParseInt(parse[2..].ToUpperInvariant(), 16);
            }

            if (Regex.IsMatch(parse, "^0[bB].?[01]+")) parse = parse[2..];

            for (var i = 0; i < parse.Length; i++)
            {
                var c = parse[i];
                if (c is '0' or '1') pow++;
                else if (_formatChars.Contains(c)) continue;
                else
                {
                    throw new ArgumentException($"The given number is illegal: \'{parse}\' at index {i}.", nameof(parse));
                }

                if (pow > 64)
                    throw new ArgumentException("The binary string must be 32 bits long or be written in " +
                                                "hexadecimal format.", nameof(parse));
                ret <<= 1;
                ret |= (uint) (c - '0');
            }

            return ret;
        }

        public uint ParseUnsigned32Bit(string parse)
        {
            uint ret = 0, pow = 0;
            if (parse[0] == '-')
                throw new ArgumentException(
                    "The given number is illegal: \'{parse}\' at index 0 because unsigned integers cannot be negative.",
                    nameof(parse));
            if (Regex.IsMatch(parse, "^0[xX]."))
            {
                ret = (uint) ParseInt(parse[2..].ToUpperInvariant(), 16);
            }

            if (Regex.IsMatch(parse, "^0[bB].?[01]+")) parse = parse[2..];

            for (var i = 0; i < parse.Length; i++)
            {
                var c = parse[i];
                if (c is '0' or '1') pow++;
                else if (_formatChars.Contains(c)) continue;
                else
                {
                    throw new ArgumentException($"The given number is illegal: \'{parse}\' at index {i}.", nameof(parse));
                }

                if (pow > 32)
                    throw new ArgumentException("The binary string must be 32 bits long or be written in " +
                                                "hexadecimal format.", nameof(parse));
                ret <<= 1;
                ret |= (uint) c - '0';
            }

            return ret;
        }

        public ulong ParseUnsigned64Bit(string parse)
        {
            ulong ret = 0, pow = 0;
            if (parse[0] == '-')
                throw new ArgumentException(
                    "The given number is illegal: \'{parse}\' at index 0 because unsigned integers cannot be negative.",
                    nameof(parse));
            if (Regex.IsMatch(parse, "^0[xX]."))
            {
                ret = (ulong) ParseLong(parse[2..].ToUpperInvariant(), 16);
            }

            if (Regex.IsMatch(parse, "^0[bB].?[01]+")) parse = parse[2..];

            for (var i = 0; i < parse.Length; i++)
            {
                var c = parse[i];
                if (c is '0' or '1') pow++;
                else if (_formatChars.Contains(c)) continue;
                else
                {
                    throw new ArgumentException($"The given number is illegal: \'{parse}\' at index {i}.", nameof(parse));
                }

                if (pow > 64)
                    throw new ArgumentException("The binary string must be 32 bits long or be written in " +
                                                "hexadecimal format.", nameof(parse));
                ret <<= 1;
                ret |= (uint) c - '0';
            }

            return ret;
        }

        public short ParseShort(string parse, int radix) => (short) ParseInt(parse, radix);

        public int ParseInt(string parse, int radix) => (int) ParseLong(parse, radix);

        public long ParseLong(string parse, int radix)
        {
            if (radix is < 1 or > 62)
            {
                throw new ArgumentOutOfRangeException(nameof(radix),
                    "The given base is invalid. Bases must be between 1 and 62.");
            }

            var ret = 0;
            var negative = parse[0] == '-';
            if (negative) parse = parse[1..];
            if (Regex.IsMatch(parse, "^0[xX].") && radix is 10 or 16 or 2)
            {
                radix = 16;
                parse = parse[2..].ToUpperInvariant();
            }
            else if (Regex.IsMatch(parse, "^0[bB].?[01]") && radix is 10 or 2 or 16)
            {
                radix = 2;
                parse = parse[2..];
            }

            for (var i = 0; i < parse.Length; i++)
            {
                ret *= radix;
                var val = CharVal(parse[i]);
                if ((!_formatChars.Contains(parse[i]) && val == -1) || (val > radix && radix != 1) ||
                    (radix == 1 && val != 1))
                {
                    throw new ArgumentException($"The given number is illegal: \'{parse}\' at index {i}.", nameof(parse));
                }

                ret += val;
            }

            return negative ? -ret : ret;
        }

        public double ParseDouble(string parse, int radix)
        {
            if (parse.Equals("NaN")) return double.NaN;
            if (radix is < 1 or > 62)
            {
                throw new ArgumentOutOfRangeException(nameof(radix),
                    "The given base is invalid. Bases must be between 1 and 62.");
            }

            double ret = 0, div = 1;
            bool negative = parse[0] == '-', dec = false;
            if (negative) parse = parse[1..];
            if (Regex.IsMatch(parse, "^0[xX].") && radix is 10 or 16 or 2)
            {
                radix = 16;
                parse = parse[2..].ToUpperInvariant();
            }
            else if (Regex.IsMatch(parse, "^0[bB].?[01]") && radix is 10 or 2 or 16)
            {
                radix = 2;
                parse = parse[2..];
            }

            for (var i = 0; i < parse.Length; i++)
            {
                if (_decimalChars.Contains(parse[i]) && !dec)
                {
                    dec = true;
                    continue;
                }

                if (radix == 10 && (parse[i] == 'e' || parse[i] == 'E'))
                {
                    ret *= parse[i + 1] == '-'
                        ? 1d / DoublePow(ParseInt(parse[(i + 2)..], 10))
                        : DoublePow(ParseInt(parse[(i + 1)..], 10));
                    return negative ? -ret : ret;
                }

                var val = CharVal(parse[i]);
                if ((!_formatChars.Contains(parse[i]) && val == -1) || (val > radix && radix != 1) ||
                    (radix == 1 && val != 1))
                {
                    throw new ArgumentException($"The given number is illegal: {parse} at index {i}.", nameof(parse));
                }

                if (dec)
                {
                    ret += val / (div *= radix);
                }
                else
                {
                    ret = (ret * radix) + val;
                }
            }

            return negative ? -ret : ret;
        }

        public float ParseFloat(string parse, int radix) => (float) ParseDouble(parse, radix);

        public decimal ParseDecimal(string parse, int radix)
        {
            if (radix is < 1 or > 62)
            {
                throw new ArgumentOutOfRangeException(nameof(radix),
                    "The given base is invalid. Bases must be between 1 and 62.");
            }

            decimal ret = 0, div = 1;
            bool negative = parse[0] == '-', dec = false;
            if (negative) parse = parse[1..];
            if (Regex.IsMatch(parse, "^0[xX].") && radix is 10 or 16 or 2)
            {
                radix = 16;
                parse = parse[2..].ToUpperInvariant();
            }
            else if (Regex.IsMatch(parse, "^0[bB].?[01]") && radix is 10 or 2 or 16)
            {
                radix = 2;
                parse = parse[2..];
            }

            for (var i = 0; i < parse.Length; i++)
            {
                if (_decimalChars.Contains(parse[i]) && !dec)
                {
                    dec = true;
                    continue;
                }

                if (radix == 10 && (parse[i] == 'e' || parse[i] == 'E'))
                {
                    ret *= parse[i + 1] == '-'
                        ? decimal.One / DecPow(ParseInt(parse[(i + 2)..], 10))
                        : DecPow(ParseInt(parse[(i + 1)..], 10));
                    return negative ? -ret : ret;
                }

                var val = CharVal(parse[i]);
                if ((!_formatChars.Contains(parse[i]) && val == -1) || (val > radix && radix != 1) ||
                    (radix == 1 && val != 1))
                {
                    throw new ArgumentException($"The given number is illegal: \'{parse}\' at index {i}.", nameof(parse));
                }

                if (dec)
                {
                    ret += val / (div *= radix);
                }
                else
                {
                    ret = (ret * radix) + val;
                }
            }

            return negative ? -ret : ret;
        }

        public bool TryParseSigned32Bit(string parse, out int result)
        {
            try
            {
                result = ParseSigned32Bit(parse);
            }
            catch
            {
                result = 0;
                return false;
            }

            return true;
        }

        public bool TryParseSigned64Bit(string parse, out long result)
        {
            try
            {
                result = ParseSigned64Bit(parse);
            }
            catch
            {
                result = 0L;
                return false;
            }

            return true;
        }

        public bool TryParseUnsigned32Bit(string parse, out uint result)
        {
            try
            {
                result = ParseUnsigned32Bit(parse);
            }
            catch
            {
                result = 0;
                return false;
            }

            return true;
        }

        public bool TryParseUnsigned64Bit(string parse, out ulong result)
        {
            try
            {
                result = ParseUnsigned64Bit(parse);
            }
            catch
            {
                result = 0L;
                return false;
            }

            return true;
        }

        public bool ParseBool(string state)
        {
            if (FalseArgs.Contains(state.ToLowerInvariant()))
            {
                return false;
            }

            if (TrueArgs.Contains(state.ToLowerInvariant()))
            {
                return true;
            }

            throw new InvalidOperationException($"The given value is illegal: \"{state}\"");
        }

        public bool TryParseShort(string parse, int radix, out short result)
        {
            try
            {
                result = ParseShort(parse, radix);
            }
            catch
            {
                result = default;
                return false;
            }

            return true;
        }

        public bool TryParseInt(string parse, int radix, out int result)
        {
            try
            {
                result = ParseInt(parse, radix);
            }
            catch
            {
                result = 0;
                return false;
            }

            return true;
        }

        public bool TryParseLong(string parse, int radix, out long result)
        {
            try
            {
                result = ParseLong(parse, radix);
            }
            catch
            {
                result = 0L;
                return false;
            }

            return true;
        }

        public bool TryParseFloat(string parse, int radix, out float result)
        {
            try
            {
                result = ParseFloat(parse, radix);
            }
            catch
            {
                result = 0;
                return false;
            }

            return true;
        }

        public bool TryParseDouble(string parse, int radix, out double result)
        {
            try
            {
                result = ParseDouble(parse, radix);
            }
            catch
            {
                result = 0;
                return false;
            }

            return true;
        }

        public bool TryParseDecimal(string parse, int radix, out decimal result)
        {
            try
            {
                result = ParseDecimal(parse, radix);
            }
            catch
            {
                result = 0M;
                return false;
            }

            return true;
        }

        public bool TryParseBool(string state, out bool result)
        {
            try
            {
                result = ParseBool(state);
            }
            catch
            {
                result = false;
                return false;
            }

            return true;
        }
    }
}