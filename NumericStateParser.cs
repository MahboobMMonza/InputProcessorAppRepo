// TODO: find a better way to parse instead of calling for O(n) substring and fix unsigned
// TODO: allow for some compatibility with FormatStyles enums and stuff from MS
// TODO: deide what to do with parse signed integer values
namespace InputProcessorApp
{
    using System.Text.RegularExpressions;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
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

        private const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

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

        private static int CharVal(char c, bool hex = false)
        {
            if (c is (< '0' or > '9') and (< 'A' or > 'Z') and (< 'a' or > 'z')) return -1;
            return c switch
            {
                <= '9' => c - '0',
                <= 'Z' => 10 + (c - 'A'),
                _ => hex? 10 + (c - 'a') : 36 + (c - 'a')
            };
        }

        /*
         * To be decided what is to be done with this
         */
        /*public int ParseSigned32Bit(string parse)
        {
            int ret = 0, pow = 0;
            if (Regex.IsMatch(parse, "^-?0[xX][_,. ][\\da-fA-F]+?"))
            {
                return ParseInt(parse, 16);
            }

            if (Regex.IsMatch(parse, "^-?0[bB][_,. ]?[01].+?")) return ParseInt(parse, 2);

            for (var i = 0; i < parse.Length; i++)
            {
                var c = parse[i];
                if (c is '0' or '1') pow++;
                else if (_formatChars.Contains(c)) continue;
                else
                {
                    throw new ArgumentException($"The given number is illegal: \'{parse}\' at index {i}.");
                }
                
                ret <<= 1;
                ret |= c - '0';
            }
            if (pow != 32)
                throw new ArgumentException("The binary string must be 32 bits long or be written in " +
                                            "hexadecimal format.", nameof(parse));
            return ret;
        }*/

        /**
         * To be decided what is to be done with this
         */
        /*public long ParseSigned64Bit(string parse)
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
                    throw new ArgumentException($"The given number is illegal: \'{parse}\' at index {i}.",
                        nameof(parse));
                }

                if (pow > 64)
                    throw new ArgumentException("The binary string must be 32 bits long or be written in " +
                                                "hexadecimal format.", nameof(parse));
                ret <<= 1;
                ret |= (uint) (c - '0');
            }

            return ret;
        }*/

        public uint ParseUnsigned32Bit(string parse, int radix = 10) => (uint) ParseInt(parse, radix);
        
        public ulong ParseUnsigned64Bit(string parse, int radix = 10) => (ulong) ParseLong(parse, radix);

        public short ParseShort(string parse, int radix) => (short) ParseInt(parse, radix);

        public int ParseInt(string parse, int radix) => (int) ParseLong(parse, radix);

        public long ParseLong(string parse, int radix)
        {
            if (radix is < 1 or > 62)
            {
                throw new ArgumentOutOfRangeException(nameof(radix),
                    "The given base is invalid. Bases must be between 1 and 62.");
            }

            long ret = 0;
            var idx = 0;
            bool negative = parse[idx] == '-', hex = false;
            StringPreprocess(parse, ref negative, ref hex, ref radix, ref idx);
            
            for (; idx < parse.Length; idx++)
            {
                var ch = parse[idx];
                if (_formatChars.Contains(ch)) continue;
                switch (radix)
                {
                    case 2:
                        ret <<= 1;
                        break;
                    case 16:
                        ret <<= 4;
                        break;
                    default:
                        ret *= radix;
                        break;
                }

                var val = CharVal(ch, hex);
                if ((!_formatChars.Contains(ch) && val == -1) || (val > radix && radix != 1) ||
                    (radix == 1 && val != 1))
                {
                    throw new ArgumentException($"The given number is illegal: \'{parse}\' at index {idx}.",
                        nameof(parse));
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
            var idx = 0;
            bool negative = parse[0] == '-', dec = false, hex = false;
            StringPreprocess(parse, ref negative, ref hex, ref radix, ref idx);

            var infinite = Regex.IsMatch(parse, @"^-?\\inf(inity)?$", Options);
            if (infinite) {
                return negative ? double.NegativeInfinity : double.PositiveInfinity;
            }
            for (; idx < parse.Length; idx++)
            {
                var ch = parse[idx];
                if (_formatChars.Contains(ch)) continue;
                
                if (_decimalChars.Contains(ch) && !dec)
                {
                    dec = true;
                    continue;
                }

                if (radix == 10 && (ch is 'e' or 'E'))
                {
                    ret *= parse[idx + 1] == '-'
                        ? 1d / DoublePow(ParseInt(parse[(idx + 2)..], 10))
                        : DoublePow(ParseInt(parse[(idx + 1)..], 10));
                    return negative ? -ret : ret;
                }

                var val = CharVal(ch, hex);


                if ((!_formatChars.Contains(ch) && val == -1) || (val > radix && radix != 1) ||
                    (radix == 1 && val != 1))
                {
                    throw new ArgumentException($"The given number is illegal: {parse} at index {idx}.", 
                        nameof(parse));
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

        private static void StringPreprocess(string parse, ref bool negative, ref bool hex, ref int radix,
            ref int index)
        {
            if (negative) index++;
            if (Regex.IsMatch(parse, @"^-?0x[._, ]?[\da-f]+?", Options) && radix is 10 or 16 or 2)
            {
                hex = true;
                radix = 16;
                index += 2;
            }
            else if (Regex.IsMatch(parse, @"^-?0b[_,. ]?[01].+?", Options) && radix is 10 or 2 or 16)
            {
                radix = 2;
                index += 2;
            }
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
            var idx = 0;
            bool negative = parse[0] == '-', dec = false, hex = false;
            StringPreprocess(parse, ref negative, ref hex, ref radix, ref idx);
            
            for (; idx < parse.Length; idx++)
            {
                var ch = parse[idx];
                if (_formatChars.Contains(ch)) continue;
                
                if (_decimalChars.Contains(ch) && !dec)
                {
                    dec = true;
                    continue;
                }

                if (radix == 10 && (ch is 'e' or 'E'))
                {
                    ret *= parse[idx + 1] == '-'
                        ? decimal.One / DecPow(ParseInt(parse[(idx + 2)..], 10))
                        : DecPow(ParseInt(parse[(idx + 1)..], 10));
                    return negative ? -ret : ret;
                }

                var val = CharVal(ch, hex);

                if ((!_formatChars.Contains(ch) && val == -1) || (val > radix && radix != 1) ||
                    (radix == 1 && val != 1))
                {
                    throw new ArgumentException($"The given number is illegal: {parse} at index {idx}.", 
                        nameof(parse));
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


        /*
         * Contingent on ParseSigned32Bit
         */
        /*public bool TryParseSigned32Bit(string parse, out int result)
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
        }*/

        /*
         * Contingent on ParseSigned64Bit
         */
        /*public bool TryParseSigned64Bit(string parse, out long result)
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
        }*/

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