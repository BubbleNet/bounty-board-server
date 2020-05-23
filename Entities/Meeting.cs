using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Models
{
    public class Meeting
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
