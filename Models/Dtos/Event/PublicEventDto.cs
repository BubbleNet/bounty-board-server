using BountyBoardServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Models
{
    public class PublicEventDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Game { get; set; }
        public string Version { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public Location EventLocation { get; set; }
        public ICollection<Meeting> Meetings { get; set; }
        public bool Repeating { get; set; }
        public Interval RepeatInterval { get; set; }
        public List<PublicUserDetailsDto> Participants { get; set; }
        public PublicUserDetailsDto Host { get; set; }
        public bool RequestNeeded { get; set; } // Indicates if a user needs to request to join an event
        public bool RequestsOpen { get; set; } // Indicates if requests are allowed for this event
    }
}
