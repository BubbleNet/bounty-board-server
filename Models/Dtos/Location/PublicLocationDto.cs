using BountyBoardServer.Entities;
using Microsoft.EntityFrameworkCore.Update.Internal;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BountyBoardServer.Models.Dtos
{
    public class PublicLocationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Point GeoLocation { get; set; }
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string Additional { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public ICollection<StringScheduleDto> Hours { get; set; }
    }
}
