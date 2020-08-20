using System;
using System.Collections.Generic;
using System.Text;

namespace LogFileReader
{
    public class Observation
    {
        public string UserName { get; set; }
        private bool success;

        public bool? Success
        {
            get { return success; }
            set 
            {
                if (value != null)
                {
                    success = (bool)value;
                }
            }
        }
        private DateTime observationTime;
        public DateTime? ObservationTime
        {
            get { return observationTime; }
            set
            {
                if (value != null)
                {
                    observationTime = (DateTime)value;
                }
            }
        }

        public string IpAddress { get; set; }
    }
}
