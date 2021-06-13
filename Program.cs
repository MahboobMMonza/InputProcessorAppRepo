using System;

namespace InputProcessorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Demo code
            InputParser ip = new InputParser(Console.In, "test", "another splitter", " ", "\t", "\'");
            /*Console.WriteLine(
                " Copy this input string: this is a test to see if the input parser works. Every whitespace on this " +
                "and every instance of 'test' or 'another splitter', (the_string_in_the_param) or the	 " +
                "single quote will be skipped over another splitter and another splitter the rest of the words	will " +
                "be grouped around them and each group will be printed line by line until the last valid sequence." +
                "   test another splitter ''");
            Console.WriteLine("Testing Next() call...");
            do
            {
                Console.WriteLine("Next() call gives: " + ip.Next());
            } while (ip.HasMoreTokens());

            Console.WriteLine("Setting a max index and checking to see how it returns...");
            // Setting indices to 5, meaning only 5 split groups will exist. Note that consecutive split patterns
            // will all be ignored until a valid sequence is encountered
            ip.Indices = 5;
            // Now that we have proved that it works, removing single character splitters
            ip.Delimiters = new[] {"test", "another splitter"};
            Console.WriteLine("Copy this input string and paste to console: These words will split whenever " +
                              "test and another splitterappear and will only testanother splittersplit into five" +
                              " groups. test this is the last group any another splitter split pattern test " +
                              "will be ignored test.");
            
            do
            {
                Console.WriteLine("Next() call gives: " + ip.Next());
            } while (ip.HasMoreTokens());*/
            ip.Delimiters = new[] {" ", "\t",};
            do
            {
                Console.WriteLine("Next(): " + ip.Next());
            } while (ip.HasMoreTokens());
        }
    }
}