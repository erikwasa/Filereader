using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogFileReader
{
    class Program
    {
        public const string Connect = "CONNECT", Auth = "AUTH", Fail = "FAIL", Success = "SUCCESS";
        public const int Time = 0, SessionId = 1, Status = 2, IpOrUsername = 3;
        public const string DatePattern = @"([12]\d{3}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01]))";
        
        static void Main()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var logFile = File.ReadLines(@"C:\Users\erikw\OneDrive\Skrivbord\Items.txt");

            Regex rgx = new Regex(DatePattern);
            
            string currentDate = "";

            var completedObservations = new List<Observation>();
            var inCompleteObservations = new List<Observation>();
            var ipDictionary = new Dictionary<string, string>();

            foreach (var row in logFile)
            {
                if (row.StartsWith('#'))
                {
                    currentDate = rgx.Match(row).Value;
                }

                else if (!string.IsNullOrWhiteSpace(row))
                {
                    var splitRow = row.Split('\t', ' ');

                    switch (splitRow[Status])
                    {
                        case Connect:

                            ipDictionary.Add(splitRow[SessionId], splitRow[IpOrUsername]);
                            break;

                        case Auth:
                            var partialObservation = new Observation();
                            partialObservation.UserName = splitRow[IpOrUsername];

                            if (ipDictionary.TryGetValue(splitRow[SessionId], out var ipAddress))
                            {
                                partialObservation.IpAddress = ipAddress;
                                partialObservation.SessionId = splitRow[SessionId];

                                inCompleteObservations.Add(partialObservation);
                            }
                            break;

                        case Fail:

                            if (ipDictionary.TryGetValue(splitRow[SessionId], out ipAddress))
                            {
                                int failIndex = inCompleteObservations.FindIndex(x => x.IpAddress == ipAddress);

                                string dateTimeString = currentDate + " " + splitRow[Time];
                                CompleteObservation(failIndex, false, dateTimeString);
                            }
                            break;

                        case Success:

                            if (ipDictionary.TryGetValue(splitRow[SessionId], out ipAddress))
                            {
                                int successIndex = inCompleteObservations.FindIndex(x => x.IpAddress == ipAddress);

                                string dateTimeString = currentDate + " " + splitRow[Time];
                                CompleteObservation(successIndex, true, dateTimeString);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }

            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);

            void CompleteObservation(int index, bool isSucccessful, string timeString)
            {
                //string.Concat(currentDate, " ", timeString);

                if (DateTime.TryParse(timeString, out var formattedTime))
                {
                    inCompleteObservations[index].Success = isSucccessful;
                    inCompleteObservations[index].Time = formattedTime.ToUniversalTime();

                    completedObservations.Add(inCompleteObservations[index]);
                    inCompleteObservations.RemoveAt(index);
                }
            }
            Console.ReadLine();
        }
    }
}
