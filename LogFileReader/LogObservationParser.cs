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

        public string DatePattern { get; private set; }
        public string DateAndTimeLogPattern { get; set; }
        public List<Session> Sessions { get; set; }
        public string CurrentDate { get; private set; }
        public List<Observation> Observations { get; set; }
        public DateTime thisLineTimeStamp;


        public LogObservationParser()
        {
            Observations = new List<Observation>();
            DatePattern = @"([12]\d{3}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01]))"; // YYYY-MM-DD
            DateAndTimeLogPattern = @"# Written at [12]\d{3}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01]) (0[1-9]|1[1-9]|2[1-3]):([0-5][0-9]):([0-5][0-9])"; // # Written at YYYY-MM-DD HH-mm-ss
        }

        public List<Observation> Reader(string path)
        {
            var logLines = File.ReadLines(path);
            Sessions = new List<Session>();
            DateTime lastLineTimeStamp = new DateTime();
            thisLineTimeStamp = new DateTime();

            foreach (var line in logLines)
            {
                Regex rgxLogTime = new Regex(DateAndTimeLogPattern);

                if (rgxLogTime.IsMatch(line))
                {
                    Regex rgxDate = new Regex(DatePattern);
                    CurrentDate = rgxDate.Match(line).Value;
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    var splitRow = line.Split('\t');

                    bool conversionFailed = TryGetDateTimeFromStrings(splitRow[TimeColumn], ref thisLineTimeStamp);

                    if (conversionFailed | lastLineTimeStamp.CompareTo(thisLineTimeStamp) > 0)
                    {
                        // If conversion failed or this lines timestamp is earlier than previous row then continue to next iteration 
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
                                var session = FindSession(splitRow[SessionIdColumn]);
                                int.TryParse(splitRow[SessionIdColumn], out int sessionId);

                                if (sessionId > 0)
                                {
                                    if (session != null)
                                    {
                                        session.ResetSessionDueToDuplicateConnection();
                                    }
                                    else if (session == null)
                                    {
                                        session = new Session();
                                        Sessions.Add(session);
                                    }
                                    session.IpAddress = splitRow[IpOrUsernameColumn];
                                    session.SessionId = sessionId;
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
                                bool success = false;
                                CompleteSessionObservation(splitRow[SessionIdColumn], success);
                                break;
                            }

                        case Success:
                            {
                                bool success = true;
                                CompleteSessionObservation(splitRow[SessionIdColumn], success);

                                break;
                            }

                        default:
                            break;
                    }                   
                } 
            }
            return Observations;
        }

        private bool TryGetDateTimeFromStrings(string timeString, ref DateTime lineTimestamp)
        {
            bool conversionFail = true;

            if(DateTime.TryParse($"{CurrentDate} {timeString}", out DateTime finalDate))
            {
                lineTimestamp = finalDate;
                conversionFail = false;
            }

            return conversionFail;
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

        private void CompleteSessionObservation(string idColumn, bool isSuccessful)
        {
            var session = FindSession(idColumn);

            if (session != null)
            {
                if (session.Username != null)
                {
                    session.IsSuccessful = isSuccessful;
                    session.EventTime = thisLineTimeStamp;
                    Observations.Add(session.CreateObservation());
                }
            }
        }


    }
}
