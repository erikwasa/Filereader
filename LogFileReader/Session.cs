using System;
using System.Collections.Generic;
using System.Text;

namespace LogFileReader
{
    public class Session
    {
        public Session()
        {

        }

        public string IpAddress { get; set; }
        public int SessionId { get; set; }
        public DateTime? EventTime { get; set; }
        public bool? IsSuccessful { get; set; }

        private string username;

        public string Username
        {
            get { return username; }
            set 
            {
                if (username == null)
                {
                    username = value;
                }
            }
        }

        public Observation CreateObservation()
        {
            var observation = new Observation();
            observation.IpAddress = this.IpAddress;
            observation.Success = this.IsSuccessful;
            observation.ObservationTime = this.EventTime;
            observation.UserName = this.Username;

            ResetObservationproperties();

            return observation;
        }
        private void ResetObservationproperties()
        {
            this.IsSuccessful = null;
            this.EventTime = null;
            this.Username = null;
        }

    }
}
