using BountyBoardServer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Entities
{
    public enum RequestStatus
    {
        Pending,
        Approved,
        Denied,
        Deleted
    }
    public class Request
    {
        public int Id { get; set; }
        public User Requester { get; set; }
        public Event Event { get; set; }
        public string Description { get; set; }
        public RequestStatus Status { get; set; }

        public HostRequestDto ToHostRequestDto()
        {
            return new HostRequestDto
            {
                Id = this.Id,
                Requester = this.Requester.ToPublicUserDetailsDto(),
                Description = this.Description,
                Status = this.Status
            };
        }
    }
}
