using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LogFileReader
{
    class LogObservationParser
    {
        public const string Connect = "CONNECT", Auth = "AUTH", Fail = "FAIL", Success = "SUCCESS";
        public const int TimeColumn = 0, SessionIdColumn = 1, StatusColumn = 2, IpOrUsernameColumn = 3;

        public string Pattern { get; private set; }
        public List<Session> Sessions { get; set; }
        public string CurrentDate { get; private set; }
        public List<Observation> Observations { get; set; }

        public LogObservationParser()
        {
            Observations = new List<Observation>();
            Pattern = @"([12]\d{3}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01]))"; // YYYY-MM-DD
        }

        public List<Observation> Reader(string path)
        {
            var logLines = File.ReadLines(path);
            Sessions = new List<Session>();
            DateTime lastLineTimeStamp = new DateTime();
            DateTime thisLineTimeStamp;

            foreach (var line in logLines)
            {
                if (line.StartsWith("# Written at"))
                {
                    Regex rgx = new Regex(Pattern);
                    CurrentDate = rgx.Match(line).Value;
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    var splitRow = line.Split('\t');

                    thisLineTimeStamp = GetDateTimeFromTimeAndDateStrings(splitRow[TimeColumn]);

                    if (lastLineTimeStamp.CompareTo(thisLineTimeStamp) > 0)
                    {
                        // If this lines timestamp is earlier than previous row then continue to next iteration 
                        continue;
                    }
                    else
                    {
                        lastLineTimeStamp = thisLineTimeStamp;
                    }

                    switch (splitRow[StatusColumn])
                    {
                        case Connect:
                            {
                                if (int.TryParse(splitRow[SessionIdColumn], out int sessionId))
                                {
                                    var session = new Session();
                                    session.IpAddress = splitRow[IpOrUsernameColumn];
                                    session.SessionId = sessionId;

                                    Sessions.Add(session);
                                }
                                break;
                            }

                        case Auth:
                            {
                                var session = FindSession(splitRow[SessionIdColumn]);
                                if (session != null)
                                {
                                    session.Username = splitRow[IpOrUsernameColumn];
                                }
                                break;
                            }

                        case Fail:
                            {
                                var session = FindSession(splitRow[SessionIdColumn]);
                                if (session != null)
                                {
                                    session.IsSuccessful = false;
                                    session.EventTime = GetDateTimeFromTimeAndDateStrings(splitRow[TimeColumn]);
                                    Observations.Add(session.CreateObservation());
                                }
                                break;
                            }

                        case Success:
                            {
                                var session = FindSession(splitRow[SessionIdColumn]);

                                if (session != null)
                                {
                                    session.IsSuccessful = true;
                                    session.EventTime = GetDateTimeFromTimeAndDateStrings(splitRow[TimeColumn]);
                                    Observations.Add(session.CreateObservation());
                                }
                                break;
                            }

                        default:
                            break;
                    }
                }
            }
            return Observations;
        }

        private DateTime GetDateTimeFromTimeAndDateStrings(string timeString)
        {
           //FELHANTERING!!!!!!!!!!!
            var finalDate = DateTime.Parse($"{CurrentDate} {timeString}");
            return finalDate;
        }

        private Session FindSession(string sessionStringId)
        {
            if (int.TryParse(sessionStringId, out int sessionId))
            {
                var session = Sessions.Find(x => x.SessionId == sessionId);
                return session;
            }
            return null;
        }
    }
}
