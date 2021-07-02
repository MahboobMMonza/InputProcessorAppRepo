namespace InputProcessorApp
{
 
    using System;
    using System.IO;
    class Program
    {
        static void Main(string[] args)
        {
            // Demo code. Read textTextStream.txt to see the samples. Unless specified, sample lines will be
            // broken up by the list of strings given after the stream reader (delimiters)
            var ip = new InputParser(new StreamReader("testTextStream.txt"), 
                "ree", "test", "another splitter", " ", "\t");
            // Splits line 1 by all the spaces and prints each word on a new line with ip.Next()
            Console.WriteLine($"--------------------First line--------------------");
            
            do
            {
                Console.WriteLine("Next(): " + ip.Next());
            } while (ip.HasMoreTokens());
            
            // Does the same thing as the first example, but this time splitting by all the delimiters that were given
            // Notice how the program interpreted a case such as "testest" as two matches for "test" and removed them
            // This is something I am considering to modify to let the user decide if they wish for this to be considered
            // a match
            Console.WriteLine("--------------------Second line--------------------");
            do
            {
                Console.WriteLine("Next(): " + ip.Next());
            } while (ip.HasMoreTokens());
            
            // This is a preview of NextSafeLine(), which trims all instances of the given delimiters from the beginning
            // and end of each line
            Console.WriteLine("--------------------Third line--------------------");
            Console.WriteLine("NextSafeLine(): " + ip.NextTrimmedLine());
            
            Console.WriteLine("--------------------Fourth line--------------------");
            ip.Indices = 6;
            do
            {
                Console.WriteLine("Next(): " + ip.Next());
            } while (ip.HasMoreTokens());

            ip.Indices = -1;
            // This is a preview of NextLine(), which just prints the next line unchanged
            Console.WriteLine("--------------------Fifth line--------------------");
            Console.WriteLine("NextLine(): " + ip.NextLine());
            // Testing the ability of the program to parse strings into numerics
            Console.WriteLine("--------------------Reading numerics--------------------");
            Console.WriteLine($"This will read 235 and output 235 - 5: {ip.NextShort() - 5}");
            Console.WriteLine($"This will read 123456789 and output the number - 1: {ip.NextInt() - 1}");
            Console.WriteLine($"This will do the same thing: {ip.NextInt() - 1}");
            Console.WriteLine($"This will read -123456789 and output the number + 1: {ip.NextInt() + 1}");
            Console.WriteLine($"This will read -0xff (-255 in hex) and output the number - 20: {ip.NextShort() - 20}");
            Console.WriteLine("This will read 123.456 as a float and output the number - 0.456" +
                              $" to 2 decimal places: {ip.NextFloat() - 0.456:F2}");
            Console.WriteLine($"This will read NaN as a double: {ip.NextDouble()}");
            Console.WriteLine($"This will read inf as a float: {ip.NextFloat()}");
            Console.WriteLine($"This will read -infinity as a double: {ip.NextDouble()}");
            Console.WriteLine($"This will read -47871085561486217 and output the number + 7: {ip.NextLong() + 7}");
            Console.WriteLine($"This will read 1097752410090582 and output the number - 82: {ip.NextLong() - 82}");
            Console.WriteLine($"This will read and print 25747 from binary to base10: {ip.NextInt()}");
            Console.WriteLine("This will read KB3VEA4VC0 in base34 and output the " +
                              $"number in base10: {ip.NextDecimal(34)}");
            // Testing program's ability to read values such as true, yes, y, t, 1 as bools that evaluate true and
            // false, no, n, f, 0 as bools that evaluate to false
            Console.WriteLine("--------------------Reading states--------------------");
            
            do
            {
                Console.WriteLine($"NextBool(): {ip.NextBool()}");
            } while (ip.HasMoreTokens());
        }
    }
}