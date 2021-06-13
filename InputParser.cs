namespace InputProcessorApp
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class that takes input from stream and splits it by tokens and reads primitive data types straight from console
    /// intermediate processing from user code.
    /// </summary>
    public class InputParser
    {
        private readonly NumericStateParser _numParser = new NumericStateParser();

        private Tokenizer _tokenizer;

        public TextReader Reader // public?
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

        public int Indices
        {
            get => _maxIndices;
            set => _maxIndices = value > 0 ? value : -1;
        }

        public string[] Delimiters
        {
            get => _delimiters;
            set { _delimiters = value is {Length: > 0} ? value : new[] {" ", "\t"}; }
        }

        public HashSet<string> FalseArgs => _numParser.FalseArgs;

        public HashSet<string> TrueArgs => _numParser.TrueArgs;

        public static HashSet<string> FixedFalseArgs => new(NumericStateParser.FixedFalseArgs);

        public static HashSet<string> FixedTrueArgs => new(NumericStateParser.FixedTrueArgs);

        private int[] _tokens;

        private string[] _delimiters;

        private string _raw;

        private int _index, _wordCount, _maxIndices = -1, _curLetter;

        private TextReader _reader;

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

        public string Next()
        {
            while (true)
            {
                if (!HasMoreTokens())
                {
                    NextSafeLine();
                }

                while (_curLetter < _tokens.Length && _tokens[_curLetter] == -1)
                {
                    _curLetter++;
                }

                int first = _curLetter, len = 0;
                if (_curLetter == _tokens.Length)
                {
                    _index = 0;
                    continue;
                }

                _index = _tokens[_curLetter];

                while (first + len < _tokens.Length && _tokens[first + len] != -1 && _tokens[first + len] == _index)
                {
                    len++;
                }

                _curLetter = first + len;
                if (_curLetter == _tokens.Length) _index++;
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
        // This is an extension to the split line string GetNextLineArray stuff
        public string NextSafeLine()
        {
            if (!HasMoreTokens() || string.IsNullOrEmpty(_raw))
            {
                do
                {
                    NextLine();
                    GenerateTokens();
                } while (!HasMoreTokens());

                _index = 0;
                return _raw;
            }

            while (_tokens[_curLetter] == -1) _curLetter++;

            _index = 0;
            return _raw[_curLetter..];
        }

        private string ReadLine() => _reader.ReadLine();

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

        public char NextChar()
        {
            while (true)
            {
                if (!HasMoreTokens())
                {
                    NextSafeLine();
                }

                while (_curLetter < _tokens.Length && _tokens[_curLetter] == -1) _curLetter++;
                if (_curLetter == _tokens.Length)
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

        public short NextShort(int radix = 10) => _numParser.ParseShort(Next(), radix);

        public int NextSigned32Bit() => _numParser.ParseSigned32Bit(Next());
        
        public uint NextUnsigned32Bit() => _numParser.ParseUnsigned32Bit(Next());

        public long NextSigned64Bit() => _numParser.ParseSigned64Bit(Next());

        public ulong NextUnsigned64Bit() => _numParser.ParseUnsigned64Bit(Next());
        
        public int NextInt(int radix = 10) => _numParser.ParseInt(Next(), radix);

        public long NextLong(int radix = 10) => _numParser.ParseLong(Next(), radix);

        public float NextFloat(int radix = 10) => _numParser.ParseFloat(Next(), radix);

        public double NextDouble(int radix = 10) => _numParser.ParseDouble(Next(), radix);

        public decimal NextDecimal(int radix = 10) => _numParser.ParseDecimal(Next(), radix);

        public bool NextBool() => _numParser.ParseBool(Next());

        public bool TryParseShort(out short result, int radix = 10) =>
            _numParser.TryParseShort(Next(), radix, out result);

        public bool TryParseInt(out int result, int radix = 10) => _numParser.TryParseInt(Next(), radix, out result);

        public bool TryParseLong(out long result, int radix = 10) => _numParser.TryParseLong(Next(), radix, out result);

        public bool TryParseFloat(out float result, int radix = 10) =>
            _numParser.TryParseFloat(Next(), radix, out result);

        public bool TryParseDouble(out double result, int radix = 10) =>
            _numParser.TryParseDouble(Next(), radix, out result);

        public bool TryParseDecimal(out decimal result, int radix = 10) =>
            _numParser.TryParseDecimal(Next(), radix, out result);

        public bool HasMoreTokens() => _index > 0 && _index <= _wordCount && _tokens is {Length: > 0};

        public void AddTrueArg(params string[] args) => AddTrueArg(args.ToList());

        public void AddTrueArg(IEnumerable<string> args) => _numParser.AddTrueArgs(args);

        public void AddFalseArg(params string[] args) => AddFalseArg(args.ToList());

        public void AddFalseArg(IEnumerable<string> args) => _numParser.AddFalseArgs(args);

        public string NumberFormat() => _numParser.Style.ToString();
    }
}