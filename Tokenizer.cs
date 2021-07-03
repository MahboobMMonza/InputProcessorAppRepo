namespace InputProcessorApp
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // TODO: Optimization to token assignment using Aho-Corasick algorithm
    
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
        public Tokenizer(IEnumerable<string> delimiters)
        {
            _lpsTables = new List<int[]>();
            _emptyDelim = false;
            var strings = delimiters.ToList();
            _patterns = strings;
            foreach (var delim in strings)
            {
                if (delim.Equals(""))
                {
                    _emptyDelim = true;
                }

                _lpsTables.Add(Precompute(delim));
            }
        }

        private List<int[]> _lpsTables;
        private List<string> _patterns;

        private bool _emptyDelim;

        /// <summary>
        /// Creates an array with coded integer indicators of each valid and invalid sequence of characters in the line.
        /// </summary>
        /// <param name="line">The current line to tokenize.</param>
        /// <param name="maxIndices">The maximum number of token groups to create.</param>
        /// <param name="wordCount">The index of the last token group. This value is always overwritten.</param>
        /// <returns>An encoded <see cref="int"/> array.</returns>
        public IEnumerable<int> Tokenize(string line, ref int maxIndices,
            ref int wordCount)
        {
            var tokens = new int[line.Length + 1];

            for (var i = 0; i < _lpsTables.Count; i++)
            {
                AssignTokens(line, ref i, ref tokens);
            }
            
            CreateIndices(ref tokens, ref maxIndices, ref wordCount);
            return tokens;
        }

        // Encodes the index of each group, and denotes split sequences with -1.
        private void CreateIndices(ref int[] tokens, ref int maxIndices, ref int wordCount)
        {
            int idx = 0, last = tokens.Length - 1;
            var neg = 0;
            if (tokens[last] > 0)
            {
                var trimEnd = tokens[last];
                while (trimEnd > 0)
                {
                    trimEnd += tokens[--last];
                }
            }
            if (_emptyDelim)
            {
                for (var i = 0; i <= last; i++)
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
                for (var i = 0; i <= last; i++)
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

            for (var i = last + 1; i < tokens.Length; i++)
            {
                tokens[i] = -1;
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
        private static int[] Precompute(string pattern)
        {
            var lps = new int[pattern.Length];
            int len = 0, i = 1;
            while (i < pattern.Length)
            {
                if (pattern[i] == pattern[len])
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