using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BountyBoardServer.Helpers;
using BountyBoardServer.Models;
using NetTopologySuite.Geometries;

namespace BountyBoardServer.Entities
{
    public enum GenderId
    {
        Unspecified,
        Male,
        Female,
        Other
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public byte[] Salt { get; set; }
        public string DisplayName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Location Location { get; set; }
        public GenderId Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Request> Requests { get; set; }

        public PrivateUserDetailsDto ToPrivateUserDetailsDto()
        {
            return new PrivateUserDetailsDto
            {
                Id = this.Id,
                Email = this.Email,
                DisplayName = this.DisplayName,
                DateOfBirth = this.DateOfBirth,
                Gender = this.Gender,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Requests = this.Requests
            };
        }

        public PublicUserDetailsDto ToPublicUserDetailsDto()
        {
            return new PublicUserDetailsDto
            {
                Id = this.Id,
                DisplayName = this.DisplayName,
                Age = UserHelper.GetAge(this.DateOfBirth),
                Gender = this.Gender,
                FirstName = this.FirstName,
                LastName = this.LastName
            };
        }
    }
}
