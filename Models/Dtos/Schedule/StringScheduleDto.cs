using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Models.Dtos
{
    public class StringScheduleDto
    {
        public int Id { get; set; }
        public string Day { get; set; }
        public string Type { get; set; }
        public string Time { get; set; }
    }
}
