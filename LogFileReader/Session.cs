using System;
using System.Collections.Generic;
using System.Text;

namespace LogFileReader
{
    public class Session
    {
        public Session()
        {
            PartialObservations = new List<Observation>();
        }
        public List<Observation> PartialObservations { get; set; }
        public string IpAddress { get; set; }
        public string SessionId { get; set; }
    }
}
