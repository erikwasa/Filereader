using System;
using System.Collections.Generic;
using System.Text;

namespace LogFileReader
{
    public class Observation
    {
        public string UserName { get; set; }
        public bool Success { get; set; }
        public DateTime Time { get; set; }
        public string IpAddress { get; set; }
        public string SessionId { get; set; }
    }
}
