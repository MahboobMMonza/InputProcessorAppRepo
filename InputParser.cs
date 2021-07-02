namespace InputProcessorApp
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class <c>InputParser</c> reads text input from a source and splits each line into groups, returning data
    /// based on those groups.
    /// </summary>
    /// <remarks>Skips over empty lines. Can return data as parsed primitives.</remarks>>
    public class InputParser
    {
        private readonly NumericStateParser _numParser = new NumericStateParser();

        private Tokenizer _tokenizer;

        /// <summary>
        /// Gets and sets the <see cref="TextReader"/> from which input is processed. Default set to standard input
        /// stream.
        /// </summary>
        public TextReader Reader
        {
            get => _reader;
            set
            {
                switch (_reader)
                {
                    case null when value != null:
                        _reader = value;
                        break;
                    case null:
                        _reader = Console.In;
                        break;
                    default:
                        _reader.Close();
                        Indices = default;
                        _raw = null;
                        _index = _curLetter = 0;
                        _reader = value;
                        break;
                }
            }
        }
        
        /// <summary>
        /// Gets and sets the number of groups into which lines should be split into.
        /// </summary>
        /// <remarks>Default value is <c>-1</c>, which indicates splitting into all possible groups.</remarks>
        public int Indices
        {
            get => _maxIndices;
            set => _maxIndices = value > 0 ? value : -1;
        }

        /// <summary>
        /// List of pattern strings to split the line by.
        /// </summary>
        /// <remarks>Default delimiters are <c>space</c> and <c>tab</c>.</remarks>
        public string[] Delimiters
        {
            get => _delimiters;
            set
            {
                _delimiters = value is {Length: > 0} ? value : new[] {" ", "\t"};
                _tokenizer = new Tokenizer(_delimiters);
            }
        }

        /// <summary>
        /// Set of arguments that can be parsed into <see cref="bool"/> as <c>false</c>.
        /// </summary>
        public HashSet<string> FalseArgs => _numParser.FalseArgs;

        /// <summary>
        /// Set of arguments that can be parsed into <see cref="bool"/> as <c>true</c>>.
        /// </summary>
        public HashSet<string> TrueArgs => _numParser.TrueArgs;

        /// <summary>
        /// Fixed set of arguments that are parsed into <see cref="bool"/> as <c>false</c> by the program. The list
        /// consists of <c>{"false", "no", "f", "n", "0"}</c>.
        /// </summary>
        public static HashSet<string> FixedFalseArgs => new(NumericStateParser.FixedFalseArgs);

        /// <summary>
        /// Fixed set of arguments that are parsed into <see cref="bool"/> as <c>true</c> by the program. The list
        /// consists of <c>{"true", "yes", "t", "y", "1"}</c>.
        /// </summary>
        public static HashSet<string> FixedTrueArgs => new(NumericStateParser.FixedTrueArgs);

        private int[] _tokens;

        private string[] _delimiters;

        private string _raw;

        private int _index, _wordCount, _maxIndices = -1, _curLetter;

        private TextReader _reader;

        /// <summary>
        /// Constructor for an <see cref="InputParser"/> class with a specified number of maximum groups per line and
        /// optionally specified delimiters.
        /// </summary>
        /// <param name="input">The input stream to read from. Defaults to standard input stream if none is
        /// provided.</param>
        /// <param name="maxIndices">The maximum number of groups formed after splitting a line. Defaults to -1
        /// if an invalid value is given.</param>
        /// <param name="delimiters">The list of string patterns to split lines by.</param>
        public InputParser(TextReader input = null, int maxIndices = default, params string[] delimiters)
        {
            _tokens = null;
            _raw = null;
            Reader = input ?? Console.In;
            Delimiters = delimiters;
            _tokenizer = new Tokenizer(Delimiters);
            _index = _curLetter = 0;
            Indices = maxIndices;
        }

        /// <summary>
        /// Constructor for an <see cref="InputParser"/> class with optionally specified delimiters.
        /// </summary>
        /// <param name="input">The input stream to read from. Defaults to standard input stream if none is
        /// provided.</param>
        /// <param name="delimiters">The list of string patterns to split lines by.</param>
        public InputParser(TextReader input = null, params string[] delimiters)
        {
            _tokens = null;
            _raw = null;
            Reader = input ?? Console.In;
            Delimiters = delimiters;
            _tokenizer = new Tokenizer(Delimiters);
            _index = _curLetter = 0;
            Indices = -1;
        }

        /// <summary>
        /// Gets the next valid word group and returns it as a string.
        /// </summary>
        /// <returns>The next valid word group as a <see cref="string"/>.</returns>
        public string Next()
        {
            while (true)
            {
                if (!HasMoreTokens())
                {
                    NextTrimmedLine();
                }

                while (_curLetter < _tokens.Length - 1 && _tokens[_curLetter] == -1)
                {
                    _curLetter++;
                }

                int first = _curLetter, len = 0;
                if (_curLetter == _tokens.Length - 1)
                {
                    _index = 0;
                    continue;
                }

                _index = _tokens[_curLetter];

                while (first + len < _tokens.Length - 1 && _tokens[first + len] != -1 && _tokens[first + len] == _index)
                {
                    len++;
                }

                _curLetter = first + len;
                _index++;
                return _raw.Substring(first, len);
            }
        }

        private void GenerateTokens()
        {
            _tokens = _tokenizer.Tokenize(_raw, ref _maxIndices, ref _wordCount).ToArray();
            _index = 1;
            _curLetter = 0;
        }

        // Next Filtered Line: "Hi how are you doing" -> "Hihowareyoudoing"
        // This is an extension idea to the split line string GetNextLineArray stuff (another idea)
        
        /// <summary>
        /// Trims any occurrences of delimiter patterns from the beginning and end of the line, and returns the
        /// next valid line as a string.
        /// </summary>
        /// <returns>A trimmed version of the next line, as a <see cref="string"/>.</returns>
        public string NextTrimmedLine()
        {
            if (!HasMoreTokens() || string.IsNullOrEmpty(_raw))
            {
                do
                {
                    NextLine();
                    GenerateTokens();
                } while (!HasMoreTokens());

                _index = 0;
            }
            
            var last = _tokens.Length - 1;
            while (_tokens[_curLetter] == -1 || _tokens[last] == -1)
            {
                if (_tokens[_curLetter] == -1) _curLetter++;
                if (_tokens[last] == -1) last--;
            }
            
            _index = 0;
            if (_curLetter == 0 && last == _tokens.Length - 1) return _raw;
            return _raw.Substring(_curLetter, last - _curLetter + 1);
        }

        private string ReadLine() => _reader.ReadLine();

        /// <summary>
        /// Returns the next valid line as-is.
        /// </summary>
        /// <returns>The next line, as a <see cref="string"/>.</returns>
        public string NextLine()
        {
            if (!HasMoreTokens() || string.IsNullOrEmpty(_raw))
            {
                do
                {
                    _raw = ReadLine();
                } while (string.IsNullOrEmpty(_raw));

                _index = 0;
                return _raw;
            }

            _index = 0;
            return _raw[_curLetter..];
        }
        
        /// <summary>
        /// Gets the next valid character from the current line.
        /// </summary>
        /// <returns>The next valid character, as a <see cref="char"/>.</returns>
        public char NextChar()
        {
            while (true)
            {
                if (!HasMoreTokens())
                {
                    NextTrimmedLine();
                }

                while (_curLetter < _tokens.Length - 1 && _tokens[_curLetter] == -1) _curLetter++;
                if (_curLetter == _tokens.Length - 1)
                {
                    _index = 0;
                    continue;
                }

                var nextIndex = _curLetter + 1;

                while (_tokens[nextIndex] == -1) nextIndex++;

                _index = _tokens[nextIndex];

                return _raw[_curLetter++];
            }
        }

        /// <summary>
        /// Gets the next group and parses it as a <c>short</c>.
        /// </summary>
        /// <param name="radix">The base in which to parse the string in. Default base 10.</param>
        /// <returns><see cref="Next"/> as <see cref="Int16"/>.</returns>
        public short NextShort(int radix = 10) => _numParser.ParseShort(Next(), radix);

        // public int NextSigned32Bit() => _numParser.ParseSigned32Bit(Next());
        
        public uint NextUnsigned32Bit() => _numParser.ParseUnsigned32Bit(Next());

        // public long NextSigned64Bit() => _numParser.ParseSigned64Bit(Next());

        public ulong NextUnsigned64Bit() => _numParser.ParseUnsigned64Bit(Next());
        
        /// <summary>
        /// Gets the next group and parses it as an <c>int</c>.
        /// </summary>
        /// <param name="radix">The base in which to parse the string in. Default base 10.</param>
        /// <returns><see cref="Next"/> as <see cref="Int32"/>.</returns>
        public int NextInt(int radix = 10) => _numParser.ParseInt(Next(), radix);

        /// <summary>
        /// Gets the next group and parses it as a <c>long</c>.
        /// </summary>
        /// <param name="radix">The base in which to parse the string in. Default base 10.</param>
        /// <returns><see cref="Next"/> as <see cref="Int64"/>.</returns>
        public long NextLong(int radix = 10) => _numParser.ParseLong(Next(), radix);

        /// <summary>
        /// Gets the next group and parses it as a <c>float</c>.
        /// </summary>
        /// <param name="radix">The base in which to parse the string in. Default base 10.</param>
        /// <returns><see cref="Next"/> as <see cref="float"/></returns>
        public float NextFloat(int radix = 10) => _numParser.ParseFloat(Next(), radix);

        /// <summary>
        /// Gets the next group and parses it as a <c>double</c>.
        /// </summary>
        /// <param name="radix">The base in which to parse the string in. Default base 10.</param>
        /// <returns><see cref="Next"/> as <see cref="double"/></returns>
        public double NextDouble(int radix = 10) => _numParser.ParseDouble(Next(), radix);

        /// <summary>
        /// Gets the next group and parses it as a <c>decimal</c>.
        /// </summary>
        /// <param name="radix">The base in which to parse the string in. Default base 10.</param>
        /// <returns><see cref="Next"/> as <see cref="decimal"/></returns>
        public decimal NextDecimal(int radix = 10) => _numParser.ParseDecimal(Next(), radix);

        /// <summary>
        /// Gets the next group and parses it as a <c>bool</c>.
        /// </summary>
        /// <returns><see cref="Next"/> as a <see cref="bool"/>.</returns>
        public bool NextBool() => _numParser.ParseBool(Next());

        /// <summary>
        /// Attempts to parse the next group as a <c>short</c>.
        /// </summary>
        /// <param name="result">The attempted parse value, as a <see cref="short"/>.</param>
        /// <param name="radix">The base in which to parse the string in. Default base 10.</param>
        /// <returns><c>true</c> if the parse succeeded; otherwise <c>false</c>.</returns>
        public bool TryParseShort(out short result, int radix = 10) =>
            _numParser.TryParseShort(Next(), radix, out result);

        /// <summary>
        /// Attempts to parse the next group as an <c>int</c>.
        /// </summary>
        /// <param name="result">The attempted parse value, as an <see cref="Int32"/>.</param>
        /// <param name="radix">The base in which to parse the string in. Default base 10.</param>
        /// <returns><c>true</c> if the parse succeeded; otherwise <c>false</c>.</returns>
        public bool TryParseInt(out int result, int radix = 10) => _numParser.TryParseInt(Next(), radix, out result);

        /// <summary>
        /// Attempts to parse the next group as a <c>long</c>.
        /// </summary>
        /// <param name="result">The attempted parse value, as a <see cref="Int64"/>.</param>
        /// <param name="radix">The base in which to parse the string in. Default base 10.</param>
        /// <returns><c>true</c> if the parse succeeded; otherwise <c>false</c>.</returns>
        public bool TryParseLong(out long result, int radix = 10) => _numParser.TryParseLong(Next(), radix, out result);

        /// <summary>
        /// Attempts to parse the next group as a <c>float</c>.
        /// </summary>
        /// <param name="result">The attempted parse value, as a <see cref="float"/>.</param>
        /// <param name="radix">The base in which to parse the string in. Default base 10.</param>
        /// <returns><c>true</c> if the parse succeeded; otherwise <c>false</c>.</returns>
        public bool TryParseFloat(out float result, int radix = 10) =>
            _numParser.TryParseFloat(Next(), radix, out result);

        /// <summary>
        /// Attempts to parse the next group as a <c>double</c>.
        /// </summary>
        /// <param name="result">The attempted parse value, as a <see cref="double"/>.</param>
        /// <param name="radix">The base in which to parse the string in. Default base 10.</param>
        /// <returns><c>true</c> if the parse succeeded; otherwise <c>false</c>.</returns>
        public bool TryParseDouble(out double result, int radix = 10) =>
            _numParser.TryParseDouble(Next(), radix, out result);

        /// <summary>
        /// Attempts to parse the next group as a <c>decimal</c>.
        /// </summary>
        /// <param name="result">The attempted parse value, as a <see cref="decimal"/>.</param>
        /// <param name="radix">The base in which to parse the string in. Default base 10.</param>
        /// <returns><c>true</c> if the parse succeeded; otherwise <c>false</c>.</returns>
        public bool TryParseDecimal(out decimal result, int radix = 10) =>
            _numParser.TryParseDecimal(Next(), radix, out result);

        /// <summary>
        /// Determines whether or not more groups (tokens) are available to retrieve from the line.
        /// </summary>
        /// <returns><c>true</c> if more groups are remaining; otherwise <c>false</c></returns>
        public bool HasMoreTokens() => _index > 0 && _index <= _wordCount && _tokens is {Length: > 0};

        /// <summary>
        /// Adds <c>string</c> arguments that will be evaluated as <c>true</c> when parsed as <see cref="bool"/>.
        /// </summary>
        /// <remarks>Allows for <c>params</c> style arguments to be inputted.</remarks>
        /// <param name="args">The collection of strings that will evaluate to <c>true</c>.</param>
        public void AddTrueArg(params string[] args) => AddTrueArg(args.ToList());

        /// <summary>
        /// Adds <c>string</c> arguments that will be evaluated as <c>true</c> when parsed as <see cref="bool"/>.
        /// </summary>
        /// <param name="args">The collection of strings that will evaluate to <c>true</c>.</param>
        public void AddTrueArg(IEnumerable<string> args) => _numParser.AddTrueArgs(args);

        /// <summary>
        /// Adds <c>string</c> arguments that will be evaluated as <c>false</c> when parsed as <see cref="bool"/>.
        /// </summary>
        /// <remarks>Allows for <c>params</c> style arguments to be inputted.</remarks>
        /// <param name="args">The collection of strings that will evaluate to <c>false</c>.</param>
        public void AddFalseArg(params string[] args) => AddFalseArg(args.ToList());

        /// <summary>
        /// Adds <c>string</c> arguments that will be evaluated as <c>false</c> when parsed as <see cref="bool"/>.
        /// </summary>
        /// <param name="args">The collection of strings that will evaluate to <c>false</c>.</param>
        public void AddFalseArg(IEnumerable<string> args) => _numParser.AddFalseArgs(args);

        /// <summary>
        /// Gets the name of the current format style used for numbers when parsing.
        /// </summary>
        /// <returns>The name of the format style as a <see cref="string"/>.</returns>
        public string NumberFormat() => _numParser.Style.ToString();
    }
}