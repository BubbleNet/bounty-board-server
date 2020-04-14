using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace BountyBoardServer.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public byte[] Salt { get; set; }
        public string DisplayName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Point Location { get; set; }
    }
}
