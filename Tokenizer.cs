namespace InputProcessorApp
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // TODO: Optimization to token assignment
    // TODO: Make tokenization for string list with single call instead of multiple calls for each string
    // i.e. make i[] and j[]
    public class Tokenizer
    {
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

        private void CreateIndices(ref int[] tokens, ref int maxIndices, ref int wordCount)
        {
            int idx = 0, e = tokens.Length - 1;
            var neg = 0;
            if (tokens[e] > 0)
            {
                var trimEnd = tokens[e];
                while (trimEnd > 0)
                {
                    trimEnd += tokens[--e];
                }
            }
            if (_emptyDelim)
            {
                for (var i = 0; i <= e; i++)
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
                for (var i = 0; i <= e; i++)
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