using System;
using System.Collections.Generic;
using System.Text;

namespace EmpathyLibrary.Models
{
    public class LineBeacon
    {
        public string Type { get; set; }
        public string ReplyToken { get; set; }
        public Source Source { get; set; }
        public long Timestamp { get; set; }
        public Beacon Beacon { get; set; }
        public bool Replied { get; set; }
    }
}
