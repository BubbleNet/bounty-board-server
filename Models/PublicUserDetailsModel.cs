using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BountyBoardServer.Entities;

namespace BountyBoardServer.Models
{
    public class PublicUserDetailsModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public int Age { get; set; }
        public GenderId Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
