namespace USAddress.Driver
{
    using System;
    using System.Linq;

    using USAddress.Driver.Properties;

    internal static class Program
    {
        private static void Main()
        {
            var parser = AddressParser.Default;
            bool loop;

            Console.WriteLine(Resources.ProgramTitle);
            Console.WriteLine(new string('-', 40));
            Console.WriteLine();
            Console.WriteLine(Resources.ExampleLabel);
            Console.WriteLine(Resources.ExampleAddress);
            Console.WriteLine();

            do
            {
                Console.WriteLine(Resources.AddressPrompt);
                var input = Console.ReadLine();

                var result = parser.ParseAddress(input);
                if (result == null)
                {
                    Console.WriteLine(Resources.ErrorNotParsable);
                }
                else
                {
                    Console.WriteLine(Resources.ResultFormat, result);

                    var properties = result
                        .GetType()
                        .GetProperties()
                        .OrderBy(x => x.Name);
                    foreach (var property in properties)
                    {
                        Console.WriteLine(
                            Resources.ResultItemFormat,
                            property.Name,
                            property.GetValue(result, null));
                    }
                }

                Console.WriteLine();
                Console.Write(Resources.TryAgainPrompt);

                var readLine = Console.ReadLine() ?? string.Empty;
                loop = readLine.ToUpperInvariant() == "Y";
            }
            while (loop);
        }
    }
}