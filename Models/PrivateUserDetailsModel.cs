using BountyBoardServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Models
{
    public class PrivateUserDetailsModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public GenderId Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
