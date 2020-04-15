using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Entities
{
    public class Request
    {
        public int Id { get; set; }
        public User Requester { get; set; }
        public Event Event { get; set; }
        public string Description { get; set; }
        public bool Approved { get; set; }
    }
}
