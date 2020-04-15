using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Models
{
    public class HostRequestModel
    {
        public int Id { get; set; }
        public PublicUserDetailsModel Requester { get; set; }
        public string Description { get; set; }
    }
}
