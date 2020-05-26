using BountyBoardServer.Models;
using BountyBoardServer.Models.Dtos;
using Microsoft.EntityFrameworkCore.Update.Internal;
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
        public string Additional { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public ICollection<Schedule> Hours { get; set; }
        public ICollection<User> Admins { get; set; }

        public Location Update(Location newLocation)
        {
            this.Name = newLocation.Name;
            this.StreetNumber = newLocation.StreetNumber;
            this.StreetName = newLocation.StreetName;
            this.Additional = newLocation.Additional;
            this.City = newLocation.City;
            this.ZipCode = newLocation.ZipCode;
            this.State = newLocation.State;
            this.Country = newLocation.Country;
            this.Description = newLocation.Description;
            return this;
        }

        public PrivateLocationDto ToPrivateLocationDto()
        {
            List<PublicUserDetailsDto> admins = new List<PublicUserDetailsDto>();
            foreach (User u in this.Admins)
            {
                admins.Add(u.ToPublicUserDetailsDto());
            }
            List<StringScheduleDto> schedule = new List<StringScheduleDto>();
            foreach(Schedule s in this.Hours)
            {
                schedule.Add(s.ToStringScheduleDto());
            }
            return new PrivateLocationDto {
                Id = this.Id,
                Name = this.Name,
                StreetNumber = this.StreetNumber,
                StreetName = this.StreetName,
                Additional = this.Additional,
                City = this.City,
                ZipCode = this.ZipCode,
                State = this.State,
                Country = this.Country,
                Description = this.Description,
                GeoLocation = this.GeoLocation,
                Hours = schedule,
                Admins = admins
            };
        }

        public PublicLocationDto ToPublicLocationDto()
        {
            List<StringScheduleDto> schedule = new List<StringScheduleDto>();
            foreach (Schedule s in this.Hours)
            {
                schedule.Add(s.ToStringScheduleDto());
            }
            return new PublicLocationDto
            {
                Id = this.Id,
                Name = this.Name,
                StreetNumber = this.StreetNumber,
                StreetName = this.StreetName,
                Additional = this.Additional,
                City = this.City,
                ZipCode = this.ZipCode,
                State = this.State,
                Country = this.Country,
                Description = this.Description,
                GeoLocation = this.GeoLocation,
                Hours = schedule
            };
        }
    }
}
