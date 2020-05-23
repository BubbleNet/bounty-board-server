using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Entities
{
    public enum Weekday
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }

    public enum ClockType
    {
        Open,
        Close
    }
    public class Schedule
    {
        public int Id { get; set; }
        public Weekday Day { get; set; }
        public ClockType Type { get; set; }
        public TimeSpan Time { get; set; }
    }
}
