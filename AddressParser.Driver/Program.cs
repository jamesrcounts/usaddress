namespace AddressParser.Driver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class Program
    {
        static void Main(string[] args)
        {
            var parser = new AddressParser();
            var loop = false;

            Console.WriteLine("Address Parser Driver");
            Console.WriteLine(new string('-', 40));
            Console.WriteLine();
            Console.WriteLine("Example Input:");
            Console.WriteLine("125 Main St, Richmond VA 23221");
            Console.WriteLine();

            do
            {
                Console.WriteLine("Type an address and press <ENTER>:");
                var input = Console.ReadLine();

                var result = parser.ParseAddress(input);
                if (result == null)
                {
                    Console.WriteLine("ERROR. Input could not be parsed.");
                }
                else
                {
                    Console.WriteLine("RESULT: {0}", result);

                    var properties = result
                        .GetType()
                        .GetProperties()
                        .OrderBy(x => x.Name);
                    foreach (var property in properties)
                    {
                        Console.WriteLine(
                            "{0,30} : {1}",
                            property.Name, 
                            property.GetValue(result, null));
                    }
                }

                Console.WriteLine();
                Console.Write("Try again? [Y/N] ");

                loop = Console.ReadLine().ToUpperInvariant() == "Y";
            }
            while (loop);
        }
    }
}
