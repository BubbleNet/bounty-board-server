using BountyBoardServer.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Entities
{
    public enum Weekday
    {
        Monday = 0,
        Tuesday = 1,
        Wednesday = 2,
        Thursday = 3,
        Friday = 4,
        Saturday = 5,
        Sunday = 6
    }

    public enum ClockType
    {
        Open = 0,
        Close = 1
    }
    public class Schedule
    {
        public int Id { get; set; }
        public Weekday Day { get; set; }
        public ClockType Type { get; set; }
        public DateTime Time { get; set; }

        public StringScheduleDto ToStringScheduleDto()
        {
            return new StringScheduleDto
            {
                Id = this.Id,
                Day = this.Day.ToString(),
                Type = this.Type.ToString(),
                Time = this.Time.ToString("h:mm tt", CultureInfo.InvariantCulture)

            };
        }
    }
}
