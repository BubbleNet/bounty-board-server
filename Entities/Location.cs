using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BountyBoardServer.Entities
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Point GeoLocation { get; set; }
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public ICollection<Schedule> Hours { get; set; }
    }
}
