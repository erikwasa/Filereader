using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogFileReader
{
    class Program
    {


        static void Main()
        {
            var logFile = @"C:\Users\erikw\Downloads\logfile_micke (1).txt";

            //var logFile = @"C:\Users\erikw\OneDrive\Skrivbord\Items.txt";

            var parser = new LogObservationParser();
            var observations = parser.Reader(logFile);

            foreach (var observation in observations)
            {
                Console.WriteLine(observation.IpAddress);
                Console.WriteLine(observation.ObservationTime);
                Console.WriteLine(observation.Success);
                Console.WriteLine(observation.UserName);
                Console.WriteLine();
            }

            Console.ReadLine();
        }
    }
}
