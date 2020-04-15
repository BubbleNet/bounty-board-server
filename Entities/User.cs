﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public Point Location { get; set; }
        public GenderId Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
