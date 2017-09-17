using System;
using System.Collections.Generic;
using System.Text;

namespace EmpathyLibrary.Models
{
    public class SignalLineBeacons
    {
        public DateTime Timestamp { get; set; }
        public string Topic { get; set; }
        public LineBeacon LineBeacon { get; set; }
    }
}
