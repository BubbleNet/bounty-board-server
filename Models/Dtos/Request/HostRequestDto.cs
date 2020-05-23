using BountyBoardServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Models
{
    public class HostRequestDto
    {
        public int Id { get; set; }
        public PublicUserDetailsDto Requester { get; set; }
        public string Description { get; set; }
        public RequestStatus Status { get; set; }
    }
}
