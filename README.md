# Input Processor
This is an Input Processor. It parses text input line-by-line, 
and splits it based on certain given string patterns, along with
limits set for the number of splits. It then returns the groups of words
around the splits as string, but can also process them into other data
types, including numerics up to base62, along with arrays of these data 
types.

## Languages/Stacks
This application was developed entirely using C#.

## Return Data Types
- String
- Character
- Boolean
- Short (16-bit)
- Integer (32-bit)
- Long Integer (64-bit)
- Float
- Double
- Decimal
- Unsigned Integer
- Unsigned Long Integer

## Algorithms
- String matching: KMP algorithm (unoptimized shifting).
- String parsing to numeric types: Custom parsing algorithm for converting
string in any base until base62.
  
## Next Steps
- Handle consecutive connected matches
  - If a delimiter was "test" and the given string had "testestimate",
  normally the program would match at index 0 and again at index 3,
    returning only "imate" in the Next() call.
    
  - Allow an option for the user to determine whether or not to continue
  from partial matches after a word has ended or restart the search from
    the beginning.
- Allow the user an option to escape pattern instances using '\\'.
- Allow for going to different lines in text files (jump to nth line).
- Implement ArrayMaker class.
- Implement Aho-Corasick algorithm for string matching.
  - Adjust for consecutive match options during a fallback at the end
  of a match.
- Decide if dedicated Signed Int32 and Int64 parsers are necessary.
- Implement conversion of UInt32 and UInt64 into bool 
  array/bitset representations.
- Document the code properly.
- Implement other methods in the parser.
- Implement the algorithm to match multiple Regex patterns when splitting.
- Possibly change the class into an interface and generalize for
different types of input streams.