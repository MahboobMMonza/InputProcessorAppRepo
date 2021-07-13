namespace InputProcessorApp
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // TODO: Optimization to token assignment using Aho-Corasick algorithm
    // TODO: Adjust Aho-Corasick algorithm to complete/blended matches

    /// <summary>
    /// Class <c>Tokenizer</c> determines all split groups possible given the delimiters and the maximum number of
    /// groups.
    /// </summary>
    /// <remarks>Implements KMP algorithm (unoptimized for real time) for each search.</remarks>
    public class Tokenizer
    {
        /// <summary>
        /// Constructor for <see cref="Tokenizer"/> with a given collection of string delimiters.
        /// </summary>
        /// <param name="delimiters">Collection of string delimiters to split by.</param>
        /// <param name="match">The matching system to indicate match hits.</param>
        public Tokenizer(IEnumerable<string> delimiters, Match match = Match.Blend)
        {
            _lpsTables = new List<int[]>();
            _emptyDelim = false;
            Delimiters = delimiters;
            PatternMatch = match;
        }

        /// <summary>
        /// Constructor for <see cref="Tokenizer"/> with a given collection of string delimiters.
        /// </summary>
        public Tokenizer(Match match = Match.Blend)
        {
            _lpsTables = new List<int[]>();
            _emptyDelim = false;
            PatternMatch = match;
        }

        private void UpdateTables()
        {
            for (var i = 0; i < _patterns.Count; i++)
            {
                if (_patterns[i].Length == 0)
                {
                    _emptyDelim = true;
                }

                _lpsTables.Add(Precompute(ref i));
            }
        }

        public IEnumerable<string> Delimiters
        {
            get => _patterns;
            set
            {
                _patterns = value.ToList();
                UpdateTables();
            }
        }

        private readonly List<int[]> _lpsTables;
        private List<string> _patterns;
        private bool _emptyDelim;

        public enum Match
        {
            Blend = 0,
            Complete = 1
        }

        public Match PatternMatch { get; set; }

        /// <summary>
        /// Creates an array with coded integer indicators of each valid and invalid sequence of characters in the line.
        /// </summary>
        /// <param name="line">The current line to tokenize.</param>
        /// <param name="maxIndices">The maximum number of token groups to create.</param>
        /// <param name="wordCount">The index of the last token group. This value is always overwritten.</param>
        /// <returns>An encoded <see cref="int"/> array.</returns>
        public IEnumerable<int> Tokenize(string line, ref int maxIndices,
            ref int wordCount, ref int last)
        {
            var tokens = new int[line.Length + 1];

            for (var i = 0; i < _lpsTables.Count; i++)
            {
                AssignTokens(line, ref i, ref tokens);
            }

            CreateIndices(ref tokens, ref maxIndices, ref wordCount, ref last);
            return tokens;
        }

        // Encodes the index of each group, and denotes split sequences with -1.
        private void CreateIndices(ref int[] tokens, ref int maxIndices, ref int wordCount, ref int last)
        {
            var idx = 0;
            last = tokens.Length - 1;
            var neg = 0;
            if (tokens[last] > 0)
            {
                var trimEnd = tokens[last];
                while (trimEnd > 0)
                {
                    trimEnd += tokens[--last];
                }

                last++;
            }

            if (_emptyDelim)
            {
                for (var i = 0; i < last; i++)
                {
                    if (maxIndices == -1 || idx < maxIndices)
                    {
                        neg += tokens[i];
                    }
                    else if (tokens[i] == -1 && idx < maxIndices)
                    {
                        tokens[i] = idx;
                        continue;
                    }

                    if (neg < 0)
                    {
                        tokens[i] = -1;
                    }
                    else
                    {
                        tokens[i] = ++idx;
                    }
                }

                wordCount = idx;
            }
            else
            {
                for (var i = 0; i < last; i++)
                {
                    if (maxIndices == -1 || idx < maxIndices)
                    {
                        neg += tokens[i];
                    }
                    else
                    {
                        tokens[i] = idx;
                    }

                    if (neg < 0)
                    {
                        tokens[i] = -1;
                        continue;
                    }

                    if (i == 0 || tokens[i - 1] < 0 && (maxIndices == -1 || idx < maxIndices)) tokens[i] = ++idx;
                    else if (i > 0 && tokens[i - 1] > 0) tokens[i] = idx;
                }
            }

            if (idx == 0) tokens = Array.Empty<int>();
            wordCount = idx;
        }

        // Searches for all matches of the current given pattern in the line.
        private void AssignTokens(string line, ref int idx, ref int[] tokens)
        {
            if (line.Length < _patterns[idx].Length) return;

            switch (_patterns[idx].Length)
            {
                case < 1:
                    break;

                default:
                {
                    int i = 0, j = i;
                    while (i < line.Length)
                    {
                        if (line[i] == _patterns[idx][j])
                        {
                            j++;
                            i++;
                        }

                        if (j == _patterns[idx].Length)
                        {
                            tokens[i - j] -= _patterns[idx].Length;
                            tokens[i] += _patterns[idx].Length;
                            j = _lpsTables[idx][j - 1];
                        }
                        else if (i < line.Length && line[i] != _patterns[idx][j])
                        {
                            if (j != 0)
                            {
                                j = _lpsTables[idx][j - 1];
                            }
                            else
                            {
                                i++;
                            }
                        }
                    }

                    break;
                }
            }
        }

        // Precomputes a fallback table for the current pattern.
        private int[] Precompute(ref int idx)
        {
            var lps = new int[_patterns[idx].Length];
            int len = 0, i = 1;
            while (i < _patterns[idx].Length)
            {
                if (_patterns[idx][i] == _patterns[idx][len])
                {
                    len++;
                    lps[i] = len;
                    i++;
                }
                else
                {
                    if (len != 0)
                    {
                        len = lps[len - 1];
                    }
                    else
                    {
                        lps[i] = 0;
                        i++;
                    }
                }
            }

            return lps;
        }
    }
}